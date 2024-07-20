#!/bin/bash

container_name=$1
image_name=$2
http_port=$3
https_port=$4
env_path=$5
source_path=$6

#https://docs.docker.com/engine/reference/commandline/run/
#https://www.aaron-powell.com/posts/2019-04-04-debugging-dotnet-in-docker-with-vscode/
#https://www.powercms.in/article/how-automatically-delete-docker-container-after-running-it
#https://www.richard-banks.org/2018/07/debugging-core-in-docker.html
#https://docs.docker.com/engine/reference/commandline/run/#mount
#https://stackoverflow.com/questions/52070171/whats-the-default-user-for-docker-exec
#https://code.visualstudio.com/docs/containers/troubleshooting#_running-as-a-nonroot-user
# here if we don't use detached mode this task block process for inreactive mode and prevent to use launch debuger in laucnch.json
#--rm doesn't work in detached mode
#mappings increase the size of docker image so we use it just in debug mode, in prod its better dockerfile restore just nugets it needs for decresing image size
#because we use `base` image directly for running app, and we don't have any source code and nuggets and entrypoint (so our container not be launch) in base layer we should map source code and vsdbg as a volume or using in launch time in launch.json on base layer. In launch.json app will run with `pipeTransport` and type `coreclr` and after connecting to base layer container with running vsdb on the container and then coreclr will launch specified `program` with `dotnet run` on the container and pass `args` to `dotnet run` as launch program (nugget path, ... as --additionalProbingPath because our dll is in debug build and need to resolve all nugget dependecies that doesn't exist in this build).

if [ -z "$(docker ps -q -f name=${container_name})" ]; then
   docker run --rm -d -it --publish $http_port:80 --publish $https_port:443 --publish-all --name $container_name --env-file $env_path --network=food-delivery --mount type=bind,src=$source_path,dst=/app --mount type=bind,src=${HOME}/vsdbg,dst=/vsdbg --mount type=bind,source=${HOME}/.nuget/packages,destination=/root/.nuget/packages,readonly --mount type=bind,source=${HOME}/.nuget/packages,destination=/home/appuser/.nuget/packages,readonly --mount type=bind,src=${HOME}/.aspnet/https,dst=/https,readonly $image_name
fi
