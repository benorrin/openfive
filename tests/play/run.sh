#!/bin/sh
# Plays the whole game through, from a new game to the closing credits, with the automated
# play-through (--play). Each part continues from the save the previous part made.
#
#   tests/play/run.sh /path/to/FAMOUS5.cue
#
# Needs a Release build (dotnet build -c Release). Saves and settings go to a temporary
# directory, not to your own.
set -u
disc=${1:?usage: $0 DISC}
here=$(cd "$(dirname "$0")" && pwd)
dll="$here/../../src/OpenFive/bin/Release/net8.0/OpenFive.dll"
home=$(mktemp -d)
trap 'rm -rf "$home"' EXIT
export OPENFIVE_HOME="$home"
failed=0
for n in 1 2 3 4 5 6 7 8 9; do
    out=$(dotnet "$dll" --disc "$disc" --skip-intro --play "$here/part$n.txt" 2>&1)
    end=$(printf '%s\n' "$out" | grep '^end:')
    printf 'part%s: %s\n' "$n" "${end:-no result}"
    case "$end" in
        *"errors 0") ;;
        *) failed=1
           printf '%s\n' "$out" | grep -E 'EXPECTED|TIMEOUT|no object|not in|object is off|not here|option' ;;
    esac
done
exit $failed
