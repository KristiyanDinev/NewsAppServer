### Dotnet
- dotnet add package Microsoft.Data.Sqlite.Core
- dotnet add package Microsoft.Data.Sqlite
- dotnet add package SQLite


### Resources
- To publish it to internet https://www.youtube.com/watch?v=ey4u7OUAF3c  Required domain


### TODO
- Maybe to check security.

### Client
- When admin edits News he stores the old Thumbnail path, all PDF paths, Id, Title, HTML body, Tags

### DB
- Thumbnail path -> stores the endpoint of the resource Ex: /thumbnail/something.png (not yet edited in the database)
- PDF paths -> stores the endpoints of the PDFs seperated by `;` Ex: /pdf/comment.pdf;/pdf/comment2.pdf (not yet edited in the database)
- (NewsForm) DeletePDFs -> stores the endpoints of the PDFs to be removed seperated by `;` Ex: /pdf/comment.pdf;/pdf/comment2.pdf

### Docs (endpoint -> explain)
- GET /news/id/{newsID} -> Ex: /news/id/1 this will get the news with that id
- GET /news/{page}/{amountPerPage} -> Ex: /news/1/5 this will return the latest news in a list of objects: [{id, title, thumbnail_base64, pdf_path, html_body, tags, posted_on_utc_timezored}]
- POST /news -> Will create a new news.
- POST /edit/news -> Will edit already existing news.
- POST /delete/news -> Will delete already existing news.
- POST /adminlogin -> returns status code 200 if loged in. otherwise it returns 401.
