#!/usr/bin/env bash
# Put a backup made by backup.sh back in place of the live database.
#
#   sudo deploy/restore.sh /var/lib/ric-costing/backups/ric-costing-20261002T020000Z.db.gz
#
# The database being replaced is kept beside it as ric-costing.db.before-restore-<time>,
# so a restore can itself be undone.
set -euo pipefail

BACKUP="${1:?usage: restore.sh <backup.db.gz>}"
DB="${RIC_DB:-/var/lib/ric-costing/ric-costing.db}"
SERVICE="${RIC_SERVICE:-ric-costing}"
SYSTEMCTL="${SYSTEMCTL:-systemctl}"
OWNER="${RIC_OWNER:-ric-costing:ric-costing}"

tmp="$(mktemp "$(dirname "$DB")/restore.XXXXXX")"
gunzip -c "$BACKUP" > "$tmp"
check="$(sqlite3 "$tmp" 'PRAGMA integrity_check;')"
if [[ "$check" != "ok" ]]; then
  echo "restore: $BACKUP does not pass an integrity check: $check" >&2
  rm -f "$tmp"
  exit 1
fi

$SYSTEMCTL stop "$SERVICE"

if [[ -f "$DB" ]]; then
  # Fold any write-ahead log into the old file before it is set aside, so it is complete.
  sqlite3 "$DB" 'PRAGMA wal_checkpoint(TRUNCATE);' > /dev/null
  mv "$DB" "$DB.before-restore-$(date -u +%Y%m%dT%H%M%SZ)"
fi
rm -f "$DB-wal" "$DB-shm"
mv "$tmp" "$DB"
chown "$OWNER" "$DB" 2>/dev/null || true
chmod 600 "$DB"

$SYSTEMCTL start "$SERVICE"
echo "restore: $DB replaced from $BACKUP"
