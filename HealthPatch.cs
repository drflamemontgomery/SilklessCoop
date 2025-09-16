using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using HarmonyLib;

namespace SilklessCoop
{

    [HarmonyPatch]
    internal class HealthPatch : MonoBehaviour
    {

        public static ManualLogSource Logger;

        [HarmonyPrefix, HarmonyPatch(typeof(HeroController), "TakeHealth")]
        public static void OnTakeHealth(int amount)
        {
            try
            {
                Logger.LogInfo($"Ouch x{amount}");
                GameObject.Find("SilklessCoop").GetComponent<GameSync>().damage += amount;
            }
            catch (Exception e)
            {
                Logger.LogError($"Error while adding shards {e}");
            }

        }
    }
}

