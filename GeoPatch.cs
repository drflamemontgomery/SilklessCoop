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
    internal class GeoPatch : MonoBehaviour
    {

        public static ManualLogSource Logger;

        [HarmonyPrefix, HarmonyPatch(typeof(CurrencyManager), "AddGeo")]
        public static void OnAddGeo(int amount)
        {
            try
            {
                Logger.LogInfo($"Gain {amount} Geo(s)");
                GameObject.Find("SilklessCoop").GetComponent<GameSync>().geoEventAmount += amount;
            }
            catch (Exception e)
            {
                Logger.LogError($"Error while adding geo {e}");
            }

        }

        [HarmonyPrefix, HarmonyPatch(typeof(CurrencyManager), "AddShards")]
        public static void OnAddShards(int amount)
        {
            try
            {
                Logger.LogInfo($"Gain {amount} Shards(s)");
                GameObject.Find("SilklessCoop").GetComponent<GameSync>().shardEventAmount += amount;
            }
            catch (Exception e)
            {
                Logger.LogError($"Error while adding shards {e}");
            }

        }
    }
}
