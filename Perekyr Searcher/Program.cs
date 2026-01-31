using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PerekyrSearcher
{
    public class RecursiveSearcher
    {
        private static string _outPutFile = null;
        private static HashSet<string> _foundFiles;

        private static Dictionary<int, string> _functionsInfo = new Dictionary<int, string>()
        {
            {1, "Search one file" },
            {2, "Search files" },
            {3, "Search files and delete them" },
            {4, "Search files by extension" },
        };

        private static Dictionary<int, Action<string, string, string>> _functions = new Dictionary<int, Action<string, string, string>>()
        {
            {1,  (directory, fileName, withExt) => SearchOneFile(directory, fileName, withExt)},
            {2,  (directory, fileName, withExt) => SearchFiles(directory, fileName, withExt)},
            {3,  (directory, fileName, withExt) => SearchFilesAndDelete(directory, fileName, withExt)},
            {4,  (directory, fileName, withExt) => SearchFilesByExt(directory, fileName, withExt)},
        };

        static void Main(string[] args)
        {
            DriveInfo[] allDrives = DriveInfo.GetDrives();
            while (true)
            {
                _foundFiles = new HashSet<string>();

                ShowBanner();

                int functionNumber;

                Console.Write("\nEnter the command number: ");
                try { functionNumber = Int32.Parse(Console.ReadLine()); }
                catch
                {
                    Console.WriteLine("Please enter command number!  press any key to continue...");
                    Console.ReadKey(); Console.Clear(); continue;
                }

                if (functionNumber > _functions.Count || functionNumber < 1)
                {
                    Console.WriteLine("\nNo such function, press any key to continue...");
                    Console.ReadKey(); Console.Clear(); continue;
                }

                ClearAndShowBanner(_functionsInfo[functionNumber]);


                if (functionNumber == 4)
                {
                    string ext = null;
                    while (true)
                    {
                        ClearAndShowBanner(_functionsInfo[functionNumber]);
                        Console.Write("\nEnter extension (.txt .exe ...): ");
                        ext = Console.ReadLine();

                        if (ext.StartsWith(".")) break;
                        Console.WriteLine("\nPlease enter extension with '.' press any key to continue...");
                        Console.ReadKey(); continue;
                    }

                    int directory = -1;
                    GetAllDrives(allDrives, ref directory, functionNumber);

                    ClearAndShowBanner(_functionsInfo[functionNumber]);

                    Console.WriteLine($"[Searched extension: {ext}]");

                    Console.Write("\nFiles search by extension started, please wait...\n");
                    _functions[functionNumber](allDrives[directory].Name, string.Empty, ext);
                    if (_foundFiles.Count == 0) WriteRed($"\nFiles not found in {allDrives[directory]}");
                    else
                    {
                        ClearBufferKey();
                        while (true)
                        {
                            Console.Write($"\n\nFound files: {_foundFiles.Count}, save them? (Y/n): ");

                            if (Console.ReadLine().ToLower() == "y")
                            {
                                Console.Write("Enter file name for saving: ");
                                string savingFileName = Console.ReadLine();
                                File.WriteAllLines(savingFileName + ".txt", _foundFiles);
                                Console.WriteLine($@"Files saved in {AppDomain.CurrentDomain.BaseDirectory}{savingFileName}.txt");
                                break;
                            }
                            break;
                        }
                    }
                    ClearBufferKey();

                    Console.Write("\n\nPress any key to continue...");
                    Console.ReadKey();
                    Console.Clear();
                }
                else
                {
                    Console.Write("\nEnter file name (without extension): ");
                    string fileName = Console.ReadLine();

                    Console.Write("\nSearch with extension? [Y/n]: ");
                    string ext = null;
                    if (WithExtension(Console.ReadLine()))
                    {
                        while (true)
                        {
                            ClearAndShowBanner(_functionsInfo[functionNumber]);
                            Console.Write("\nEnter file extension (.txt .exe ...): ");
                            ext = Console.ReadLine();

                            if (ext.StartsWith(".")) break;
                            Console.WriteLine("\nPlease enter file extension with '.' press any key to continue...");
                            Console.ReadKey(); continue;
                        }
                    }

                    int directory = -1;
                    GetAllDrives(allDrives, ref directory, functionNumber);

                    ClearAndShowBanner(_functionsInfo[functionNumber]);

                    Console.WriteLine($"[Searched file: {fileName}{ext}]");

                    if (functionNumber == 1)
                    {
                        Console.Write("\nFile search started, please wait...\n");
                        _functions[functionNumber](allDrives[directory].Name, fileName, ext);
                        if (_outPutFile == null) WriteRed($"File not found in {allDrives[directory]}");
                    }
                    else
                    {
                        Console.Write("\nFiles search started, please wait...\n");
                        _functions[functionNumber](allDrives[directory].Name, fileName, ext);
                        if (_foundFiles.Count == 0) WriteRed($"Files not found in {allDrives[directory]}");
                        else
                        {
                            ClearBufferKey();
                            if (functionNumber != 3)
                            {
                                while (true)
                                {
                                    Console.Write($"\n\nFound files: {_foundFiles.Count}, save them? (Y/n): ");

                                    if (Console.ReadLine().ToLower() == "y")
                                    {
                                        Console.Write("Enter file name for saving: ");
                                        string savingFileName = Console.ReadLine();
                                        File.WriteAllLines(savingFileName + ".txt", _foundFiles);
                                        Console.WriteLine($@"Files saved in {AppDomain.CurrentDomain.BaseDirectory}{savingFileName}.txt");
                                        break;
                                    }
                                    break;
                                }
                            }

                        }
                    }
                    ClearBufferKey();

                    Console.Write("\n\nPress any key to continue...");
                    Console.ReadKey();
                    Console.Clear();
                }
            }
        }

        private static string SearchOneFile(string directory, string fileName, string ext)
        {
            if (_outPutFile != null) return _outPutFile;

            string[] files = Directory.GetFiles(directory);
            foreach (string file in Directory.EnumerateFiles(directory))
            {
                if (ext != null)
                {
                    if (Path.GetFileName(file) == fileName + ext)
                    {
                        WriteGreen($"File found: {file}");
                        return _outPutFile = file;
                    }
                }
                else
                {
                    if (Path.GetFileNameWithoutExtension(file) == fileName)
                    {
                        WriteGreen($"File found: {file}");
                        return _outPutFile = file;
                    }
                }
            }

            Directory.EnumerateDirectories(directory).AsParallel().ToList()
            .ForEach(dir =>
            {
                try
                {
                    _outPutFile = SearchOneFile(dir, fileName, ext);
                    return;
                }
                catch { }
            });

            return _outPutFile;
        }
        private static void SearchFiles(string directory, string fileName, string ext)
        {
            foreach (string file in Directory.EnumerateFiles(directory))
            {
                if (ext != null)
                {
                    if (Path.GetFileName(file) == fileName + ext)
                    {
                        WriteGreen($"File found: {file}");
                        _foundFiles.Add(file);
                    }
                }
                else
                {
                    if (Path.GetFileNameWithoutExtension(file) == fileName)
                    {
                        WriteGreen($"File found: {file}");
                        _foundFiles.Add(file);
                    }
                }
            }

            Directory.EnumerateDirectories(directory).AsParallel().ToList()
                .ForEach(dir =>
                {
                    try { SearchFiles(dir, fileName, ext); }
                    catch { }
                });
        }
        private static void SearchFilesAndDelete(string directory, string fileName, string ext)
        {
            foreach (string file in Directory.EnumerateFiles(directory))
            {
                if (ext != null)
                {
                    if (Path.GetFileName(file) == fileName + ext)
                    {
                        File.Delete(file);
                        WriteGreen($"File deleted: {file}");
                        _foundFiles.Add(file);
                    }
                }
                else
                {
                    if (Path.GetFileNameWithoutExtension(file) == fileName)
                    {
                        File.Delete(file);
                        WriteGreen($"File deleted: {file}");
                        _foundFiles.Add(file);
                    }
                }
            }

            Directory.EnumerateDirectories(directory).AsParallel().ToList()
                .ForEach(dir =>
                {
                    try { SearchFilesAndDelete(dir, fileName, ext); }
                    catch { }
                });
        }
        private static void SearchFilesByExt(string directory, string fileName, string ext)
        {
            foreach (string file in Directory.EnumerateFiles(directory))
            {
                if (Path.GetExtension(file) == ext)
                {
                    WriteGreen($"File found: {file}");
                    _foundFiles.Add(file);
                }
            }

            Directory.EnumerateDirectories(directory).AsParallel().ToList()
                .ForEach(dir =>
                {
                    try { SearchFilesByExt(dir, fileName, ext); }
                    catch { }
                });
        }

        private static void ShowBanner()
        {
            Console.WriteLine($"  ____               _                 ____                      _               \r\n |  _ \\ ___ _ __ ___| | ___   _ _ __  / ___|  ___  __ _ _ __ ___| |__   ___ _ __ \r\n | |_) / _ \\ '__/ _ \\ |/ / | | | '__| \\___ \\ / _ \\/ _` | '__/ __| '_ \\ / _ \\ '__|\r\n |  __/  __/ | |  __/   <| |_| | |     ___) |  __/ (_| | | | (__| | | |  __/ |   \r\n |_|   \\___|_|  \\___|_|\\_\\\\__, |_|    |____/ \\___|\\__,_|_|  \\___|_| |_|\\___|_|   \r\n                          |___/                                                  \n");
            ShowFunctions();
        }
        private static void ShowFunctions()
        {
            int i = 1;
            foreach (var func in _functionsInfo)
            {
                Console.WriteLine($"[{i}] - {func.Value}");
                i++;
            }
        }
        private static void ClearAndShowBanner(string function)
        {
            Console.Clear();
            Console.WriteLine($"  ____               _                 ____                      _               \r\n |  _ \\ ___ _ __ ___| | ___   _ _ __  / ___|  ___  __ _ _ __ ___| |__   ___ _ __ \r\n | |_) / _ \\ '__/ _ \\ |/ / | | | '__| \\___ \\ / _ \\/ _` | '__/ __| '_ \\ / _ \\ '__|\r\n |  __/  __/ | |  __/   <| |_| | |     ___) |  __/ (_| | | | (__| | | |  __/ |   \r\n |_|   \\___|_|  \\___|_|\\_\\\\__, |_|    |____/ \\___|\\__,_|_|  \\___|_| |_|\\___|_|   \r\n                          |___/                                                  \n");
            Console.WriteLine($"[Select function: {function}]");
        }
        private static void ClearBufferKey()
        {
            while (Console.KeyAvailable) // Очистка буфера от прошлых нажатий
            {
                Console.ReadKey(true);
            }
        }
        private static void GetAllDrives(DriveInfo[] drives, ref int dir, int funcNumber)
        {
            while (true)
            {
                ClearAndShowBanner(_functionsInfo[funcNumber]);

                Console.WriteLine("\nList of disks:");
                for (int i = 0; i < drives.Length; i++) Console.WriteLine($"[{i + 1}] - {drives[i].Name}");

                Console.Write("\nEnter start directory: ");
                try { dir = Int32.Parse(Console.ReadLine()) - 1; }
                catch
                {
                    Console.WriteLine("\nPlease enter number of disk, press any key to continue...");
                    Console.ReadKey(); continue;
                }

                if (dir + 1 < 1 || dir > drives.Length - 1)
                {
                    Console.WriteLine("\nNo such disk, press any key to continue...");
                    Console.ReadKey(); continue;
                }

                break;
            }
        }

        private static void WriteGreen(string text)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("\n" + text);
            Console.ForegroundColor = ConsoleColor.White;
        }
        private static void WriteRed(string text)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("\n" + text);
            Console.ForegroundColor = ConsoleColor.White;
        }

        private static bool WithExtension(string input) => input.ToLower() == "y" ? true : false;
    }
}
