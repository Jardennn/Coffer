using System.Text.Json;

namespace Coffer.Services
{
  public class ProfileService
  {
    private static string GetConfigPath()
    {
      string configDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
      return Path.Combine(configDir, "Coffer", "profile.json");
    }

    public static BackupProfile load()
    {
      string path = GetConfigPath();

      if (!File.Exists(path))
        return new BackupProfile();

      string json = File.ReadAllText(path);
      return JsonSerializer.Deserialize<BackupProfile>(json) ?? new BackupProfile();
    }

    public static void Save(BackupProfile profile)
    {
      string path = GetConfigPath();
      string json = JsonSerializer.Serialize(profile, new JsonSerializerOptions
          {
          WriteIndented = true
          });

      File.WriteAllText(path, json);
    }
  }
}
