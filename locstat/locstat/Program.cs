﻿namespace locstat
{
    public class LocStatHandler
    {
        private long totalLines = 0;
        private Dictionary<string, long> extensionData;

        public LocStatHandler()
        {
            this.extensionData = new Dictionary<string, long>();
        }

        private void Log(string msg)
        {
            Console.WriteLine(msg);
        }

        public void HandlePath(string path)
        {
            Log($"Running LocStat on path: {path}");

            bool isDirectory = Directory.Exists(path);
            bool isFile = File.Exists(path);

            if (isDirectory)
            {
                HandleDirectory(path);
            }
            else
            if (isFile)
            {
                HandleFile(path);
            }
            else
            {
                throw new Exception("The specified path does not exist!");
            }

            Log($"Total Lines Counted: {totalLines}");
            Log("Lines Per Language:");
            foreach (var entry in extensionData)
            {
                Log($"[\"{entry.Key}\"] : {entry.Value} lines");
            }
        }

        public int HandlePath(string path)
        {
            Dictionary<string, int> extensionData = new();

            DirectoryInfo directory = new DirectoryInfo(path);
            Console.WriteLine($"Running locstat on path : \"{directory.FullName}\"");
            int totalLinesCounted = HandleDir(directory, extensionData);
            Console.WriteLine($"Total lines counted : {totalLinesCounted}");
            foreach (var entry in extensionData)
                Console.WriteLine($"LOC[\"{entry.Key}\"] : {entry.Value} lines");
            return totalLinesCounted;
        }

        public void HandleDirectory(string path)
        {
            DirectoryInfo dir = new DirectoryInfo(path);
            HandleDirectory(dir, null); // TODO : FIXME! don't pass null
        }

        public int HandleDirectory(DirectoryInfo directory, Dictionary<string, int> extensionData)
        {
            Log($"Handling directory: \"{directory.FullName}\"");

            int lineCount = 0;

            FileInfo[] files = directory.GetFiles();
            foreach (var file in files)
                lineCount += HandleFile(file, extensionData);

            DirectoryInfo[] dirs = directory.GetDirectories();
            foreach (var child in dirs)
                lineCount += HandleDirectory(child, extensionData);

            return lineCount;
        }

        public void HandleFile(string path)
        {
            FileInfo fileInfo = new FileInfo(path);
            HandleFile(fileInfo, null); // TODO : FIXME!
        }

        

        public int HandleDir(DirectoryInfo directory, Dictionary<string, int> extensionData)
        {
            int lineCount = 0;

            FileInfo[] files = directory.GetFiles();
            foreach (var file in files)
                lineCount += HandleFile(file, extensionData);

            DirectoryInfo[] dirs = directory.GetDirectories();
            foreach (var child in dirs)
                lineCount += HandleDir(child, extensionData);

            return lineCount;
        }

        public int HandleFile(FileInfo fileInfo, Dictionary<string, int> extensionData)
        {
            Log($"Handling file: \"{fileInfo.FullName}\"");

            int lineCount = 0;

            try
            {
                using (FileStream file = fileInfo.Open(FileMode.Open, FileAccess.Read))
                using (TextReader reader = new StreamReader(file))
                {
                    string? line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        ++lineCount;
                    }
                }
                Console.WriteLine($"[File \"{fileInfo.Name}\"] : {lineCount} lines");
                
                string extension = Path.GetExtension(fileInfo.Name);
                if (extensionData.ContainsKey(extension))
                    extensionData[extension] += lineCount;
                else
                    extensionData.Add(extension, lineCount);
            }
            catch
            {
                Console.WriteLine($"[File \"{fileInfo.Name}\"] : could not access file!");
            }

            return lineCount;
        }
    }

    public class Program
    {
        static void Main(string[] args)
        {
            LocStatHandler handler = new LocStatHandler();
            handler.HandlePath("./");
        }
    }
}
