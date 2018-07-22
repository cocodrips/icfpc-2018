mkdir -p data/
cp icfpcontest2018.github.io/assets/dfltTracesL.zip data/
cp icfpcontest2018.github.io/assets/dfltTracesF.zip data/
cp icfpcontest2018.github.io/assets/problemsL.zip data/
cp icfpcontest2018.github.io/assets/problemsF.zip data/

mkdir -p data/dfltTracesL data/dfltTracesF data/problemsL data/dfltTracesF
unzip data/dfltTracesL.zip -d data/dfltTracesL
unzip data/dfltTracesL.zip -d data/dfltTracesF
unzip data/problemsL.zip -d data/problemsL
unzip data/problemsL.zip -d data/problemsF
rm data/*.zip