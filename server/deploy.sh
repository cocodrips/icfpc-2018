ln -s ../simulator/css/style.css  static/css/style-simulator.css
mkdir -p static/images
ln -s ../simulator/images/demo.png  static/images
ln -s ../simulator/images/buildatron4000.png  static/images
ls -1 ../simulator/js | xargs -I{} ln -s {} static/js
