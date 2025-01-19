### Dotnet
- dotnet add package Microsoft.Data.Sqlite.Core
- dotnet add package Microsoft.Data.Sqlite
- dotnet add package SQLite
- dotnet add Microsoft.AspNetCore.Authentication.Certificate
- dotnet publish -c release -r [OS] --self-contained
 Options: https://learn.microsoft.com/en-us/dotnet/core/rid-catalog

### Resources
- To publish it to internet https://www.youtube.com/watch?v=ey4u7OUAF3c  Required domain
- It requires Git bash. For getting the certificate for https: `openssl genrsa -out private.key 1024
openssl req -new -x509 -key private.key -out publickey.cer -days 365
openssl pkcs12 -export -out public_privatekey.pfx -inkey private.key -in publickey.cer`


### Client
- When admin edits or deletes News he stores the old Thumbnail path, all PDF paths, Id, Title, HTML body, Tags

### DB
- Thumbnail path -> stores the endpoint of the resource Ex: /thumbnail/something.png (not yet edited in the database)
- PDF paths -> stores the endpoints of the PDFs seperated by `;` Ex: /pdf/comment.pdf;/pdf/comment2.pdf (not yet edited in the database)
- (NewsForm) DeletePDFs -> stores the endpoints of the PDFs to be removed seperated by `;` Ex: /pdf/comment.pdf;/pdf/comment2.pdf
- Default system admin password: `UybRuyibINbvcyrteTYCRTUVYIugcxtETYCRTUVigYCYR`
- Rules for passwords: No `&`, `=`, `\`

### Docs (endpoint -> explain)
- GET `/news/id/{newsID}` -> Ex: `/news/id/1` this will get the news with that id
- GET `/news/{page}/{amountPerPage}` -> Ex: `/news/1/5` this will return the latest news: {"News": [{id, title, thumbnail_endpoint, pdf_endpoint, html_body, tags, posted_on_utc_timezored}]}
- POST `/news/search` -> body data: `search` and `tags` for the this will return a map with one key that points to a list of all News that contain any of the search in the title or any or filter in the tags.
- POST `/news` -> Will create a new news.
- POST `/news/edit` -> Will edit already existing news.
- POST `/news/delete` -> Will delete already existing news.
- POST `/admin/login` -> `adminPassword` - the password to login; returns status code 200 if loged in. otherwise it returns 401.
- POST `/admin/add` -> `adminPassword` - the admin password to add; `currentAdmin` - the admin's password who adds the new admin.
- POST `/admin/remove` -> `adminPassword` - the admin password to remove; `currentAdmin` - the admin's password who rmoves the admin.
- POST `/admin/edit` -> `oldAdminPassword` - the admin's old password; `currentAdmin` - the admin's password who changes the other admin's password; `newAdminPassword` - the other admin's new password