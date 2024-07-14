CREATE TABLE IF NOT EXISTS hashtag_scanner (scannerID INTEGER PRIMARY KEY UNIQUE NOT NULL, version NUMERIC (12) UNIQUE NOT NULL, scanTime TEXT NOT NULL);
CREATE TABLE IF NOT EXISTS hashtag (tag TEXT NOT NULL, moreID TEXT NOT NULL, objectID TEXT NOT NULL, snippet TEXT, documentOrder INTEGER DEFAULT (0), lastModified TEXT NOT NULL, PRIMARY KEY (tag, objectID), CONSTRAINT FK_moreID FOREIGN KEY (moreID) REFERENCES hashtag_page (moreID) ON DELETE CASCADE);
CREATE TABLE IF NOT EXISTS hashtag_page (moreID PRIMARY KEY, pageID TEXT NOT NULL, titleID TEXT, notebookID TEXT NOT NULL, sectionID TEXT NOT NULL, path TEXT, name TEXT);
CREATE TABLE IF NOT EXISTS hashtag_notebook (notebookID TEXT PRIMARY KEY, name TEXT, lastModified TEXT NOT NULL DEFAULT '');
CREATE INDEX IF NOT EXISTS IDX_moreID ON hashtag (moreID);
CREATE INDEX IF NOT EXISTS IDX_pageID ON hashtag_page (pageID);
CREATE INDEX IF NOT EXISTS IDX_tag ON hashtag (tag);
CREATE VIEW IF NOT EXISTS page_hashtags (moreID, tags) AS SELECT t.moreID, group_concat(DISTINCT(t.tag)) AS tags FROM hashtag t GROUP BY t.moreID;
REPLACE INTO hashtag_scanner (scannerID, version, scanTime) VALUES (0, 4,'0001-01-01T00:00:00.0000Z');
