using System;
using Xunit;
using Coffer.Services;
using Coffer.Services.Models;

namespace Coffer.Tests
{
  public class ProfileServiceTest
  {
    [Fact]
    public void SaveThenLoad_ReturnsCorrectProfile()
    {
      string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
      Directory.CreateDirectory(tempDir);

      try
      {
        var original = new BackupProfile
        {
          ProfileName = "test",
          DestinationPath = "/mnt/backup/",
          Filters = new FilterConfig { MaxSizeMB = 5000}
        };

        ProfileService.SaveTo(original, tempDir);

        var loaded = ProfileService.LoadFrom(original.ProfileName, tempDir);

        Assert.Equal(original.DestinationPath, loaded.DestinationPath);
        Assert.Equal(original.Filters.MaxSizeMB, loaded.Filters.MaxSizeMB);
      }
      
      finally
      {
        Directory.Delete(tempDir, true);
      }
    }

    [Fact]
    public void DoesReturnAndMakeDefault()
    {
      BackupProfile defaults = new BackupProfile();
      string profileName = "test";
      
      try
      {
        var newProfile = ProfileService.Load(profileName);

        Assert.Equal(defaults.Filters, newProfile.Filters);
        Assert.Equal(defaults.copyConfig, newProfile.copyConfig);
      }

      finally
      {
        string configDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Coffer");
        string path = Path.Combine(configDir, $"{profileName}.json");

        File.Delete(path);
      }
    }

    [Fact]
    public void CopyActuallyCopies()
    {
      BackupProfile source = new BackupProfile
      {
        ProfileName = "Source",
        DestinationPath = "/mnt/backup/",
        Filters = new FilterConfig { MaxSizeMB = 5000}
      };

      ProfileService.Save(source, "Source");

      string configDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Coffer");

      try
      {
        ProfileService.CopyProfile("Source", "Clone");
        var clone = ProfileService.Load("Clone");

        Assert.Equal(source.DestinationPath, clone.DestinationPath);
        Assert.Equal(source.Filters.MaxSizeMB, clone.Filters.MaxSizeMB);
        Assert.DoesNotMatch(source.ProfileName, clone.ProfileName);
      }

      finally
      {
        string clonePath = Path.Combine(configDir, "Clone.json");
        string sourcePath = Path.Combine(configDir, "Source.json");
 
        File.Delete(clonePath);
        File.Delete(sourcePath);
      }
    }

    [Fact]
    public void DeletesFile()
    {
      BackupProfile tempProfile = new BackupProfile
      {
        ProfileName = "Temp",
        DestinationPath = "/mnt/backups/"
      };
      ProfileService.Save(tempProfile, "Temp");

      string configDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Coffer");
      string tempPath = Path.Combine(configDir, "Temp.json");

      ProfileService.RemoveProfile("Temp");

      Assert.False(File.Exists(tempPath));
    }
  }
}

