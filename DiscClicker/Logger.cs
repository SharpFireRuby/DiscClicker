using System;
using DiscClicker;
using MelonLoader;
using UnityEngine;
using Il2CppTMPro;
using RumbleModdingAPI;

namespace DiscClicker {
    public class CustomLogger {
        public void LogCustom(array message) {
            if (File.Exists(DiscClicker.defaultPath + "/dev.txt")) {
                foreach (var item in message) {
                    LoggerInstance.Msg(message.toString());
                }
            }
            else return;
        }
    }
}