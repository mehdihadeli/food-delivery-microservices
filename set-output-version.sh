#!/bin/bash

#https://stackoverflow.com/questions/72343466/while-using-github-actions-im-facing-permission-denied-error
#https://stackoverflow.com/questions/60566805/getting-next-tag-version-using-semantic-releases
#https://docs.github.com/en/actions/using-workflows/workflow-commands-for-github-actions
#https://unix.stackexchange.com/questions/31414/how-can-i-pass-a-command-line-argument-into-a-shell-script
#https://medium.com/@gpanga/automating-releases-of-net-sdks-using-semantic-release-e3df46013876
#https://github.com/Gabrielpanga/dotnet-nuget-example/blob/main/updateVersion.sh

echo "semantic_nextRelease_version=$1" >> $GITHUB_OUTPUT
echo "semantic_nextRelease_channel=$2" >> $GITHUB_OUTPUT
echo "semantic_nextRelease_gitHead=$3" >> $GITHUB_OUTPUT
echo "semantic_nextRelease_gitTag=$4" >> $GITHUB_OUTPUT
echo "semantic_lastRelease_version=$5" >> $GITHUB_OUTPUT
echo "semantic_lastRelease_channel=$6" >> $GITHUB_OUTPUT
echo "semantic_lastRelease_gitHead=$7" >> $GITHUB_OUTPUT
echo "semantic_lastRelease_gitTag=$8" >> $GITHUB_OUTPUT
