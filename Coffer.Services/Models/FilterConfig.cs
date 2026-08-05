using System;
using System.IO;
using System.Collections.Generic;

namespace Coffer.Services.Models
{
  public class FilterConfig
  {
    public List<string> ExcludeExtensions {get; set;} = new() {};
    public long MaxSizeMB {get; set;} = 500;
    public List<string> ExcludeFolders {get; set;} = new() {};
    public bool SkipHidden {get; set;} = false;
  }
}
