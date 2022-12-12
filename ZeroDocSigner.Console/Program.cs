using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using ZeroDocSigner.Common.Algorithm;
using ZeroDocSigner.Common.Manager;

using var store = new X509Store();
store.Open(OpenFlags.ReadOnly);
using var certificate = store.Certificates.First();
store.Close();

var data = File.ReadAllBytes(@"C:\Users\dimat\source\repos\ZeroDocSigner\ZeroDocSigner.Console\bin\Debug\net7.0\;ko_signed_really.docx");

var manager = new SignatureManager(data, DocumentType.Archive, certificate);

var param = new SignatureParameters()
{
    HashAlgorithmName = HashAlgorithmName.SHA256,
    SignatureAlgorithmName = SignatureAlgorithmName.RSA
};

//manager.CreateSignature(param, true);
manager.AddSignature(param);
Console.WriteLine(manager.Verify());
//var bt = manager.BuildFile();
//Console.WriteLine(bt.Length);
//File.WriteAllBytes(@"C:\Users\dimat\source\repos\ZeroDocSigner\ZeroDocSigner.Console\bin\Debug\net7.0\;ko_signed_really.docx", bt);
