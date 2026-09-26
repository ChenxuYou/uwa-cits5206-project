#!/usr/bin/env bash
# Take a consistent copy of the costing database, check it, compress it, and prune old
# copies. Safe while the application is running: sqlite3's .backup copies a consistent
# snapshot even with the WAL in use, which copying the file with cp does not guarantee.
#
# Run daily by ric-costing-backup.timer, and by release.sh before every deployment.
# Prints the path of the backup it made.
set -euo pipefail

DB="${RIC_DB:-/var/lib/ric-costing/ric-costing.db}"
DEST="${RIC_BACKUP_DIR:-/var/lib/ric-costing/backups}"
KEEP_DAYS="${RIC_BACKUP_KEEP_DAYS:-30}"

if [[ ! -f "$DB" ]]; then
  echo "backup: no database at $DB" >&2
  exit 1
fi

mkdir -p "$DEST"
chmod 700 "$DEST"

stamp="$(date -u +%Y%m%dT%H%M%SZ)"
out="$DEST/ric-costing-$stamp.db"

sqlite3 "$DB" ".backup '$out'"

check="$(sqlite3 "$out" 'PRAGMA integrity_check;')"
if [[ "$check" != "ok" ]]; then
  echo "backup: integrity check failed on $out: $check" >&2
  rm -f "$out"
  exit 1
fi

gzip "$out"
chmod 600 "$out.gz"
find "$DEST" -name 'ric-costing-*.db.gz' -mtime +"$KEEP_DAYS" -delete

echo "$out.gz"
