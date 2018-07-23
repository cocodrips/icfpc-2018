#!/bin/sh
mkdir -p data/
cp icfpcontest2018.github.io/assets/dfltTracesL.zip data/
cp icfpcontest2018.github.io/assets/dfltTracesF.zip data/
cp icfpcontest2018.github.io/assets/problemsL.zip data/
cp icfpcontest2018.github.io/assets/problemsF.zip data/

mkdir -p data/dfltTraces data/problems
mkdir -p simulator/data/dfltTraces simulator/data/problems
unzip data/dfltTracesL.zip -d data/dfltTraces
unzip data/dfltTracesF.zip -d data/dfltTraces
unzip data/problemsL.zip -d data/problems
unzip data/problemsF.zip -d data/problems
unzip data/dfltTracesL.zip -d simulator/data/dfltTraces
unzip data/dfltTracesF.zip -d simulator/data/dfltTraces
unzip data/problemsL.zip -d simulator/data/problems
unzip data/problemsF.zip -d simulator/data/problems
rm data/*.zip
