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
    data_path = pathlib.Path('../icfpcontest2018.github.io/_data/')
    full_df = pd.read_csv(data_path / 'full_standings_live.csv')
    la_df = pd.read_csv(data_path / 'lgtn_standings_live.csv')
    for game_type in ['FA', 'LA', 'FD', 'FR']:
        df = full_df
        if game_type.startswith('L'):
            df = la_df
        gtype = df[df.probNum.str.startswith(game_type)]
        best_energies = gtype.groupby('probNum').min()['energy']
        for problem, energy in best_energies.items():
            print(problem, energy)
            score = LeaderBoardScore(problem=int(problem[2:]),
                                     game_type=game_type,
                                     energy=energy)
            db.session.add(score)
    db.session.commit()


@app.route("/")
def _index():
    return _scoreboard('LA')


@app.route("/scoreboard/<_type>")
def _scoreboard(_type):
    (problems, ai_names, highest, 
     scores, board_scores, sum_scores) = get_latest_scores(_type)
    return render_template('index.html',
                           problems=problems,
                           ai_names=ai_names,
                           highest=highest,
                           scores=scores,
                           board_scores=board_scores,
                           sum_scores=sum_scores)


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
            'energy': _result['energy'],
            'create_at': _result['create_at'],
        }

        ai_names.setdefault(ai_name, datetime.datetime.now())
        ai_names[ai_name] = min(ai_names[ai_name], create_at)
        problems.add(problem)
        if highest.get(problem):
            highest[problem] = min(enery, highest[problem])
        else:
            highest[problem] = enery
    ai_names = [a for t, a in
                sorted([(time, ai) for ai, time in ai_names.items()],
                       reverse=True)]
    
    sql = queries.query_board_score.format(game_type=_type)
    results = db.engine.execute(text(sql))
    board_scores = {}
    for result in results:
        board_scores[result['problem']] = result['energy']

    sql = queries.query_sum_score
    results = db.engine.execute(text(sql))
    sum_scores = collections.defaultdict(list)
    for result in results:
        sum_scores[result['u_name']].append(
            (result['time_at'], result['ai_name'], result['score']))
    return (sorted(list(problems)), ai_names, highest,
            scores, board_scores, sum_scores)


@app.route("/add", methods=['POST'])
def add_data():
    print(request.data)
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
    cmd = "/usr/local/bin/node score.js {2}{0:03d} {1}".format(
        problem,
        fpath, game_type
    )
    print(cmd)
    proc = subprocess.Popen(cmd.split(' '),
                            cwd="../simulator",
                            stdout=subprocess.PIPE, stderr=subprocess.PIPE)
    # try:
    proc.wait()
    outs = proc.stdout.read()
    errs = proc.stderr.read()

    if proc.returncode != 0:
        return errs

    output = json.loads(outs.decode('utf-8'))
    score.energy = output['energy']
    score.commands = output['commands']
    score.spent_time = output['time']
    db.session.commit()
    return str(score.id)


@app.route("/data/<sid>", methods=['GET'])
def get_data(sid):
    score = Score.query.filter_by(id=sid).first()
    if not score:
        abort(500)
    fname = "{}_prob{}.nbt".format(score.ai_name, score.problem)
    fpath = '/data/{}.nbt'.format(sid)
    return send_file(fpath, as_attachment=True,
                     attachment_filename=fname,
                     mimetype='application/octet-stream')


@app.route("/submission", methods=['GET'])
def get_submission():
    result = db.engine.execute('''
        SELECT
          s.id,
          s.problem
        FROM (SELECT
                u_name,
                ai_name,
                problem,
                min(energy) AS score_min
              FROM score
              WHERE energy > 0
              GROUP BY u_name,
                ai_name,
                problem) latest
          LEFT JOIN score s
            ON latest.u_name = s.u_name
               AND latest.ai_name = s.ai_name
               AND latest.problem = s.problem
               AND latest.score_min = s.energy;
        ''')

    with tempfile.TemporaryDirectory() as temp_dir:
        for row in result:
            fid = row['id']
            problem = row['problem']
            shutil.copyfile('/data/{}.nbt'.format(fid),
                            '{0}/LA{1:03d}.nbt'.format(temp_dir, problem))
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
        # processes=10,
    )
