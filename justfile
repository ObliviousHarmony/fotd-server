BUILD_CONFIG := "debug"
CPP_CACHE_IMAGE := "fom/build-cpp"
DOTNET_CACHE_IMAGE := "fom/build-dotnet"
BUILD_VOLUME := "fom-server-build"
NUGET_CACHE_VOLUME := "fom-server-nuget-cache"

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

test: test-cpp test-dotnet

test-cpp:
	docker run --rm \
		--platform=linux/amd64 \
		--mount type=bind,src="{{justfile_directory()}}",dst="/workspace" \
		--mount type=volume,src="{{BUILD_VOLUME}}",dst="/workspace/out",volume-nocopy \
		{{CPP_CACHE_IMAGE}} test

test-dotnet:
	docker run --rm \
		--platform=linux/amd64 \
		--mount type=volume,src="{{NUGET_CACHE_VOLUME}}",dst="/root/.nuget/packages" \
		--mount type=bind,src="{{justfile_directory()}}",dst="/workspace" \
		--mount type=volume,src="{{BUILD_VOLUME}}",dst="/workspace/out",volume-nocopy \
		{{DOTNET_CACHE_IMAGE}} test