const fs = require('fs');
const path = require('path');
const puppeteer = require('puppeteer');
const sleep = msec => new Promise(resolve => setTimeout(resolve, msec));

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

const run = async (problemName, traceFile) => {
  const problemType = problemName.substr(0, 2);
  const problemNum = problemName.substr(2);

  const browser = await puppeteer.launch({
    headless: true,
    args: ['--no-sandbox', '--disable-web-security', '--disable-setuid-sandbox']
  });
  const page = await browser.newPage();

  await page.goto(`file:${path.join(__dirname, 'exec-trace-novis-full.html')}`);

  if (['LA', 'FA'].includes(problemType)) {
    const modelInput = await page.$('#tgtModelFileIn');
    const modelFile = `../data/problems/${problemName}_tgt.mdl`;
    await modelInput.uploadFile(modelFile);
    await page.click('#srcModelEmpty');
  } else if (problemType === 'FD') {
    const modelInput = await page.$('#srcModelFileIn');
    const modelFile = `../data/problems/${problemName}_src.mdl`;
    await modelInput.uploadFile(modelFile);
    await page.click('#tgtModelEmpty');
  } else {
    // FR
    const targetInput = await page.$('#tgtModelFileIn');
    const targetFile = `../data/problems/${problemName}_tgt.mdl`;
    await targetInput.uploadFile(targetFile);
    const sourceInput = await page.$('#srcModelFileIn');
    const sourceFile = `../data/problems/${problemName}_src.mdl`;
    await sourceFile.uploadFile(sourceFile);
  }

  const traceInput = await page.$('#traceFileIn');
  await traceInput.uploadFile(traceFile);

  await page.select('#stepsPerFrame', '10000000');
  await page.click('#execTrace');
  const executionStart = Date.now();

  let content = '';
  while (true) {
    content = await page.$eval('#stdout', item => item.textContent);
    if (content.startsWith('Success::')) {
      break;
    }

    if (content.startsWith('Failure::')) {
      console.error(content);
      await browser.close();
      process.exit(1);
    }

    // 15分でタイムアウトする
    if (Date.now() - executionStart > 15 * 60 * 1000) {
      await browser.close();
      process.exit(1);
    }

    await sleep(100);
  }

  const match = content.match(
    /^Time:\s+(\d+)\nCommands:\s+(\d+)\nEnergy:\s+(\d+)/m
  );
  console.log(
    JSON.stringify({ time: match[1], commands: match[2], energy: match[3] })
  );

  await browser.close();
};

run(problemName, traceFile);

exports.run = run;
