import collections
import os
import io
import json
import subprocess
import numbers
from flask import Flask, request, abort, send_file, render_template
from sqlalchemy.sql import text
import zipfile
from flask_sqlalchemy import SQLAlchemy
from sys import platform

import locale

if platform == "linux" or platform == "linux2":
    locale.setlocale(locale.LC_NUMERIC, 'ja_JP.utf8')
else:
    locale.setlocale(locale.LC_NUMERIC, 'ja_JP')

app = Flask(__name__)
app.config[
    'SQLALCHEMY_DATABASE_URI'] = 'postgresql://root:root@localhost:{}/icfpc'.format(
    os.environ.get('PSQL_PORT', 5432)
)
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


@app.route("/")
def hello():
    return "Score server"


@app.route("/scoreboard")
def _scoreboard():
    problems, ai_names, highest, scores = get_latest_scores()
    return render_template('index.html',
                           problems=problems,
                           ai_names=ai_names,
                           highest=highest,
                           scores=scores)


def get_latest_scores():
    '''現在のスコアを取得する'''
    sql = """
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
        min(create_at) AS create_min
      FROM score
      WHERE energy > 0 
      GROUP BY u_name,
        ai_name,
        problem) latest
  LEFT JOIN score s
    ON latest.u_name = s.u_name
       AND latest.ai_name = s.ai_name
       AND latest.problem = s.problem
       AND latest.create_min = s.create_at;
"""

    results = db.engine.execute(text(sql))
    scores = collections.defaultdict(dict)

    problems = set()
    ai_names = set()
    highest = {}

    for _result in results:
        ai_name = _result['ai_name']
        problem = _result['problem']
        enery = _result['energy']
        scores[ai_name][problem] = enery
        ai_names.add(ai_name)
        problems.add(problem)
        if highest.get(problem):
            highest[problem] = min(enery, highest[problem])
        else:
            highest[problem] = enery
    return sorted(list(problems)), ai_names, highest, scores


@app.route("/add", methods=['POST'])
def add_data():
    print(request.data)
    f = request.files.get('nbt')
    u_name = request.form.get('user')
    ai_name = request.form.get('ai')
    problem = int(request.form.get('problem'))
    if not u_name or not ai_name or not problem:
        abort(500)

    score = Score(u_name=u_name, ai_name=ai_name, problem=problem)
    db.session.add(score)
    db.session.commit()

    fpath = '/data/{}.nbt'.format(score.id)
    f.save(fpath)
    cmd = "/usr/bin/node score.js ../data/problemsL/LA{0:03d}_tgt.mdl {1}".format(
        problem,
        fpath
    )

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

    memory_file = io.BytesIO()
    with zipfile.ZipFile(memory_file, 'w') as zf:
        zf.setpassword('9364648f7acd496a948fba7c76a10501')
        for row in result:
            fid = row['id']
            problem = row['problem']
            data = zipfile.ZipInfo(filename="LA{0:03d}.nbt".format(problem))
            data.compress_type = zipfile.ZIP_DEFLATED
            with open('/data/{}.nbt'.format(fid), 'rb') as f:
                zf.writestr(data, f.read())
    memory_file.seek(0)
    return send_file(memory_file, attachment_filename='submission.zip', as_attachment=True)

if __name__ == '__main__':
    app.run(
        host=os.environ.get('FLASK_HOST', '0.0.0.0'),
        port=int(os.environ.get('FLASK_PORT', 5050)),
        debug=bool(os.environ.get('FLASK_DEBUG', 1)),
        # processes=10,
    )
