# On-Premises Deployment Plan

This plan deploys the ASP.NET Core application and its SQLite database on a Linux server
managed by the client. Nginx is the only public service and terminates HTTPS. The application
container listens on the private Docker network only.

## 1. Deployment decisions

| Area | Decision |
| --- | --- |
| Host | Client-managed Linux VM or physical server with Docker Engine and Compose v2 |
| Application | `ric-costing` container from the repository `Dockerfile` |
| Database | SQLite at `/srv/ric-costing/data/ric-costing-v7.db`, bind-mounted into the container |
| Proxy | Nginx container; ports 80 and 443 are the only application ports on the host |
| TLS | Certificate issued by the UWA/internal CA, or an approved public CA if the hostname is public |
| DNS | The chosen costing hostname resolves to the on-prem host's firewall address |
| Secrets | A host-local `.env.production` file with mode `600`; never in Git or the image |

The current application creates the schema with EF Core `EnsureCreatedAsync()`. That is suitable
for first boot of an empty database, but it does not safely upgrade an existing database. EF Core
migrations remain a release gate before the first schema-changing production upgrade.

## 2. Host preparation

On the deployment host, install a supported Linux distribution, Docker Engine, and the Docker
Compose plugin. Create a restricted deployment account and the application directories:

```sh
sudo install -d -m 750 -o deploy -g deploy /srv/ric-costing/{data,certs,nginx}
sudo ufw allow from <approved-admin-network> to any port 22 proto tcp
sudo ufw allow 80/tcp
sudo ufw allow 443/tcp
sudo ufw --force enable
```

Do not expose port `8080`. The Compose file publishes only Nginx's ports, and the application is
reachable from Nginx through the private `ric-internal` Docker network.

Copy the repository or a reviewed release bundle to `/srv/ric-costing/app`. Keep the deployment
directory separate from the database and certificate directories so an image update cannot
replace either.

## 3. Database setup

The database is created automatically on the first application start. Create the environment
file on the host and protect it before starting the stack:

```sh
cd /srv/ric-costing/app/deploy
cp .env.production.example .env.production
chmod 600 .env.production
$EDITOR .env.production
```

Set a unique, long bootstrap password. On first start, the application creates exactly one
administrator if the user table is empty. After signing in, create the required approver and
custodian accounts in the application, then remove the bootstrap credentials from the file and
restart the stack. Never use the development demo accounts in production.

The database directory must be included in host backups. A consistent SQLite backup should use
SQLite's backup command while the application is stopped, or the SQLite online backup API. A
simple maintenance window procedure is:

```sh
docker compose stop app
cp -a data/ric-costing-v7.db "backups/ric-costing-$(date +%Y%m%d-%H%M%S).db"
docker compose start app
```

Retain encrypted, off-host copies according to UWA's retention policy. Test a restore before
handover: restore a copy to a separate directory, start a temporary stack against it, and sign
in with a test account.

## 4. Application deployment

Build and start the reviewed release from `/srv/ric-costing/app`:

```sh
cd /srv/ric-costing/app/deploy
mkdir -p data certs backups
docker compose build --pull app
docker compose up -d
docker compose ps
docker compose logs --tail=100 app
```

The expected application listener is `http://app:8080` inside the Compose network. Confirm that
the first startup log reports schema creation and, when configured, bootstrap administrator
creation. Do not publish the app container directly to the host.

Before DNS cutover, verify the application through Nginx with a temporary hosts-file entry and
complete these checks:

1. Login, logout, password change, and account deactivation.
2. A complete costing cycle, approval, and PDF export.
3. Access control between two test users and the administrator view.
4. Restart persistence: the database and accounts remain after `docker compose down` followed by `docker compose up -d`.
5. A backup and restore test.

## 5. Nginx and HTTPS

Replace `costing.example.uwa.edu.au` in `deploy/nginx/default.conf` with the approved hostname.
Place the certificate chain and private key on the host with restrictive permissions:

```sh
sudo install -m 644 fullchain.pem /srv/ric-costing/certs/fullchain.pem
sudo install -m 600 privkey.pem /srv/ric-costing/certs/privkey.pem
```

The certificate must contain the exact DNS name users will enter. The included Nginx config:

- redirects HTTP to HTTPS with a permanent redirect;
- enables TLS 1.2 and 1.3;
- forwards the original HTTPS scheme to ASP.NET Core;
- passes the original host and client address to the application.

Reload after certificate renewal without stopping the application:

```sh
docker compose exec nginx nginx -t
docker compose exec nginx nginx -s reload
```

The external firewall should allow TCP 80 and 443, and SSH only from the administration
network. If the certificate uses an internal CA, distribute that CA certificate to managed
client devices before DNS cutover.

## 6. Release, rollback, and operations

Tag every deployed source revision. For an upgrade, take a database backup, build the new image,
run the staging checks, then deploy:

```sh
docker compose stop app
cp -a data/ric-costing-v7.db "backups/pre-release-$(date +%Y%m%d-%H%M%S).db"
docker compose build --pull app
docker compose up -d app
docker compose logs --tail=100 app
```

If the new application fails validation and no database schema has changed, restore the previous
image tag and start the app again. If a schema change is introduced, the release must include a
tested EF Core migration and a documented rollback or restore procedure before deployment.

Monitor container status, disk space under `/srv/ric-costing/data`, Nginx error logs, application
logs, certificate expiry, and backup freshness. Do not treat a running container as proof that
backups or HTTPS are working.

## 7. Completion gates

Deployment is ready for client handover when all of the following are recorded:

- hostname and certificate owner are confirmed;
- host and firewall access are documented;
- the production bootstrap account has been replaced with named client accounts;
- HTTPS, secure cookies, redirects, and PDF export pass acceptance checks;
- an encrypted backup has been restored successfully;
- the current image tag, database backup location, and rollback steps are in the handover pack;
- EF Core migrations replace `EnsureCreatedAsync()` before any release that changes the model.
