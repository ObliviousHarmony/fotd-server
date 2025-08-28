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

# Build, passing through any arguments.
mkdir -p /out/dotnet
cd /workspace
# We're targeting the tests project because it will transitively build the others.
# This avoids the stdout pollution from 4 separate restore/build loops.
dotnet build server-tests -c $FOMSERVER_BUILD_CONFIG "$@"
