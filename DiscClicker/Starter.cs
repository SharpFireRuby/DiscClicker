using Il2CppRUMBLE.Managers;
using Il2CppRUMBLE.Players;
using Il2CppRUMBLE.Players.Subsystems;
using MelonLoader;
using RumbleModdingAPI;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DiscClicker {
    public static class BuildInfo {
        public const string Name = "DiscClicker";
        public const string Author = "SharpFireRuby";
        public const string Version = "0.1.4";
    }

    public class DiscClicker : MelonMod {
        private string currentScene;

        private string defaultPath = "UserData/DiscClickerSavs";
        private bool buttonSaved;
        private ulong discPointsLast;
        private uint discPointsPerSecond;
        private float timeDistance;
        private ulong discPointsCalc;

        private ulong discFistMod = 1;
        private bool fistBumped;
        public static uint fistCount;
        public static uint fistCountTotal;
        private static bool firstCall;

        private InGameUiHandler InGameUiHandler = new InGameUiHandler();
        private bool sceneInit = false;
        public bool InGameUiCreated = false;
        private static GameObject poolPlayerBoxAny;
        private static List<GameObject> startedPoolPlayerBoxAny = new List<GameObject>();

        private static bool debug = false;

        public override void OnSceneWasLoaded(int buildIndex, string sceneName) {
            currentScene = sceneName;
            if (currentScene == "Loader") {
                if (!Directory.Exists(defaultPath)) {
                    Directory.CreateDirectory(defaultPath);
                }
            }

            if (currentScene == "Loader") return;
            if (!sceneInit) {
                if (InGameUiCreated) InGameUiHandler.MoveUi(false);
                if (currentScene == "Gym") {
                    if (!InGameUiCreated) {
                        InGameUiHandler.CreateUi();
                        InGameUiCreated = true;
                        poolPlayerBoxAny = GameObject.Find("Game Instance/Pre-Initializable/PoolManager/Pool: PlayerBoxInteractionVFX (RUMBLE.Pools.PooledVisualEffect)");
                    }
                    InGameUiHandler.MoveUi(0.2f, 1.5f, 1.3f, 90, 220, 0, true);
                }
                else if (currentScene == "Park") {
                    InGameUiHandler.MoveUi(-29.3f, -1.5f, -7.5f, 90, 82, 0, true);
                }
                else {
                    InGameUiHandler.MoveUi(false);
                }
            }

            MelonCoroutines.Start(AutoSave());
            MelonCoroutines.Start(AfterSceneLoad());
            sceneInit = true;
        }

        public IEnumerator AfterSceneLoad() {
            yield return null;
            if (InGameUiCreated && currentScene != "Gym" && currentScene != "Park") InGameUiHandler.MoveUi(false);
            LoggerInstance.Msg("Checking and Parsing sav files...");
            if (!File.Exists(defaultPath + "/DiscPoints.sav")) File.Create(defaultPath + "/DiscPoints.sav");
            else discPointsLast = ulong.Parse(File.ReadAllText(defaultPath + "/DiscPoints.sav"));
            if (!File.Exists(defaultPath + "/FistTotal.sav")) File.Create(defaultPath + "/FistTotal.sav");
            else fistCountTotal = uint.Parse(File.ReadAllText(defaultPath + "/FistTotal.sav"));
        }

        public override void OnSceneWasUnloaded(int buildIndex, string sceneName) {
            if (InGameUiCreated) InGameUiHandler.MoveUi(false);

            LoggerInstance.Msg("New scene! Saving all varibles...");
            discPointsCalc = (ulong)(discPointsLast + (timeDistance * discPointsPerSecond) + (fistCount * discFistMod));
            File.WriteAllText(defaultPath + "/DiscPoints.sav", discPointsCalc.ToString());
            fistCount = 0;
            File.WriteAllText(defaultPath + "/FistTotal.sav", fistCountTotal.ToString());
            sceneInit = false;
        }

        public override void OnFixedUpdate() {
            if (currentScene == "Gym" || currentScene == "Park") {
                discPointsCalc = (ulong)(discPointsLast + (timeDistance * discPointsPerSecond) + (fistCount * discFistMod));
                // use the naming system that cookie clicker does, probably with a funtion
                InGameUiHandler.UpdateUi(discPointsCalc); // disckPointsCalcRead
            }

            if (InGameUiCreated) {
                for (int i = 0; i < poolPlayerBoxAny.transform.childCount; i++) {
                    GameObject child = poolPlayerBoxAny.transform.GetChild(i).gameObject;
                    if (!child.active) {
                        startedPoolPlayerBoxAny.Remove(child);
                    }
                }
            }
            if (debug) {
                if (Calls.ControllerMap.RightController.GetSecondary() > 0 && fistBumped == false) {
                    fistBumped = true;
                    fistCount++;
                    fistCountTotal++;
                }
                else if (Calls.ControllerMap.RightController.GetSecondary() <= 0) {
                    fistBumped = false;
                }
                if (Calls.ControllerMap.LeftController.GetSecondary() > 0 && buttonSaved == false) {
                    buttonSaved = true;
                    discPointsCalc = (ulong)(discPointsLast + (timeDistance * discPointsPerSecond) + (fistCount * discFistMod));
                    File.WriteAllText(defaultPath + "/DiscPoints.sav", discPointsLast.ToString());
                    fistCount = 0;
                    File.WriteAllText(defaultPath + "/FistTotal.sav", fistCountTotal.ToString());
                }
                else if (Calls.ControllerMap.LeftController.GetSecondary() <= 0) {
                    buttonSaved = false;
                    return;
                }
            }

            firstCall = false;
        }

        public IEnumerator AutoSave() {
            while (true) {
                yield return new WaitForSeconds(30);
                LoggerInstance.Msg("Auto Saved Discs!");
                discPointsCalc = (ulong)(discPointsLast + (timeDistance * discPointsPerSecond) + (fistCount * discFistMod));
                File.WriteAllText(defaultPath + "/DiscPoints.sav", discPointsCalc.ToString());
                discPointsLast = ulong.Parse(File.ReadAllText(defaultPath + "/DiscPoints.sav"));
                fistCount = 0;
                File.WriteAllText(defaultPath + "/FistTotal.sav", fistCountTotal.ToString());
            }
        }

        public override void OnDeinitializeMelon() {
            LoggerInstance.Msg("Melon Deinitialized, saving Disc count!");
            discPointsLast += (ulong)(timeDistance * discPointsPerSecond) + (fistCount * discFistMod);
            File.WriteAllText(defaultPath + "/DiscPoints.sav", discPointsLast.ToString());
            fistCount = 0;
            File.WriteAllText(defaultPath + "/FistTotal.sav", fistCountTotal.ToString());
        }

        [HarmonyLib.HarmonyPatch(typeof(Il2CppRUMBLE.Players.Subsystems.PlayerBoxInteractionSystem), "OnPlayerBoxInteraction", new Type[] { typeof(PlayerBoxInteractionTrigger), typeof(PlayerBoxInteractionTrigger) })]
        public static class FistBumpDetection {
            private static bool playerFisted = false;

            private static void Postfix(PlayerBoxInteractionTrigger first, PlayerBoxInteractionTrigger second) {
                // this function is called twice per bump, this makes every other trigger
                if (!firstCall) {
                    firstCall = true;
                    return;
                }
                else {
                    firstCall = false;
                }

                for (int i = 0; i < poolPlayerBoxAny.transform.childCount; i++) {
                    GameObject child = poolPlayerBoxAny.transform.GetChild(i).gameObject;
                    if (child.active) {
                        if (!startedPoolPlayerBoxAny.Contains(child)) {
                            bool leftClosest = (IsHandLocalClosest(child.transform.position, 1));
                            bool rightCloset = (IsHandLocalClosest(child.transform.position, 2));
                            if (leftClosest && rightCloset) {
                                playerFisted = true;
                                if (debug) Melon<DiscClicker>.Instance.LoggerInstance.Msg("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
                            }
                            else playerFisted = false;
                        }
                        startedPoolPlayerBoxAny.Add(child);
                    }
                }

                if (!playerFisted) return;

                fistCount++;
                fistCountTotal++;
            }

            private static bool IsHandLocalClosest (Vector3 pos, int handChildIndex) {
                List<Transform> handList = new List<Transform>();
                float closestDistance = float.MaxValue;
                if (debug) Melon<DiscClicker>.Instance.LoggerInstance.Msg(closestDistance + " float.MaxValue");
                Transform closestHand = null;
                int closestHandHash = 0;

                foreach (Player player in PlayerManager.Instance.AllPlayers) {
                    handList.Add(player.Controller.gameObject.transform.GetChild(1).GetChild(handChildIndex));
                    if (debug) Melon<DiscClicker>.Instance.LoggerInstance.Msg(player.Controller.gameObject.transform.GetChild(1).GetChild(handChildIndex).position + " " + handChildIndex);
                }
                foreach (Transform hand in handList) {
                    if (Vector3.Distance(pos, hand.position) < closestDistance) {
                        closestHand = hand;
                        closestHandHash = closestHand.GetHashCode();
                        closestDistance = Vector3.Distance(pos, hand.position);
                        if (debug) {
                            Melon<DiscClicker>.Instance.LoggerInstance.Msg(closestHand.name + " closestHand.name");
                            Melon<DiscClicker>.Instance.LoggerInstance.Msg(closestHandHash + " closestHand.hash");
                            Melon<DiscClicker>.Instance.LoggerInstance.Msg(closestDistance + " closestDistance");
                        }
                    }
                }
                int returnHash = PlayerManager.instance.localPlayer.Controller.gameObject.transform.GetChild(1).GetChild(handChildIndex).GetHashCode();
                if (debug) {
                    Melon<DiscClicker>.Instance.LoggerInstance.Msg(PlayerManager.instance.localPlayer.Controller.gameObject.transform.GetChild(1).GetChild(handChildIndex).name + " return.name");
                    Melon<DiscClicker>.Instance.LoggerInstance.Msg(returnHash + " return.hash");
                }
                return closestHandHash == returnHash;
            }
        }
    }
}

