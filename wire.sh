#!/bin/bash
set -e

# ============================================================
# Shopwave — Project Reference Wiring Script
# Run inside Shopwave folder: ./wire.sh
# ============================================================

ROOT="."
SRC="$ROOT/src"
MODULES="$SRC/Modules"
API="$SRC/API/Shopwave.API/Shopwave.API.csproj"

echo ""
echo "🔗 Wiring project references..."
echo ""

wire_module() {
  local module=$1

  APP="$MODULES/$module/Shopwave.Modules.$module.Application/Shopwave.Modules.$module.Application.csproj"
  DOMAIN="$MODULES/$module/Shopwave.Modules.$module.Domain/Shopwave.Modules.$module.Domain.csproj"
  INFRA="$MODULES/$module/Shopwave.Modules.$module.Infrastructure/Shopwave.Modules.$module.Infrastructure.csproj"

  echo "→ Wiring $module"

  # Application → Domain
  dotnet add "$APP" reference "$DOMAIN"

  # Infrastructure → Application
  dotnet add "$INFRA" reference "$APP"

  # API → Application
  dotnet add "$API" reference "$APP"
}

# ── Modules ─────────────────────────────────────
for module in Identity Catalog Inventory Orders Payments Stores
do
  wire_module $module
done

echo ""
echo "✅ Wiring complete."
echo ""