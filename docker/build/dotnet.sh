#!/bin/bash
set -euo pipefail

ACTION=${1:-build}
if [[ "$ACTION" != "build" && "$ACTION" != "test" ]]; then
	echo "❌ Error: First argument must be 'build' or 'test'."
	echo "Usage: $1 [build|test]"
	exit 1
fi

BUILD_CONFIG=${FOMSERVER_BUILD_CONFIG:-Debug}

NATIVE_LIB="/workspace/out/build/$BUILD_CONFIG/fom-network/libFOMNetwork.so"
if [[ ! -f "$NATIVE_LIB" ]]; then
	echo "❌ Error: Native code must be built first: $NATIVE_LIB"
	exit 1
fi

dotnet build /workspace/ManagedOnly.slnf -c "$BUILD_CONFIG"

if [[ "$ACTION" == "build" ]]; then
	dotnet publish /workspace/master-server/MasterServer.csproj \
		-c "$BUILD_CONFIG" \
		--no-restore --no-build \
		--output /workspace/out/publish/master
	dotnet publish /workspace/world-server/WorldServer.csproj \
		-c "$BUILD_CONFIG" \
		--no-restore --no-build \
		--output /workspace/out/publish/world
fi

if [[ "$ACTION" == "test" ]]; then
	dotnet test /workspace/ManagedOnly.slnf \
		-c "$BUILD_CONFIG" \
		--no-build \
		--logger "console;verbosity=normal"
fi
