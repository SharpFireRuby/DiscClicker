using Il2CppRUMBLE.Managers;
using Il2CppRUMBLE.Players;
using Il2CppRUMBLE.Players.Subsystems;
using MelonLoader;
using MelonLoader.Utils;
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
        public const string Version = "0.1.3";
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
        private static readonly bool debug = false;

        public override void OnSceneWasLoaded(int buildIndex, string sceneName) {
            currentScene = sceneName;
            if (currentScene == "Loader") {
                LoggerInstance.Msg(File.ReadAllText(defaultPath + "/DiscPoints.sav"));
                discPointsLast = ulong.Parse(File.ReadAllText(defaultPath + "/DiscPoints.sav"));
                fistCountTotal = uint.Parse(File.ReadAllText(defaultPath + "/FistTotal.sav"));
            }
            if (currentScene == "Loader") return;
            
            File.WriteAllText(defaultPath + "/DiscPoints.sav", discPointsLast.ToString());
            File.WriteAllText(defaultPath + "/FistTotal.sav", fistCountTotal.ToString());
            if (InGameUiCreated) InGameUiHandler.MoveUi(false);
            if (!sceneInit) {
                if (currentScene == "Gym") {
                    if (!InGameUiCreated) {
                        InGameUiHandler.CreateUi();
                        InGameUiCreated = true;
                        poolPlayerBoxAny = GameObject.Find("Game Instance/Pre-Initializable/PoolManager/Pool: PlayerBoxInteractionVFX (RUMBLE.Pools.PooledVisualEffect)");
                    }
                    InGameUiHandler.MoveUi(0.2f, 1.5f, 1.3f, 90, 220, 0, true);
                }
                else if (currentScene == "Park") {
                    InGameUiHandler.MoveUi(-29.3f, -1.5f, -5.6f, 90, 94, 0, true);
                }
                else {
                    InGameUiHandler.MoveUi(false);
                }
            }
            MelonCoroutines.Start(AutoSave());
            sceneInit = true;
        }

        public override void OnSceneWasUnloaded(int buildIndex, string sceneName) {
            sceneInit = false;
        }

        public override void OnFixedUpdate() {
            if (currentScene == "Gym" || currentScene == "Park") {
                discPointsCalc = (ulong)(discPointsLast + (timeDistance * discPointsPerSecond) + (fistCount * discFistMod));
                // use the naming system that cookie clicker does, probably with a funtion
                InGameUiHandler.UpdateUi(discPointsCalc); // disckPointsCalcRead
                // change the backBoard UI com
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
                if (Calls.ControllerMap.RightController.GetSecondary() > 0 && fistBumped == false) { // Replace with Fist Bump Detector
                    fistBumped = true;
                    fistCount++;
                    fistCountTotal++;
                }
                else if (Calls.ControllerMap.RightController.GetSecondary() <= 0) {
                    fistBumped = false;
                }
                if (Calls.ControllerMap.LeftController.GetSecondary() > 0 && buttonSaved == false) { // Replace with Fist Bump Detector
                    buttonSaved = true;
                    File.WriteAllText(defaultPath + "/DiscPoints.sav", discPointsLast.ToString());
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
                yield return new WaitForSeconds(360);
                File.WriteAllText(defaultPath + "/DiscPoints.sav", discPointsLast.ToString());
                File.WriteAllText(defaultPath + "/FistTotal.sav", fistCountTotal.ToString());
            }
        }

        public override void OnDeinitializeMelon() {
            discPointsLast += (ulong)(timeDistance * discPointsPerSecond) + (fistCount * discFistMod);
            File.WriteAllText(defaultPath + "/DiscPoints.sav", discPointsLast.ToString());
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

