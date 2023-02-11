# https://www.gitpod.io/docs/configure/workspaces/workspace-image
# https://hub.docker.com/u/gitpod
# https://hub.docker.com/r/gitpod/workspace-dotnet
# https://www.gitpod.io/docs/configure/workspaces/workspace-image#use-a-custom-dockerfile
# https://docs.docker.com/engine/reference/builder/
# this is a docker file and we can use any commands that allowed in a docker file
FROM gitpod/workspace-dotnet:latest

# Install custom tools, runtime, etc.
RUN brew install fzf
RUN sudo apt-get install tree
