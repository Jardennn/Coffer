using Coffer.Services.Models;

namespace Coffer.Services
{
  public class BackupProfile
  {
    public string ProfileName {get; set;} = "Default"; 
    public List<string> SourcePaths {get; set;} = new() {};
    public string DestinationPath {get; set;} = "";
    public FilterConfig Filters {get; set;} = new FilterConfig();
  } 
}

