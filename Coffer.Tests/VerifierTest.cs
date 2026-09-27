using Xunit;
using System;
using Coffer.Services;

namespace Coffer.Tests
{
  public class VerifierTest
  { 
    [Fact]
    public void MatchingHashesReturnTrue()
    {
      string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
      Directory.CreateDirectory(tempDir);
     
      try
      {
        string sourceFile = Path.Combine(tempDir, "SourceTest.txt");
        string copiedFile = Path.Combine(tempDir, "CopiedTest.txt");
        File.WriteAllText(sourceFile, "Text yet to be copied");

        string sourceHash = Copier.CopyFile(sourceFile, copiedFile, 4);
        bool copiedHash = Verifier.Verify(sourceHash, copiedFile, 4);

        Assert.True(copiedHash);
      }
      
      finally 
      {
        Directory.Delete(tempDir, true);
      }
    }

    [Fact]
    public void MismatchingHashesReturnFalse()
    {
      string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
      Directory.CreateDirectory(tempDir);
     
      try
      {
        string sourceFile = Path.Combine(tempDir, "SourceTest.txt");
        string copiedFile = Path.Combine(tempDir, "CopiedTest.txt");
        File.WriteAllText(sourceFile, "Text yet to be copied");
 
        string sourceHash = Copier.CopyFile(sourceFile, copiedFile, 4);
        File.WriteAllText(copiedFile, "Text that got liberty");
        bool copiedHash = Verifier.Verify(sourceHash, copiedFile, 4);

        Assert.False(copiedHash);
      }

      finally
      {
        Directory.Delete(tempDir, true);
      }
    }
  }
}
