using Xunit;
using System;
using Coffer.Services;
using Coffer.Core;
using Coffer.Services.Models;

namespace Coffer.Tests
{
  public class ArgumentParserTest
  {
    [Fact]
    public void ValidNumericValue_SetsCorrectly()
    {
      // Should cover: MaxSizeMB,
      // MinSizeMB,
      // chunkSize,
      // CreatedWithinDays,
      // ModifiedWithinDays,

      var profile = new BackupProfile();

      // MaxSizeMB
      ArgumentParser.ApplyModify(new[] {"--max-mb", "300"}, profile);
      Assert.Equal(300, profile.Filters.MaxSizeMB);

      // MinSizeMB
      ArgumentParser.ApplyModify(new[] {"--min-mb", "10"}, profile);
      Assert.Equal(10, profile.Filters.MinSizeMB);

      // ChunkSizeMB
      ArgumentParser.ApplyModify(new[] {"--chunk-size", "7"}, profile);
      Assert.Equal(7, profile.copyConfig.ChunkSizeMB);

      // CreatedWithinDays
      ArgumentParser.ApplyModify(new[] {"--created-within-days", "9"}, profile);
      Assert.Equal(9, profile.Filters.CreatedWithinDays);
      
      // ModifiedWithinDays
      ArgumentParser.ApplyModify(new[] {"--mod-within-days", "5"}, profile);
      Assert.Equal(5, profile.Filters.ModifiedWithinDays);
    }

    [Fact]
    public void InvalidNumericValue_ReturnsError()
    {
      var profile = new BackupProfile();

      // Should cover: MaxSizeMB,
      // MinSizeMB,
      // chunkSize,
      // CreatedWithinDays,
      // ModifiedWithinDays,

      // MaxSizeMB
      (bool success, bool mod, string? message) modify = ArgumentParser.ApplyModify(new[] {"--max-mb", "test"}, profile);
      Assert.False(modify.success);
      Assert.NotNull(modify.message);

      // negative
      modify = ArgumentParser.ApplyModify(new[] {"--max-mb", "-5"}, profile);
      Assert.False(modify.success);
      Assert.NotNull(modify.message);

      // zero
      modify = ArgumentParser.ApplyModify(new[] {"--max-mb", "0"}, profile);
      Assert.False(modify.success);
      Assert.NotNull(modify.message);

      modify = ArgumentParser.ApplyModify(new[] {"--max-mb"}, profile);
      Assert.False(modify.success);
      Assert.NotNull(modify.message);

      modify = ArgumentParser.ApplyModify(new[] {"--max-mb", "--source"}, profile);
      Assert.False(modify.success);
      Assert.NotNull(modify.message);

      // MinSizeMB
      modify = ArgumentParser.ApplyModify(new[] {"--min-mb", "hello"}, profile);
      Assert.False(modify.success);
      Assert.NotNull(modify.message);

      modify = ArgumentParser.ApplyModify(new[] {"--min-mb", "-4"}, profile);
      Assert.False(modify.success);
      Assert.NotNull(modify.message);

      modify = ArgumentParser.ApplyModify(new[] {"--min-mb"}, profile);
      Assert.False(modify.success);
      Assert.NotNull(modify.message);

      modify = ArgumentParser.ApplyModify(new[] {"--min-mb", "--source"}, profile);
      Assert.False(modify.success);
      Assert.NotNull(modify.message);

      // ChunkSizeMB
      modify = ArgumentParser.ApplyModify(new[] {"--chunk-size", "?"}, profile);
      Assert.False(modify.success);
      Assert.NotNull(modify.message);

      modify = ArgumentParser.ApplyModify(new[] {"--chunk-size", "-6"}, profile);
      Assert.False(modify.success);
      Assert.NotNull(modify.message);

      modify = ArgumentParser.ApplyModify(new[] {"--chunk-size", "0"}, profile);
      Assert.False(modify.success);
      Assert.NotNull(modify.message);

      modify = ArgumentParser.ApplyModify(new[] {"--chunk-size"}, profile);
      Assert.False(modify.success);
      Assert.NotNull(modify.message);

      modify = ArgumentParser.ApplyModify(new[] {"--chunk-size", "--source"}, profile);
      Assert.False(modify.success);
      Assert.NotNull(modify.message);

      // CreatedWithinDays
      modify = ArgumentParser.ApplyModify(new[] {"--created-within-days", "meow"}, profile);
      Assert.False(modify.success);
      Assert.NotNull(modify.message);

      modify = ArgumentParser.ApplyModify(new[] {"--created-within-days", "-10"}, profile);
      Assert.False(modify.success);
      Assert.NotNull(modify.message);

      modify = ArgumentParser.ApplyModify(new[] {"--created-within-days", "0"}, profile);
      Assert.False(modify.success);
      Assert.NotNull(modify.message);

      modify = ArgumentParser.ApplyModify(new[] {"--created-within-days"}, profile);
      Assert.False(modify.success);
      Assert.NotNull(modify.message);

      modify = ArgumentParser.ApplyModify(new[] {"--created-within-days", "--source"}, profile);
      Assert.False(modify.success);
      Assert.NotNull(modify.message);

      // ModifiedWithinDays
      modify = ArgumentParser.ApplyModify(new[] {"--mod-within-days", "cat"}, profile);
      Assert.False(modify.success);
      Assert.NotNull(modify.message);

      modify = ArgumentParser.ApplyModify(new[] {"--mod-within-days", "-3"}, profile);
      Assert.False(modify.success);
      Assert.NotNull(modify.message);

      modify = ArgumentParser.ApplyModify(new[] {"--mod-within-days", "0"}, profile);
      Assert.False(modify.success);
      Assert.NotNull(modify.message);

      modify = ArgumentParser.ApplyModify(new[] {"--mod-within-days"}, profile);
      Assert.False(modify.success);
      Assert.NotNull(modify.message);

      modify = ArgumentParser.ApplyModify(new[] {"--mod-within-days", "--source"}, profile);
      Assert.False(modify.success);
      Assert.NotNull(modify.message);
    }

    [Fact]
    public void ValidDateFormat_SetsCorrectly()
    {
      var profile = new BackupProfile();

      /*
       This covers the filters:
       ModifiedAfter
       ModifiedBefore
       CreatedAfter
       CreatedBefore
       */

      // ModifiedAfter
      (bool success, bool mod, string? message) modify = ArgumentParser.ApplyModify(new[] {"--mod-after", "2026-09-25"}, profile);
      Assert.Equal(new DateTime(2026, 09, 25), profile.Filters.ModifiedAfter);

      // ModifiedBefore
      modify = ArgumentParser.ApplyModify(new[] {"--mod-before", "2026-09-25"}, profile);
      Assert.Equal(new DateTime(2026, 09, 25), profile.Filters.ModifiedBefore);

      // CreatedAfter
      modify = ArgumentParser.ApplyModify(new[] {"--created-after", "2026-09,25"}, profile);
      Assert.Equal(new DateTime(2026, 09, 25), profile.Filters.CreatedAfter);

      //CreatedBefore
      modify = ArgumentParser.ApplyModify(new[] {"--created-before", "2026-09-25"}, profile);
      Assert.Equal(new DateTime(2026, 09, 25), profile.Filters.CreatedBefore);
    }

    [Fact]
    public void InvalidDateFormat_ReturnsError()
    {
      var profile = new BackupProfile();
      
      //ModifiedAfter
      
      //text
      (bool success, bool mod, string? message) modify = ArgumentParser.ApplyModify(new[] {"--mod-after", "test"}, profile);
      Assert.False(modify.success);
      Assert.NotNull(modify.message);
 
      // number
      
      modify = ArgumentParser.ApplyModify(new[] {"--mod-after", "67"}, profile);
      Assert.False(modify.success);
      Assert.NotNull(modify.message);
 
      modify = ArgumentParser.ApplyModify(new[] {"--mod-after"}, profile);
      Assert.False(modify.success);
      Assert.NotNull(modify.message);
 
      modify = ArgumentParser.ApplyModify(new[] {"--mod-after", "--source"}, profile);
      Assert.False(modify.success);
      Assert.NotNull(modify.message);
 
      // ModifiedBefore
      modify = ArgumentParser.ApplyModify(new[] {"--mod-before", "test"}, profile);
      Assert.False(modify.success);
      Assert.NotNull(modify.message);
 
      modify = ArgumentParser.ApplyModify(new[] {"--mod-before", "67"}, profile);
      Assert.False(modify.success);
      Assert.NotNull(modify.message);
 
      modify = ArgumentParser.ApplyModify(new[] {"--mod-before"}, profile);
      Assert.False(modify.success);
      Assert.NotNull(modify.message);
 
      modify = ArgumentParser.ApplyModify(new[] {"--mod-before", "--source"}, profile);
      Assert.False(modify.success);
      Assert.NotNull(modify.message);
 
      // CreatedAfter
      modify = ArgumentParser.ApplyModify(new[] {"--created-after", "test"}, profile);
      Assert.False(modify.success);
      Assert.NotNull(modify.message);
 
      modify = ArgumentParser.ApplyModify(new[] {"--created-after", "67"}, profile);
      Assert.False(modify.success);
      Assert.NotNull(modify.message);
 
      modify = ArgumentParser.ApplyModify(new[] {"--created-after"}, profile);
      Assert.False(modify.success);
      Assert.NotNull(modify.message);
 
      modify = ArgumentParser.ApplyModify(new[] {"--created-after", "--source"}, profile);
      Assert.False(modify.success);
      Assert.NotNull(modify.message);
    
      // CreatedBefore
      modify = ArgumentParser.ApplyModify(new[] {"--created-before", "test"}, profile);
      Assert.False(modify.success);
      Assert.NotNull(modify.message);
 
      modify = ArgumentParser.ApplyModify(new[] {"--created-before", "67"}, profile);
      Assert.False(modify.success);
      Assert.NotNull(modify.message);
     
      modify = ArgumentParser.ApplyModify(new[] {"--created-before"}, profile);
      Assert.False(modify.success);
      Assert.NotNull(modify.message);
     
      modify = ArgumentParser.ApplyModify(new[] {"--mod-before", "--source"}, profile);
      Assert.False(modify.success);
      Assert.NotNull(modify.message);
     
    }

    [Fact]
    public void AppendingToListAddsCorrectly()
    {
      var profile = new BackupProfile();

      /*
      This is will cover:
      sources,
      excludeExtensions,
      includeExtensions,
      ExcludeFolders,
      ExcludePaths,
      ExcludeFileNames,
      IncludeFolders,
      IncludePaths,
      IncludeFileNames,
       */

      // source
      (bool success, bool mod, string? message) modify = ArgumentParser.ApplyModify(new[] {"--source", "meow"}, profile);
      Assert.Equal(new List<string>{"meow"}, profile.SourcePaths);

      modify = ArgumentParser.ApplyModify(new[] {"--source", "cat"}, profile);
      Assert.Equal(new List<string>{"meow", "cat"}, profile.SourcePaths);

      modify = ArgumentParser.ApplyModify(new[] {"--source", "hello", "world"}, profile);
      Assert.Equal(new List<string>{"meow", "cat", "hello", "world"}, profile.SourcePaths);

      // excludeExtensions
      modify = ArgumentParser.ApplyModify(new[] {"--exclude-ext", ".txt"}, profile);
      Assert.Equal(new List<string>{".txt"}, profile.Filters.ExcludeExtensions);

      modify = ArgumentParser.ApplyModify(new[] {"--exclude-ext", ".pdf"}, profile);
      Assert.Equal(new List<string>{".txt", ".pdf"}, profile.Filters.ExcludeExtensions);

      modify = ArgumentParser.ApplyModify(new[] {"--exclude-ext", ".jpg", ".png"}, profile);
      Assert.Equal(new List<string>{".txt", ".pdf", ".jpg", ".png"}, profile.Filters.ExcludeExtensions);

      // Include extensions
      
      modify = ArgumentParser.ApplyModify(new[] {"--include-ext", ".txt"}, profile);
      Assert.Equal(new List<string>{".txt"}, profile.Filters.IncludeExtensions);

      modify = ArgumentParser.ApplyModify(new[] {"--include-ext", ".pdf"}, profile);
      Assert.Equal(new List<string>{".txt", ".pdf"}, profile.Filters.IncludeExtensions);

      modify = ArgumentParser.ApplyModify(new[] {"--include-ext", ".jpg", ".png"}, profile);
      Assert.Equal(new List<string>{".txt", ".pdf", ".jpg", ".png"}, profile.Filters.IncludeExtensions);
    
      // Exclude Folders

      modify = ArgumentParser.ApplyModify(new[] {"--exclude-folder", "meow"}, profile);
      Assert.Equal(new List<string>{"meow"}, profile.Filters.ExcludeFolders);

      modify = ArgumentParser.ApplyModify(new[] {"--exclude-folder", "cat"}, profile);
      Assert.Equal(new List<string>{"meow", "cat"}, profile.Filters.ExcludeFolders);

      modify = ArgumentParser.ApplyModify(new[] {"--exclude-folder", "hello", "world"}, profile);
      Assert.Equal(new List<string>{"meow", "cat", "hello", "world"}, profile.Filters.ExcludeFolders);
    
      // Exclude Paths

      modify = ArgumentParser.ApplyModify(new[] {"--exclude-path", "meow"}, profile);
      Assert.Equal(new List<string>{"meow"}, profile.Filters.ExcludePaths);

      modify = ArgumentParser.ApplyModify(new[] {"--exclude-path", "cat"}, profile);
      Assert.Equal(new List<string>{"meow", "cat"}, profile.Filters.ExcludePaths);

      modify = ArgumentParser.ApplyModify(new[] {"--exclude-path", "hello", "world"}, profile);
      Assert.Equal(new List<string>{"meow", "cat", "hello", "world"}, profile.Filters.ExcludePaths);
    
      // Exclude File

      modify = ArgumentParser.ApplyModify(new[] {"--exclude-file", "meow"}, profile);
      Assert.Equal(new List<string>{"meow"}, profile.Filters.ExcludeFileName);

      modify = ArgumentParser.ApplyModify(new[] {"--exclude-file", "cat"}, profile);
      Assert.Equal(new List<string>{"meow", "cat"}, profile.Filters.ExcludeFileName);

      modify = ArgumentParser.ApplyModify(new[] {"--exclude-file", "hello", "world"}, profile);
      Assert.Equal(new List<string>{"meow", "cat", "hello", "world"}, profile.Filters.ExcludeFileName);
    
      // Include Folders
      
      modify = ArgumentParser.ApplyModify(new[] {"--include-folder", "meow"}, profile);
      Assert.Equal(new List<string>{"meow"}, profile.Filters.IncludeFolders);

      modify = ArgumentParser.ApplyModify(new[] {"--include-folder", "cat"}, profile);
      Assert.Equal(new List<string>{"meow", "cat"}, profile.Filters.IncludeFolders);

      modify = ArgumentParser.ApplyModify(new[] {"--include-folder", "hello", "world"}, profile);
      Assert.Equal(new List<string>{"meow", "cat", "hello", "world"}, profile.Filters.IncludeFolders);
    
      // Include Paths

      modify = ArgumentParser.ApplyModify(new[] {"--include-path", "meow"}, profile);
      Assert.Equal(new List<string>{"meow"}, profile.Filters.IncludePaths);

      modify = ArgumentParser.ApplyModify(new[] {"--include-path", "cat"}, profile);
      Assert.Equal(new List<string>{"meow", "cat"}, profile.Filters.IncludePaths);

      modify = ArgumentParser.ApplyModify(new[] {"--include-path", "hello", "world"}, profile);
      Assert.Equal(new List<string>{"meow", "cat", "hello", "world"}, profile.Filters.IncludePaths);

      // Include File
      
      modify = ArgumentParser.ApplyModify(new[] {"--include-file", "meow"}, profile);
      Assert.Equal(new List<string>{"meow"}, profile.Filters.IncludeFileName);

      modify = ArgumentParser.ApplyModify(new[] {"--include-file", "cat"}, profile);
      Assert.Equal(new List<string>{"meow", "cat"}, profile.Filters.IncludeFileName);

      modify = ArgumentParser.ApplyModify(new[] {"--include-file", "hello", "world"}, profile);
      Assert.Equal(new List<string>{"meow", "cat", "hello", "world"}, profile.Filters.IncludeFileName);
    }

    [Fact]
    public void AppendingListNoValue()
    {
      var profile = new BackupProfile();
      
      // no value, any other character is accepted anyway
      // Also a test for if a filter (specifically list that are not supposed to be null) that if it's null to see if a new list or atleast the list updates ok.
      // this will also cover the destination setting as there is no test for string values and it is the only one.
      /*
        sources,
      excludeExtensions,
      includeExtensions,
      ExcludeFolders,
      ExcludePaths,
      ExcludeFileNames,
      IncludeFolders,
      IncludePaths,
      IncludeFileNames,
      */

      // Destination
      (bool success, bool mod, string? message) modify = ArgumentParser.ApplyModify(new[] {"--destination"}, profile);
      Assert.False(modify.success);
      Assert.NotNull(modify.message);
 
      modify = ArgumentParser.ApplyModify(new[] {"--destination", "--source"}, profile);
      Assert.False(modify.success);
      Assert.NotNull(modify.message);

      //sources
      modify = ArgumentParser.ApplyModify(new[] {"--source"}, profile);
      Assert.False(modify.success);
      Assert.NotNull(modify.message);
 
      modify = ArgumentParser.ApplyModify(new[] {"--source", "--destination"}, profile);
      Assert.False(modify.success);
      Assert.NotNull(modify.message);
 
      // excludeExtensions
      
      modify = ArgumentParser.ApplyModify(new[] {"--exclude-ext"}, profile);
      Assert.False(modify.success);
      Assert.NotNull(modify.message);
 
      modify = ArgumentParser.ApplyModify(new[] {"--exclude-ext", "--source"}, profile);
      Assert.False(modify.success);
      Assert.NotNull(modify.message);
 
      // includeExtensions
      modify = ArgumentParser.ApplyModify(new[] {"--include-ext"}, profile);
      Assert.False(modify.success);
      Assert.NotNull(modify.message);
 
      modify = ArgumentParser.ApplyModify(new[] {"--include-ext", "--source"}, profile);
      Assert.False(modify.success);
      Assert.NotNull(modify.message);
 
      // exclude folders
      modify = ArgumentParser.ApplyModify(new[] {"--exclude-folder"}, profile);
      Assert.False(modify.success);
      Assert.NotNull(modify.message);
 
      modify = ArgumentParser.ApplyModify(new[] {"--exclude-folder", "--source"}, profile);
      Assert.False(modify.success);
      Assert.NotNull(modify.message);
  
      // exclude path
      modify = ArgumentParser.ApplyModify(new[] {"--exclude-path"}, profile);
      Assert.False(modify.success);
      Assert.NotNull(modify.message);
 
      modify = ArgumentParser.ApplyModify(new[] {"--exclude-path", "--source"}, profile);
      Assert.False(modify.success);
      Assert.NotNull(modify.message);
 
      // exclude file
      modify = ArgumentParser.ApplyModify(new[] {"--exclude-file"}, profile);
      Assert.False(modify.success);
      Assert.NotNull(modify.message);
 
      modify = ArgumentParser.ApplyModify(new[] {"--exclude-file", "--source"}, profile);
      Assert.False(modify.success);
      Assert.NotNull(modify.message);
 
      //include folder
      modify = ArgumentParser.ApplyModify(new[] {"--include-folder"}, profile);
      Assert.False(modify.success);
      Assert.NotNull(modify.message);
 
      modify = ArgumentParser.ApplyModify(new[] {"--include-folder", "--source"}, profile);
      Assert.False(modify.success);
      Assert.NotNull(modify.message);
 
      //include path
      modify = ArgumentParser.ApplyModify(new[] {"--include-path"}, profile);
      Assert.False(modify.success);
      Assert.NotNull(modify.message);
 
      modify = ArgumentParser.ApplyModify(new[] {"--include-path", "--source"}, profile);
      Assert.False(modify.success);
      Assert.NotNull(modify.message);
 
      //include file
      modify = ArgumentParser.ApplyModify(new[] {"--include-file"}, profile);
      Assert.False(modify.success);
      Assert.NotNull(modify.message);
 
      modify = ArgumentParser.ApplyModify(new[] {"--include-file", "--source"}, profile);
      Assert.False(modify.success);
      Assert.NotNull(modify.message);
    }

    [Fact]
    public void AppendingListNull()
    {
      var profile = new BackupProfile();

      profile.SourcePaths = null;
      profile.DestinationPath = null;
      profile.Filters.ExcludeExtensions = null;
      profile.Filters.ExcludeFolders = null;
      profile.Filters.ExcludePaths = null;
      profile.Filters.ExcludeFileName = null;

      (bool success, bool mod, string? message) modify = ArgumentParser.ApplyModify(new[] 
          {
          "--source", "/path/to/source",
          "--destination", "/path/to/destination",
          "--exclude-ext", ".txt",
          "--exclude-folder", "folder",
          "--exclude-path", "/path/to/exclude",
          "--exclude-file", "filename"
          }, profile);

      Assert.NotNull(profile.SourcePaths);
      Assert.NotNull(profile.DestinationPath);
      Assert.NotNull(profile.Filters.ExcludeExtensions);
      Assert.NotNull(profile.Filters.ExcludeFolders);
      Assert.NotNull(profile.Filters.ExcludePaths);
      Assert.NotNull(profile.Filters.ExcludeFileName);
    }

    [Fact]
    public void RemovingFromListRemoves()
    {
      var profile = new BackupProfile{
        SourcePaths = {"meow", "cat", "hello", "world"},
         
        Filters = new FilterConfig
        {
          ExcludeExtensions = {".txt", ".pdf", ".jpg", ".png"},
          IncludeExtensions = {".txt", ".pdf", ".jpg", ".png"},
          
          ExcludeFolders = {"meow", "cat", "hello", "world"},
          ExcludePaths = {"meow", "cat", "hello", "world"},
          ExcludeFileName = {"meow", "cat", "hello", "world"},

          IncludeFolders = {"meow", "cat", "hello", "world"},
          IncludePaths = {"meow", "cat", "hello", "world"},
          IncludeFileName = {"meow", "cat", "hello", "world"}
        }
      };

      // Source
      
      (bool success, bool mod, string? message) modify = ArgumentParser.ApplyModify(new[] {"--rm-src", "meow"}, profile);
      Assert.Equal(new List<string>{"cat", "hello", "world"}, profile.SourcePaths);

      modify = ArgumentParser.ApplyModify(new[] {"--rm-src", "hello", "world"}, profile);
      Assert.Equal(new List<string>{"cat"}, profile.SourcePaths);

      // excludeExtensions
      
      modify = ArgumentParser.ApplyModify(new[] {"--rm-excluded-ext", ".txt"}, profile);
      Assert.Equal(new List<string>{".pdf", ".jpg", ".png"}, profile.Filters.ExcludeExtensions);

      modify = ArgumentParser.ApplyModify(new[] {"--rm-excluded-ext", ".jpg", ".png"}, profile);
      Assert.Equal(new List<string>{".pdf"}, profile.Filters.ExcludeExtensions);

      // includeExtensions
      
      modify = ArgumentParser.ApplyModify(new[] {"--rm-included-ext", ".txt"}, profile);
      Assert.Equal(new List<string>{".pdf", ".jpg", ".png"}, profile.Filters.IncludeExtensions);

      modify = ArgumentParser.ApplyModify(new[] {"--rm-included-ext", ".jpg", ".png"}, profile);
      Assert.Equal(new List<string>{".pdf"}, profile.Filters.IncludeExtensions);
      
      // exclude Folders

      modify = ArgumentParser.ApplyModify(new[] {"--rm-excluded-folder", "meow"}, profile);
      Assert.Equal(new List<string>{"cat", "hello", "world"}, profile.Filters.ExcludeFolders);

      modify = ArgumentParser.ApplyModify(new[] {"--rm-excluded-folder", "hello", "world"}, profile);
      Assert.Equal(new List<string>{"cat"}, profile.Filters.ExcludeFolders);
 
      // exclude Paths

      modify = ArgumentParser.ApplyModify(new[] {"--rm-excluded-path", "meow"}, profile);
      Assert.Equal(new List<string>{"cat", "hello", "world"}, profile.Filters.ExcludePaths);

      modify = ArgumentParser.ApplyModify(new[] {"--rm-excluded-path", "hello", "world"}, profile);
      Assert.Equal(new List<string>{"cat"}, profile.Filters.ExcludePaths);
 

      // exclude files
      
      modify = ArgumentParser.ApplyModify(new[] {"--rm-excluded-file", "meow"}, profile);
      Assert.Equal(new List<string>{"cat", "hello", "world"}, profile.Filters.ExcludeFileName);

      modify = ArgumentParser.ApplyModify(new[] {"--rm-excluded-file", "hello", "world"}, profile);
      Assert.Equal(new List<string>{"cat"}, profile.Filters.ExcludeFileName);
 
      // include Folders
      
      modify = ArgumentParser.ApplyModify(new[] {"--rm-included-folder", "meow"}, profile);
      Assert.Equal(new List<string>{"cat", "hello", "world"}, profile.Filters.IncludeFolders);

      modify = ArgumentParser.ApplyModify(new[] {"--rm-included-folder", "hello", "world"}, profile);
      Assert.Equal(new List<string>{"cat"}, profile.Filters.IncludeFolders);

      // include Paths
     
      modify = ArgumentParser.ApplyModify(new[] {"--rm-included-path", "meow"}, profile);
      Assert.Equal(new List<string>{"cat", "hello", "world"}, profile.Filters.IncludePaths);

      modify = ArgumentParser.ApplyModify(new[] {"--rm-included-path", "hello", "world"}, profile);
      Assert.Equal(new List<string>{"cat"}, profile.Filters.IncludePaths);

      // include files
      
      modify = ArgumentParser.ApplyModify(new[] {"--rm-included-file", "meow"}, profile);
      Assert.Equal(new List<string>{"cat", "hello", "world"}, profile.Filters.IncludeFileName);

      modify = ArgumentParser.ApplyModify(new[] {"--rm-included-file", "hello", "world"}, profile);
      Assert.Equal(new List<string>{"cat"}, profile.Filters.IncludeFileName);
    }

    [Fact]
    public void RemovingFromListNoValue()
    {
      var profile = new BackupProfile();

      
      /*
        sources,
      excludeExtensions,
      includeExtensions,
      ExcludeFolders,
      ExcludePaths,
      ExcludeFileNames,
      IncludeFolders,
      IncludePaths,
      IncludeFileNames,
      */   
      
      // Source
      (bool success, bool mod, string? message) modify = ArgumentParser.ApplyModify(new[] {"--rm-src"}, profile);
      Assert.False(modify.success);
      Assert.NotNull(modify.message);

      modify = ArgumentParser.ApplyModify(new[] {"--rm-src", "--source"}, profile);
      Assert.False(modify.success);
      Assert.NotNull(modify.message);

      // excludeExtensions
      modify = ArgumentParser.ApplyModify(new[] {"--rm-excluded-ext"}, profile);
      Assert.False(modify.success);
      Assert.NotNull(modify.message);

      modify = ArgumentParser.ApplyModify(new[] {"--rm-excluded-ext", "--source"}, profile);
      Assert.False(modify.success);
      Assert.NotNull(modify.message);

      // IncludeExtensions
      modify = ArgumentParser.ApplyModify(new[] {"--rm-included-ext"}, profile);
      Assert.False(modify.success);
      Assert.NotNull(modify.message);

      modify = ArgumentParser.ApplyModify(new[] {"--rm-included-ext", "--source"}, profile);
      Assert.False(modify.success);
      Assert.NotNull(modify.message);

      // ExcludeFolders
      modify = ArgumentParser.ApplyModify(new[] {"--rm-excluded-folder"}, profile);
      Assert.False(modify.success);
      Assert.NotNull(modify.message);

      modify = ArgumentParser.ApplyModify(new[] {"--rm-excluded-folder", "--source"}, profile);
      Assert.False(modify.success);
      Assert.NotNull(modify.message);

      // ExcludePaths
      modify = ArgumentParser.ApplyModify(new[] {"--rm-excluded-path"}, profile);
      Assert.False(modify.success);
      Assert.NotNull(modify.message);

      modify = ArgumentParser.ApplyModify(new[] {"--rm-excluded-path", "--source"}, profile);
      Assert.False(modify.success);
      Assert.NotNull(modify.message);

      // ExcludeFileNames
      modify = ArgumentParser.ApplyModify(new[] {"--rm-excluded-file"}, profile);
      Assert.False(modify.success);
      Assert.NotNull(modify.message);

      modify = ArgumentParser.ApplyModify(new[] {"--rm-excluded-file", "--source"}, profile);
      Assert.False(modify.success);
      Assert.NotNull(modify.message);
      
      // IncludeFolders
      modify = ArgumentParser.ApplyModify(new[] {"--rm-included-folder"}, profile);
      Assert.False(modify.success);
      Assert.NotNull(modify.message);

      modify = ArgumentParser.ApplyModify(new[] {"--rm-included-folder", "--source"}, profile);
      Assert.False(modify.success);
      Assert.NotNull(modify.message);

      // IncludePaths
      modify = ArgumentParser.ApplyModify(new[] {"--rm-included-path"}, profile);
      Assert.False(modify.success);
      Assert.NotNull(modify.message);

      modify = ArgumentParser.ApplyModify(new[] {"--rm-included-path", "--source"}, profile);
      Assert.False(modify.success);
      Assert.NotNull(modify.message);
      
      // IncludeFileNames
      modify = ArgumentParser.ApplyModify(new[] {"--rm-included-file"}, profile);
      Assert.False(modify.success);
      Assert.NotNull(modify.message);

      modify = ArgumentParser.ApplyModify(new[] {"--rm-included-file", "--source"}, profile);
      Assert.False(modify.success);
      Assert.NotNull(modify.message);
    }

    [Fact]
    public void RemovingFromListNull()
    {
      var profile = new BackupProfile();

      profile.SourcePaths = null;
      profile.Filters.ExcludeExtensions = null;
      profile.Filters.ExcludeFolders = null;
      profile.Filters.ExcludePaths = null;
      profile.Filters.ExcludeFileName = null;

      (bool success, bool mod, string? message) modify = ArgumentParser.ApplyModify(new[] 
          {
          "--rm-src", "/path/to/source",
          "--rm-excluded-ext", ".txt",
          "--rm-excluded-folder", "folder",
          "--rm-excluded-path", "/path/to/exclude",
          "--rmexcluded-file", "filename"
          }, profile);

      Assert.NotNull(profile.SourcePaths);
      Assert.NotNull(profile.DestinationPath);
      Assert.NotNull(profile.Filters.ExcludeExtensions);
      Assert.NotNull(profile.Filters.ExcludeFolders);
      Assert.NotNull(profile.Filters.ExcludePaths);
      Assert.NotNull(profile.Filters.ExcludeFileName);
    }

    [Fact]
    public void ClearingListsClears()
    {
      /*
      clear sources
      clear exclude extensions
      clear include extensions
      clear exclude folder
      clear exclude path
      clear exclude file name
      clear include folder
      clear include path
      clear include file name
      */

      var profile = new BackupProfile{
        SourcePaths = {"meow", "cat", "hello", "world"},
         
        Filters = new FilterConfig
        {
          ExcludeExtensions = {".txt", ".pdf", ".jpg", ".png"},
          IncludeExtensions = {".txt", ".pdf", ".jpg", ".png"},
          
          ExcludeFolders = {"meow", "cat", "hello", "world"},
          ExcludePaths = {"meow", "cat", "hello", "world"},
          ExcludeFileName = {"meow", "cat", "hello", "world"},

          IncludeFolders = {"meow", "cat", "hello", "world"},
          IncludePaths = {"meow", "cat", "hello", "world"},
          IncludeFileName = {"meow", "cat", "hello", "world"}
        }
      };

      // sources
      (bool success, bool mod, string? message) modify = ArgumentParser.ApplyModify(new[] {"--clr-sources"}, profile);
      Assert.Empty(profile.SourcePaths);

      // ExcludeExtensions
      modify = ArgumentParser.ApplyModify(new[] {"--clr-exclude-ext"}, profile);
      Assert.Empty(profile.Filters.ExcludeExtensions);

      // IncludeExtensions
      modify = ArgumentParser.ApplyModify(new[] {"--clr-include-ext"}, profile);
      Assert.Null(profile.Filters.IncludeExtensions);

      // Exclude folders
      modify = ArgumentParser.ApplyModify(new[] {"--clr-exclude-folder"}, profile);
      Assert.Empty(profile.Filters.ExcludeFolders);

      // exclude paths
      modify = ArgumentParser.ApplyModify(new[] {"--clr-exclude-path"}, profile);
      Assert.Empty(profile.Filters.ExcludePaths);

      // exclude files
      modify = ArgumentParser.ApplyModify(new[] {"--clr-exclude-file"}, profile);
      Assert.Empty(profile.Filters.ExcludeFileName);

      // include folders
      modify = ArgumentParser.ApplyModify(new[] {"--clr-include-folder"}, profile);
      Assert.Null(profile.Filters.IncludeFolders);

      // include paths
      modify = ArgumentParser.ApplyModify(new[] {"--clr-include-path"}, profile);
      Assert.Null(profile.Filters.IncludePaths);

      // include files
      modify = ArgumentParser.ApplyModify(new[] {"--clr-include-file"}, profile);
      Assert.Null(profile.Filters.IncludeFileName); 
    }

    [Fact]
    public void NullifyingNullifyCorrectly()
    {
      /*
        MaxMB
        ModAfter
        ModBefore
        CreatedAfter
        CreatedBefore
        ModifiedWithin
        CreatedWithin
      */

      var profile = new BackupProfile
      {
        Filters = new FilterConfig
        {
          MaxSizeMB = 400,
          ModifiedAfter = new DateTime(2026, 09, 25),
          ModifiedBefore = new DateTime(2026, 09, 25),
          CreatedAfter = new DateTime(2026, 09, 25),
          CreatedBefore = new DateTime(2026, 09, 25),
          ModifiedWithinDays = 3,
          CreatedWithinDays = 6
        }
      };

      (bool success, bool mod, string? message) modify = ArgumentParser.ApplyModify(new[] {"--clr-max-mb"}, profile);
      Assert.Null(profile.Filters.MaxSizeMB);

      modify = ArgumentParser.ApplyModify(new[] {"--clr-mod-after"}, profile);
      Assert.Null(profile.Filters.ModifiedAfter);

      modify = ArgumentParser.ApplyModify(new[] {"--clr-mod-before"}, profile);
      Assert.Null(profile.Filters.ModifiedBefore);

      modify = ArgumentParser.ApplyModify(new[] {"--clr-created-after"}, profile);
      Assert.Null(profile.Filters.CreatedAfter);

      modify = ArgumentParser.ApplyModify(new[] {"--clr-created-before"}, profile);
      Assert.Null(profile.Filters.CreatedBefore);

      modify = ArgumentParser.ApplyModify(new[] {"--clr-modified-within"}, profile);
      Assert.Null(profile.Filters.ModifiedWithinDays);

      modify = ArgumentParser.ApplyModify(new[] {"--clr-created-within"}, profile);
      Assert.Null(profile.Filters.CreatedWithinDays);
    }

    [Fact] 
    public void ValidBooleanValue_SetCorrectly()
    {
      /*
        SkipHidden
        SkipReadOnly
        SkipSystemFiles
        FollowSymlinks
        VerifyAfter

        True
        false
      */
      var profile = new BackupProfile(); 

      // SkipHidden
      (bool success, bool mod, string? message) modify = ArgumentParser.ApplyModify(new[] {"--skip-hidden", "true"}, profile);
      Assert.True(profile.Filters.SkipHidden);

      modify = ArgumentParser.ApplyModify(new[] {"--skip-hidden", "false"}, profile);
      Assert.False(profile.Filters.SkipHidden);

      // SkipReadOnly
      modify = ArgumentParser.ApplyModify(new[] {"--skip-read-only", "true"}, profile);
      Assert.True(profile.Filters.SkipReadOnly);

      modify = ArgumentParser.ApplyModify(new[] {"--skip-read-only", "false"}, profile);
      Assert.False(profile.Filters.SkipReadOnly); 

      // system files
      modify = ArgumentParser.ApplyModify(new[] {"--skip-system-files", "true"}, profile);
      Assert.True(profile.Filters.SkipSystemFiles);

      modify = ArgumentParser.ApplyModify(new[] {"--skip-system-files", "false"}, profile);
      Assert.False(profile.Filters.SkipSystemFiles);

      // symlinks
      modify = ArgumentParser.ApplyModify(new[] {"--follow-symlinks", "true"}, profile);
      Assert.True(profile.Filters.FollowSymlinks);

      modify = ArgumentParser.ApplyModify(new[] {"--follow-symlinks", "false"}, profile);
      Assert.False(profile.Filters.FollowSymlinks);
      
      // verify-after
      modify = ArgumentParser.ApplyModify(new[] {"--verify-after", "true"}, profile);
      Assert.True(profile.copyConfig.VerifyAfterCopy);

      modify = ArgumentParser.ApplyModify(new[] {"--verify-after", "false"}, profile);
      Assert.False(profile.copyConfig.VerifyAfterCopy);
    }

    [Fact]
    public void InvalidBooleanValue_ReturnError()
    {
      // no value
      // numaric value
      // string

      var profile = new BackupProfile();

      // skip hidden
      (bool success, bool mod, string? message) modify = ArgumentParser.ApplyModify(new[] {"--skip-hidden"}, profile);
      Assert.Equal("No value was passed to --skip-hidden. Expected a boolean value (true/false).", modify.message);

      modify = ArgumentParser.ApplyModify(new[] {"--skip-hidden", "67"}, profile);
      Assert.Equal("Invalid value for --skip-hidden: '67'. Expected a boolean value (true/false).", modify.message);

      modify = ArgumentParser.ApplyModify(new[] {"--skip-hidden", "hello"}, profile);
      Assert.Equal("Invalid value for --skip-hidden: 'hello'. Expected a boolean value (true/false).", modify.message);

      // skip read only

      modify = ArgumentParser.ApplyModify(new[] {"--skip-read-only"}, profile);
      Assert.Equal("No value was passed to --skip-read-only. Expected a boolean value (true/false).", modify.message);

      modify = ArgumentParser.ApplyModify(new[] {"--skip-read-only", "67"}, profile);
      Assert.Equal("Invalid value for --skip-read-only: '67'. Expected a boolean value (true/false).", modify.message);

      modify = ArgumentParser.ApplyModify(new[] {"--skip-read-only", "hello"}, profile);
      Assert.Equal("Invalid value for --skip-read-only: 'hello'. Expected a boolean value (true/false).", modify.message);

      // skip system files
      
      modify = ArgumentParser.ApplyModify(new[] {"--skip-system-files"}, profile);
      Assert.Equal("No value was passed to --skip-system-files. Expected a boolean value (true/false).", modify.message);

      modify = ArgumentParser.ApplyModify(new[] {"--skip-system-files", "67"}, profile);
      Assert.Equal("Invalid value for --skip-system-files: '67'. Expected a boolean value (true/false).", modify.message);

      modify = ArgumentParser.ApplyModify(new[] {"--skip-system-files", "hello"}, profile);
      Assert.Equal("Invalid value for --skip-system-files: 'hello'. Expected a boolean value (true/false).", modify.message);

      // follow symlinks
      
      modify = ArgumentParser.ApplyModify(new[] {"--follow-symlinks"}, profile);
      Assert.Equal("No value was passed to --follow-symlinks. Expected a boolean value (true/false).", modify.message);

      modify = ArgumentParser.ApplyModify(new[] {"--follow-symlinks", "67"}, profile);
      Assert.Equal("Invalid value for --follow-symlinks: '67'. Expected a boolean value (true/false).", modify.message);

      modify = ArgumentParser.ApplyModify(new[] {"--follow-symlinks", "hello"}, profile);
      Assert.Equal("Invalid value for --follow-symlinks: 'hello'. Expected a boolean value (true/false).", modify.message);

      // verify-after

      modify = ArgumentParser.ApplyModify(new[] {"--verify-after"}, profile);
      Assert.Equal("No value was passed to --verify-after. Expected a boolean value (true/false).", modify.message);

      modify = ArgumentParser.ApplyModify(new[] {"--verify-after", "67"}, profile);
      Assert.Equal("Invalid value for --verify-after: '67'. Expected a boolean value (true/false).", modify.message);

      modify = ArgumentParser.ApplyModify(new[] {"--verify-after", "hello"}, profile);
      Assert.Equal("Invalid value for --skip-hidden: 'hello'. Expected a boolean value (true/false).", modify.message);
    }

    [Fact]
    public void DuplicateModeTests()
    {
      // overwrite
      // skip
      // keep-new
      // rename
      // invalid value
      // no value

      var profile = new BackupProfile();

      (bool success, bool mod, string? message) modify = ArgumentParser.ApplyModify(new[] {"--set-dup", "overwrite"}, profile);
      Assert.Equal(DuplicateMode.Overwrite, profile.copyConfig.duplicateHandle);

      modify = ArgumentParser.ApplyModify(new[] {"--set-dup", "skip"}, profile);
      Assert.Equal(DuplicateMode.Skip, profile.copyConfig.duplicateHandle);

      modify = ArgumentParser.ApplyModify(new[] {"--set-dup", "keep-new"}, profile);
      Assert.Equal(DuplicateMode.KeepNewer, profile.copyConfig.duplicateHandle);

      modify = ArgumentParser.ApplyModify(new[] {"--set-dup", "rename"}, profile);
      Assert.Equal(DuplicateMode.Rename, profile.copyConfig.duplicateHandle);

      modify = ArgumentParser.ApplyModify(new[] {"--set-dup", "meow"}, profile);
      Assert.Equal("Invalid value for --set-dup: meow", modify.message);

      modify = ArgumentParser.ApplyModify(new[] {"--set-dup"}, profile);
      Assert.Equal("No value was passed to --set-dup. Expected a duplication mode.", modify.message);
    }

    [Fact]
    public void ValidProfileNameReturnsName()
    {
      // This will just input a profile name and check if the GetProfileName function returns the name.
      string[] args = {"--profile", "testProfile"};

      string? name = ArgumentParser.GetProfileName(args);

      Assert.Equal("testProfile", name);
    }

    [Fact]
    public void NoValuesOnProfileGetterReturnsDefault()
    {
      string[] args = {"--profile"};
      
      string? name = ArgumentParser.GetProfileName(args);

      Assert.Equal("default", name);
    }

    [Fact]
    public void NoProfileArgReturnsNull()
    {
      string[] args = {"--source"};

      string? name = ArgumentParser.GetProfileName(args);
      
      Assert.Null(name);
    }


  }
}
