# Deploying the costing tool

A runbook for one Ubuntu 24.04 server: the application behind Caddy, which provides HTTPS,
with the database, its backups and the cookie keys under `/var/lib/ric-costing`. It is what
M5 asks for — staging over HTTPS, no demo credentials, a tested backup and rollback — and it
answers C2, H1 and H2 in the 25 September audit.

Every step below was rehearsed end to end on 25 September 2026 with a stand-in for
`systemctl`: first release, sign-in over HTTPS by IP address with Caddy's internal
certificate, forced password change, backup, restore, and a deliberately broken release
rolled back automatically.

| Path | What it holds | Survives a redeploy |
| --- | --- | --- |
| `/opt/ric-costing/releases/<time>/` | One published build per release, the last five kept | — |
| `/opt/ric-costing/current` | A link to the release that is live | — |
| `/opt/ric-costing/deploy/` | These scripts | — |
| `/var/lib/ric-costing/ric-costing.db` | **The database. Sealed records live here.** | Yes |
| `/var/lib/ric-costing/keys/` | Keys that sign the login cookie and form tokens (H1) | Yes |
| `/var/lib/ric-costing/backups/` | Daily and pre-release backups, 30 days | Yes |
| `/etc/ric-costing/ric-costing.env` | The connection string, and the bootstrap administrator once | Yes |

## Why plain `http://` cannot work

Outside Development the sign-in cookie is marked `Secure`, so a browser discards it on a
plain `http://` page and sends the person back to the sign-in form every time. That is
deliberate: the cookie is what proves who approved a sealed record. The application
therefore listens on `127.0.0.1:5000` only, and Caddy serves it over HTTPS. Running the
server in Development "to get it working" is not a workaround — it prints the demo
passwords on the sign-in page and creates those accounts.

## 1. Install

```bash
sudo apt-get update
sudo apt-get install -y aspnetcore-runtime-10.0 caddy sqlite3

sudo useradd --system --home-dir /var/lib/ric-costing --shell /usr/sbin/nologin ric-costing
sudo install -d -o ric-costing -g ric-costing -m 700 /var/lib/ric-costing
sudo install -d -m 755 /opt/ric-costing/releases /opt/ric-costing/deploy /etc/ric-costing
sudo cp deploy/*.sh /opt/ric-costing/deploy/ && sudo chmod 755 /opt/ric-costing/deploy/*.sh
```

## 2. Configure

`/etc/ric-costing/ric-costing.env`, readable by root only (`sudo chmod 600`):

```ini
ConnectionStrings__CostingDb=Data Source=/var/lib/ric-costing/ric-costing.db

# First start only. Creates one administrator while the user table is empty; that person is
# made to choose a new password at first sign-in. Delete these two lines afterwards.
Bootstrap__AdminUserName=admin
Bootstrap__AdminPassword=<at least 12 characters, not reused anywhere>
Bootstrap__AdminDisplayName=<the administrator's name as it should appear on records>
```

The application refuses to start outside Development if the database path is relative
(H2), so a typo here stops it at once instead of creating a database in the release folder.
The keys go in `/var/lib/ric-costing/keys` unless `DataProtection__KeysDirectory` says
otherwise.

```bash
sudo cp deploy/ric-costing.service deploy/ric-costing-backup.service deploy/ric-costing-backup.timer /etc/systemd/system/
sudo systemctl daemon-reload
sudo systemctl enable ric-costing ric-costing-backup.timer
sudo systemctl start ric-costing-backup.timer
```

### Caddy

By IP address, before there is a domain name:

```bash
sudo cp deploy/Caddyfile.ip /etc/caddy/Caddyfile
echo 'RIC_HOST=<server IP address>' | sudo tee -a /etc/default/caddy
sudo systemctl restart caddy
```

Once a name resolves to the server, switch to `Caddyfile.domain` with `RIC_HOST=<name>`;
Caddy fetches and renews a Let's Encrypt certificate itself. Open ports 80 and 443, and
nothing else: port 5000 is loopback only.

#### Trusting the test certificate

With `Caddyfile.ip` the browser warns that the certificate is not trusted, because Caddy
issued it. Testers can click through the warning, or trust Caddy's root once: copy
`/var/lib/caddy/.local/share/caddy/pki/authorities/local/root.crt` to their machine and
import it under *Trusted Root Certification Authorities* (Windows: `certmgr.msc`; macOS:
Keychain Access, then mark it *Always Trust*). Do not ask the client to do this — give them
the domain-name setup.

## 3. Release

On a machine with the .NET 10 SDK:

```bash
dotnet publish src/CostingTool.csproj -c Release -o out
rsync -a --delete out/ <server>:/tmp/ric-costing-out/
ssh <server> sudo /opt/ric-costing/deploy/release.sh /tmp/ric-costing-out
```

`release.sh` copies the build into `releases/`, backs the database up, switches `current`,
restarts the service and waits up to 60 seconds for the sign-in page to answer. If it does
not, it switches back to the previous release and exits non-zero. Schema changes are EF Core
migrations and are applied when the new build starts, which is why the backup comes first.

The first release creates the database and the bootstrap administrator. Sign in as that
administrator, choose a new password when asked, create the other accounts under
*Admin → Users* (each person chooses their own password at first sign-in), then delete the
two `Bootstrap__` lines from the environment file.

## Rolling back

`release.sh` rolls back the code by itself. If the failed build had already applied a
migration, the previous code may not read the database; restore the backup it made first
(the newest file in `backups/`, printed by `release.sh`):

```bash
sudo /opt/ric-costing/deploy/restore.sh /var/lib/ric-costing/backups/ric-costing-<time>.db.gz
```

To go back to an older release deliberately:

```bash
sudo ln -sfn /opt/ric-costing/releases/<time> /opt/ric-costing/current && sudo systemctl restart ric-costing
```

## Backups

`ric-costing-backup.timer` runs `backup.sh` at 02:00 Perth time every day. It uses
SQLite's online backup, checks the copy with `PRAGMA integrity_check`, compresses it and
keeps 30 days. **Copies on the same disk are not enough on their own:** copy
`/var/lib/ric-costing/backups/` off the server as well (UWA storage, or the team's Teams
area) until the hosting question in `plan.md` §5 is settled.

`restore.sh <file>` checks the backup, stops the service, sets the current database aside
as `ric-costing.db.before-restore-<time>`, puts the backup in place and starts the service.
Rehearse it once after the first release: take a backup, add a test cycle, restore, and
check the cycle has gone.

## Checking it

```bash
systemctl status ric-costing
journalctl -u ric-costing -n 100
curl -sI https://<host>/Account/Login | grep -iE 'x-frame|content-security'   # anti-framing headers (M1)
ls -l /var/lib/ric-costing/backups | tail -3
```
