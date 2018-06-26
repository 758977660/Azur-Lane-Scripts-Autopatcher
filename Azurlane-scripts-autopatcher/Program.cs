﻿using System;
using System.IO;
using System.Windows.Forms;

namespace Azurlane
{
    internal enum Tasks
    {
        Encrypt,
        Decrypt,
        Decompile,
        Recompile,
        Unpack,
        Repack
    }

    internal class Program
    {
        internal static int ExceptionCount = 0;

        [STAThread]
        private static void Main(string[] args)
        {
            if (args.Length > 1)
            {
                Console.WriteLine(@"Invalid argument, usage: Azurlane.exe <path-to-scripts>");
                return;
            }
            else if (args.Length < 1)
            {
                using (var openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.Title = @"Open a scripts file...";
                    openFileDialog.Filter = @"Azurlane AssetBundle|scripts*";
                    openFileDialog.CheckFileExists = true;
                    openFileDialog.Multiselect = true;
                    openFileDialog.ShowDialog();

                    if (File.Exists(openFileDialog.FileName))
                        args = new[] { openFileDialog.FileName };
                    else
                    {
                        Console.WriteLine(@"Please open a scripts file...");
                        Console.ReadKey();
                        return;
                    }
                }
            }

            if (!File.Exists(args[0]))
            {
                Console.WriteLine(Directory.Exists(args[0]) ? $"{args[0]} is a directory, please input a file..." : $"{args[0]} doesn't exists...");
                return;
            }

            var filePath = Path.GetDirectoryName(args[0]);
            var fileName = Path.GetFileName(args[0]);

            var listOfLua = new string[]
            {
                "aircraft_template.lua.txt",
                "enemy_data_statistics.lua.txt",
                "weapon_property.lua.txt"
            };

            var listOfMod = new string[]
            {
                "weakenemy",
                "godmode",
                "godmode-cd",
                "godmode-dmg",
                "godmode-dmg-cd",
                "godmode-weakenemy"
            };

            for (var i = 0; i < listOfMod.Length; i++)
                listOfMod[i] = string.Format("{0}-{1}", fileName, listOfMod[i]);

            if (File.Exists(PathMgr.Temp(fileName)))
                File.Delete(PathMgr.Temp(fileName));

            if (Directory.Exists(Path.Combine(PathMgr.Temp("Unity_Assets_Files"), fileName)))
                Utils.DeleteDirectory(Path.Combine(PathMgr.Temp("Unity_Assets_Files"), fileName));

            foreach (var mod in listOfMod)
            {
                if (File.Exists(PathMgr.Temp(mod)))
                    File.Delete(PathMgr.Temp(mod));
                if (Directory.Exists(Path.Combine(PathMgr.Temp("Unity_Assets_Files"), mod)))
                    Utils.DeleteDirectory(Path.Combine(PathMgr.Temp("Unity_Assets_Files"), mod));
            }

            try
            {
                var index = 1;

                Console.Write("[+] Copying AssetBundle to temp workspace...");
                File.Copy(args[0], PathMgr.Temp(fileName), true);
                Console.Write(" <done>");
                Console.WriteLine();

                Console.Write("[+] Decrypting and Unpacking AssetBundle...");
                AssetBundle.Run(PathMgr.Temp(fileName), Tasks.Decrypt);
                AssetBundle.Run(PathMgr.Temp(fileName), Tasks.Unpack);
                Console.Write(" <done>");
                Console.WriteLine();

                Console.Write($"[+] Decrypting and Decompiling Lua...");
                foreach (var lua in listOfLua)
                {
                    Console.Write($" {index}/{listOfLua.Length}");

                    Lua.Run(PathMgr.Lua(fileName, lua), Tasks.Decrypt);
                    Lua.Run(PathMgr.Lua(fileName, lua), Tasks.Decompile);

                    if (index == listOfLua.Length)
                        Console.Write(" <done>");

                    index++;
                }
                Console.WriteLine();
                index = 1;

                Console.Write($"[+] Cloning Lua and AssetBundle...");
                foreach (var mod in listOfMod)
                {
                    Console.Write($" {index}/{listOfMod.Length}");

                    try
                    {
                        if (!Directory.Exists(PathMgr.Lua(mod)))
                            Directory.CreateDirectory(PathMgr.Lua(mod));

                        foreach (var lua in listOfLua)
                            File.Copy(PathMgr.Lua(fileName, lua), PathMgr.Lua(mod, lua), true);
                    }
                    catch (Exception e)
                    {
                        Utils.ExceptionLogger("Exception detected during cloning lua and assetbundle", e);
                    }
                    File.Copy(PathMgr.Temp(fileName), PathMgr.Temp(mod), true);

                    if (index == listOfMod.Length)
                        Console.Write(" <done>");

                    index++;
                }
                Console.WriteLine();
                index = 1;

                Console.Write("[+] Rewriting Lua...");
                foreach (var mod in listOfMod)
                {
                    Console.Write($" {index}/{listOfMod.Length}");

                    foreach (var lua in listOfLua)
                    {
                        if (mod.Contains("godmode-weakenemy"))
                        {
                            Write.GodMode(PathMgr.Lua(mod, lua));
                            Write.WeakEnemy(PathMgr.Lua(mod, lua), 20);
                        }
                        else if (mod.Contains("godmode-dmg-cd"))
                        {
                            Write.GodMode(PathMgr.Lua(mod, lua));
                            Write.Damage(PathMgr.Lua(mod, lua), 325);
                            Write.Cooldown(PathMgr.Lua(mod, lua), 100);
                        }
                        else if (mod.Contains("godmode-dmg"))
                        {
                            Write.GodMode(PathMgr.Lua(mod, lua));
                            Write.Damage(PathMgr.Lua(mod, lua), 325);
                        }
                        else if (mod.Contains("godmode-cd"))
                        {
                            Write.GodMode(PathMgr.Lua(mod, lua));
                            Write.Cooldown(PathMgr.Lua(mod, lua), 100);
                        }
                        else if (mod.Contains("godmode"))
                        {
                            Write.GodMode(PathMgr.Lua(mod, lua));
                        }
                        else if (mod.Contains("weakenemy"))
                        {
                            Write.WeakEnemy(PathMgr.Lua(mod, lua), 10);
                        }
                    }
                    if (index == listOfMod.Length)
                        Console.Write(" <done>");

                    index++;
                }
                Console.WriteLine();
                index = 1;

                Console.Write("[+] Recompiling and Encypting Lua...");
                foreach (var mod in listOfMod)
                {
                    Console.Write($" {index}/{listOfMod.Length}");

                    foreach (var lua in listOfLua)
                    {
                        Lua.Run(PathMgr.Lua(mod, lua), Tasks.Recompile);
                        Lua.Run(PathMgr.Lua(mod, lua), Tasks.Encrypt);
                    }

                    if (index == listOfMod.Length)
                        Console.Write(" <done>");

                    index++;
                }
                Console.WriteLine();
                index = 1;

                Console.Write("[+] Repacking & encrypting assetbundle...");
                foreach (var mod in listOfMod)
                {
                    Console.Write($" {index}/{listOfMod.Length}");

                    AssetBundle.Run(PathMgr.Temp(mod), Tasks.Repack);
                    AssetBundle.Run(PathMgr.Temp(mod), Tasks.Encrypt);

                    if (index == listOfMod.Length)
                        Console.Write(" <done>");

                    index++;
                }
                Console.WriteLine();
                index = 1;

                Console.Write("[+] Cleaning...");
                foreach (var mod in listOfMod)
                {
                    if (File.Exists(Path.Combine(filePath, mod)))
                        File.Delete(Path.Combine(filePath, mod));

                    File.Copy(PathMgr.Temp(mod), Path.Combine(filePath, mod), true);
                }
                if (File.Exists(PathMgr.Temp(fileName)))
                    File.Delete(PathMgr.Temp(fileName));
                if (Directory.Exists(Path.Combine(PathMgr.Temp("Unity_Assets_Files"), fileName)))
                    Utils.DeleteDirectory(Path.Combine(PathMgr.Temp("Unity_Assets_Files"), fileName));

                foreach (var mod in listOfMod)
                {
                    if (File.Exists(PathMgr.Temp(mod)))
                        File.Delete(PathMgr.Temp(mod));
                    if (Directory.Exists(Path.Combine(PathMgr.Temp("Unity_Assets_Files"), mod)))
                        Utils.DeleteDirectory(Path.Combine(PathMgr.Temp("Unity_Assets_Files"), mod));
                }
                Console.Write(" <done>\n");
                Console.WriteLine();
            }
            finally
            {
                if (ExceptionCount != 0)
                    Console.WriteLine("Exception Detected, please check Logs.txt");

                Console.WriteLine("Press any key to exit.");
                Console.ReadLine();
            }
        }
    }
}