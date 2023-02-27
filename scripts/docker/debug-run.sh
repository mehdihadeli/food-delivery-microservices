#!/bin/bash

container_name=$1
image_name=$2
http_port=$3
https_port=$4
env_path=$5

#https://docs.docker.com/engine/reference/commandline/run/
#https://www.aaron-powell.com/posts/2019-04-04-debugging-dotnet-in-docker-with-vscode/
#https://www.powercms.in/article/how-automatically-delete-docker-container-after-running-it
#https://www.richard-banks.org/2018/07/debugging-core-in-docker.html
#https://docs.docker.com/engine/reference/commandline/run/#mount
#https://oprea.rocks/blog/how-to-properly-override-the-entrypoint-using-docker-run
#https://codewithyury.com/docker-run-vs-cmd-vs-entrypoint/
#https://devopscube.com/keep-docker-container-running/
#https://docs.docker.com/engine/reference/run/#entrypoint-default-command-to-execute-at-runtime
#https://github.com/microsoft/vscode-docker/issues/3831#issuecomment-1433567030
#https://stackoverflow.com/questions/52070171/whats-the-default-user-for-docker-exec
#https://code.visualstudio.com/docs/containers/troubleshooting#_running-as-a-nonroot-user
# here if we don't use detached mode this task block process for inreactive mode and prevent to use launch debuger in laucnch.json
#--rm doesn't work in detached mode
#here we use full build image for debugging but we change dcoker file `entrypoint` durring `docker run` for for preventing launch app in docker container but with using new entrypoint our stage will run on app working directory and then in our launch.json we launch our app inner container with connecting to `container_name` container with `pipeTransport` and `vsdbg` (internaly use dcoker exec and run vsdb on container) and then with using coreclr type and prgram to run, it will run this program with `dotnet run` by coreclr and passed `args` 
#mappings increase the size of docker image so we use it just in debug mode, in prod its better dockerfile restore just nugets it needs for decresing image size

if [ -z "$(docker ps -q -f name=${container_name})" ]; then
    docker run -it --rm -d --publish $http_port:80 --publish $https_port:443 --publish-all --name $container_name --entrypoint 'bash' --env-file $env_path --network=ecommerce --mount type=bind,src=${HOME}/vsdbg,dst=/vsdbg --mount type=bind,source=${HOME}/.nuget/packages,destination=/root/.nuget/packages,readonly --mount type=bind,source=${HOME}/.nuget/packages,destination=/home/appuser/.nuget/packages,readonly --mount type=bind,src=${HOME}/.aspnet/https,dst=/https,readonly $image_name
fi
