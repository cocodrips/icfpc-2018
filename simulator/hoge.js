const path = require('path');
const puppeteer = require('puppeteer');
const sleep = msec => new Promise(resolve => setTimeout(resolve, msec));

if (process.argv.length < 4) {
  console.error('too few arguments');
  process.exit(1);
}

const modelFile = process.argv[2];
const traceFile = process.argv[3];

(async () => {
  const browser = await puppeteer.launch({
    headless: true,
    args: ['--no-sandbox', '--disable-web-security']
  });
  const page = await browser.newPage();

  await page.goto(`file:${path.join(__dirname, 'exec-trace-novis.html')}`);

  const modelInput = await page.$('#tgtModelFileIn');
  const traceInput = await page.$('#traceFileIn');

  await modelInput.uploadFile(modelFile);
  await traceInput.uploadFile(traceFile);
  await page.click('#execTrace');
  const executionStart = Date.now();

  let content = '';
  while (true) {
    content = await page.$eval('#stdout', item => item.textContent);
    if (content.startsWith('Success::')) {
      break;
    }

    // 15分でタイムアウトする
    if (Date.now() - executionStart > 15 * 60 * 1000) {
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
})();
