#!/usr/bin/env bash
set -euo pipefail

# Generate a Base64 32-byte key if not provided
if [ -z "${PGEXPLORER_AES_KEY:-}" ]; then
  echo "[entrypoint] PGEXPLORER_AES_KEY not set. Generating a random key for this container instance." >&2
  export PGEXPLORER_AES_KEY="$(openssl rand -base64 32)"
fi

exec dotnet Pg.Explorer.Host.dll
