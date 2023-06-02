# ZeroDocSigner

## Description

The purpose of this project is to create digital signature inside of doc files (any doc files).

### Tech stack

- Backend - **ASP.NET Core API**

### Supported document types

| Document Type             | Extensions          | Description                           |
| ------------------------- | ------------------- | ------------------------------------- |
| Microsoft Office (legacy) | .doc, .xls, .ppt    | Supported only by custom algorithm    |
| Microsoft Office          | .docx, .xlsx, .pptx | Fully supported                       |
| OpenDocument              | .odt, .ods, .odp    | Fully supported                       |
| PDF                       | .pdf                | Fully supported                       |
| Other                     | .\*                 | *Maybe* supported by custom algorithm |

### Base digital signature algorithm

- **RSA**

### Signature type

- **Invisible**

## Startup

1. Download [.NET SDK 7](https://dotnet.microsoft.com/en-us/download)
2. Clone repository with `git clone https://github.com/dmitry-ship-it/ZeroDocSigner`
3. Go to main API folder `cd ZeroDocSigner/ZeroDocSigner.Api`
4. Run the application `dotnet run -c Release`

| ⚠️ This application uses Active Directory authentication, so it is does not supported on any OS except Windows |
| -------------------------------------------------------------------------------------------------------------- |
