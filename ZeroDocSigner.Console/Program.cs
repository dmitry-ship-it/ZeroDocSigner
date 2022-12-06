using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using ZeroDocSigner.Common;
using ZeroDocSigner.Common.Algorithm;

//using var rsa = RSA.Create();

//var data = new byte[] { 0x21, 0x54, 0xAB, 0x64 };
//var hash = SHA256.HashData(data);

//var sigInfo = new SignatureInfo()
//{
//    Signatures = new[]
//    {
//        new Signature()
//        {
//            Sequence = rsa.SignHash(hash, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1)
//            ,
//            HashAlgorithmName = HashAlgorithmName.SHA256,
//            SignatureAlgorithmName = SignatureAlgorithmName.RSA
//        }
//    },
//};

//var opt = new JsonSerializerOptions
//{
//    WriteIndented = true
//};

//var str = JsonSerializer.Serialize(sigInfo, opt);
//Console.WriteLine(str);
//Console.WriteLine();

//var obj = JsonSerializer.Deserialize<SignatureInfo>(str)!;
//Console.WriteLine(obj.Signatures[0].SignatureAlgorithmName);
//Console.WriteLine(obj.Signatures[0].HashAlgorithmName);

using var store = new X509Store();
store.Open(OpenFlags.ReadOnly);
using var certificate = store.Certificates.First();
store.Close();

var path = Path.Combine(Directory.GetCurrentDirectory(), "test.txt");
//Console.WriteLine(path);
var manager = new SignatureManager(new FileInfo(path), certificate);

var param = new SignatureParameters()
{
    HashAlgorithmName = HashAlgorithmName.SHA256,
    SignatureAlgorithmName = SignatureAlgorithmName.RSA
};

manager.CreateSignature(param, true);
Console.WriteLine(manager.Verify());
