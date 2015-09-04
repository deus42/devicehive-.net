using System;
using System.IO;
using Android.App;
using Android.Content;
using Newtonsoft.Json.Linq;

namespace DeviceHive.Droid
{
    public class SettingManager
    {
        public string SettingPath
        {
            get
            {
                var path = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                var filename = Path.Combine(path, "settings.json");
                return filename;
            }
        }

        public T GetSetting<T>(string name)
        {
            var settings = ReadSettings();

            var value = settings[name];
            return value == null ? default(T) : value.ToObject<T>();
        }

        public void SetSetting<T>(string name, T value)
        {
            var settings = ReadSettings();

            settings[name] = JToken.FromObject(value);
            WriteSettings(settings);
        }

        public void DeleteSetting(string name)
        {
            var settings = ReadSettings();

            settings.Remove(name);
            WriteSettings(settings);
        }

        private JObject ReadSettings()
        {
            return File.Exists(SettingPath) ? JObject.Parse(File.ReadAllText(SettingPath)) : new JObject();
        }

        private void WriteSettings(JObject settings)
        {
            File.WriteAllText(SettingPath, settings.ToString());
        }
    }

    public static class SharedPreferencesExtensions
    {
        public static string GetString(this Application application, string key)
        {
            var prefs = Application.Context.GetSharedPreferences(application.PackageName, FileCreationMode.Private);
            return prefs.GetString(key, null);
        }

        public static void SaveString(this Application application, string key, string value)
        {
            var prefs = Application.Context.GetSharedPreferences(application.PackageName, FileCreationMode.Private);
            var prefEditor = prefs.Edit();
            prefEditor.PutString(key, value);
            prefEditor.Commit();
        }
    }
}
