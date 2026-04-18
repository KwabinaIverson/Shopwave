#!/bin/bash
# ============================================================
# Shopwave — Solution Scaffold Script (Mac / Linux)
# Run: chmod +x scaffold.sh && ./scaffold.sh
# Requires: .NET 8 SDK — https://dotnet.microsoft.com/download
# ============================================================

set -e

ROOT="."
SRC="$ROOT/src"
TESTS="$ROOT/tests"
MODULES="$SRC/Modules"

echo ""
echo "🌊 Scaffolding Shopwave..."
echo ""

# ── Solution ──────────────────────────────────────────────
dotnet new sln -n Shopwave

# ── API Project ───────────────────────────────────────────
mkdir -p $SRC/API
dotnet new webapi -n Shopwave.API -o $SRC/API/Shopwave.API --no-openapi
dotnet sln Shopwave.sln add $SRC/API/Shopwave.API/Shopwave.API.csproj

# ── Helper: create a classlib and add to solution ─────────
create_project() {
  local path=$1
  local name=$2
  mkdir -p "$path"
  dotnet new classlib -n "$name" -o "$path/$name"
  dotnet sln Shopwave.sln add "$path/$name/$name.csproj"
  rm -f "$path/$name/Class1.cs"
}

# ── Helper: create a test project and add to solution ─────
create_test_project() {
  local path=$1
  local name=$2
  mkdir -p "$path"
  dotnet new xunit -n "$name" -o "$path/$name"
  dotnet sln Shopwave.sln add "$path/$name/$name.csproj"
  rm -f "$path/$name/UnitTest1.cs"
}

# ── Modules ───────────────────────────────────────────────
for module in Identity Catalog Inventory Orders Payments Stores
do
  create_project "$MODULES/$module" "Shopwave.Modules.$module.Domain"
  create_project "$MODULES/$module" "Shopwave.Modules.$module.Application"
  create_project "$MODULES/$module" "Shopwave.Modules.$module.Infrastructure"
done

# ── Single Modules ─────────────────────────────────────────
create_project "$MODULES/Notifications" "Shopwave.Modules.Notifications"
create_project "$MODULES/Analytics" "Shopwave.Modules.Analytics"

# ── Shared Kernel ─────────────────────────────────────────
create_project "$SRC/Shared" "Shopwave.Shared"

# ── Test Projects ─────────────────────────────────────────
for module in Identity Catalog Inventory Orders Payments
do
  create_test_project "$TESTS" "Shopwave.Modules.$module.Tests"
done

echo ""
echo "✅ Shopwave solution scaffolded successfully."
echo ""
echo "Next steps:"
echo "  dotnet build"
echo ""