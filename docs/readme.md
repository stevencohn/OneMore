## How to recreate the onemore-wiki

1. Update content in the OneMore Wiki notebook
2. Right-click the notebook and archive
3. Move zip to a new folder
4. Copy the top-level contents/files of C:\Github\OneMore\docs into that same folder
5. PS> `.\build.ps1 '.\OneMore Wiki.zip'`
6. Test by running `http-server`
7. http://localhost:8080
8. Copy updated files to C:\Github\OneMore\docs
9. Commit changes and wait for Github Pages to update Web site
