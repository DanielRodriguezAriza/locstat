namespace LocStat
{
    public struct LocStatHandlerConfig
    {
        public bool Recursive { get; set; }

        public LocStatHandlerConfig()
        {
            this.Recursive = false;
        }
    }

    public class LocStatHandler
    {
        LocStatHandlerConfig config;

        private long totalLines = 0;

        private List<string> allowedExtensions; // Maybe should be renamed to "known" extensions or whatever the fuck idk.
        private Dictionary<string, long> foundExtensions;

        public LocStatHandler(LocStatHandlerConfig config = default)
        {
            this.config = config;
            this.allowedExtensions = new List<string>()
            {
                // A list of default extensions that are allowed without the user having to add them by hand
                ".c", ".cpp", ".cs", ".js", ".json", ".css", ".html", ".xml", ".py", ".h"
            };
            this.foundExtensions = new Dictionary<string, long>();
        }

        private void AddFoundExtension(string extension, long lineCount)
        {
            if (this.foundExtensions.ContainsKey(extension))
                this.foundExtensions[extension] += lineCount;
            else
                this.foundExtensions.Add(extension, lineCount);
            totalLines += lineCount;
        }


        private void Log(string msg)
        {
            Console.WriteLine(msg);
        }

        public void HandlePath(string path, string[] args)
        {
            foreach (var arg in args)
                if (arg == "-R" || arg == "--recursive") // Shitty, make real argument parsing system so that other (unknown) args will fail when given.
                    this.config.Recursive = true;

            HandlePath(path);
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

            Log("Lines Per Language:");
            foreach (var entry in foundExtensions)
            {
                Log($"[\"{entry.Key}\"] : {entry.Value} lines");
            }
            Log($"Total Lines Counted: {totalLines}");
        }

        public long HandleDirectory(string path)
        {
            DirectoryInfo dir = new DirectoryInfo(path);
            return HandleDirectory(dir);
        }

        public long HandleDirectory(DirectoryInfo directory)
        {
            Log($"Handling directory: \"{directory.FullName}\"");

            long lineCount = 0;

            FileInfo[] files = directory.GetFiles();
            foreach (var file in files)
                lineCount += HandleFile(file);

            if (this.config.Recursive)
            {
                DirectoryInfo[] dirs = directory.GetDirectories();
                foreach (var child in dirs)
                    lineCount += HandleDirectory(child);
            }

            return lineCount;
        }

        public long HandleFile(string path)
        {
            FileInfo fileInfo = new FileInfo(path);
            return HandleFile(fileInfo);
        }

        public long HandleFile(FileInfo fileInfo)
        {
            long lineCount = 0;
            string fileName = fileInfo.Name;
            string extension = fileInfo.Extension.ToLowerInvariant();

            if (!this.allowedExtensions.Contains(extension))
                return 0;

            Log($"Handling file: \"{fileInfo.FullName}\"");
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

                Log($" - Lines : {lineCount}");
                AddFoundExtension(extension, lineCount);
            }
            catch
            {
                Console.WriteLine($" - ERROR : Could not access file!");
            }

            return lineCount;
        }
    }

    public class Program
    {
        static void Main(string[] args)
        {
            LocStatHandler handler = new LocStatHandler();
            handler.HandlePath("./", args);
        }
    }
}
