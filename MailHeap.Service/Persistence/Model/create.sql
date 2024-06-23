CREATE TABLE "MESSAGES"
(
    "ID"            INTEGER NOT NULL,
    "TIMESTAMP"     REAL    NOT NULL,
    "STATE"         INTEGER NOT NULL,
    "ENVELOPE_FROM" TEXT    NOT NULL,
    "ENVELOPE_TO"   TEXT    NOT NULL,
    "FROM"          TEXT,
    "SUBJECT"       TEXT,
    "SOURCE_IP"     TEXT,
    "SOURCE_PORT"   INTEGER,
    "SECURED"       INTEGER NOT NULL,
    "HELLO_NAME"    TEXT,
    "MESSAGE_DATA"  BLOB    NOT NULL,
    "PARAMS"        TEXT,
    "FORWARD_TO"    TEXT,
    PRIMARY KEY ("ID" AUTOINCREMENT)
);

CREATE INDEX "IX_MESSAGES_STATE" ON "MESSAGES" (
                                                "STATE",
                                                "TIMESTAMP"
    );
