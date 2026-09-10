using System;
using System.IO;
using System.Collections.Generic;

namespace Coffer.Services.Models
{
    public class BackupProfile
    {
        public string ProfileName { get; set; } = "Default";
        public List<string> SourcePaths { get; set; } = new() { };
        public string DestinationPath { get; set; } = "";
        public FilterConfig Filters { get; set; } = new FilterConfig();
        public CopyConfig copyConfig { get; set; } = new CopyConfig();
    }
}
