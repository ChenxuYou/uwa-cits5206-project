#!/usr/bin/env bash
# Deploy a published build, and roll back to the previous one if it does not come up.
#
#   dotnet publish src/CostingTool.csproj -c Release -o out     # on a machine with the SDK
#   rsync -a out/ server:/tmp/ric-costing-out/
#   sudo /opt/ric-costing/deploy/release.sh /tmp/ric-costing-out
#
# What it does, in order:
#   1. copies the build to /opt/ric-costing/releases/<time>
#   2. backs up the database (the new build may apply a migration when it starts)
#   3. points /opt/ric-costing/current at the new release and restarts the service
#   4. waits for the sign-in page to answer; if it does not, points `current` back at the
#      previous release, restarts, and exits non-zero
#
# A rollback restores the previous CODE. If the new build had already migrated the
# database, restore the backup from step 2 as well (deploy/README.md, "Rolling back").
set -euo pipefail

BUILD="${1:?usage: release.sh <folder produced by dotnet publish>}"
ROOT="${RIC_ROOT:-/opt/ric-costing}"
SERVICE="${RIC_SERVICE:-ric-costing}"
SYSTEMCTL="${SYSTEMCTL:-systemctl}"
HEALTH_URL="${RIC_HEALTH_URL:-http://127.0.0.1:5000/Account/Login}"
HEALTH_SECONDS="${RIC_HEALTH_SECONDS:-60}"
KEEP="${RIC_KEEP_RELEASES:-5}"
HERE="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

if [[ ! -f "$BUILD/CostingTool.dll" ]]; then
  echo "release: $BUILD does not look like a published build (no CostingTool.dll)" >&2
  exit 1
fi

release="$ROOT/releases/$(date -u +%Y%m%dT%H%M%SZ)"
mkdir -p "$ROOT/releases"
cp -a "$BUILD" "$release"
previous="$(readlink -f "$ROOT/current" 2>/dev/null || true)"

DB="${RIC_DB:-/var/lib/ric-costing/ric-costing.db}"
if [[ -f "$DB" ]]; then
  echo "release: backing up the database before starting the new build"
  RIC_DB="$DB" "$HERE/backup.sh"
else
  echo "release: no database at $DB yet, so nothing to back up (first release)"
fi

healthy() {
  local deadline=$((SECONDS + HEALTH_SECONDS))
  while (( SECONDS < deadline )); do
    if curl -fs -o /dev/null "$HEALTH_URL"; then
      return 0
    fi
    sleep 2
  done
  return 1
}

ln -sfn "$release" "$ROOT/current"
$SYSTEMCTL restart "$SERVICE"

if healthy; then
  echo "release: $release is live"
  ls -1dt "$ROOT"/releases/* | tail -n +$((KEEP + 1)) | xargs -r rm -rf
  exit 0
fi

echo "release: $release did not answer at $HEALTH_URL within ${HEALTH_SECONDS}s" >&2
if [[ -n "$previous" && -d "$previous" ]]; then
  ln -sfn "$previous" "$ROOT/current"
  $SYSTEMCTL restart "$SERVICE"
  if healthy; then
    echo "release: rolled back to $previous" >&2
  else
    echo "release: rolled back to $previous, which is not answering either; see journalctl -u $SERVICE" >&2
  fi
else
  echo "release: there is no previous release to roll back to" >&2
fi
exit 1
