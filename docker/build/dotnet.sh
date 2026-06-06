#!/bin/bash
set -euo pipefail

ACTION=${1:-build}
if [[ "$ACTION" != "build" && "$ACTION" != "publish" && "$ACTION" != "test" ]]; then
  echo "❌ Error: First argument must be 'build', 'publish', or 'test'."
  echo "Usage: $0 [build|publish|test]"
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
  # Deployment artifact. out/ is the fom-server-build volume, so this is what
  # docker-compose runs from (it mounts the same volume at /app and runs
  # /app/publish/{master,world}).
  rm -rf /workspace/out/publish/master /workspace/out/publish/world
  dotnet publish /workspace/master-server/MasterServer.csproj \
    -c "$BUILD_CONFIG" \
    --no-restore --no-build \
    --output /workspace/out/publish/master
  dotnet publish /workspace/world-server/WorldServer.csproj \
    -c "$BUILD_CONFIG" \
    --no-restore --no-build \
    --output /workspace/out/publish/world
fi

if [[ "$ACTION" == "publish" ]]; then
  # Host-visible copy. build/ lives on the repo bind mount (outside the out/
  # volume), so this output lands on the host for the user to grab.
  rm -rf /workspace/build/linux/master /workspace/build/linux/world
  dotnet publish /workspace/master-server/MasterServer.csproj \
    -c "$BUILD_CONFIG" \
    --no-restore --no-build \
    --output /workspace/build/linux/master
  dotnet publish /workspace/world-server/WorldServer.csproj \
    -c "$BUILD_CONFIG" \
    --no-restore --no-build \
    --output /workspace/build/linux/world
fi

if [[ "$ACTION" == "test" ]]; then
  dotnet test /workspace/ManagedOnly.slnf \
    -c "$BUILD_CONFIG" \
    --no-build \
    --logger "console;verbosity=normal"
fi
