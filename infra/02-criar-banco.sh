#!/usr/bin/env bash
# Cria o SQL Server, as regras de firewall e o banco Azure SQL.
source "$(dirname "$0")/variaveis.sh"
: "${SQL_ADMIN_PASSWORD:?Defina SQL_ADMIN_PASSWORD no infra/.env}"

az sql server create \
  --name "$SQL_SERVER" \
  --resource-group "$RESOURCE_GROUP" \
  --location "$LOCATION" \
  --admin-user "$SQL_ADMIN_USER" \
  --admin-password "$SQL_ADMIN_PASSWORD" \
  -o table

# 0.0.0.0 é o código da Azure para "liberar os serviços da própria Azure" (o App Service)
az sql server firewall-rule create \
  --resource-group "$RESOURCE_GROUP" \
  --server "$SQL_SERVER" \
  --name AllowAzureServices \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0 \
  -o table

# Libera o seu IP atual, para fazer os SELECTs de evidência
MEU_IP=$(curl -s https://api.ipify.org)
az sql server firewall-rule create \
  --resource-group "$RESOURCE_GROUP" \
  --server "$SQL_SERVER" \
  --name AcessoLocal \
  --start-ip-address "$MEU_IP" \
  --end-ip-address "$MEU_IP" \
  -o table

az sql db create \
  --resource-group "$RESOURCE_GROUP" \
  --server "$SQL_SERVER" \
  --name "$SQL_DATABASE" \
  --edition Basic \
  --capacity 5 \
  --backup-storage-redundancy Local \
  -o table