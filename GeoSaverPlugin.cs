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
        private const string VersionString = "1.1.0";

        public static GeoSaverPlugin Instance { get; private set; }
        public string Dir => Path.GetDirectoryName(Info.Location);

        private Harmony harmony;
        public static Player player;

        // Config variables
        public static ConfigEntry<bool> EnableVelSave;

        public static ConfigEntry<bool> EnableVelForward;
        public static ConfigEntry<bool> EnableUnground;
        public static ConfigEntry<bool> EnableBoostRefill;
        public static ConfigEntry<bool> EnableBoostLoad;
        public static ConfigEntry<bool> EnableMovestyleLoad;
        public static ConfigEntry<bool> EnableStorageLoad;

        public static ConfigEntry<KeyboardShortcut> LoadKey;
        public static ConfigEntry<KeyboardShortcut> SaveKey;

        public static ConfigEntry<string> MenuOrder;

        public static bool moveMe = false;
        public static Vector3 desLoc;
        public static Quaternion desRot;
        public static string desStg;
        public static float desStorage;
        public static float desBoost;
        public static MoveStyle desEquippedMoveStyle;
        public static MoveStyle desCurrentMoveStyle;

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
            EnableBoostRefill = Config.Bind("General", "Enable Boost Refill", true, "Refills the boost meter when loading.");
            EnableBoostLoad = Config.Bind("General", "Enable Boost Loading", false, "Loads the saved boost you had when making the save.");
            EnableStorageLoad = Config.Bind("General", "Enable Storage Loading", true, "Loads saved goon storage you had when making the save.");
            EnableMovestyleLoad = Config.Bind("General", "Enable Movestyle Loading", false, "Loads the movestyle you had when making the save.");

            LoadKey = Config.Bind("Controls", "Load last Location Key", new KeyboardShortcut(KeyCode.X), "Hotkey to load the last loaded location, this only applies if the location is in the current stage.");
            SaveKey = Config.Bind("Controls", "Temp Save Location Key", new KeyboardShortcut(KeyCode.Z), "Hotkey to make a temporary save, this does not make a file but can be loaded with the load last location button.");

            MenuOrder = Config.Bind("General", "Menu Order", "Load, Save, TempLoad, TempSave", "Order of the main app menu. List the name seperated by a comma, options can be left out if you don't need them. Valid options are: Load, Save, TempLoad, TempSave");
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
                        PlacePlayer(desLoc, desRot, desStorage, desBoost, desEquippedMoveStyle, desCurrentMoveStyle, false);
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
            desStorage = player.wallrunAbility.lastSpeed;
            desBoost = player.boostCharge;
            desEquippedMoveStyle = player.moveStyleEquipped;
            desCurrentMoveStyle = player.moveStyle;
        }

        public static void TempLoadButton()
        {
            PlacePlayer(desLoc, desRot, desStorage, desBoost, desEquippedMoveStyle, desCurrentMoveStyle, true);
        }

        public static void PlacePlayer(Vector3 loc, Quaternion rot, float storage, float boost, MoveStyle EquippedMoveStyle, MoveStyle CurrentMoveStyle, bool velSave)
        {
            Vector3 originalVel = player.GetVelocity();
            WorldHandler.instance.PlacePlayerAt(player, loc, rot, true);
            if (EnableStorageLoad.Value)
            {
                player.wallrunAbility.lastSpeed = storage;
                player.wallrunAbility.customVelocity = storage * player.wallrunAbility.customVelocity.normalized;
            }

            if (EnableBoostLoad.Value)
            {
                player.boostCharge = boost;
            }
            else if (EnableBoostRefill.Value)
            {
                player.boostCharge = 100f;
            }

            if (EnableMovestyleLoad.Value)
            {
                try
                {
                    player.moveStyleEquipped = EquippedMoveStyle;
                    player.InitMovement(EquippedMoveStyle);
                    bool flag = CurrentMoveStyle != MoveStyle.ON_FOOT;
                    player.SwitchToEquippedMovestyle(flag, false, true, false);
                }
                catch
                {
                    player.moveStyleEquipped = MoveStyle.ON_FOOT;
                    player.InitMovement(MoveStyle.ON_FOOT);
                    player.SwitchToEquippedMovestyle(false, false, true, false);
                }
            }

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