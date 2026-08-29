#!/bin/sh
# Applies the SQL scripts when the Firebird container creates its database.
#
# The image's own .sql handling pipes files into isql without a connection
# charset. The container runs under C.UTF-8, so the ISO-8859-1 bytes in
# seed.sql get read as UTF-8 lead bytes and the accented category names are
# swallowed. Running isql here lets us pass -ch ISO8859_1 explicitly.
#
# Mounted into /docker-entrypoint-initdb.d; the .sql files are mounted into
# /sql so that the image does not also pick them up on its own.

echo "Applying schema.sql and seed.sql with charset ISO8859_1."

for script in /sql/schema.sql /sql/seed.sql; do
    [ -f "$script" ] || continue

    echo "  running $script"

    /opt/firebird/bin/isql -b -q \
        -ch ISO8859_1 \
        -user SYSDBA -password "$FIREBIRD_ROOT_PASSWORD" \
        "$FIREBIRD_DATABASE" -i "$script"

    if [ $? -ne 0 ]; then
        echo "ERROR: $script failed." >&2
        exit 1
    fi
done

echo "Database initialised."
