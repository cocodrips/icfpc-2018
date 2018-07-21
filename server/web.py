import os
from flask import Flask, request, abort, send_file
from flask_sqlalchemy import SQLAlchemy

app = Flask(__name__)
app.config[
    'SQLALCHEMY_DATABASE_URI'] = 'postgresql://root:root@localhost:15432/icfpc'
app.config['SQLALCHEMY_TRACK_MODIFICATIONS'] = True
db = SQLAlchemy(app)


class Score(db.Model):
    __tablename__ = 'score'
    id = db.Column(db.Integer, primary_key=True, autoincrement=True)
    u_name = db.Column(db.String(80))
    ai_name = db.Column(db.String(80))
    score = db.Column(db.Integer, default=-10)
    problem = db.Column(db.Integer)
    create_at = db.Column(db.DateTime, default=db.func.now())


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


@app.route("/add", methods=['POST'])
def add_data():
    print(request.data)
    f = request.files.get('nbt')
    u_name = request.form.get('user')
    ai_name = request.form.get('ai')
    problem = request.form.get('problem')
    if not u_name or not ai_name or not problem:
        abort(500)

    score = Score(u_name=u_name, ai_name=ai_name, problem=problem)
    db.session.add(score)
    db.session.commit()

    f.save('/data/{}.nbt'.format(score.id))
    return "OK"


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


if __name__ == '__main__':
    app.run(
        host=os.environ.get('FLASK_HOST', '0.0.0.0'),
        port=int(os.environ.get('FLASK_PORT', 5050)),
        debug=bool(os.environ.get('FLASK_DEBUG', 1)),
        processes=10,
    )
