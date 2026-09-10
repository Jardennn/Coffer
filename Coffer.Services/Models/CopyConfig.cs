namespace Coffer.Services.Models
{
  public enum DuplicateMode
  {
    Overwrite,
    Skip,
    KeepNewer,
    Rename
  }
  public class CopyConfig
  {
    public DuplicateMode duplicateHandel { get; set; } = DuplicateMode.KeepNewer;
    public int ChunkSizeMB {get; set;} = 4;
    public bool VerifyAfterCopy {get; set;} = true;
  }
}
