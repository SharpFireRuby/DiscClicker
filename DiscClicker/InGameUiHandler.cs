using System;
using DiscClicker;
using MelonLoader;
using UnityEngine;
using Il2CppTMPro;
using RumbleModdingAPI;

namespace DiscClicker {
    public class InGameUiHandler {
        public GameObject? backBoard;

        public void CreateUi() {
            backBoard = GameObject.Instantiate(GameObject.Find("--------------LOGIC--------------/Heinhouser products/RegionSelector/Model/"));
            GameObject.DontDestroyOnLoad(backBoard);
            for (int i = 0; i < backBoard.transform.childCount; i++) {
                if (i != 2 && i != 7) {
                    GameObject.Destroy(backBoard.transform.GetChild(i).gameObject);
                }
            }
            GameObject.Destroy(backBoard.transform.GetChild(7).GetChild(0).gameObject);
            backBoard.name = "DiscClickerUi";
            backBoard.SetActive(false);
        }
        public void MoveUi(float x, float y, float z, int xr, int yr, int zr, bool visible) {
            backBoard.SetActive(visible);
            backBoard.transform.position = new Vector3(x, y, z);
            backBoard.transform.rotation = Quaternion.Euler(xr, yr, zr);
            backBoard.transform.localScale = Vector3.one * 2;
        }
        public void MoveUi(bool visible) {
            backBoard.SetActive(visible);
        }
        public void UpdateUi(ulong uiDisplay) {
            backBoard.transform.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>().text = uiDisplay.ToString();
        }
    }
}
