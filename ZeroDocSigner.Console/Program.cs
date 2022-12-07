using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using ZeroDocSigner.Api.Models;
using ZeroDocSigner.Common;
using ZeroDocSigner.Common.Algorithm;

//using var store = new X509Store();
//store.Open(OpenFlags.ReadOnly);
//using var certificate = store.Certificates.First();
//store.Close();

//var path = Path.Combine(Directory.GetCurrentDirectory(), "CV.pdf");
////Console.WriteLine(path);
//var manager = new SignatureManager(File.ReadAllBytes(path), certificate);

//var param = new SignatureParameters()
//{
//    HashAlgorithmName = HashAlgorithmName.SHA256,
//    SignatureAlgorithmName = SignatureAlgorithmName.RSA
//};

////manager.CreateSignature(param, true);
////manager.CreateSignature(param);
//Console.WriteLine(manager.Verify());

/*
var data = new SignModel()
{
    Force = false,
    Parameters = SignatureParameters.Default,
    Data = File.ReadAllBytes(@"C:\Users\dimat\source\repos\ZeroDocSigner\ZeroDocSigner.Console\bin\Debug\net7.0\CV.pdf")
};

var content = JsonContent.Create(data);

using var client = new HttpClient();

var response = await client.PostAsync("https://localhost:7008/signature/sign",
    content);

var d = await response.Content.ReadFromJsonAsync<byte[]>();

await File.WriteAllBytesAsync(@"C:\Users\dimat\source\repos\ZeroDocSigner\ZeroDocSigner.Console\bin\Debug\net7.0\CV_cp.pdf", d);
*/

var data = new BaseModel()
{
    Data = File.ReadAllBytes(@"C:\Users\dimat\source\repos\ZeroDocSigner\ZeroDocSigner.Console\bin\Debug\net7.0\CV_cp.pdf")
};

var content = JsonContent.Create(data);

using var client = new HttpClient();

var response = await client.PostAsync(
    "https://localhost:7008/signature/verify",
    content);

var d = await response.Content.ReadFromJsonAsync<bool>();

Console.WriteLine(d);

