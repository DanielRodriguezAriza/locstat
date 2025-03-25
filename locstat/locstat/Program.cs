namespace locstat
{
    public class Program
    {
        public int HandlePath(string path)
        {
            DirectoryInfo directory = new DirectoryInfo(path);
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
            return lineCount;
        }

        static void Main(string[] args)
        {
            DirectoryInfo directory = new DirectoryInfo("./");
            Console.WriteLine($"Running locstat on path : \"{directory.FullName}\"");

            DirectoryInfo[] childDirectories = directory.GetDirectories();
        }
    }
}
