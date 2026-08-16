using System;
using System.IO;
using System.Collections.Generic;

namespace Coffer.Services.Models
{
  public class FilterConfig 
  {
    // Extensions filters
    public List<string> ExcludeExtensions {get; set;} = new() {};
    public List<string>? IncludeExtensions {get; set;} = null;

    // Size filters
    public long? MaxSizeMB {get; set;} = 500; // Nullable to have the option to set unlimited.
    public long MinSizeMB {get; set;} = 0;

    // Folders and files filters
    public List<string> ExcludeFolders {get; set;} = new() {};
    public List<string> ExcludePaths {get; set;} = new() {}; // This could also be excluding specific files.

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
