BUILD_CONFIG := "debug"
CPP_CACHE_IMAGE := "fom/build-cpp"
DOTNET_CACHE_IMAGE := "fom/build-dotnet"
BUILD_VOLUME := "fom-server-build"
NUGET_CACHE_VOLUME := "fom-server-nuget-cache"

[group("format")]
[parallel]
format: _format-cpp _format-dotnet

[group("format")]
_format-cpp:
  clang-format -i $(find ./fom-network -name '*.h' -o -name '*.cpp')

[group("format")]
_format-dotnet:
  dotnet format ManagedOnly.slnf

[group("format")]
format-check: format-check-cpp format-check-dotnet

[group("format")]
format-check-cpp:
  clang-format --dry-run --Werror $(find ./fom-network -name '*.h' -o -name '*.cpp')

[group("format")]
format-check-dotnet:
  dotnet format ManagedOnly.slnf --verify-no-changes

[group("docker")]
[doc('Creates the Docker images used for building the project.')]
[parallel]
docker-build: _docker-build-cpp _docker-build-dotnet

[group("docker")]
_docker-build-cpp:
  docker build \
    --platform=linux/amd64 \
    -f docker/build/cpp.Dockerfile \
    -t {{CPP_CACHE_IMAGE}} \
    docker/build

[group("docker")]
_docker-build-dotnet:
  docker build \
    --platform=linux/amd64 \
    -f docker/build/dotnet.Dockerfile \
    -t {{DOTNET_CACHE_IMAGE}} \
    docker/build

[group("build")]
build:
  docker run --rm \
    --platform=linux/amd64 \
    --mount type=bind,src="{{justfile_directory()}}",dst="/workspace" \
    --mount type=volume,src="{{BUILD_VOLUME}}",dst="/workspace/out",volume-nocopy \
    {{CPP_CACHE_IMAGE}} build
  docker run --rm \
    --platform=linux/amd64 \
    --mount type=volume,src="{{NUGET_CACHE_VOLUME}}",dst="/root/.nuget/packages" \
    --mount type=bind,src="{{justfile_directory()}}",dst="/workspace" \
    --mount type=volume,src="{{BUILD_VOLUME}}",dst="/workspace/out",volume-nocopy \
    {{DOTNET_CACHE_IMAGE}} build

[group("test")]
test: test-cpp test-dotnet

[group("test")]
test-cpp:
  docker run --rm \
    --platform=linux/amd64 \
    --mount type=bind,src="{{justfile_directory()}}",dst="/workspace" \
    --mount type=volume,src="{{BUILD_VOLUME}}",dst="/workspace/out",volume-nocopy \
    {{CPP_CACHE_IMAGE}} test

[group("test")]
test-dotnet:
  docker run --rm \
    --platform=linux/amd64 \
    --mount type=volume,src="{{NUGET_CACHE_VOLUME}}",dst="/root/.nuget/packages" \
    --mount type=bind,src="{{justfile_directory()}}",dst="/workspace" \
    --mount type=volume,src="{{BUILD_VOLUME}}",dst="/workspace/out",volume-nocopy \
    {{DOTNET_CACHE_IMAGE}} test

[group("server")]
ms-up:
  docker-compose -f docker/server/docker-compose.yml up -d master-server

[group("server")]
ms-down:
  docker-compose -f docker/server/docker-compose.yml down master-server

[group("server")]
ms-recreate:
  docker-compose -f docker/server/docker-compose.yml up -d --force-recreate master-server

[group("server")]
ms-destroy:
  docker-compose -f docker/server/docker-compose.yml down master-server

[group("server")]
db-up:
  docker-compose -f docker/server/docker-compose.yml up -d db

[group("server")]
db-down:
  docker-compose -f docker/server/docker-compose.yml down db
