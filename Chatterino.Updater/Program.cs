﻿using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Chatterino.Updater
{
    class Program
    {
        private static bool update2 = false;

        static void Main(string[] args)
        {
            ZipConstants.DefaultCodePage = 850;

            Directory.SetCurrentDirectory(new FileInfo(Assembly.GetEntryAssembly().Location).Directory.FullName);

            if (File.Exists("../update2"))
            {
                update2 = true;
            }

            var options = new Options();
            CommandLine.Parser.Default.ParseArguments(args, options);

            Thread.Sleep(2000);

            var locked = false;

            try
            {
                if (File.Exists("..\\Chatterino.exe"))
                {
                    using (Stream stream = new FileStream("..\\Chatterino.exe", FileMode.Open))
                    {

                    }
                }
            }
            catch
            {
                locked = true;
            }

            if (locked)
            {
                Console.WriteLine("Make sure to close all instances of chatterino before updating.");
                Console.WriteLine("Press any key to continiue.");
                Console.ReadKey();
            }

            var exc =
                ExtractZipFile(
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Chatterino",
                        "update.zip"), null, "..");

            if (exc != null)
            {
                Console.WriteLine("Error: " + exc.Message);

                Console.ReadKey();
            }
            else
            {
                try
                {
                    if (update2)
                    {
                        File.Delete("../update2");
                    }
                    else
                    {
                        File.WriteAllText("../update2", "Don't delete this file, it could break the updating process!");
                    }
                }
                catch
                {
                }

                if (new FileInfo(Assembly.GetEntryAssembly().Location).Directory.FullName.Contains("Updater.new"))
                {
                    File.WriteAllText("../removeupdatenew", "do not delete this file");
                }
                else
                {
                    if (File.Exists("../removeupdatenew"))
                    {
                        try
                        {
                            Directory.Delete("../Updater.new", true);
                            File.Delete("../removeupdatenew");
                        }
                        catch
                        {
                        }
                    }
                }

                if (options.RestartChatterino)
                {
                    try
                    {
                        Process.Start(Path.Combine(new DirectoryInfo(new FileInfo(Assembly.GetEntryAssembly().Location).Directory.FullName).Parent.FullName, "Chatterino.exe"));
                    }
                    catch { }
                }
            }
        }

        static Exception ExtractZipFile(string archiveFilenameIn, string password, string outFolder)
        {
            ZipFile zf = null;
            try
            {
                var fs = File.OpenRead(archiveFilenameIn);
                zf = new ZipFile(fs);

                foreach (ZipEntry zipEntry in zf)
                {
                    if (!zipEntry.IsFile)
                    {
                        continue;
                    }
                    var entryFileName = zipEntry.Name;

                    var buffer = new byte[4096];
                    var zipStream = zf.GetInputStream(zipEntry);

                    if (!update2)
                    {
                        entryFileName = Regex.Replace(entryFileName, @"^Updater/", "Updater2/");
                    }

                    var fullZipToPath = Path.Combine(outFolder, entryFileName);
                    var directoryName = Path.GetDirectoryName(fullZipToPath);
                    if (directoryName.Length > 0)
                        Directory.CreateDirectory(directoryName);

                    using (var streamWriter = File.Create(fullZipToPath))
                    {
                        StreamUtils.Copy(zipStream, streamWriter, buffer);
                    }
                }
            }
            catch (Exception exc)
            {
                return exc;
            }
            finally
            {
                if (zf != null)
                {
                    zf.IsStreamOwner = true;
                    zf.Close();
                }
            }
            return null;
        }
    }
}
