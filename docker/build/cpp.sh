#!/bin/bash
set -euo pipefail

ACTION=${1:-build}
if [[ "$ACTION" != "build" && "$ACTION" != "test" ]]; then
  echo "❌ Error: First argument must be 'build' or 'test'."
  echo "Usage: $0 [build|test]"
  exit 1
fi

BUILD_PRESET=${FOMSERVER_BUILD_PRESET:-Debug}

cmake --preset "$BUILD_PRESET"
cmake --build --preset "$BUILD_PRESET"

if [[ "$ACTION" == "test" ]]; then
  ctest --test-dir "/workspace/out/build/$BUILD_PRESET" --output-on-failure
fi
