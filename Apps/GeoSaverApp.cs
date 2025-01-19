using CommonAPI;
using CommonAPI.Phone;
using GeoSaver.Apps;
using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;

namespace GeoSaver
{
    public class GeoSaverApp : CustomApp
    {
        private static Sprite IconSprite = null;

        public static void Init()
        {
            string iconPath = Path.Combine(GeoSaverPlugin.Instance.Dir, "GeoSaver_Icon.png");

            try
            {
                IconSprite = TextureUtility.LoadSprite(iconPath);
                PhoneAPI.RegisterApp<GeoSaverApp>("GeoSaver", IconSprite);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading icon sprite: {ex.Message}");
                PhoneAPI.RegisterApp<GeoSaverApp>("GeoSaver", null);
            }
        }

        public override void OnAppInit()
        {
            base.OnAppInit();
            CreateIconlessTitleBar("GeoSaver");

            ScrollView = PhoneScrollView.Create(this);

            var button = PhoneUIUtility.CreateSimpleButton("Save Location");
            button.OnConfirm += () =>
            {
                MyPhone.OpenApp(typeof(SaveApp));
            };

            ScrollView.AddButton(button);

            button = PhoneUIUtility.CreateSimpleButton("Load Location");
            button.OnConfirm += () =>
            {
                MyPhone.OpenApp(typeof(LoadApp));
            };

            ScrollView.AddButton(button);

            button = PhoneUIUtility.CreateSimpleButton("Save Temp Location");
            button.OnConfirm += () =>
            {
                GeoSaverPlugin.TempSaveButton();
            };

            ScrollView.AddButton(button);

            button = PhoneUIUtility.CreateSimpleButton("Load Last Location");
            button.OnConfirm += () =>
            {
                GeoSaverPlugin.TempLoadButton();
            };

            ScrollView.AddButton(button);

            
        }
    }
}