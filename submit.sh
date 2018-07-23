#!/bin/sh
mkdir -p submissions

pid="9364648f7acd496a948fba7c76a10501"
name="submission_$(date +%H%M).zip"

# upload
curl -o "submissions/${name}" http://54.244.193.90:5050/submission
aws s3 cp submissions/${name} s3://icfpc-udon-submit/submit/${name} --acl public-read 
url="https://s3-us-west-2.amazonaws.com/icfpc-udon-submit/submit/${name}"

# create shasum
shasum -a 256 submissions/${name}
sha=$(shasum -a 256 submissions/${name} | cut -d ' ' -f 1)
echo ${sha}

# shacheck
curl -L -f -o submission.zip ${url} && \
echo "${sha}  submission.zip" | shasum -c && \
unzip -P ${pid} -qt submission.zip

echo "======================"
echo ${pid}
echo ${url}
echo ${sha}
echo "======================"

aws s3api put-bucket-policy --bucket icfpc-udon-submit --policy file://public.json



