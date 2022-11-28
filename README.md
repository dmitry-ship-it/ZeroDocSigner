# ZeroDocSigner

## Description
The purpose of this project is to create digital signature inside of doc files (any doc files).

### Tech stack
Tech selection (based on my experience, complexity and my own desire):
- Backend
  - ASP.NET Core
- Frontend (CLI)
  - .NET
  - C++
  - Rust
- Frontend (desktop)
  - .NET MAUI
  - .NET WPF
  - Electron (JS/TS)
  - C++/QT

### Key questions
- [ ] Should it be cross-platform?
- [ ] Should it be password protected?
- [ ] Is it OK if CLI app would be fat?
- [ ] Is it OK if desktop app would be fat?
- [ ] Is C# based app would be easier to maintain during production?
- [ ] Is C++ based app would be easier to maintain during production?
- [ ] Is JS/TS based app would be easier to maintain during production?

### File platforms & extensions which should be included into available-to-sign list
1. Old Microsoft Office files (binary):
**.doc, .xls, .ppt**
2. New Microsoft Office files (archive):
**.docx, .xlsx, .pptx**
3. OpenOffice files (archive):
**.odt, .ods, .odp**
4. PDF (binary + text):
**.pdf**

### Base digital signature algorithm
- [ ] RSA
- [ ] DSA
- [ ] ECDSA

### Signature type
- [ ] Invisible
- [ ] Visible for doc readers
- [ ] Visible for humans (placed on one of pages)