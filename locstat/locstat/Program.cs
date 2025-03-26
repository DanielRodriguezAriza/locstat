namespace LocStat
{
    public struct Command
    {
        public string ShortCommand { get; set; }
        public string LongCommand { get; set; }
        public string[] Arguments { get; set; }
        public string Description { get; set; }
        public Action<string, string[], int> Function { get; set; }
    }

    public struct LocStatHandlerConfig
    {
        public bool DebugEnabled { get; set; }
        public bool AllowRecursive { get; set; }
        public List<string> AllowedExtensions { get; set; }

        public LocStatHandlerConfig()
        {
            this.DebugEnabled = false;
            this.AllowRecursive = false;
            this.AllowedExtensions = new List<string>()
            {
                // A list of default extensions that are allowed without the user having to add them by hand
                ".c", ".cpp", ".cs", ".js", ".json", ".css", ".html", ".xml", ".py", ".h"
            };
        }
    }

    public class LocStatProgram
    {
        private LocStatHandler handler;
        private Command[] commands;
        private string path;
        private bool helpWasExecuted;

        public LocStatProgram()
        {
            this.helpWasExecuted = false;
            this.path = "./";
            this.handler = new LocStatHandler();
            this.commands = new Command[]
            {
                new Command
                {
                    ShortCommand = "-h",
                    LongCommand = "--help",
                    Arguments = new string[] { },
                    Description = "Display this help message.",
                    Function = CmdHelp
                },
                new Command
                {
                    ShortCommand = "-R",
                    LongCommand = "--allow-recursive",
                    Arguments = new string[] { },
                    Description = "Allow recursively searching through child directories for files.",
                    Function = CmdAllowRecursive
                },
                new Command
                {
                    ShortCommand = "-E",
                    LongCommand = "--allowed-extensions",
                    Arguments = new string[] { "<mode>", "<extensions>" },
                    Description = "Allow only the specified extensions to be counted. The extensions are specified in a comma separated string. Modes are: set, add, remove",
                    Function = CmdAllowedExtensions
                },
                new Command
                {
                    ShortCommand = "-d",
                    LongCommand = "--debug-enabled",
                    Arguments = new string[] { },
                    Description = "Enable debug logging.",
                    Function = CmdDebugEnabled
                },
                new Command
                {
                    ShortCommand = "-P",
                    LongCommand = "--path",
                    Arguments = new string[] { "<path>"},
                    Description = "Specify the path where the command will be executed.",
                    Function = CmdSetPath
                }
            };
        }

        public void Run(string[] args)
        {
            ParseCommands(args);

            // Do not run any code if the user has asked for help
            if (this.helpWasExecuted)
                return;

            this.handler.HandlePath(this.path);
        }

        private void ParseCommands(string[] args)
        {
            for(int i = 0; i < args.Length; ++i)
            {
                var arg = args[i];
                int argsRemaining = arg.Length - i - 1;

                bool commandFound = false;
                foreach (var cmd in this.commands)
                {
                    if (cmd.ShortCommand == arg || cmd.LongCommand == arg)
                    {
                        commandFound = true;

                        if (argsRemaining < cmd.Arguments.Length)
                        {
                            throw new Exception($"Not enough arguments found : {cmd.Arguments.Length} were expected, but {argsRemaining} were found!");
                        }

                        cmd.Function(cmd.LongCommand, args, i);
                        i += cmd.Arguments.Length;

                        break;
                    }
                }

                if (!commandFound)
                {
                    throw new Exception($"Unknown argument found : \"{arg}\"");
                }
            }
        }

        private void Log(string msg)
        {
            Console.WriteLine(msg);
        }

        private void CmdHelp(string cmdName, string[] args, int index)
        {
            this.helpWasExecuted = true;
            Log("Help:");
            foreach (var cmd in commands)
                Log($"{cmd.ShortCommand} {cmd.LongCommand} {cmd.Arguments} {cmd.Description}");
        }

        private void CmdAllowRecursive(string cmdName, string[] args, int index)
        {
            this.handler.Config.AllowRecursive = true;
        }

        private void CmdAllowedExtensions(string cmdName, string[] args, int index)
        {
            string modeString = args[index + 1].ToLowerInvariant();
            string extensionsString = args[index + 2].ToLowerInvariant();

            string[] extensions = extensionsString.Split(',');

            switch (modeString)
            {
                case "set":
                    this.handler.Config.AllowedExtensions = new List<string>(extensions);
                    break;
                case "add":
                    this.handler.Config.AllowedExtensions.AddRange(extensions);
                    break;
                case "remove":
                    foreach (var ext in extensions)
                        this.handler.Config.AllowedExtensions.Remove(ext);
                    break;
                default:
                    throw new Exception($"Unknown mode for command {cmdName} : \"{modeString}\"");
            }
        }

        private void CmdDebugEnabled(string cmdName, string[] args, int index)
        {
            this.handler.Config.DebugEnabled = true;
        }

        private void CmdSetPath(string cmdName, string[] args, int index)
        {
            this.path = args[index + 1];
        }
    }

    public class LocStatHandler
    {
        public long TotalLines;
        public LocStatHandlerConfig Config;
        public Dictionary<string, long> FoundExtensions;

        public LocStatHandler(LocStatHandlerConfig config)
        {
            this.Config = config;
            this.FoundExtensions = new Dictionary<string, long>();
        }

        public LocStatHandler()
        {
            this.TotalLines = 0;
            this.Config = new LocStatHandlerConfig();
            this.FoundExtensions = new Dictionary<string, long>();
        }

        private void AddFoundExtension(string extension, long lineCount)
        {
            if (this.FoundExtensions.ContainsKey(extension))
                this.FoundExtensions[extension] += lineCount;
            else
                this.FoundExtensions.Add(extension, lineCount);
            TotalLines += lineCount;
        }

        private void Log(string msg)
        {
            Console.WriteLine(msg);
        }

        private void DebugLog(string msg)
        {
            if (this.Config.DebugEnabled)
                Log(msg);
        }

        public void HandlePath(string path)
        {
            DebugLog($"Running LocStat on path: {path}");

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
            Log("{");
            foreach (var entry in FoundExtensions)
            {
                Log($"  \"{entry.Key}\" : {entry.Value}, ");
            }
            Log("}");
            Log($"Total Lines Counted: {TotalLines}");
        }

        public long HandleDirectory(string path)
        {
            DirectoryInfo dir = new DirectoryInfo(path);
            return HandleDirectory(dir);
        }

        public long HandleDirectory(DirectoryInfo directory)
        {
            DebugLog($"Handling directory: \"{directory.FullName}\"");

            long lineCount = 0;

            FileInfo[] files = directory.GetFiles();
            foreach (var file in files)
                lineCount += HandleFile(file);

            if (this.Config.AllowRecursive)
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

            if (!this.Config.AllowedExtensions.Contains(extension))
                return 0;

            DebugLog($"Handling file: \"{fileInfo.FullName}\"");
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

                DebugLog($" - Lines : {lineCount}");
                AddFoundExtension(extension, lineCount);
            }
            catch
            {
                DebugLog($" - ERROR : Could not access file!");
            }

            return lineCount;
        }
    }

    public class Program
    {
        static void Main(string[] args)
        {
            LocStatProgram program = new LocStatProgram();
            program.Run(args);
        }
    }
}
