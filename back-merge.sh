#!/bin/bash

# https://github.com/saitho/semantic-release-backmerge/issues/43
# https://github.com/saitho/semantic-release-backmerge/issues/42
# https://codewithhugo.com/fix-git-failed-to-push-updates-were-rejected/
if [ $GITHUB_REF_NAME = 'main' ]
# backmerge main to develop
then git checkout develop
    git merge origin/main -Xours --no-commit
    git commit -m "chore(release): 🔧 preparations for next release with a backmerge [skip ci]" --author="github-actions[bot] <github-actions[bot]@users.noreply.github.com>"
    git push origin develop

# backmerge develop to all feature branches: list all remote feature branches that have not been merged to any branches and has remote branch(-r)
for branch in $(git branch -r --no-merged | grep "origin/feat/" | awk '{print $1}'| sed 's/^origin\///'); do
    git checkout $branch
    git merge origin/develop -Xours --no-commit
    git commit -m "chore(release): 🔧 preparations for next release with a backmerge [skip ci]" --author="github-actions[bot] <github-actions[bot]@users.noreply.github.com>"
    git push origin $branch
done
fi
