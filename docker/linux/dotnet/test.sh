#!/bin/bash
set -euo pipefail

FOMSERVER_BUILD_CONFIG=${1:-}
if [[ "$FOMSERVER_BUILD_CONFIG" != "Debug" && "$FOMSERVER_BUILD_CONFIG" != "Release" ]]; then
	echo "❌ Error: First argument must be either 'Debug' or 'Release'."
	echo "Usage: $0 <Debug|Release> [extra args...]"
	exit 1
fi
shift # drop the config argument, leaving the rest in "$@"

# Run tests, passing through any arguments.
# Targeting the DLL avoids CLI warnings.
dotnet test "/out/dotnet/ServerTests/$FOMSERVER_BUILD_CONFIG/net8.0/ServerTests.dll" "$@"
