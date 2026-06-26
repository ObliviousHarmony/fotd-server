set windows-shell := ["powershell.exe", "-NoLogo", "-Command"]

CPP_CACHE_IMAGE := "fom/build-cpp"
DOTNET_CACHE_IMAGE := "fom/build-dotnet"
BUILD_VOLUME := "fom-server-build"
NUGET_CACHE_VOLUME := "fom-server-nuget-cache"

# Conditionally mount either a named volume or a host directory for build outputs and NuGet cache.
BUILD_VOLUME_BIND := env("BUILD_VOLUME_BIND", "")
BUILD_VOLUME_MOUNT := if BUILD_VOLUME_BIND == "" {
  '--mount type=volume,src="' + BUILD_VOLUME + '",dst="/workspace/out",volume-nocopy'
} else {
  '--mount type=bind,src="' + BUILD_VOLUME_BIND + '",dst="/workspace/out"'
}
NUGET_CACHE_BIND := env("NUGET_CACHE_BIND", "")
NUGET_CACHE_MOUNT := if NUGET_CACHE_BIND == "" {
  '--mount type=volume,src="' + NUGET_CACHE_VOLUME + '",dst="/root/.nuget/packages",volume-nocopy'
} else {
  '--mount type=bind,src="' + NUGET_CACHE_BIND + '",dst="/root/.nuget/packages"'
}

GHIDRA_PROJECT := "FOTD"
GHIDRA_DEFAULT := if os_family() == "windows" { 'C:\Program Files (x86)\Ghidra' } else { "/opt/ghidra" }
GHIDRA_HOME := env("GHIDRA_INSTALL_DIR", GHIDRA_DEFAULT)
GAME_DIR := env("FOTD_GAME_DIR", parent_directory(justfile_directory()) / "client")

# Optional per-machine recipes (e.g. a deploy script). Copy local.just.example to
# local.just (gitignored) to add your own. Absent on a machine, nothing breaks.
import? 'local.just'

[group("format")]
[unix]
format-check-cpp:
  clang-format --dry-run --Werror $(find fom-network -name "*.cpp" -o -name "*.h")

[group("format")]
[windows]
format-check-cpp:
    Get-ChildItem -Path "fom-network" -Recurse -Include *.cpp,*.h -File | ForEach-Object { clang-format --dry-run --Werror $_.FullName }

[group("format")]
format-check-dotnet:
  dotnet format ManagedOnly.slnf --verify-no-changes

[group("format")]
[parallel]
format: format-cpp format-dotnet

[group("format")]
[unix]
format-cpp:
  clang-format -i $(find fom-network -name "*.cpp" -o -name "*.h")

[group("format")]
[windows]
format-cpp:
    Get-ChildItem -Path "fom-network" -Recurse -Include *.cpp,*.h -File | ForEach-Object { clang-format -i $_.FullName }

[group("format")]
format-dotnet:
  dotnet format ManagedOnly.slnf

[group("docker")]
docker-images: _docker-images-cpp _docker-images-dotnet

[group("docker")]
_docker-images-cpp:
  docker build \
    --platform=linux/amd64 \
    -f docker/build/cpp.Dockerfile \
    -t {{CPP_CACHE_IMAGE}} \
    docker/build

[group("docker")]
_docker-images-dotnet:
  docker build \
    --platform=linux/amd64 \
    -f docker/build/dotnet.Dockerfile \
    -t {{DOTNET_CACHE_IMAGE}} \
    docker/build

[group("build")]
[doc('Builds C++/C# in Docker and publishes to out/publish/{master,world} in the build volume (run by `just server-up`). Pass a config (default Release).')]
docker-build config="Release": docker-images
  docker run --rm \
    --platform=linux/amd64 \
    -e FOMSERVER_BUILD_PRESET={{config}} \
    --mount type=bind,src="{{justfile_directory()}}",dst="/workspace" \
    {{BUILD_VOLUME_MOUNT}} \
    {{CPP_CACHE_IMAGE}} build
  docker run --rm \
    --platform=linux/amd64 \
    -e FOMSERVER_BUILD_CONFIG={{config}} \
    {{NUGET_CACHE_MOUNT}} \
    --mount type=bind,src="{{justfile_directory()}}",dst="/workspace" \
    {{BUILD_VOLUME_MOUNT}} \
    {{DOTNET_CACHE_IMAGE}} build

[group("build")]
[doc('Publishes both servers to build/win/{master,world}. Requires a Visual Studio developer environment (cmake + dotnet on PATH). Pass a config (default Release).')]
[windows]
publish config="Release":
    cmake --preset {{config}}-Windows
    cmake --build --preset {{config}}-Windows
    if (Test-Path "build/win") { Remove-Item -Recurse -Force "build/win" }
    dotnet publish master-server\MasterServer.csproj -c {{config}} -o build\win\master
    dotnet publish world-server\WorldServer.csproj -c {{config}} -o build\win\world
    Write-Host "Published servers into build/win/master and build/win/world"

[group("build")]
[doc('Publishes both servers to build/linux/{master,world} via Docker (alias of publish-docker on Unix).')]
[unix]
publish config="Release": (publish-docker config)

[group("build")]
[doc('Publishes both servers to build/linux/{master,world} via Docker. Runs from any host (use on Windows to cross-build the Linux copy). Pass a config (default Release).')]
publish-docker config="Release": docker-images
  docker run --rm \
    --platform=linux/amd64 \
    -e FOMSERVER_BUILD_PRESET={{config}} \
    --mount type=bind,src="{{justfile_directory()}}",dst="/workspace" \
    {{BUILD_VOLUME_MOUNT}} \
    {{CPP_CACHE_IMAGE}} build
  docker run --rm \
    --platform=linux/amd64 \
    -e FOMSERVER_BUILD_CONFIG={{config}} \
    {{NUGET_CACHE_MOUNT}} \
    --mount type=bind,src="{{justfile_directory()}}",dst="/workspace" \
    {{BUILD_VOLUME_MOUNT}} \
    {{DOTNET_CACHE_IMAGE}} publish

[group("build")]
[doc('Windows: publishes both the native (build/win) and Docker/Linux (build/linux) copies. Requires Docker. Pass a config (default Release).')]
[windows]
publish-all config="Release": (publish config) (publish-docker config)

[group("test")]
test: test-cpp test-dotnet

[group("test")]
test-cpp: docker-images
  docker run --rm \
    --platform=linux/amd64 \
    --mount type=bind,src="{{justfile_directory()}}",dst="/workspace" \
    {{BUILD_VOLUME_MOUNT}} \
    {{CPP_CACHE_IMAGE}} test

[group("test")]
test-dotnet: docker-images
  docker run --rm \
    --platform=linux/amd64 \
    {{NUGET_CACHE_MOUNT}} \
    --mount type=bind,src="{{justfile_directory()}}",dst="/workspace" \
    {{BUILD_VOLUME_MOUNT}} \
    {{DOTNET_CACHE_IMAGE}} test

[group("server")]
db-up:
  docker-compose -f docker/server/docker-compose.yml up -d db

[group("server")]
db-down:
  docker-compose -f docker/server/docker-compose.yml down db

[group("server")]
ms-up:
  docker-compose -f docker/server/docker-compose.yml up -d master-server

[group("server")]
ms-down:
  docker-compose -f docker/server/docker-compose.yml down master-server

[group("server")]
ws-up:
  docker-compose -f docker/server/docker-compose.yml up -d world-server-1

[group("server")]
ws-down:
  docker-compose -f docker/server/docker-compose.yml down world-server-1

[group("server")]
server-up: db-up ms-up ws-up

[group("server")]
server-down: db-down ms-down ws-down

[group("ghidra")]
[doc('Rebuild the labeled Ghidra project at disassembly/ from the committed JSON onto a fresh import of your game binaries.')]
[windows]
ghidra-gen:
    $launcher = "{{GHIDRA_HOME}}\Ghidra\Features\PyGhidra\support\pyghidra_launcher.py"; \
    if (-not (Test-Path $launcher)) { throw "Ghidra not found at {{GHIDRA_HOME}}. Set GHIDRA_INSTALL_DIR." }; \
    $proj = "{{justfile_directory()}}\disassembly"; \
    if (Test-Path "$proj\{{GHIDRA_PROJECT}}.lock") { throw "{{GHIDRA_PROJECT}} is locked (open in Ghidra, or a stale lock) - close it in Ghidra, or delete $proj\{{GHIDRA_PROJECT}}.lock, then re-run." }; \
    Remove-Item -Recurse -Force -ErrorAction SilentlyContinue "$proj\{{GHIDRA_PROJECT}}.*"; \
    foreach ($b in "CShell.dll", "Object.lto", "fom_client.exe") { \
        $f = Get-ChildItem "{{GAME_DIR}}" -Recurse -Filter $b -ErrorAction SilentlyContinue | Select-Object -First 1; \
        if ($f) { py $launcher "{{GHIDRA_HOME}}" --headless $proj {{GHIDRA_PROJECT}} -import $f.FullName -scriptPath "$proj\scripts" -postScript build_program.py } \
        else { Write-Warning "skipping $b (not found under {{GAME_DIR}})" } \
    }

[group("ghidra")]
[doc('Rebuild the labeled Ghidra project at disassembly/ from the committed JSON onto a fresh import of your game binaries.')]
[unix]
ghidra-gen:
    #!/usr/bin/env bash
    set -euo pipefail
    launcher="{{GHIDRA_HOME}}/Ghidra/Features/PyGhidra/support/pyghidra_launcher.py"
    [ -f "$launcher" ] || { echo "Ghidra not found at {{GHIDRA_HOME}}. Set GHIDRA_INSTALL_DIR." >&2; exit 1; }
    proj="{{justfile_directory()}}/disassembly"
    [ -e "$proj/{{GHIDRA_PROJECT}}.lock" ] && { echo "{{GHIDRA_PROJECT}} is locked (open in Ghidra, or a stale lock) - close it in Ghidra, or delete $proj/{{GHIDRA_PROJECT}}.lock, then re-run." >&2; exit 1; } || true
    rm -rf "$proj/{{GHIDRA_PROJECT}}".{gpr,rep,lock,lock~}
    for b in CShell.dll Object.lto fom_client.exe; do
        bin="$(find "{{GAME_DIR}}" -name "$b" -type f -print -quit 2>/dev/null || true)"
        if [ -n "$bin" ]; then python3 "$launcher" "{{GHIDRA_HOME}}" --headless "$proj" {{GHIDRA_PROJECT}} -import "$bin" -scriptPath "$proj/scripts" -postScript build_program.py
        else echo "skipping $b (not found under {{GAME_DIR}})"; fi
    done

[group("ghidra")]
[doc('Dump the labeled Ghidra project at disassembly/ back to JSON (run with the project closed in the GUI).')]
[windows]
ghidra-dump:
    $launcher = "{{GHIDRA_HOME}}\Ghidra\Features\PyGhidra\support\pyghidra_launcher.py"; \
    if (-not (Test-Path $launcher)) { throw "Ghidra not found at {{GHIDRA_HOME}}. Set GHIDRA_INSTALL_DIR." }; \
    $proj = "{{justfile_directory()}}\disassembly"; \
    if (Test-Path "$proj\{{GHIDRA_PROJECT}}.lock") { throw "{{GHIDRA_PROJECT}} is locked (open in Ghidra, or a stale lock) - close it in Ghidra, or delete $proj\{{GHIDRA_PROJECT}}.lock, then re-run." }; \
    py $launcher "{{GHIDRA_HOME}}" --headless $proj {{GHIDRA_PROJECT}} -process -noanalysis -scriptPath "$proj\scripts" -postScript export_program.py

[group("ghidra")]
[doc('Dump the labeled Ghidra project at disassembly/ back to JSON (run with the project closed in the GUI).')]
[unix]
ghidra-dump:
    #!/usr/bin/env bash
    set -euo pipefail
    launcher="{{GHIDRA_HOME}}/Ghidra/Features/PyGhidra/support/pyghidra_launcher.py"
    [ -f "$launcher" ] || { echo "Ghidra not found at {{GHIDRA_HOME}}. Set GHIDRA_INSTALL_DIR." >&2; exit 1; }
    proj="{{justfile_directory()}}/disassembly"
    [ -e "$proj/{{GHIDRA_PROJECT}}.lock" ] && { echo "{{GHIDRA_PROJECT}} is locked (open in Ghidra, or a stale lock) - close it in Ghidra, or delete $proj/{{GHIDRA_PROJECT}}.lock, then re-run." >&2; exit 1; } || true
    python3 "$launcher" "{{GHIDRA_HOME}}" --headless "$proj" {{GHIDRA_PROJECT}} -process -noanalysis -scriptPath "$proj/scripts" -postScript export_program.py
