using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using ZeroDocSigner.Common;
using ZeroDocSigner.Common.Algorithm;

using var store = new X509Store();
store.Open(OpenFlags.ReadOnly);
using var certificate = store.Certificates.First();
store.Close();

var path = Path.Combine(Directory.GetCurrentDirectory(), "CV.pdf");
//Console.WriteLine(path);
var manager = new SignatureManager(new FileInfo(path), certificate);

var param = new SignatureParameters()
{
    HashAlgorithmName = HashAlgorithmName.SHA256,
    SignatureAlgorithmName = SignatureAlgorithmName.RSA
};

//manager.CreateSignature(param, true);
//manager.CreateSignature(param);
Console.WriteLine(manager.Verify());
