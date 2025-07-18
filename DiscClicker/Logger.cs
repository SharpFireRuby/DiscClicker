using MelonLoader;
using System;
using System.IO;

namespace DiscClicker {
    public class CustomLogger : MelonMod {
        public void LogCustom(string defaultPath, Array message) {
            if (File.Exists(defaultPath + "/dev.txt")) {
                foreach (var item in message) {
                    LoggerInstance.Msg(message);
                }
            }
            else return;
        }
    }
}