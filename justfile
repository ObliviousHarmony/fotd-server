set windows-shell := ["powershell.exe", "-NoLogo", "-Command"]

BUILD_CONFIG := "debug"
CPP_CACHE_IMAGE := "fom/build-cpp"
DOTNET_CACHE_IMAGE := "fom/build-dotnet"
BUILD_VOLUME := "fom-server-build"
NUGET_CACHE_VOLUME := "fom-server-nuget-cache"

# Conditionally mount either a named volume or a host directory for build outputs and NuGet cache.
BUILD_VOLUME_BIND := env("BUILD_VOLUME_BIND", "")
BUILD_VOLUME_MOUNT := if BUILD_VOLUME_BIND == "" {
  '--mount type=volume,src="' + BUILD_VOLUME + '",dst="/workspace/out",volume-nocopy'
} else {
  '--mount type=bind,src="' + BUILD_VOLUME_BIND + '",dst="/workspace/out"'
}
NUGET_CACHE_BIND := env("NUGET_CACHE_BIND", "")
NUGET_CACHE_MOUNT := if NUGET_CACHE_BIND == "" {
  '--mount type=volume,src="' + NUGET_CACHE_VOLUME + '",dst="/root/.nuget/packages",volume-nocopy'
} else {
  '--mount type=bind,src="' + NUGET_CACHE_BIND + '",dst="/root/.nuget/packages"'
}

[group("format")]
[unix]
format-check-cpp:
  clang-format --dry-run --Werror $(find fom-network -name "*.cpp" -o -name "*.h")

[group("format")]
[windows]
format-check-cpp:
    Get-ChildItem -Path "fom-network" -Recurse -Include *.cpp,*.h -File | ForEach-Object { clang-format --dry-run --Werror $_.FullName }

[group("format")]
format-check-dotnet:
  dotnet format ManagedOnly.slnf --verify-no-changes

[group("format")]
[parallel]
format: format-cpp format-dotnet

[group("format")]
[unix]
format-cpp:
  clang-format -i $(find fom-network -name "*.cpp" -o -name "*.h")

[group("format")]
[windows]
format-cpp:
    Get-ChildItem -Path "fom-network" -Recurse -Include *.cpp,*.h -File | ForEach-Object { clang-format -i $_.FullName }

[group("format")]
format-dotnet:
  dotnet format ManagedOnly.slnf

[group("docker")]
[doc('Creates the Docker images used for building the project.')]
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
    {{BUILD_VOLUME_MOUNT}} \
    {{CPP_CACHE_IMAGE}} build
  docker run --rm \
    --platform=linux/amd64 \
    {{NUGET_CACHE_MOUNT}} \
    --mount type=bind,src="{{justfile_directory()}}",dst="/workspace" \
    {{BUILD_VOLUME_MOUNT}} \
    {{DOTNET_CACHE_IMAGE}} build

[group("test")]
test: test-cpp test-dotnet

[group("test")]
test-cpp:
  docker run --rm \
    --platform=linux/amd64 \
    --mount type=bind,src="{{justfile_directory()}}",dst="/workspace" \
    {{BUILD_VOLUME_MOUNT}} \
    {{CPP_CACHE_IMAGE}} test

[group("test")]
test-dotnet:
  docker run --rm \
    --platform=linux/amd64 \
    {{NUGET_CACHE_MOUNT}} \
    --mount type=bind,src="{{justfile_directory()}}",dst="/workspace" \
    {{BUILD_VOLUME_MOUNT}} \
    {{DOTNET_CACHE_IMAGE}} test

[group("server")]
db-up:
  docker-compose -f docker/server/docker-compose.yml up -d db

[group("server")]
db-down:
  docker-compose -f docker/server/docker-compose.yml down db

[group("server")]
ms-up:
  docker-compose -f docker/server/docker-compose.yml up -d master-server

[group("server")]
ms-down:
  docker-compose -f docker/server/docker-compose.yml down master-server

[group("server")]
ws-up:
  docker-compose -f docker/server/docker-compose.yml up -d world-server

[group("server")]
ws-down:
  docker-compose -f docker/server/docker-compose.yml down world-server

[group("server")]
server-up: db-up ms-up ws-up

[group("server")]
server-down: db-down ms-down ws-down
