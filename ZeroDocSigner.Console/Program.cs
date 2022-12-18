using System.Diagnostics;
using ZeroDocSigner.Api.Models;

namespace ZeroDocSigner.Cli
{
    public static class Program
    {
        static void PrintInfo()
        {
            Console.WriteLine();
            Console.WriteLine("-s or --sign to create digitally sign file");
            Console.WriteLine("-f or --force to override existing digital signature inside file");
            Console.WriteLine("-v or --verify to verify signed file");
            Console.WriteLine();
        }

        static async Task Execute(string[] args)
        {
            var commands = new ConsoleCommands(args);
            var path = commands.FileInfo.FullName;
            var data = File.ReadAllBytes(path);
            using var client = new Client("http://localhost:5000");

            if (commands.IsSign)
            {
                var signModel = new SignModel
                {
                    Data = data,
                    Force = commands.IsForce
                };

                var signed = await client.SignAsync(signModel);

                var modifiedPathPart = commands.FileInfo
                    .Extension
                    .Replace(".", "_signed.");

                File.WriteAllBytes(
                    path.Replace(
                        commands.FileInfo.Extension,
                        modifiedPathPart),
                    signed);

                Console.WriteLine("Done");
            }
            else if (commands.IsVerify)
            {
                var dataModel = new DataModel
                {
                    Data = data
                };

                if (await client.VerifyAsync(dataModel))
                {
                    Console.WriteLine("OK");
                }
                else
                {
                    Console.WriteLine("Signature invalid or not found");
                }
            }
            else
            {
                Console.WriteLine("Unknown command");
            }
        }

        static async Task Main(string[] args)
        {
            try
            {
                await Execute(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);
                PrintInfo();
            }
        }
    }
}
