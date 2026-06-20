CREATE TABLE IF NOT EXISTS layout (layoutID INTEGER PRIMARY KEY AUTOINCREMENT, name TEXT NOT NULL, UNIQUE (name));
CREATE TABLE IF NOT EXISTS layout_window (windowID INTEGER PRIMARY KEY AUTOINCREMENT, layoutID INTEGER NOT NULL REFERENCES layout (layoutID) ON DELETE CASCADE, name TEXT NOT NULL, alias TEXT, location TEXT, uri TEXT NOT NULL, notebookID TEXT NOT NULL, sectionID TEXT NOT NULL, pageID TEXT NOT NULL, zOrder INTEGER NOT NULL DEFAULT 0, device TEXT, winLeft INTEGER, winTop INTEGER, winRight INTEGER, winBottom  INTEGER);
CREATE INDEX IF NOT EXISTS idx_layouts_layout ON layout (layoutID);
CREATE UNIQUE INDEX IF NOT EXISTS idx_layouts_alias_per_window ON layout_window (layoutID, alias) WHERE alias IS NOT NULL;
CREATE UNIQUE INDEX IF NOT EXISTS idx_layouts_target_page ON layout_window (layoutID, pageID);
