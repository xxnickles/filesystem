using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace FileSystemTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var app = new Program();
            app.CreateConfig();
            app.ReadConfig();
            app.CreateDirectory();
            app.CreateFile();
            Console.Read();
            app.ArchiveConfig();
            app.SaveImage();
            app.CopySavedData();
            app.DeleteTmp();            
        }

        private readonly FileSystem _filesystem;
        public Program()
        {
            _filesystem = new FileSystem();
        }

        public string[] folders =
        {
            @"Workspace\",
            @"Workspace\Archive\",
            @"Workspace\Tmp\",
            @"Workspace\Tmp\SaveData\"
        };

        public enum FolderNames
        {
            Workspace,
            Archive,
            Tmp,
            SaveData
        }

        public string configFile => GetUserDataFolder() + "Config.txt";


        public void CreateDirectory()
        {
            var total = folders.Length;
            for (int i = 0; i < total; i++)
            {
                var dirName = GetFolderByName((FolderNames)i);
                // Creates and overwrites
                if (_filesystem.DirectoryExist(dirName))
                {
                    Console.WriteLine($"Dir {dirName} exists");
                }
                else
                {
                    _filesystem.CreateDirectory(dirName);
                    Console.WriteLine($"Create Dir {dirName}");
                }
            }           
        }

        public void DeleteTmp()
        {
            var tmpDir = GetFolderByName(FolderNames.Tmp);
            if (_filesystem.DirectoryExist(tmpDir))
            {
                _filesystem.DeleteDirectory(tmpDir);
            }            
        }

        public void CopySavedData()
        {
            var saveDataDir = GetFolderByName(FolderNames.SaveData);
            if(_filesystem.DirectoryExist(saveDataDir))
            {
                var destDirName = GetFolderByName(FolderNames.Archive);
                destDirName += _filesystem.GetFileName(saveDataDir.TrimEnd(_filesystem.DirectorySeparatorChar));
                destDirName += "_" + DateTime.Now.ToString("yyyyMMddHHmmss");
                _filesystem.MoveDirectory(saveDataDir, destDirName);
            }
        }

        public string GetFolderByName(FolderNames name)
        {
            return GetUserDataFolder() + folders[(int)name];
        }

        public void CreateFile()
        {
            var path = GetFolderByName(FolderNames.SaveData) + "TestFile.txt";
            _filesystem.WriteAllText(path, "Hello World");

            var fileInfo = new FileInfo(path);
            var name = _filesystem.GetFileNameWithoutExtension(fileInfo.FullName);
            var ext = fileInfo.Extension;
            var size = fileInfo.Length;

            Console.WriteLine($"Created file {name} with ext {ext} with a size of {size} bytes");
        }

        public void CreateConfig()
        {
            if (!_filesystem.FileExists(configFile))
            {
                _filesystem.WriteAllLines(configFile, folders);
            }            
        }

        public void ReadConfig()
        {
            var lines = _filesystem.ReadAllLines(configFile);
            var total = lines.Length;
            Array.Resize(ref folders, total);
            for (int i = 0; i < total; i++)
            {
                var pathString = lines[i];
                Console.WriteLine($"Setting path - {pathString}");
                folders[i] = pathString;
            }
        }

        public void ArchiveConfig()
        {
            var configPath = configFile;
            var configName = _filesystem.GetFileName(configPath);
            var tmpPath = GetFolderByName(FolderNames.Tmp) + configName;
            var newPath = GetFolderByName(FolderNames.SaveData) + configName;

            _filesystem.CopyFile(configPath, tmpPath);
            var lines = _filesystem.ReadAllLines(tmpPath);
            var configString = string.Join(Environment.NewLine, lines);
            var workspaceDirName = _filesystem.GetDirectoryName(GetFolderByName(FolderNames.Workspace));
            var newWorkspaceDirName = workspaceDirName + DateTime.Now.ToString("yyyyMMddHHmmss");

            configString = configString.Replace(workspaceDirName, newWorkspaceDirName);

            _filesystem.WriteAllText(tmpPath, configString);

            _filesystem.MoveFile(tmpPath, newPath);
        }

        public void SaveImage()
        {
            var imageFileName = GetFolderByName(FolderNames.SaveData) +"Image.jpg";
            var bitmap = new Bitmap(128, 128, PixelFormat.Format24bppRgb);
            var g = Graphics.FromImage(bitmap);
            g.Clear(Color.Magenta);

            bitmap.Save(imageFileName, ImageFormat.Jpeg);
        }

        public string GetUserDataFolder()
        {
            var dir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            dir += @"\FileSystemTest\";

            if (!_filesystem.DirectoryExist(dir))
            {
                _filesystem.CreateDirectory(dir);
            }

            return dir;
        }
    }
}
