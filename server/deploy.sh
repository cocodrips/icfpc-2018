ln -sf $(pwd)/../simulator/css/style.css  static/css/style-simulator.css
mkdir -p static/images
ln -sf $(pwd)/../simulator/images/demo.png  static/images
ln -sf $(pwd)/../simulator/images/buildatron4000.png  static/images
ls -1 ../simulator/js | xargs -I{} ln -sf $(pwd)/{} static/js
ln -sf /data static/result
ln -sf $(pwd)/../data static/data