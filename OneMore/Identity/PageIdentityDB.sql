CREATE TABLE IF NOT EXISTS identity_schema (schemaID INTEGER PRIMARY KEY UNIQUE NOT NULL, version NUMERIC (12) UNIQUE NOT NULL);
CREATE TABLE IF NOT EXISTS identity_page (pageKey INTEGER PRIMARY KEY AUTOINCREMENT, pageID TEXT NOT NULL, notebookKey TEXT NOT NULL, sectionKey TEXT NOT NULL, title TEXT NOT NULL, created TEXT NOT NULL, modified TEXT NOT NULL, level INTEGER NOT NULL DEFAULT 1, missingSince TEXT, lastSeen TEXT NOT NULL, pageGuid TEXT);
CREATE INDEX IF NOT EXISTS IDX_identity_pageID ON identity_page (pageID);
CREATE INDEX IF NOT EXISTS IDX_identity_notebookKey ON identity_page (notebookKey);
CREATE INDEX IF NOT EXISTS IDX_identity_missingSince ON identity_page (missingSince);
CREATE INDEX IF NOT EXISTS IDX_identity_pageGuid ON identity_page (pageGuid);
REPLACE INTO identity_schema (schemaID, version) VALUES (0, 1);
