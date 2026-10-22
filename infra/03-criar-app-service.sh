#!/usr/bin/env bash
# Cria o App Service Plan (Linux) e o Web App .NET 8 que recebe o deploy da Release.
source "$(dirname "$0")/variaveis.sh"

az appservice plan create \
  --name "$APP_PLAN" \
  --resource-group "$RESOURCE_GROUP" \
  --location "$APP_LOCATION" \
  --is-linux \
  --sku "$APP_SKU" \
  -o table

az webapp create \
  --name "$WEBAPP" \
  --resource-group "$RESOURCE_GROUP" \
  --plan "$APP_PLAN" \
  --runtime "DOTNETCORE:8.0" \
  -o table \
  --query "{nome:name, url:defaultHostName, estado:state}" -o table