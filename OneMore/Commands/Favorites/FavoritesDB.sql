CREATE TABLE IF NOT EXISTS favorites_folder (folderID INTEGER PRIMARY KEY AUTOINCREMENT, name TEXT NOT NULL, UNIQUE(name));
CREATE TABLE IF NOT EXISTS favorite (favoriteID INTEGER PRIMARY KEY AUTOINCREMENT, folderID INTEGER REFERENCES favorites_folder(folderID) ON DELETE CASCADE, name TEXT NOT NULL, alias TEXT, location TEXT, uri TEXT NOT NULL, notebookID TEXT NOT NULL, sectionID TEXT NOT NULL, pageID TEXT, sortOrder INTEGER NOT NULL DEFAULT 0);
CREATE INDEX IF NOT EXISTS idx_favorites_folder ON Favorite(folderID);
CREATE UNIQUE INDEX IF NOT EXISTS idx_favorite_alias_per_folder ON Favorite(folderID, alias) WHERE folderID IS NOT NULL AND alias IS NOT NULL;
CREATE UNIQUE INDEX IF NOT EXISTS idx_favorite_alias_root ON Favorite(alias) WHERE folderID IS NULL AND alias IS NOT NULL;
CREATE UNIQUE INDEX IF NOT EXISTS idx_favorite_target_page ON favorite(notebookID, sectionID, pageID) WHERE pageID IS NOT NULL;
CREATE UNIQUE INDEX IF NOT EXISTS idx_favorite_target_section ON favorite(notebookID, sectionID) WHERE pageID IS NULL;
