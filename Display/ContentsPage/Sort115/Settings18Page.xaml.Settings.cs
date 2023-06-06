using System;
using Display.Models;
using System.Diagnostics;
using System.IO;
using System.Xml.Serialization;
using Windows.Storage;

namespace Display.ContentsPage.Sort115
{
    public partial class Settings18Page
    {
        private const string SettingsFile = "sort115.xml";

        public Sort115Settings Settings;

        internal void EnsureSettings()
        {
            try
            {
                var applicationData = ApplicationData.Current.LocalSettings.Values[SettingsFile];

                if (applicationData == null)
                {
                    Settings = null;
                }
                else
                {
                    var serializer = new XmlSerializer(typeof(Sort115Settings));
                    Settings = serializer.Deserialize(new StringReader(applicationData.ToString() ?? string.Empty)) as Sort115Settings;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"发生错误：{ex.Message}");

                Settings = null;
                ApplicationData.Current.LocalSettings.Values.Remove(SettingsFile);
            }

            Settings ??= new Sort115Settings();

            Settings.PropertyChanged += Settings_PropertyChanged;
        }

        private void Settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Debug.WriteLine("Settings改变了");
        }
    }
}
