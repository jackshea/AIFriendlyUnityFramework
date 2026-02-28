#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
DOMAIN_DIR="$ROOT_DIR/Assets/_Project/Game.Domain"

if [[ ! -d "$DOMAIN_DIR" ]]; then
  echo "[verify-domain-purity] Domain directory not found: $DOMAIN_DIR"
  exit 1
fi

fail=0

check_forbidden() {
  local pattern="$1"
  local label="$2"
  if rg -n "$pattern" "$DOMAIN_DIR" --glob "*.cs" >/tmp/alf_purity_hits.txt; then
    echo "[verify-domain-purity] Forbidden usage detected: $label"
    cat /tmp/alf_purity_hits.txt
    fail=1
  fi
}

check_forbidden "using\s+UnityEngine" "UnityEngine namespace"
check_forbidden "\bMonoBehaviour\b" "MonoBehaviour"
check_forbidden "\bGameObject\b" "GameObject"
check_forbidden "\bTransform\b" "Transform"
check_forbidden "Time\.deltaTime" "Time.deltaTime"

if [[ "$fail" -ne 0 ]]; then
  echo "[verify-domain-purity] FAILED"
  exit 1
fi

echo "[verify-domain-purity] PASSED"
