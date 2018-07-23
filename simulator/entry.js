const aws = require('aws-sdk');
const fs = require('fs');
const request = require('request-promise-native');
const s3 = new aws.S3();
const score = require('./score_calc');

const problemName = process.argv[2];
const s3Params = {
  Bucket: 'icfpc-udon-2018',
  Key: process.argv[3]
};

async function sendError(e) {
  console.log('sendError', e);
  const id = process.argv[3]
    .split('/')[1]
    .split('_')[1]
    .split('.')[0];
  await request.post({
    uri: `http://54.244.193.90:5050/update/${id}`,
    headers: {
      'Content-Type': 'application/json'
    },
    json: {
      time: null,
      commands: null,
      energy: null,
      message: e.toString()
    }
  });
  console.log(e.toString());
}

(async () => {
  const data = await s3
    .getObject(s3Params)
    .promise()
    .catch(async e => {
      await sendError(e);
      process.exit(1);
    });
  fs.writeFileSync('trace.nbt', data.Body, 'binary');
  const result = await score.run(problemName, 'trace.nbt').catch(async e => {
    await sendError(e);
    process.exit(1);
  });
  // key: nbt/LA001_1.nbt
  const id = process.argv[3]
    .split('/')[1]
    .split('_')[1]
    .split('.')[0];
  await request.post({
    uri: `http://54.244.193.90:5050/update/${id}`,
    headers: {
      'Content-Type': 'application/json'
    },
    json: JSON.parse(result)
  });
})();
