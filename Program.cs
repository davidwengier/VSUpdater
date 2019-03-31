﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace VSUpdater
{
    class Program
    {
        const string VSInstallerLocation = @"C:\Program Files (x86)\Microsoft Visual Studio\Installer\vs_installer.exe";
        const string VSInstallerArgs = "update --quiet --norestart --force --installWhileDownloading --installpath ";
        static void Main(string directory, string[] exclude)
        {
            directory ??= Environment.CurrentDirectory;
            exclude ??= Array.Empty<string>();

            Console.WriteLine($"Updating all VS installs under {directory}.");

            var count = 0;
            foreach (var dir in Directory.GetDirectories(directory))
            {
                if (exclude.Any(e => Path.GetFileName(dir).Equals(e, StringComparison.OrdinalIgnoreCase)))
                {
                    Console.WriteLine($"Skipping {dir} because it's in the exclude list.");
                    continue;
                }
                
                // first lets make sure this is a VS install
                if (!File.Exists(Path.Combine(dir, "Common7", "IDE", "devenv.exe")))
                {
                    Console.WriteLine($"Skipping {dir} because it does not appear to be a VS install.");
                    continue;
                }

                UpdateVS(dir);
                count++;
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

        private static void UpdateVS(string dir)
        {
            Console.WriteLine($"Updating {dir}.");

            var psi = new ProcessStartInfo
            {
                FileName = VSInstallerLocation,
                Arguments = VSInstallerArgs + dir,
                UseShellExecute = false,
                RedirectStandardOutput = true,
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

        private static void StdOutReceived(object sender, DataReceivedEventArgs e)
        {
            Console.ResetColor();
            Console.WriteLine(e.Data);
        }
        private static void StdErrReceived(object sender, DataReceivedEventArgs e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(e.Data);
        }

    }
}
