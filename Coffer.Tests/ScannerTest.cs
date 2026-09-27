using Xunit;
using System;
using System.IO;
using Coffer.Services;
using Coffer.Services.Models;

namespace Coffer.Tests
{
  public class ScannerTest
  {
    [Fact]
    public void NoFilters()
    {
      string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
      Directory.CreateDirectory(tempDir);
      List<FileInfo> expected = new List<FileInfo>(); 

      for (int i = 0; i < 5; i++)
      {
        string fileName = $"test_{i}";
        string filePath = Path.Combine(tempDir, fileName);
        File.WriteAllText(filePath, "I'm a part of a test");
        FileInfo file = new FileInfo(filePath);
        expected.Add(file);
      }

      try
      {
        FilterConfig filters = new FilterConfig(); 
        
        List<FileInfo> result = Scanner.Scan(tempDir, filters);

        Assert.Equal(expected, result);
      }

      finally 
      {
        Directory.Delete(tempDir, true);
      }
    }

    [Fact]
    public void ExclusionFilters()
    {
      string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
      Directory.CreateDirectory(tempDir);
      List<FileInfo> expected = new List<FileInfo>(); 

      // Excluding extension
 
      string filePath = Path.Combine(tempDir, "test_ext.txt");
      File.WriteAllText(filePath, "I'm a part of a test");
      FileInfo file = new FileInfo(filePath);
      expected.Add(file);

      // Excluding folder name
      
      string folderPath = Path.Combine(tempDir, "test");
      Directory.CreateDirectory(folderPath);

      filePath = Path.Combine(folderPath, "test_folder");
      File.WriteAllText(filePath, "I'm a part of a test");
      file = new FileInfo(filePath);
      expected.Add(file);

      // Excluding path name

      filePath = Path.Combine(tempDir, "test_path");
      File.WriteAllText(filePath, "I'm a part of a test");
      file = new FileInfo(filePath);
      expected.Add(file);

      // File name full

      filePath = Path.Combine(tempDir, "IMG");
      File.WriteAllText(filePath, "I'm a part of a test");
      file = new FileInfo(filePath);
      expected.Add(file);

      // File name partial
      
      filePath = Path.Combine(tempDir, "IMG_05052005");
      File.WriteAllText(filePath, "I'm a part of a test");
      file = new FileInfo(filePath);
      expected.Add(file);

      try
      {
        FilterConfig filters = new FilterConfig
        {
          ExcludeExtensions = {".txt"},
          ExcludeFolders = {"test"},
          ExcludePaths = {Path.Combine(tempDir, "test_path")},
          ExcludeFileName = {"IMG"}
        }; 
        
        List<FileInfo> result = Scanner.Scan(tempDir, filters);

        Assert.All(result, item => Assert.DoesNotContain(item, expected));
      }

      finally 
      {
        Directory.Delete(tempDir, true);
      }     
    }

    [Fact]
    public void SizeFilters()
    {
      string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
      Directory.CreateDirectory(tempDir);
      List<FileInfo> expected = new List<FileInfo>(); 
      
      // MaxSizeMB
      long Size = 5 * 1024 * 1024;       
      string filePath = Path.Combine(tempDir, "testsizeMax");

      using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
      {
        fs.SetLength(Size);
      }

      FileInfo file = new FileInfo(filePath);
      expected.Append(file);

      // MinSizeMB
      Size = 2 * 1024 * 1024;
      filePath = Path.Combine(tempDir, "testsizeMin");

      using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
      {
        fs.SetLength(Size);
      }

      file = new FileInfo(filePath);
      expected.Append(file);

      try
      {
        FilterConfig filters = new FilterConfig
        {
          MaxSizeMB = 4,
          MinSizeMB = 3
        }; 
        
        List<FileInfo> result = Scanner.Scan(tempDir, filters);

        Assert.All(result, item => Assert.DoesNotContain(item, expected));
      }

      finally 
      {
        Directory.Delete(tempDir, true);
      }
    }

    [Fact]
    public void BooleanFilters()
    {
      string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
      Directory.CreateDirectory(tempDir);
      List<FileInfo> expected = new List<FileInfo>(); 
      
      // Hidden
      
      string filePath = Path.Combine(tempDir, ".testHidden");
      File.WriteAllText(filePath, "I'm a part of a test");
      
      if (OperatingSystem.IsWindows())
      { 
        File.SetAttributes(filePath, FileAttributes.Hidden);
      }

      FileInfo file = new FileInfo(filePath);
      expected.Append(file);     

      // ReadOnly

      filePath = Path.Combine(tempDir, "testReadOnly");
      File.WriteAllText(filePath, "I'm a part of a test");
      file = new FileInfo(filePath);
      file.IsReadOnly = true;
      expected.Append(file);

      // System File
      
      filePath = Path.Combine(tempDir, "testSystemFile");
      if (OperatingSystem.IsWindows())
      {
        File.WriteAllText(filePath, "I'm a part of a test");
        File.SetAttributes(filePath, FileAttributes.System);
      }
      else
      {
        filePath = Path.Combine("/bin", "testSystemFile");
      }

      // Need to figure out how to make this work on linux without touching the system directorys.

      file = new FileInfo(filePath);
      expected.Append(file);

      // Symlink

      filePath = Path.Combine(tempDir, "testLinkedTo");
      File.WriteAllText(filePath, "I'm a part of a test");
      string symlinkPath = Path.Combine(tempDir, "testSymlink");
      File.CreateSymbolicLink(symlinkPath, filePath);
      file = new FileInfo(symlinkPath);
      expected.Append(file);

      try
      {
        FilterConfig filters = new FilterConfig
        {
          SkipHidden = true,
          SkipReadOnly = true,
          SkipSystemFiles = true,
          FollowSymlinks = true
        };

        List<FileInfo> result = Scanner.Scan(tempDir, filters);

        Assert.All(result, item => Assert.DoesNotContain(item, expected));
      }

      finally
      {
        Directory.Delete(tempDir, true);
      }
    }

    [Fact]
    public void InclusionFilters()
    {
      string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
      Directory.CreateDirectory(tempDir);
      List<FileInfo> expected = new List<FileInfo>(); 
  
      // Extensions
      
      string filePath = Path.Combine(tempDir, "test_ext_include.txt");
      File.WriteAllText(filePath, "I'm part of a test");
      FileInfo file = new FileInfo(filePath);
      expected.Add(file);

      // Folder name
      
      string folderPath = Path.Combine(tempDir, "test_include");
      Directory.CreateDirectory(folderPath);

      filePath = Path.Combine(folderPath, "test_folder_include");
      File.WriteAllText(filePath, "I'm part of a test");
      file = new FileInfo(filePath);
      expected.Add(file); 

      // Path

      filePath = Path.Combine(tempDir, "test_path_include");
      File.WriteAllText(filePath, "I'm a part of a test");
      file = new FileInfo(filePath);
      expected.Add(file); 

      // File name part

      filePath = Path.Combine(tempDir, "IMG_05052005");
      File.WriteAllText(filePath, "I'm part of a test");
      file = new FileInfo(filePath);
      expected.Add(file);

      // File name full

      filePath = Path.Combine(tempDir, "IMG");
      File.WriteAllText(filePath, "I'm part of a test");
      file = new FileInfo(filePath);
      expected.Add(file);

      try
      {
        FilterConfig filters = new FilterConfig
        {
          IncludeExtensions = {".txt"},
          IncludeFolders = {"test_include"},
          IncludePaths = {Path.Combine(tempDir, "test_path_include")},
          IncludeFileName = {"IMG"}
        };

        List<FileInfo> result = Scanner.Scan(tempDir, filters);

        Assert.Equal(expected, result);
      }

      finally
      {
        Directory.Delete(tempDir, true);
      }
    }

    public void TimeFilters()
    {
      string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
      Directory.CreateDirectory(tempDir);
      List<FileInfo> expected = new List<FileInfo>(); 
   
      // Modified/created before 'after' date

      string filePath = Path.Combine(tempDir, "testBefore");
      File.WriteAllText(filePath, "I'm a part of a test");
      FileInfo file = new FileInfo(filePath);
      file.LastWriteTime = new DateTime(2026, 09, 23);
      file.CreationTime = new DateTime(2026, 09, 23);
      expected.Append(file);

      // Modiefed/created after 'before' date

      filePath = Path.Combine(tempDir, "testAfter");
      File.WriteAllText(filePath, "I'm part of a test");
      file = new FileInfo(filePath);
      file.LastWriteTime = new DateTime(2026, 09, 17);
      file.CreationTime = new DateTime(2026, 09 , 17);
      expected.Append(file);

      try
      {
        FilterConfig filters = new FilterConfig
        {
          ModifiedAfter = new DateTime(2026, 09, 25),
          ModifiedBefore = new DateTime(2026, 09, 15),
          CreatedAfter = new DateTime(2026, 09, 25),
          CreatedBefore = new DateTime(2025, 09, 15),
          ModifiedWithinDays = 2,
          CreatedWithinDays = 2
        };
      }

      finally
      {
        Directory.Delete(tempDir);
      }
    }
  }
}
