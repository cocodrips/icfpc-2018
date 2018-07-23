import collections
import os
import io
import json
import subprocess
import numbers
from flask import Flask, request, abort, send_file, render_template, redirect
from sqlalchemy.sql import text
from flask_sqlalchemy import SQLAlchemy
from sys import platform
import tempfile
import shutil
import locale
import queries
import datetime

if platform == "linux" or platform == "linux2":
    locale.setlocale(locale.LC_NUMERIC, 'ja_JP.utf8')
else:
    locale.setlocale(locale.LC_NUMERIC, 'ja_JP')

app = Flask(__name__)
app.config[
    'SQLALCHEMY_DATABASE_URI'] = 'postgresql://root:root@localhost:{}/icfpc'.format(
    os.environ.get('PSQL_PORT', 5432)
)
app.config['TEAM_ID'] = '9364648f7acd496a948fba7c76a10501'
app.config['SQLALCHEMY_TRACK_MODIFICATIONS'] = True
db = SQLAlchemy(app)


class Score(db.Model):
    __tablename__ = 'score'
    id = db.Column(db.Integer, primary_key=True, autoincrement=True)
    u_name = db.Column(db.String(80))
    ai_name = db.Column(db.String(80))
    energy = db.Column(db.BigInteger, default=-10)
    problem = db.Column(db.Integer)
    commands = db.Column(db.Integer, default=-10)
    spent_time = db.Column(db.Integer, default=-10)
    create_at = db.Column(db.DateTime, default=db.func.now())
    game_type = db.Column(db.String(10), default='LA')
    message = db.Column(db.String(255))


class LeaderBoardScore(db.Model):
    __tablename__ = 'board_score'
    id = db.Column(db.Integer, primary_key=True, autoincrement=True)
    energy = db.Column(db.BigInteger, default=-10)
    problem = db.Column(db.Integer)
    game_type = db.Column(db.String(10), default='LA')


@app.context_processor
def utility_processer():
    def format_number(amount):
        if isinstance(amount, numbers.Number):
            return locale.format('%d', amount, True)
        return True

    return dict(format_number=format_number)


@app.cli.command()
def init_db():
    """
    db insert test
    """
    db.drop_all()
    db.create_all()


@app.cli.command()
def update_leader_board():
    """
    db insert test
    """
    import pandas as pd
    import pathlib

    # db.engine.execute(text('''TRUNCATE TABLE board_score'''))
    # db.session.commit()

    data_path = pathlib.Path('../icfpcontest2018.github.io/_data/')
    full_df = pd.read_csv(data_path / 'full_standings_live.csv')
    la_df = pd.read_csv(data_path / 'lgtn_standings_live.csv')
    _i = 0
    for game_type in ['FA', 'LA', 'FD', 'FR']:
        df = full_df
        if game_type.startswith('L'):
            df = la_df
        gtype = df[df.probNum.str.startswith(game_type)]
        for _, (energy, probNum) in gtype.groupby('probNum').head(10)[
            ['energy', 'probNum']].iterrows():
            score = LeaderBoardScore(problem=int(probNum[2:]),
                                     game_type=game_type,
                                     energy=energy)

            db.session.add(score)
            if _i != int(probNum[2:]):
                print(probNum, )
                db.session.commit()
                _i = int(probNum[2:])
        db.session.commit()


@app.route("/")
def _index():
    return _scoreboard('FA')


@app.route("/scoreboard/<_type>")
def _scoreboard(_type):
    (problems, ai_names, highest,
     scores, board_scores, sum_scores) = get_latest_scores(_type)

    best_query = queries.query_board_score_sum.format(game_type=_type, rank=1)
    bests = db.engine.execute(text(best_query))
    tens_query = queries.query_board_score_sum.format(game_type=_type, rank=10)
    tens = db.engine.execute(text(tens_query))

    board_score_sums = {}
    for best, ten in zip(bests, tens):
        board_score_sums = {
            1: best['score'],
            10: ten['score']
        }
    return render_template('index.html',
                           problems=problems,
                           ai_names=ai_names,
                           highest=highest,
                           scores=scores,
                           board_scores=board_scores,
                           sum_scores=sum_scores,
                           board_score_sums=board_score_sums)


def get_latest_scores(_type):
    '''現在のスコアを取得する'''
    sql = queries.query_latest_scores.format(game_type=_type)
    results = db.engine.execute(text(sql))
    scores = collections.defaultdict(dict)

    problems = set()
    ai_names = {}
    highest = {}

    for _result in results:
        ai_name = _result['ai_name']
        problem = _result['problem']
        enery = _result['energy']
        create_at = _result['create_at']
        scores[ai_name][problem] = {
            'id': _result['id'],
            'energy': _result['energy'],
            'create_at': _result['create_at'],
        }

        ai_names.setdefault(ai_name, datetime.datetime.now())
        ai_names[ai_name] = min(ai_names[ai_name], create_at)
        problems.add(problem)
        if highest.get(problem):
            if enery > 0:
                highest[problem] = min(enery, highest[problem])
        else:
            if enery > 0:
                highest[problem] = enery
    ai_names = [a for t, a in
                sorted([(time, ai) for ai, time in ai_names.items()],
                       reverse=True)]

    sql = queries.query_board_score.format(game_type=_type)
    results = db.engine.execute(text(sql))
    board_scores = collections.defaultdict(dict)

    for result in results:
        board_scores[result['problem']][result['rank']] = result['energy']

    sql = queries.query_sum_score.format(game_type=_type)
    results = db.engine.execute(text(sql))
    sum_scores = collections.defaultdict(list)
    for result in results:
        sum_scores[result['u_name']].append(
            (result['time_at'], result['ai_name'], result['score']))
    return (sorted(list(problems)), ai_names, highest,
            scores, board_scores, sum_scores)


@app.route("/add", methods=['POST'])
def add_data():
    f = request.files.get('nbt')
    u_name = request.form.get('user')
    ai_name = request.form.get('ai')
    problem = int(request.form.get('problem'))
    game_type = request.form.get('type', 'LA')
    if not u_name or not ai_name or not problem:
        abort(500)

    score = Score(u_name=u_name, ai_name=ai_name,
                  problem=problem, game_type=game_type)
    db.session.add(score)
    db.session.commit()

    fpath = '/data/{}.nbt'.format(score.id)
    f.save(fpath)

    cmd = "/usr/local/bin/aws s3 cp {3} s3://icfpc-udon-2018/nbt/{0}{1:03d}_{2}.nbt".format(
        game_type,
        problem,
        score.id, fpath
    )
    proc = subprocess.Popen(cmd.split(' '), stdout=subprocess.PIPE,
                            stderr=subprocess.PIPE)
    proc.wait()
    errs = proc.stderr.read()

    if proc.returncode != 0:
        return errs

    return str(score.id)


@app.route("/update/<_id>", methods=['POST'])
def update_score(_id):
    score = Score.query.filter_by(id=_id).first()
    data = request.json
    if data['energy']:
        score.energy = data['energy']
        score.commands = data['commands']
        score.spent_time = data['time']
    else:
        score.energy = 0
        score.message = data['message']
    db.session.commit()
    return "OK"


@app.route("/data/<sid>", methods=['GET'])
def get_data(sid):
    score = Score.query.filter_by(id=sid).first()
    if not score:
        abort(500)
    fname = "{}_{}{:03d}.nbt".format(score.ai_name,
                                     score.game_type,
                                     score.problem)
    fpath = '/data/{}.nbt'.format(sid)
    return send_file(fpath, as_attachment=True,
                     attachment_filename=fname,
                     mimetype='application/octet-stream')


@app.route("/simulate/<_id>", methods=['GET'])
def simulate(_id):
    return render_template('exec-trace.html',
                           id=_id)


@app.route('/nbt/<_id>')
def get_nbt(_id):
    return app.send_static_file('result/{}.nbt'.format(_id))


@app.route('/mdl/<_type>/<_id>')
def get_mdl(_type, _id):
    return app.send_static_file("data/problems/{}{:03d}_tgt.mdl".format(
        _type, int(_id)
    ))


@app.route("/submission", methods=['GET'])
def get_submission():
    result = db.engine.execute('''
    SELECT
  min(id) as id_,
  problem,
  game_type
FROM (SELECT
        id,
        problem,
        game_type,
        rank()
        OVER (
          PARTITION BY (problem, game_type)
          ORDER BY
            energy ASC ) AS rank
      FROM score
      WHERE energy > 0) t
WHERE rank = 1 AND game_type NOT LIKE 'LA'
GROUP BY (game_type, problem);
''')

    with tempfile.TemporaryDirectory() as temp_dir:
        for row in result:
            fid = row['id_']
            problem = row['problem']
            game_type = row['game_type']
            shutil.copyfile('/data/{}.nbt'.format(fid),
                            '{0}/{2}{1:03d}.nbt'.format(temp_dir,
                                                        problem,
                                                        game_type))
        zip_filename = '{}/submission.zip'.format(temp_dir)
        subprocess.call(
            'zip -e --password 9364648f7acd496a948fba7c76a10501 {} *.nbt'.format(
                zip_filename), shell=True, cwd=temp_dir);
        return send_file(zip_filename, attachment_filename='submission.zip',
                         as_attachment=True)


if __name__ == '__main__':
    app.run(
        host=os.environ.get('FLASK_HOST', '0.0.0.0'),
        port=int(os.environ.get('FLASK_PORT', 5050)),
        debug=bool(os.environ.get('FLASK_DEBUG', 1)),
    )
