extractParam() {
    shopt -s nullglob
    testfiles=(*/TestResults/*.trx) 
    if [ ${#testfiles[@]} -gt 0 ]; then
        cat */TestResults/*.trx |grep "<Counters" | sed -E "s/^.*$1=.([[:digit:]]+).*$/\1/" | awk 'BEGIN { total = 0 } { total += $1 } END { print total }'
    else
        echo 0
    fi
}

total=$(extractParam "total")
successes=$(extractParam "passed")

mkdir -p tests
echo "Total: $total" > tests/$1.txt
echo "Succès: $successes" >> tests/$1.txt
