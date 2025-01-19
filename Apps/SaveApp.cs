using CommonAPI.Phone;
using System.Collections.Generic;
using System.IO;

namespace GeoSaver.Apps
{
    public class SaveApp : CustomApp
    {
        public override bool Available => false;
        public static List<string> currentPath = new List<string>();

        public static void Init()
        {
            PhoneAPI.RegisterApp<SaveApp>("Save Location");
        }

        public override void OnAppInit()
        {
            base.OnAppInit();
            CreateIconlessTitleBar("Save Location");
        }

        public override void OnAppEnable()
        {
            base.OnAppEnable();
            if (ScrollView == null)
            {
                ScrollView = PhoneScrollView.Create(this);
            }
            else
            {
                ScrollView.RemoveAllButtons();
            }
            PopulateList();
        }

        public override void OnAppDisable()
        {
            base.OnAppDisable();
            currentPath.Clear();
        }

        private void PopulateList()
        {
            var (folders, files) = GetAvailableLocations(currentPath);

            if (currentPath.Count > 0)
            {
                var backButton = PhoneUIUtility.CreateSimpleButton("..");
                backButton.OnConfirm += () =>
                {
                    currentPath.RemoveAt(currentPath.Count - 1);
                    OnAppEnable();
                };
                ScrollView.AddButton(backButton);
            }

            var saveButton = PhoneUIUtility.CreateSimpleButton("Save Here");
            saveButton.OnConfirm += () =>
            {
                SaveLocation();
            };
            ScrollView.AddButton(saveButton);

            foreach (string folder in folders)
            {
                var button = CreateFolderButton(folder);
                ScrollView.AddButton(button);
            }
        }

        public static (List<string> folders, List<string> files) GetAvailableLocations(List<string> path)
        {
            var Instance = GeoSaverPlugin.Instance;
            string locationsPath = Path.Combine(Path.GetDirectoryName(Instance.Config.ConfigFilePath), "Locations");
            locationsPath = Path.Combine(locationsPath, Path.Combine(path.ToArray()));

            List<string> folderNames = new List<string>();
            List<string> fileNames = new List<string>();

            if (Directory.Exists(locationsPath))
            {
                string[] folders = Directory.GetDirectories(locationsPath);
                foreach (string folder in folders)
                {
                    folderNames.Add(Path.GetFileName(folder));
                }

                string[] files = Directory.GetFiles(locationsPath, "*.txt");
                foreach (string file in files)
                {
                    fileNames.Add(Path.GetFileNameWithoutExtension(file));
                }
            }
            else
            {
                Directory.CreateDirectory(locationsPath);
            }

            return (folderNames, fileNames);
        }

        private SimplePhoneButton CreateFolderButton(string folderName)
        {
            var button = PhoneUIUtility.CreateSimpleButton(folderName + "/");
            button.OnConfirm += () =>
            {
                currentPath.Add(folderName);
                OnAppEnable();
            };
            return button;
        }

        private void SaveLocation()
        {
            var Instance = GeoSaverPlugin.Instance;
            string locationsPath = Path.Combine(Path.GetDirectoryName(Instance.Config.ConfigFilePath), "Locations");
            string fullPath = Path.Combine(locationsPath, Path.Combine(currentPath.ToArray()));

            LocationFileManager.SaveLocation(fullPath);

            MyPhone.CloseCurrentApp();
        }
    }
}