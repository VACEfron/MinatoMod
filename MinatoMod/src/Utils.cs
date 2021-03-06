using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using static MinatoMod.Minato;
using UnityEngine;

namespace MinatoMod
{
    [HarmonyPatch]
    public static class Utils
    {
        public static PlayerControl MinatoPlayer;
        public static PlayerControl MinatoTarget;
        public static Color MinatoColor = new Color(1, 0.8f, 0.2f);

        public static Dictionary<byte, Vector3> DeadBodyLocations = new Dictionary<byte, Vector3>();

        // Reset after game ends.
        [HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.NextGame))]
        public static class GameEnded
        {
            public static void Postfix(EndGameManager __instance)
            {
                MinatoPlayer = null;
                MinatoTarget = null;
                SealButton = null;
                TeleportButton = null;
                DeadBodyLocations.Clear();
            }
        }

        // Save location of the dead body when a player dies.
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
        public static class SaveDeadBodies
        {
            public static void Postfix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
            {
                DeadBodyLocations[target.PlayerId] = target.transform.position;
            }
        }

        public static List<PlayerControl> GetCrewmates()
        {
            return PlayerControl.AllPlayerControls.ToArray().Where(x => !x.Data.IsImpostor).ToList();
        }

        public static List<PlayerControl> GetImpostors()
        {
            return PlayerControl.AllPlayerControls.ToArray().Where(x => x.Data.IsImpostor).ToList();
        }

        public static PlayerControl GetPlayerById(byte id)
        {
            return PlayerControl.AllPlayerControls.ToArray().FirstOrDefault(x => x.PlayerId == id);
        }

        public static PlayerControl GetPlayerByName(string name)
        {
            return PlayerControl.AllPlayerControls.ToArray().FirstOrDefault(x => x.nameText.Text == name);
        }
    }    
}
