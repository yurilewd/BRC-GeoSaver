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

        public class SaveData
        {
            public Vector3 Location { get; set; }
            public Quaternion Rotation { get; set; }
            public string Stage { get; set; }
            public float Storage { get; set; }
            public float Boost { get; set; }

            public MoveStyle EquippedMoveStyle { get; set; }
            public MoveStyle CurrentMoveStyle { get; set; }
            // Add new properties here as needed

            public string Serialize()
            {
                return string.Join("|", new string[]
                {
                    $"{Location.x},{Location.y},{Location.z}",
                    $"{Rotation.x},{Rotation.y},{Rotation.z},{Rotation.w}",
                    Stage.ToString(),
                    Storage.ToString(),
                    Boost.ToString(),
                    $"{EquippedMoveStyle},{CurrentMoveStyle}",
                    // Add new fields here as needed
                });
            }

            public static SaveData Deserialize(string data)
            {
                string[] parts = data.Split('|');
                SaveData saveData = new SaveData
                {
                    Location = ParseVector3(parts[0]),
                    Rotation = ParseQuaternion(parts[1]),
                    Stage = parts[2],
                    Storage = parts.Length > 3 ? ParseFloat(parts[3]) : 0f,
                    Boost = parts.Length > 4 ? ParseFloat(parts[4]) : 0f,
                };

                if (parts.Length > 5)
                {
                    (saveData.EquippedMoveStyle, saveData.CurrentMoveStyle) = ParseMoveStyles(parts[5]);
                }
                else
                {
                    saveData.EquippedMoveStyle = saveData.CurrentMoveStyle = MoveStyle.SKATEBOARD;
                }

                return saveData;
            }
        }

        public static void SaveLocation(string customFolderPath = null)
        {
            var player = GeoSaverPlugin.player;
            SaveData saveData = new SaveData
            {
                Location = player.tf.position,
                Rotation = player.tf.rotation,
                Stage = Core.instance.baseModule.stageManager.baseModule.currentStage.ToString(),
                Storage = player.wallrunAbility.lastSpeed,
                Boost = player.boostCharge,
                EquippedMoveStyle = player.moveStyleEquipped,
                CurrentMoveStyle = player.moveStyle
            };

            string folderPath = customFolderPath ?? locationsFolderPath;
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            Vector3 roundedLoc = new Vector3(
                Mathf.Round(saveData.Location.x),
                Mathf.Round(saveData.Location.y),
                Mathf.Round(saveData.Location.z)
            );

            string fileName = $"{saveData.Stage}_{roundedLoc.x}_{roundedLoc.y}_{roundedLoc.z}.txt";
            fileName = string.Join("_", fileName.Split(Path.GetInvalidFileNameChars()));
            string filePath = Path.Combine(folderPath, fileName);

            int counter = 1;
            while (File.Exists(filePath))
            {
                filePath = Path.Combine(folderPath, $"{saveData.Stage}_{roundedLoc.x}_{roundedLoc.y}_{roundedLoc.z}_{counter}.txt");
                counter++;
            }

            File.WriteAllText(filePath, saveData.Serialize());
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
                string saveDataString = File.ReadAllText(filePath);
                SaveData saveData = SaveData.Deserialize(saveDataString);

                var player = GeoSaverPlugin.player;
                if (player == null)
                {
                    Debug.LogError("Player reference is null");
                    return false;
                }

                GeoSaverPlugin.desLoc = saveData.Location;
                GeoSaverPlugin.desRot = saveData.Rotation;
                GeoSaverPlugin.desStg = saveData.Stage.ToString();
                GeoSaverPlugin.desStorage = saveData.Storage;
                GeoSaverPlugin.desBoost = saveData.Boost;
                GeoSaverPlugin.desEquippedMoveStyle = saveData.EquippedMoveStyle;
                GeoSaverPlugin.desCurrentMoveStyle = saveData.CurrentMoveStyle;

                if (saveData.Stage.ToString().StartsWith("mapstation/"))
                {
                    HandleMapstationLoad(saveData);
                }
                else
                {
                    HandleNormalLoad(saveData);
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error loading location: {ex.Message}");
                return false;
            }
        }

        private static void HandleMapstationLoad(SaveData saveData)
        {
            if (saveData.Stage.ToString() != Core.instance.baseModule.stageManager.baseModule.currentStage.ToString())
            {
                GeoSaverPlugin.moveMe = true;
                string cleanStageName = saveData.Stage.ToString().Remove(0, "mapstation/".Length);

                var stageID = APIManager.API.GetStageID(cleanStageName);

                if (APIManager.API.GetCustomStageByID(stageID) != null)
                {
                    Stage customStage = (Stage)stageID;
                    Core.instance.baseModule.stageManager.ExitCurrentStage(customStage);
                }
                else
                {
                    Debug.LogError($"Stage not found: {cleanStageName}");
                }
            }
            else
            {
                GeoSaverPlugin.PlacePlayer(saveData.Location, saveData.Rotation, saveData.Storage, saveData.Boost, saveData.EquippedMoveStyle, saveData.CurrentMoveStyle, true);
            }
        }

        private static void HandleNormalLoad(SaveData saveData)
        {
            Stage stageEnum = ParseStage(saveData.Stage.ToString());
            if (stageEnum != Core.instance.baseModule.stageManager.baseModule.currentStage)
            {
                Core.instance.baseModule.stageManager.ExitCurrentStage(stageEnum);
                GeoSaverPlugin.moveMe = true;
            }
            else
            {
                GeoSaverPlugin.PlacePlayer(saveData.Location, saveData.Rotation, saveData.Storage, saveData.Boost, saveData.EquippedMoveStyle, saveData.CurrentMoveStyle, true);
            }
        }

        public static Stage ParseStage(string stageName)
        {
            return (Stage)Enum.Parse(typeof(Stage), stageName, true);
        }

        public static MoveStyle ParseMovestyle(string moveStyle)
        {
            return (MoveStyle)Enum.Parse(typeof(MoveStyle), moveStyle, true);
        }

        private static (MoveStyle Equipped, MoveStyle Current) ParseMoveStyles(string moveStylesString)
        {
            string[] moveStyles = moveStylesString.Split(',');
            return (
                moveStyles.Length > 0 ? ParseMovestyle(moveStyles[0]) : MoveStyle.SKATEBOARD,
                moveStyles.Length > 1 ? ParseMovestyle(moveStyles[1]) : MoveStyle.ON_FOOT
            );
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

        private static float ParseFloat(string floatString)
        {
            return float.Parse(floatString);
        }
    }
}