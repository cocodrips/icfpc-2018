# icfpc-2018

## ローカルでスコアチェック

### セットアップ

```sh
cd simulator
npm i
```

### チェック

```sh
cd simulator
node score.js <model path> <nbt path>
```

```sh
# sample
 $ node score.js ../data/problemsL/LA001_tgt.mdl ../data/dfltTracesL/LA001.nbt
{"time":"1398","commands":"1398","energy":"335123860"}

# with error
 $ node score.js ../data/problemsL/LA001_tgt.mdl ../tools/LA000.nbt
Failure::
Error with SMove <0,0,-1> by bot 01 (invalid coordinate (0,0,0) + <0,0,-1>)

Time: 1
Commands: 1
Energy: 24020
Harmonics: High
#Full: 0
Active Bots:
    BId: 01; Pos: (0,0,0); CBIds: 19 = |{02, ...}|; Cmd: SMove <0,0,-1>

 $ echo $?
1
```

## data download

Download problem and default-trace to `data/`.

```sh
$ sh deploy_data.sh
```
