#!/usr/bin/env bash
# Uso: bash scripts/run-tests.sh [EditMode|PlayMode] [filtro]
set -u
PLATFORM="${1:-EditMode}"
FILTER="${2:-}"
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
UNITY="C:/Program Files/Unity/Hub/Editor/6000.0.85f1/Editor/Unity.exe"
RESULTS="$ROOT/.superpowers/test-results-$PLATFORM.xml"
LOG="$ROOT/.superpowers/test-$PLATFORM.log"
if [ -z "${GRAPHICS:-}" ]; then NG="-nographics"; else NG=""; fi
ARGS=(-batchmode $NG -projectPath "$ROOT/game" -runTests -testPlatform "$PLATFORM" -testResults "$RESULTS" -logFile "$LOG")
[ -n "$FILTER" ] && ARGS+=(-testFilter "$FILTER")
"$UNITY" "${ARGS[@]}"
CODE=$?
grep -o 'total="[0-9]*" passed="[0-9]*" failed="[0-9]*"' "$RESULTS" | head -1
exit $CODE
