## How to recreate the onemore-wiki

1. Update content in the OneMore Wiki notebook
2. Right-click the notebook and archive
3. Move zip to a new folder
4. Copy the top-level contents/files of C:\Github\OneMore\docs into that same folder
5. PS> `.\build.ps1 '.\OneMore Wiki.zip'` — also builds the `pagefind/` search
   index if the `pagefind` CLI is on PATH (see prerequisite below); if it's
   not found, the build proceeds without search and prints a warning
6. Test by running `http-server`
7. http://localhost:8080
8. Copy updated files to C:\Github\OneMore\docs
9. Commit changes and wait for Github Pages to update Web site

### Prerequisite: Pagefind (search)

The site's search box is powered by [Pagefind](https://pagefind.app/), which
crawls the built HTML and generates a static search index — no server
required, and it's committed to `docs/` like everything else. Install the
CLI once per machine, e.g.:

- `npm install -g pagefind` (if Node is available), or
- `pip install pagefind` (if Python is available), or
- download a standalone binary from the
  [Pagefind releases page](https://github.com/CloudCannon/pagefind/releases)
  and put it on PATH
