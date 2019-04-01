using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;

namespace VSUpdater
{
    internal class Program
    {
        internal static void Main(string directory, string[] exclude)
        {
            directory ??= @"C:\Program Files (x86)\Microsoft Visual Studio";
            exclude ??= Array.Empty<string>();

            Console.WriteLine($"Updating all VS installs under {directory}.");

            var count = 0;
            foreach (var dir in Directory.GetDirectories(directory))
            {
                if (TryUpdate(dir, exclude))
                {
                    count++;
                    continue;
                }

                // we look one more directory down because the default structure is Year\Edition
                foreach (var subDir in Directory.GetDirectories(dir))
                {
                    if (TryUpdate(subDir, exclude))
                    {
                        count++;
                    }
                }
            }

            if (count == 0)
            {
                Console.WriteLine("Couldn't find any VS installs to update.");
            }
            else
            {
                Console.WriteLine($"Updated {count} VS installs.");
            }
        }

        private static bool TryUpdate(string directory, string[] exclude )
        {
            if (File.Exists(Path.Combine(directory, "Common7", "IDE", "devenv.exe")))
            {
                if (exclude.Any(e => Path.GetFileName(directory).Equals(e, StringComparison.OrdinalIgnoreCase)))
                {
                    Console.WriteLine($"Skipping {directory} because it's in the exclude list.");
                    return false;
                }

                UpdateVS(directory);
                return true;
            }

            Console.WriteLine($"Skipping {directory} because it does not appear to be a VS install.");
            return false;
        }

        private static void UpdateVS(string dir)
        {
            Console.WriteLine($"Updating {dir}.");

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = @"C:\Program Files (x86)\Microsoft Visual Studio\Installer\vs_installer.exe",
                    Arguments = $@"update --quiet --norestart --force --installWhileDownloading --installpath ""{dir}""",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using (var process = new Process())
                {
                    process.StartInfo = psi;
                    process.OutputDataReceived += new DataReceivedEventHandler(StdOutReceived);
                    process.ErrorDataReceived += new DataReceivedEventHandler(StdErrReceived);
                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    process.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: " + ex.Message);
                Console.ResetColor();
            }
        }

        private static void StdOutReceived(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine(e.Data);
        }
        
        private static void StdErrReceived(object sender, DataReceivedEventArgs e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(e.Data);
            Console.ResetColor();
        }

    }
}
