using System;
using System.IO;
using System.Collections.Generic;

namespace Coffer.Services.Models
{
  public class FilterConfig 
  {
    // Extensions filters
    private List<string> _ExcludeExtensions = new() {};
    public List<string> ExcludeExtensions {
      get => _ExcludeExtensions;
      set => _ExcludeExtensions = value ?? new() {};
    }
    
    public List<string>? IncludeExtensions {get; set;} = null;

    // Size filters
    public long? MaxSizeMB {get; set;} = 500; // Nullable to have the option to set unlimited.
    public long MinSizeMB {get; set;} = 0;

    // Folders and files filters
    private List<string> _ExcludeFolders = new() {};
    public List<string> ExcludeFolders {
      get => _ExcludeFolders;
      set => _ExcludeFolders = value ?? new() {};
    }
    
    private List<string> _ExcludePaths = new() {}; // This could also be excluding specific files.
    public List<string> ExcludePaths {
      get => _ExcludePaths;
      set => _ExcludePaths = value ?? new() {};
    }

    private List<string> _ExcludeFileName = new() {};
    public List<string> ExcludeFileName {
      get => _ExcludeFileName;
      set => _ExcludeFileName = value ?? new() {};
    }

    public List<string>? IncludeFolders {get; set;} = null;
    public List<string>? IncludePaths {get; set;} = null;
    public List<string>? IncludeFileName {get; set;} = null;

    // Attributes filters
    public bool SkipHidden {get; set;} = false;
    public bool SkipReadOnly {get; set;} = false;
    public bool SkipSystemFiles {get; set;} = false;
    public bool FollowSymlinks {get; set;} = false; 
   
    // Time and dates filters 
    public DateTime? ModifiedAfter {get; set;} = null;
    public DateTime? ModifiedBefore {get; set;} = null;
    public DateTime? CreatedAfter {get; set;} = null;
    public DateTime? CreatedBefore {get; set;} = null;
    public int? ModifiedWithinDays {get; set;} = null;
    public int? CreatedWithinDays {get; set;} = null;
  }
}
