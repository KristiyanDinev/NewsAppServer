### Dotnet
- dotnet add package Microsoft.Data.Sqlite.Core
- dotnet add package Microsoft.Data.Sqlite
- dotnet add package SQLite
- dotnet publish -c release -r [OS] --self-contained
 Options: https://learn.microsoft.com/en-us/dotnet/core/rid-catalog

### Resources
- To publish it to internet https://www.youtube.com/watch?v=ey4u7OUAF3c  Required domain


### TODO

### Client
- When admin edits or deletes News he stores the old Thumbnail path, all PDF paths, Id, Title, HTML body, Tags

### DB
- Thumbnail path -> stores the endpoint of the resource Ex: /thumbnail/something.png (not yet edited in the database)
- PDF paths -> stores the endpoints of the PDFs seperated by `;` Ex: /pdf/comment.pdf;/pdf/comment2.pdf (not yet edited in the database)
- (NewsForm) DeletePDFs -> stores the endpoints of the PDFs to be removed seperated by `;` Ex: /pdf/comment.pdf;/pdf/comment2.pdf
- This adds the password `1@#c4V5B6N7M8(0,(*mN76B5V4c3347E65R*^T&y^&r%6E4W5C3` to admin login `INSERT INTO Admins VALUES ('1@#c4V5B6N7M8(0,(*mN76B5V4c3347E65R*^T&y^&r%6E4W5C3');`

### Docs (endpoint -> explain)
- GET `/news/id/{newsID}` -> Ex: `/news/id/1` this will get the news with that id
- GET `/news/{page}/{amountPerPage}` -> Ex: `/news/1/5` this will return the latest news in a list of objects: [{id, title, thumbnail_endpoint, pdf_endpoint, html_body, tags, posted_on_utc_timezored}]
- POST `/news` -> Will create a new news.
- POST `/edit/news` -> Will edit already existing news.
- POST `/delete/news` -> Will delete already existing news.
- POST `/admin/login` -> returns status code 200 if loged in. otherwise it returns 401.
- POST `/admin/add` -> `adminPassword` - the admin password to add; `currentAdmin` - the admin's password who adds the new admin.
- POST `/admin/remove` -> `adminPassword` - the admin password to remove; `currentAdmin` - the admin's password who rmoves the admin.
- POST `/admin/edit` -> `oldAdminPassword` - the admin's old password; `currentAdmin` - the admin's password who changes the other admin's password; `newAdminPassword` - the other admin's new password