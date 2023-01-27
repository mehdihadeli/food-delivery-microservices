#!/bin/bash

# https://github.com/saitho/semantic-release-backmerge/issues/43
# https://github.com/saitho/semantic-release-backmerge/issues/42
# https://codewithhugo.com/fix-git-failed-to-push-updates-were-rejected/  

if [ $GITHUB_REF_NAME = 'main' ]
# backmerge main to dev
then git checkout dev
    git merge origin/main -Xours
    git push origin dev

# backmerge dev to all feature branches: list all remote feature branches that have not been merged to any branches and has remote branch(-r)
for branch in $(git branch -r --no-merged | grep "origin/feat/" | awk '{print $1}'| sed 's/^origin\///'); do
    git checkout $branch
    git merge origin/dev -Xours
    git push origin $branch
done
fi