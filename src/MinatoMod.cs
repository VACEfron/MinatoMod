using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.IL2CPP;
using static GameData;
using HarmonyLib;
using Reactor;
using Reactor.Extensions;
using UnhollowerBaseLib;
using UnityEngine;

namespace MinatoMod
{
    [BepInPlugin(Id)]
    [BepInProcess("Among Us.exe")]
    [BepInDependency(ReactorPlugin.Id)]
    public class MinatoMod : BasePlugin
    {
        public const string Id = "minato.role.mod";

        public Harmony Harmony { get; } = new Harmony(Id);        

        public override void Load()
        {
            RegisterCustomRpcAttribute.Register(this);
            AssetBundleHandler.LoadAssetBundle();
            Harmony.PatchAll();
        }

        [HarmonyPatch(typeof(VersionShower), nameof(VersionShower.Start))]
        public static class VersionShowerUpdate
        {
            public static void Postfix(VersionShower __instance)
            {
                __instance.text.Text += "\n\n\n\n\n\n\n\n\n[00FF00FF]Loaded Minato mod[]";
            }
        }

        [HarmonyPriority(Priority.VeryHigh)]
        [HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
        public static class PingTrackerUpdate
        {
            public static void Postfix(PingTracker __instance)
            {
                __instance.text.Text += "\n\n\nMinato by:\n[FF69B4FF]VAC Efron[]";
            }
        }
    }
}
