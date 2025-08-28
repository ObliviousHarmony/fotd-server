#!/bin/bash
set -euo pipefail

FOMSERVER_BUILD_CONFIG=${1:-}
if [[ "$FOMSERVER_BUILD_CONFIG" != "Debug" && "$FOMSERVER_BUILD_CONFIG" != "Release" ]]; then
  echo "❌ Error: First argument must be either 'Debug' or 'Release'."
  echo "Usage: $0 <Debug|Release> [extra args...]"
  exit 1
fi
shift # drop the config argument, leaving the rest in "$@"

# Sync source into container-local workspace
rsync -a --delete \
  --exclude='.vs' \
  --exclude='.vscode' \
  --exclude='.git' \
  --exclude='/out' \
  --exclude='/*-server/bin' \
  --exclude='/*-server/obj' \
  /src/ /workspace/

# Build projects into /out/dotnet/<project>/<config>, forwarding extra args
mkdir -p /out/dotnet
cd /workspace
dotnet build master-server -c $FOMSERVER_BUILD_CONFIG -o /out/dotnet/master-server/$FOMSERVER_BUILD_CONFIG "$@"
dotnet build world-server  -c $FOMSERVER_BUILD_CONFIG -o /out/dotnet/world-server/$FOMSERVER_BUILD_CONFIG "$@"
