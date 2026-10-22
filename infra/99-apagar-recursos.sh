#!/usr/bin/env bash
# Apaga SOMENTE o Resource Group da CP6 (e tudo dentro dele). Não toca na Sprint 4.
source "$(dirname "$0")/variaveis.sh"

az group delete --name "$RESOURCE_GROUP" --yes --no-wait