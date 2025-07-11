using DiscClicker;
using Il2CppPhoton.Voice.PUN;
using Il2CppSystem.Xml.Schema;
using MelonLoader;
using MelonLoader.Utils;
using RumbleModdingAPI;
using System;
using System.IO;
using UnityEngine;

namespace DiscClicker {
    public class FileManagerDC {
        private readonly string defaultPath = MelonEnvironment.UserDataDirectory + "\\DiscClickerSavs";

        public ulong ReadFromSav(string file) {
            string temp = File.ReadAllText(defaultPath + file);
            return ulong.Parse(temp);
        }
        public Array ReadFromUp(string file) {
            return null;
        }
        public void WriteToSav(string file, string data) { 
            File.WriteAllText(defaultPath + file, data);
        }
    }
}