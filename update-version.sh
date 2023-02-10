#!/bin/bash

# update package version and version for in the csproj file to new version and commit it again with using https://github.com/semantic-release/git plugin
# https://unix.stackexchange.com/questions/50313/how-do-i-perform-an-action-on-all-files-with-a-specific-extension-in-subfolders
new_version=$1
for file in *.Packages.props; do
  current_version=$(grep -oP '(?<=<Version>)[^<]+' "$file")
  if [ "$current_version" != "$new_version" ]; then
    sed -i "s#<ApplicationVersion>.*#<ApplicationVersion>$new_version</ApplicationVersion>#" "$file"
    sed -i "s#<InformationalVersion>.*#<InformationalVersion>$new_version</InformationalVersion>#" "$file"
    sed -i "s#<Version>.*#<Version>$new_version</Version>#" "$file"
  else
    echo application verion not changed
  fi
done
