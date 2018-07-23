
query_sum_score = """
SELECT
  ai_name,
  u_name,
  time_at,
  score
FROM (
       SELECT
         ai_name,
         u_name,
         time_at,
         score,
         cnt
       FROM
         (SELECT
            max(create_at) time_at,
            ai_name,
            u_name,
            sum(energy)    score,
            count(1)       cnt
          FROM (SELECT
                  ai_name,
                  u_name,
                  energy,
                  problem,
                  create_at,
                  rank()
                  OVER (
                    PARTITION BY (ai_name, u_name, problem)
                    ORDER BY
                      create_at DESC ) AS rank
                FROM (select * from score where energy > 0) t4
                WHERE game_type LIKE '{game_type}'
               ) AS t1
          WHERE problem IN (SELECT
                                  DISTINCT problem
                                FROM board_score
                                WHERE
                                  t1.problem < 35
                                  AND game_type LIKE '{game_type}')
                AND rank = 1

          GROUP BY ai_name, u_name) AS t2
     ) AS t3
WHERE cnt = (SELECT count(1)
             FROM (SELECT
                     DISTINCT problem
                   FROM board_score
                   WHERE
                     problem < 35
                     AND game_type LIKE '{game_type}') t)
"""

query_latest_scores = """
SELECT
  id,
  u_name,
  ai_name,
  problem,
  energy,
  create_at,
  game_type
FROM (SELECT
        *,
        row_number()
        OVER (
          PARTITION BY (ai_name, u_name, problem)
          ORDER BY (energy) ) AS rank
      FROM (SELECT *
            FROM score
            WHERE energy > 0 AND game_type LIKE '{game_type}') t1
     ) t2
WHERE rank = 1;
"""

query_error_scores = """
SELECT
  id,
  u_name,
  ai_name,
  problem,
  energy,
  create_at,
  message
FROM (SELECT
        *,
        row_number()
        OVER (
          PARTITION BY (ai_name, u_name, problem)
          ORDER BY create_at DESC ) AS rank
      FROM (SELECT *
            FROM score
            WHERE game_type LIKE '{game_type}') t1
     ) t2
WHERE rank = 1
AND energy <= 0;
"""

query_board_score = """
SELECT *
FROM
  (SELECT
     problem,
     energy,
     rank()
     OVER (
       PARTITION BY (problem)
       ORDER BY
         energy ASC ) AS rank
   FROM board_score
   WHERE game_type LIKE '{game_type}') t1
WHERE rank = 1 OR rank = 10;
"""

query_board_score_sum = """
SELECT sum(energy) as score
FROM (
       SELECT
         problem,
         energy,
         rank()
         OVER (
           PARTITION BY (problem)
           ORDER BY
             energy ASC ) AS rank
       FROM board_score
       WHERE game_type LIKE '{game_type}'
        AND problem < 35
     ) t
where rank = '{rank}'
"""