CREATE TABLE IF NOT EXISTS layout (layoutID INTEGER PRIMARY KEY AUTOINCREMENT, name TEXT NOT NULL, UNIQUE (name));
CREATE TABLE IF NOT EXISTS layouts_schema (schemaID INTEGER PRIMARY KEY UNIQUE NOT NULL, version NUMERIC (12) UNIQUE NOT NULL);
CREATE TABLE IF NOT EXISTS layout_window (windowID INTEGER PRIMARY KEY AUTOINCREMENT, layoutID INTEGER NOT NULL REFERENCES layout (layoutID) ON DELETE CASCADE, name TEXT NOT NULL, alias TEXT, location TEXT, uri TEXT NOT NULL, notebookID TEXT NOT NULL, sectionID TEXT NOT NULL, pageID TEXT NOT NULL, zOrder INTEGER NOT NULL DEFAULT 0, device TEXT, winLeft INTEGER, winTop INTEGER, winRight INTEGER, winBottom  INTEGER, pageKey INTEGER);
CREATE INDEX IF NOT EXISTS idx_layouts_layout ON layout (layoutID);
CREATE UNIQUE INDEX IF NOT EXISTS idx_layouts_alias_per_window ON layout_window (layoutID, alias) WHERE alias IS NOT NULL;
CREATE UNIQUE INDEX IF NOT EXISTS idx_layouts_target_page ON layout_window (layoutID, pageID);
CREATE UNIQUE INDEX IF NOT EXISTS idx_layouts_target_pagekey ON layout_window (layoutID, pageKey) WHERE pageKey IS NOT NULL;
REPLACE INTO layouts_schema (schemaID, version) VALUES (0, 2);
