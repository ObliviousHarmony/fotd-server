#!/bin/bash
set -euo pipefail

ACTION=${1:-build}
if [[ "$ACTION" != "build" && "$ACTION" != "test" ]]; then
	echo "❌ Error: First argument must be 'build' or 'test'."
	echo "Usage: $1 [build|test]"
	exit 1
fi

BUILD_CONFIG=${FOMSERVER_BUILD_CONFIG:-Debug}

cmake -S /workspace -B /workspace/out -DCMAKE_BUILD_TYPE="$BUILD_CONFIG"
cmake --build /workspace/out --config "$BUILD_CONFIG" -j"$(nproc)"

if [[ "$ACTION" == "test" ]]; then
	ctest --test-dir /workspace/out --output-on-failure -C "$BUILD_CONFIG"
fi
