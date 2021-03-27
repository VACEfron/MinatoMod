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

        public static AssetBundle AssetsBundle;

        public override void Load()
        {
            RegisterCustomRpcAttribute.Register(this);
            LoadAssetBundle();
            Harmony.PatchAll();
        }        

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSetInfected))]
        public static class SetRandomMinato
        {
            public static void Postfix([HarmonyArgument(0)] Il2CppReferenceArray<PlayerInfo> infected)
            {
                var random = new System.Random();

                Func<List<PlayerControl>, PlayerControl> GetRandomFromList = x => x[random.Next(x.Count)];

                var minato = GetRandomFromList(infected.Select(x => x.Object).ToList());                
                Utils.MinatoPlayer = minato;                
                Minato.SetMinatoButtons();

                CustomRpc.HandleCustomRpc(minato.PlayerId, CustomRpc.CustomRpcType.SetMinato);
            }
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
        public static class SaveDeadBodies
        {
            public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
            {
                Utils.DeadBodyLocations[target.PlayerId] = target.transform.position;
            }
        }

        [HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.NextGame))]
        public static class GameEnded
        {
            public static void Postfix(EndGameManager __instance)
            {
                Utils.Reset();
            }
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

        private void LoadAssetBundle()
        {
            byte[] bundleRead = Assembly.GetCallingAssembly().GetManifestResourceStream("MinatoMod.src.Assets.bundle").ReadFully();
            AssetsBundle = AssetBundle.LoadFromMemory(bundleRead);
        }
    }
}
