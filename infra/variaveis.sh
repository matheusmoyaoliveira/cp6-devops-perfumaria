#!/usr/bin/env bash
# Nomes e região compartilhados pelos scripts de infraestrutura da CP6.
set -euo pipefail

RM="rm562822"
LOCATION="centralus"

# Região do App Service. Pode ser diferente da do banco, se o limite de F1 bloquear centralus.
APP_LOCATION="$LOCATION"

RESOURCE_GROUP="rg-cp6-perfumaria"
SQL_SERVER="sql-cp6-perfumaria-$RM"
SQL_DATABASE="sqldb-perfumaria"
SQL_ADMIN_USER="perfumariaadmin"
APP_PLAN="asp-cp6-perfumaria"
APP_SKU="F1"
WEBAPP="app-cp6-perfumaria-$RM"

# Carrega a senha do infra/.env, que não vai para o Git
ENV_FILE="$(dirname "${BASH_SOURCE[0]}")/.env"
if [ -f "$ENV_FILE" ]; then
  set -a
  source "$ENV_FILE"
  set +a
fi