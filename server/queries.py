
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
                FROM score
               ) AS t1
          WHERE energy > 0
                AND (problem <= 5 OR problem = 10 OR problem = 20)
                AND rank = 1
          GROUP BY ai_name, u_name) AS t2
     ) AS t3
WHERE cnt = 7
"""

query_latest_scores ="""
SELECT
  s.u_name,
  s.ai_name,
  s.energy,
  s.problem,
  s.commands,
  s.spent_time,
  s.create_at
FROM (SELECT
        u_name,
        ai_name,
        problem,
        max(create_at) AS create_min
      FROM score
      WHERE energy > 0 and game_type LIKE '{game_type}'
      GROUP BY u_name,
        ai_name,
        problem) latest
  LEFT JOIN score s
    ON latest.u_name = s.u_name
       AND latest.ai_name = s.ai_name
       AND latest.problem = s.problem
       AND latest.create_min = s.create_at;
"""