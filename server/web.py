import os
from flask import Flask, request

app = Flask(__name__)


@app.route("/")
def hello():
    return "Hello World!"

@app.route("/add", methods=['POST'])
def add_data():
    print (request.data)
    f = request.files.get('nbt')
    return "OK"
    


if __name__ == '__main__':
    app.run(
        host=os.environ.get('FLASK_HOST', '0.0.0.0'),
        port=int(os.environ.get('FLASK_PORT', 5000)),
        debug=bool(os.environ.get('FLASK_DEBUG', 1))
    )
