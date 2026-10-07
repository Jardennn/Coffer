using System;
using System.IO;
using System.Collections.Generic;

namespace Coffer.Services.Models
{
    public class BackupProfile
    {
        private string _ProfileName = "Default";
        public string ProfileName {
          get => _ProfileName;
          set => _ProfileName = value ?? "Default";
        }
        
        private List<string> _SourcePaths = new() { };
        public List<string> SourcePaths {
          get => _SourcePaths;
          set => _SourcePaths = value ?? new() { };
        }

        private string _DestinationPath = "";
        public string DestinationPath {
          get => _DestinationPath;
          set => _DestinationPath = value ?? "";
        }

        public FilterConfig Filters { get; set; } = new FilterConfig();
        public CopyConfig copyConfig { get; set; } = new CopyConfig();
    }
}
