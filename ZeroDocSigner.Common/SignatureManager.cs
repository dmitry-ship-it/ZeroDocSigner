using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using ZeroDocSigner.Common.Algorithm;

namespace ZeroDocSigner.Common
{
    public class SignatureManager : ISigner, IVerifier
    {
        private readonly FileInfo _file;
        private readonly X509Certificate2 _certificate;

        private readonly long _signaturesStart;

        public SignatureManager(
            FileInfo file,
            X509Certificate2 certificate)
        {
            if (!file.Exists)
            {
                throw new ArgumentException($"File {file.Name} does not exist.");
            }

            _file = file;
            _certificate = certificate;

            _signaturesStart = GetSignaturesPos();

            long GetSignaturesPos()
            {
                var start = SignatureInfo.StartSequence;
                using var file = _file.OpenRead();
                using var reader = new StreamReader(file);

                var read = 0L;
                var nextLine = string.Empty;
                while (!reader.EndOfStream)
                {
                    nextLine = reader.ReadLine();
                    read += nextLine.Length;
                    if (nextLine.Contains(start))
                    {
                        return read + start.Length + 1;
                    }
                }

                return -1;
            }
        }

        public bool FileContainsSignature => _signaturesStart != -1;

        public void AddSignature()
        {
            throw new NotImplementedException();
        }

        public void CreateSignature(
            SignatureParameters parameters,
            bool force = false)
        {
            if (!force && _signaturesStart != -1)
            {
                throw new ArgumentException("This file already contains signature(s).");
            }

            var fileData = GetFileData();
            var signatures = SignatureInfo.GetNewSignatureInfo(fileData, _certificate, parameters);

            var result = fileData.Concat(
                Encoding.Default.GetBytes(signatures.ToString()))
                    .ToArray();

            File.WriteAllBytes(_file.FullName, result);
        }

        public bool Verify()
        {
            if (_signaturesStart == -1)
            {
                throw new InvalidOperationException("Nothing to verify.");
            }

            var signatures = GetSignatures();
            return signatures.Signatures.Any(Verify);
        }

        public bool Verify(Signature signature)
        {
            var verifier = SignatureAlgorithm.Create(
                signature.Parameters,
                _certificate);

            var data = GetFileData();

            using var hasher = HashAlgorithm.Create(signature.Parameters.HashAlgorithmName.Name!)!;

            return verifier.VerifySignature(hasher.ComputeHash(data), signature);
        }

        private SignatureInfo? GetSignatures()
        {
            if (_signaturesStart == -1)
            {
                return null;
            }

            using var file = _file.OpenRead();
            file.Position = _signaturesStart;
            using var reader = new StreamReader(file);

            var content = reader.ReadToEnd();
            Console.WriteLine(content);
            return JsonSerializer.Deserialize<SignatureInfo>(content);
        }

        private byte[] GetFileData()
        {
            if (_signaturesStart == -1)
            {
                Console.WriteLine(File.ReadAllText(_file.FullName));
                return File.ReadAllBytes(_file.FullName);
            }

            using var reader = _file.OpenRead();
            var data = new byte[_signaturesStart - SignatureInfo.StartSequence.Length - 2];
            reader.ReadExactly(data);

            //Console.WriteLine(Encoding.Default.GetString(data));
            //Console.WriteLine("-----------------------------------");

            return data;
        }
    }
}
