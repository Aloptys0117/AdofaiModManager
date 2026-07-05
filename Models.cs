using System.Collections.Generic;

namespace AdofaiModManager
{
    public class ModInfo
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string Author { get; set; }
        public string Version { get; set; }
        public string ManagerVersion { get; set; }
        public string AssemblyName { get; set; }
        public string EntryMethod { get; set; }
        public string HomePage { get; set; }
        public string Repository { get; set; }
        public string Description { get; set; }
        public string FolderPath { get; set; }
    }

    public class OnlineModInfo
    {
        public string DisplayName { get; set; }
        public string Version { get; set; }
        public string Author { get; set; }
        public string DownloadUrl { get; set; }
    }

    public class OnlineModInfoRaw
    {
        public string name { get; set; }
        public string version { get; set; }
        public string download { get; set; }
        public string parsedDownload { get; set; }
        public string cachedUsername { get; set; }
    }
}
