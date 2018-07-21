mkdir -p data/
cp icfpcontest2018.github.io/assets/dfltTracesL.zip data/
cp icfpcontest2018.github.io/assets/problemsL.zip data/

mkdir -p data/dfltTracesL data/problemsL
unzip data/dfltTracesL.zip -d data/dfltTracesL
unzip data/problemsL.zip -d data/problemsL
rm data/*.zip