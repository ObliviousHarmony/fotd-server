BUILD_CONFIG := "debug"
CPP_CACHE_IMAGE := "fom/build-cpp"
DOTNET_CACHE_IMAGE := "fom/build-dotnet"
BUILD_VOLUME := "fom-server-build"
NUGET_CACHE_VOLUME := "fom-server-nuget-cache"

[group("docker")]
[doc('Creates the Docker images used for building the project.')]
[parallel]
docker-build: _docker-build-cpp _docker-build-dotnet

_docker-build-cpp:
	docker build \
		--platform=linux/amd64 \
		-f docker/build/cpp.Dockerfile \
		-t {{CPP_CACHE_IMAGE}} \
		docker/build

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
