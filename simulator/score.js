const fs = require('fs');
const score = require('./score_calc');

if (process.argv.length < 4) {
  console.error('too few arguments');
  process.exit(1);
}

// Usage:
//   node score.js FA001 nbt_file
//
//   LA001: target only
//   FA001: target only
//   FD001: source only
//   FR001: target and source

const problemName = process.argv[2];
const traceFile = process.argv[3];
try {
  fs.statSync(traceFile);
} catch (e) {
  console.error('トレースファイルが存在しません');
  process.exit(1);
}

(async () => {
  const result = await score.run(problemName, traceFile);
  console.log(result);
})();
