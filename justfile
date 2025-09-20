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
		-v .:/workspace:ro \
		-v {{BUILD_VOLUME}}:/workspace/out \
		{{CPP_CACHE_IMAGE}} build
	docker run --rm \
		--platform=linux/amd64 \
		-v {{NUGET_CACHE_VOLUME}}:/root/.nuget/packages \
		-v .:/workspace:ro \
		-v {{BUILD_VOLUME}}:/workspace/out \
		{{DOTNET_CACHE_IMAGE}} build

test: test-cpp test-dotnet

test-cpp:
	docker run --rm \
		--platform=linux/amd64 \
		-v .:/workspace:ro \
		-v {{BUILD_VOLUME}}:/workspace/out \
		{{CPP_CACHE_IMAGE}} test

test-dotnet:
	docker run --rm \
		--platform=linux/amd64 \
		-v {{NUGET_CACHE_VOLUME}}:/root/.nuget/packages \
		-v .:/workspace:ro \
		-v {{BUILD_VOLUME}}:/workspace/out \
		{{DOTNET_CACHE_IMAGE}} test