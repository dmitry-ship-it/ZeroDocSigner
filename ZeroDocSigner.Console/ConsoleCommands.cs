using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZeroDocSigner.Cli
{
    internal class ConsoleCommands
    {
        public bool IsSign { get; private set; }
        public bool IsVerify { get; private set; }
        public bool IsForce { get; private set; }
        public FileInfo FileInfo { get; private set; } = null!;

        public ConsoleCommands(string[] args)
        {
            SetCommand(args);
            SetForce(args);
            SetFileInfo(args);
        }

        private void SetCommand(string[] args)
        {
            var signCommand = args.SingleOrDefault(a => a.Contains(
                "-s", StringComparison.CurrentCultureIgnoreCase));

            var verifyCommand = args.SingleOrDefault(a => a.Contains(
                "-v", StringComparison.CurrentCultureIgnoreCase));

            if (signCommand is null && verifyCommand is null)
            {
                throw new ArgumentException("Please, pick one of actions", nameof(args));
            }

            CheckCommand(signCommand, "-s", "--sign");
            CheckCommand(verifyCommand, "-v", "--verify");

            if (signCommand is not null && verifyCommand is not null)
            {
                throw new ArgumentException(
                    $"Please, pick only one of this options ({signCommand.ToLower()} or {verifyCommand.ToLower()})",
                    nameof(args));
            }

            if (signCommand is null)
            {
                IsVerify = true;
            }
            else
            {
                IsSign = true;
            }
        }

        private void SetForce(string[] args)
        {
            var force = args.SingleOrDefault(a => a.Equals("-f"));
            if (force is null)
            {
                IsForce = false;
            }

            CheckCommand(force, "-f", "--force");
        }

        private void SetFileInfo(string[] args)
        {
            var path = args.SingleOrDefault(a => !a.Contains('-'));

            if (path is null)
            {
                throw new FileNotFoundException(path, nameof(args));
            }

            var dir = Directory.GetCurrentDirectory();

            FileInfo = new(Path.Combine(dir, path));
        }

        private static void CheckCommand(string? command, params string[] expected)
        {
            const string exceptionMessage = "Unknown command \"{0}\"";
            if (command is not null && expected.All(c => !c.Equals(
                command, StringComparison.CurrentCultureIgnoreCase)))
            {
                throw new ArgumentException(
                    string.Format(exceptionMessage, command.ToLower()),
                    nameof(command));
            }
        }
    }
}
