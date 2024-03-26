using System;
using System.Diagnostics;
using System.IO;
using System.Xml.Serialization;
using Windows.Storage;
using Sort115Settings = Display.Models.Entities._115.Sort115Settings;

namespace Display.Views.Sort115
{
    public partial class Settings18Page
    {
        private const string SettingsFile = "sort115.xml";

        private static Sort115Settings _settings;

        public static Sort115Settings Settings
        {
            get
            {
                if (_settings == null)
                {
                    EnsureSettings();
                }

                return _settings;
            }
        }

        private static void EnsureSettings()
        {
            try
            {
                var applicationData = ApplicationData.Current.LocalSettings.Values[SettingsFile];

                if (applicationData == null)
                {
                    _settings = null;
                }
                else
                {
                    var serializer = new XmlSerializer(typeof(Sort115Settings));
                    _settings = serializer.Deserialize(new StringReader(applicationData.ToString() ?? string.Empty)) as Sort115Settings;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"保存Sort115Settings时发生错误：{ex.Message}");

                _settings = null;
                ApplicationData.Current.LocalSettings.Values.Remove(SettingsFile);
            }

            _settings ??= new Sort115Settings();

            _settings.PropertyChanged += Settings_PropertyChanged;
        }

        private static void Settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            SaveSettings();
        }

        public static void SaveSettings()
        {
            try
            {
                var serializer = new XmlSerializer(typeof(Sort115Settings));
                var stringWriter = new StringWriter();
                serializer.Serialize(stringWriter, Settings);
                var applicationData = stringWriter.ToString();
                ApplicationData.Current.LocalSettings.Values[SettingsFile] = applicationData;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"保存Sort115Settings时发生错误：{ex.Message}");
            }
        }
    }
}
