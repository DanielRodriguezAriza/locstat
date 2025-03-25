using System.Reflection;

namespace locstat
{
    public class LocStatHandler
    {
        public LocStatHandler(string path, string[] extensions)
        {
            HandlePath(path, extensions);
        }

        public int HandlePath(string path, string[] extensions)
        {
            DirectoryInfo directory = new DirectoryInfo(path);
            Console.WriteLine($"Running locstat on path : \"{directory.FullName}\"");
            int totalLinesCounted = HandleDir(directory, extensions);
            Console.WriteLine($"Total lines counted : {totalLinesCounted}");
            return totalLinesCounted;
        }

        public int HandleDir(DirectoryInfo directory, string[] extensions)
        {
            int lineCount = 0;

            FileInfo[] files = directory.GetFiles();
            foreach (var file in files)
                lineCount += HandleFile(file, extensions);

            DirectoryInfo[] dirs = directory.GetDirectories();
            foreach (var child in dirs)
                lineCount += HandleDir(child, extensions);

            return lineCount;
        }

        public int HandleFile(FileInfo fileInfo, string[] extensions)
        {
            int lineCount = 0;
            bool canProcess = false;

            foreach (var extension in extensions)
                if (fileInfo.Name.EndsWith(extension))
                    canProcess = true;

            if (!canProcess)
                return 0;

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
            LocStatHandler handler = new LocStatHandler("./", new string[] { "cs" });
        }
    }
}
