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
```

## data download

Download problem and default-trace to `data/`.

```sh
$ sh deploy_data.sh
```
