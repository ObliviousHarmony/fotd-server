#!/bin/bash
set -euo pipefail

FOMSERVER_BUILD_CONFIG=${1:-}
if [[ "$FOMSERVER_BUILD_CONFIG" != "Debug" && "$FOMSERVER_BUILD_CONFIG" != "Release" ]]; then
	echo "❌ Error: First argument must be either 'Debug' or 'Release'."
	echo "Usage: $0 <Debug|Release> [extra args...]"
	exit 1
fi
shift # drop the config argument, leaving the rest in "$@"

# Sync source into container-local workspace.
rsync -a --delete \
	--exclude='.vs' \
	--exclude='.vscode' \
	--exclude='.git' \
	--exclude='/out' \
	--exclude='/*-server/bin' \
	--exclude='/*-server/obj' \
	--exclude='/server-tests/bin' \
	--exclude='/server-tests/obj' \
	/src/ /workspace/

# Configure and build, passing through any extra args to CMake.
mkdir -p /out/cpp/build/$FOMSERVER_BUILD_CONFIG
cd /workspace
cmake -S /workspace -B /out/cpp/build/$FOMSERVER_BUILD_CONFIG -DCMAKE_BUILD_TYPE=$FOMSERVER_BUILD_CONFIG "$@"
cmake --build /out/cpp/build/$FOMSERVER_BUILD_CONFIG --config $FOMSERVER_BUILD_CONFIG -j$(nproc) "$@"
