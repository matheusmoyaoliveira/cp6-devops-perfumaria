#!/usr/bin/env bash
# Cria o Resource Group que agrupa todos os recursos da CP6.
source "$(dirname "$0")/variaveis.sh"

az group create \
  --name "$RESOURCE_GROUP" \
  --location "$LOCATION" \
  --tags projeto=cp6 \
  -o table