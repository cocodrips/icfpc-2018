const puppeteer = require('puppeteer');
const fs = require('fs');
const sleep = msec => new Promise(resolve => setTimeout(resolve, msec));
const path = require('path');

function checkFile(filepath) {
  try {
    fs.statSync(filepath);
    return true;
  } catch (e) {
    return false;
  }
}

exports.run = async (problemName, traceFile) => {
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
    const modelFile = `./data/problems/${problemName}_tgt.mdl`;
    fs.statSync(modelFile);

    await modelInput.uploadFile(modelFile);
    await page.click('#srcModelEmpty');
  } else if (problemType === 'FD') {
    const modelInput = await page.$('#srcModelFileIn');
    const modelFile = `./data/problems/${problemName}_src.mdl`;
    fs.statSync(modelFile);

    await modelInput.uploadFile(modelFile);
    await page.click('#tgtModelEmpty');
  } else {
    // FR
    const targetInput = await page.$('#tgtModelFileIn');
    const targetFile = `./data/problems/${problemName}_tgt.mdl`;
    fs.statSync(targetFile);
    await targetInput.uploadFile(targetFile);

    const sourceInput = await page.$('#srcModelFileIn');
    const sourceFile = `./data/problems/${problemName}_src.mdl`;
    fs.statSync(sourceFile);
    await sourceInput.uploadFile(sourceFile);
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
      await browser.close();
      throw new Error(content);
    }

    // 15分でタイムアウトする
    if (Date.now() - executionStart > 10 * 60 * 1000) {
      await browser.close();
      throw new Error('timeout');
    }

    await sleep(100);
  }

  const match = content.match(
    /^Time:\s+(\d+)\nCommands:\s+(\d+)\nEnergy:\s+(\d+)/m
  );

  await browser.close();

  return JSON.stringify({
    time: match[1],
    commands: match[2],
    energy: match[3]
  });
};
