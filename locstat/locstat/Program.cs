namespace locstat
{
    public class LocStatHandler
    {
        public LocStatHandler(string path)
        {
            HandlePath(path);
        }

        public int HandlePath(string path)
        {
            DirectoryInfo directory = new DirectoryInfo(path);
            Console.WriteLine($"Running locstat on path : \"{directory.FullName}\"");
            int totalLinesCounted = HandleDir(directory);
            Console.WriteLine($"Total lines counted : {totalLinesCounted}");
            return totalLinesCounted;
        }

        public int HandleDir(DirectoryInfo directory)
        {
            int lineCount = 0;

            FileInfo[] files = directory.GetFiles();
            foreach (var file in files)
                lineCount += HandleFile(file);

            DirectoryInfo[] dirs = directory.GetDirectories();
            foreach (var child in dirs)
                lineCount += HandleDir(child);

            return lineCount;
        }

        public int HandleFile(FileInfo fileInfo)
        {
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
            LocStatHandler handler = new LocStatHandler("./");
        }
    }
}
