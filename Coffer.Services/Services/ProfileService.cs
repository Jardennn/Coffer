using System.Text.Json;
using System.Text.Json.Nodes;
using Coffer.Services.Models;

namespace Coffer.Services
{
    public class ProfileService
    {
        private static string GetConfigPath(string profileName)
        {
            string configDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Coffer");
            if (!Directory.Exists(configDir))
                Directory.CreateDirectory(configDir);
            return Path.Combine(configDir, $"{profileName}.json");
        }

        public static BackupProfile Load(string profileName = "default")
        {
            string path = GetConfigPath(profileName);

            if (!File.Exists(path))
            {
                if (profileName != "default")
                {
                    Console.WriteLine($"Profile named {profileName} was not found, do you wish to create it?");
                    char confirm = Char.Parse(Console.ReadLine() ?? " ");
                    while (true)
                    {
                        if (char.ToLower(confirm) == 'n')
                        {
                            Console.WriteLine("Profile not created.");
                            Environment.Exit(0);
                        }
                        else if (char.ToLower(confirm) == 'y')
                        {
                            BackupProfile newProfile = new BackupProfile { ProfileName = profileName };
                            Save(newProfile, profileName);
                            Console.WriteLine($"Profile {profileName} saved and loaded.");
                            return newProfile;
                        }
                    }
                }
                else
                {
                    BackupProfile newProfile = new BackupProfile { ProfileName = profileName };
                    Save(newProfile);
                    Console.WriteLine($"Default profile generated.");
                    return newProfile;
                }
            }

            string json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<BackupProfile>(json) ?? new BackupProfile();
        }

        public static void Save(BackupProfile profile, string profileName = "default")
        {
            string path = GetConfigPath(profileName);
            string json = JsonSerializer.Serialize(profile, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(path, json);
        }

        public static void SetActiveProfile(string name = "default")
        {
            string configpath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string fullpath = Path.Combine(configpath, "Coffer", "active.json");

            var active = new { activeProfile = $"{name}" };
            string json = JsonSerializer.Serialize(active, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(fullpath, json);
        }

        public static string GetActiveProfile()
        {
            string configpath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Coffer");
            if (!Directory.Exists(configpath))
                Directory.CreateDirectory(configpath);
            string fullpath = Path.Combine(configpath, "active.json");

            if (!File.Exists(fullpath))
            {
                var active = new { activeProfile = "default" };
                string json = JsonSerializer.Serialize(active, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                File.WriteAllText(fullpath, json);
            }

            string jsonString = File.ReadAllText(fullpath);
            JsonNode jsondata = JsonNode.Parse(jsonString);
            string profilename = jsondata["activeProfile"]?.ToString();

            if (profilename == "" || profilename == null)
            {
                return "default";
            }
            else
            {
                return profilename;
            }
        }

        public static void ListProfiles()
        {
            string configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Coffer");
            var results = new List<FileInfo>();
            var configDir = new DirectoryInfo(configPath);

            var enumeriationOptions = new EnumerationOptions
            {
                IgnoreInaccessible = true,
                RecurseSubdirectories = false
            };

            foreach (var file in configDir.EnumerateFiles("*.json", enumeriationOptions))
            {
                if (file.Name != "active.json")
                {
                    Console.WriteLine(file.Name);
                }
            }
        }

    }
}
