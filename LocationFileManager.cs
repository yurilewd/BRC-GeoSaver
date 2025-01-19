using MapStation.API;
using Reptile;
using System;
using System.IO;
using UnityEngine;

namespace GeoSaver
{
    internal class LocationFileManager
    {
        private static string configFolderPath = Path.GetDirectoryName(GeoSaverPlugin.Instance.Config.ConfigFilePath);
        private static string locationsFolderPath = Path.Combine(configFolderPath, "Locations");

        public static void SaveLocation(string customFolderPath = null)
        {
            var player = GeoSaverPlugin.player;
            Vector3 loc = player.tf.position;
            Quaternion rot = player.tf.rotation;
            Stage stg = Core.instance.baseModule.stageManager.baseModule.currentStage;

            Vector3 roundedLoc = new Vector3(
                Mathf.Round(loc.x),
                Mathf.Round(loc.y),
                Mathf.Round(loc.z)
            );

            string saveData = $"{loc.x},{loc.y},{loc.z}|{rot.x},{rot.y},{rot.z},{rot.w}|{stg}";

            string folderPath = customFolderPath ?? locationsFolderPath;
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string fileName = $"{stg}_{roundedLoc.x}_{roundedLoc.y}_{roundedLoc.z}.txt";
            fileName = string.Join("_", fileName.Split(Path.GetInvalidFileNameChars()));
            string filePath = Path.Combine(folderPath, fileName);

            int counter = 1;
            while (File.Exists(filePath))
            {
                filePath = Path.Combine(folderPath, $"{stg}_{roundedLoc.x}_{roundedLoc.y}_{roundedLoc.z}_{counter}.txt");
                counter++;
            }

            File.WriteAllText(filePath, saveData);
        }

        public static bool LoadLocation(string name)
        {
            string filePath = Path.Combine(locationsFolderPath, name + ".txt");
            if (!File.Exists(filePath))
            {
                Debug.LogWarning($"Save file not found at {filePath}");
                return false;
            }

            try
            {
                string[] saveData = File.ReadAllText(filePath).Split('|');

                if (saveData.Length != 3)
                {
                    Debug.LogError("Invalid save data format");
                    return false;
                }

                Vector3 loc = ParseVector3(saveData[0]);
                Quaternion rot = ParseQuaternion(saveData[1]);
                string stageName = saveData[2];

                var player = GeoSaverPlugin.player;
                if (player == null)
                {
                    Debug.LogError("Player reference is null");
                    return false;
                }

                if (stageName.StartsWith("mapstation/"))
                {
                    GeoSaverPlugin.desLoc = loc;
                    GeoSaverPlugin.desRot = rot;
                    GeoSaverPlugin.desStg = stageName;
                    if (stageName != Core.instance.baseModule.stageManager.baseModule.currentStage.ToString())
                    {
                        GeoSaverPlugin.moveMe = true;
                        string cleanStageName = stageName.Remove(0, "mapstation/".Length);
                        if (APIManager.API.GetCustomStageByID(APIManager.API.GetStageID(cleanStageName)) != null)
                        {
                            Stage customStage = (Stage)APIManager.API.GetStageID(cleanStageName);
                            Core.instance.baseModule.stageManager.ExitCurrentStage(customStage);
                        }
                        else
                        {
                            Debug.LogError("Stage not found");
                        }
                    }
                    else
                    {
                        GeoSaverPlugin.PlacePlayer(loc, rot, true);
                    }
                }
                else
                {
                    GeoSaverPlugin.desLoc = loc;
                    GeoSaverPlugin.desRot = rot;
                    GeoSaverPlugin.desStg = stageName;
                    Stage stageEnum = StringToStage(stageName);
                    if (stageEnum != Core.instance.baseModule.stageManager.baseModule.currentStage)
                    {
                        Core.instance.baseModule.stageManager.ExitCurrentStage(stageEnum);

                        GeoSaverPlugin.moveMe = true;
                    }
                    else
                    {
                        GeoSaverPlugin.PlacePlayer(loc, rot, true);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error loading location: {ex.Message}");
                return false;
            }
        }

        public static Stage StringToStage(string stageName)
        {
            return (Stage)Enum.Parse(typeof(Stage), stageName, true);
        }

        private static Vector3 ParseVector3(string vectorString)
        {
            string[] values = vectorString.Split(',');
            return new Vector3(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]));
        }

        private static Quaternion ParseQuaternion(string quaternionString)
        {
            string[] values = quaternionString.Split(',');
            return new Quaternion(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]), float.Parse(values[3]));
        }
    }
}