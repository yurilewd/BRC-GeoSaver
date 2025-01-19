using BepInEx;
using BepInEx.Configuration;
using GeoSaver.Apps;
using GeoSaver.Patches;
using HarmonyLib;
using Reptile;
using System.IO;
using UnityEngine;

namespace GeoSaver
{
    [BepInPlugin(MyGUID, PluginName, VersionString)]
    public class GeoSaverPlugin : BaseUnityPlugin
    {
        public const string MyGUID = "com.Yuri.GeoSaver";
        private const string PluginName = "GeoSaver";
        private const string VersionString = "1.0.0";

        public static GeoSaverPlugin Instance { get; private set; }
        public string Dir => Path.GetDirectoryName(Info.Location);

        private Harmony harmony;
        public static Player player;

        // Config variables
        public static ConfigEntry<bool> EnableVelSave;
        public static ConfigEntry<bool> EnableVelForward;
        public static ConfigEntry<bool> EnableUnground;

        public static ConfigEntry<KeyboardShortcut> LoadKey;
        public static ConfigEntry<KeyboardShortcut> SaveKey;

        public static bool moveMe = false;
        public static Vector3 desLoc;
        public static Quaternion desRot;
        public static string desStg;

        private void Awake()
        {
            harmony = new Harmony(MyGUID);
            harmony.PatchAll(typeof(PlayerPatch));

            Instance = this;

            InitConfig();

            GeoSaverApp.Init();
            LoadApp.Init();
            SaveApp.Init();
        }

        private void InitConfig()
        {
            EnableVelSave = Config.Bind("General", "Enable Velocity Retention", false, "Retains current velocity when loading a state in the same stage.");
            EnableVelForward = Config.Bind("General", "Enable Forward Velocity Correction", true, "When velocity is retained adjust the returned velocity to the players new forward direction.");
            EnableUnground = Config.Bind("General", "Enable Force Unground", false, "Will force the player to be airborne for a moment when loading a position, this will often make storage goons trigger automatically.");

            LoadKey = Config.Bind("Controls", "Load last Location Key", new KeyboardShortcut(KeyCode.X), "Hotkey to load the last loaded location, this only applies if the location is in the current stage.");
            SaveKey = Config.Bind("Controls", "Temp Save Location Key", new KeyboardShortcut(KeyCode.Z), "Hotkey to make a temporary save, this does not make a file but can be loaded with the load last location button.");
        }

        private void Update()
        {
            if (player != null)
            {
                if (desStg == Core.instance.baseModule.stageManager.baseModule.currentStage.ToString())
                {
                    if (LoadKey.Value.IsDown())
                    {
                        TempLoadButton();
                    }

                    if (moveMe)
                    {
                        PlacePlayer(desLoc, desRot, false);
                        moveMe = false;
                    }
                }

                if (SaveKey.Value.IsDown())
                {
                    TempSaveButton();
                }
            }
        }

        public static void TempSaveButton()
        {
            desLoc = player.tf.position;
            desRot = player.tf.rotation;
            desStg = Core.instance.baseModule.stageManager.baseModule.currentStage.ToString();
        }

        public static void TempLoadButton()
        {
            PlacePlayer(desLoc, desRot, true);
        }

        public static void PlacePlayer(Vector3 loc, Quaternion rot, bool velSave)
        {
            Vector3 originalVel = player.GetVelocity();
            WorldHandler.instance.PlacePlayerAt(player, loc, rot, true);

            if (EnableUnground.Value)
            {
                player.ForceUnground();
            }

            if (EnableVelSave.Value && velSave)
            {
                Vector3 newVel = EnableVelForward.Value ? AlignVelocityWithForward(originalVel, player.transform.forward) : originalVel;

                if (newVel.y > 0 || EnableUnground.Value)
                {
                    player.ForceUnground();
                }

                player.SetVelocity(newVel);
            }
        }

        private static Vector3 AlignVelocityWithForward(Vector3 velocity, Vector3 forward)
        {
            Vector3 verticalVel = Vector3.up * velocity.y;
            Vector3 horizontalVel = Vector3.ProjectOnPlane(velocity, Vector3.up);
            float horizontalSpeed = horizontalVel.magnitude;
            return (forward * horizontalSpeed) + verticalVel;
        }

    }
}