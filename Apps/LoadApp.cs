using CommonAPI.Phone;
using Reptile;
using System.Collections.Generic;
using System.IO;

namespace GeoSaver.Apps
{
    public class LoadApp : CustomApp
    {
        public override bool Available => false;
        private static List<string> currentPath = new List<string>();

        private static string stage;

        public static void Init()
        {
            PhoneAPI.RegisterApp<LoadApp>("Load Location");
        }

        public override void OnAppInit()
        {
            base.OnAppInit();
            CreateIconlessTitleBar("Load Location");
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
            if (stage != Core.instance.baseModule.stageManager.baseModule.currentStage.ToString() && stage != null)
            {
                currentPath.Clear();
            }
            stage = Core.instance.baseModule.stageManager.baseModule.currentStage.ToString();
            PopulateList();
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

            foreach (string folder in folders)
            {
                var button = CreateFolderButton(folder);
                ScrollView.AddButton(button);
            }

            foreach (string file in files)
            {
                var button = CreateLocationButton(file);
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

        private static SimplePhoneButton CreateLocationButton(string preset)
        {
            var button = PhoneUIUtility.CreateSimpleButton(preset);
            button.OnConfirm += () =>
            {
                LocationFileManager.LoadLocation(Path.Combine(Path.Combine(currentPath.ToArray()), preset));
            };
            return button;
        }
    }
}