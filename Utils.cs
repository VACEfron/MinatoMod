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

        public static void Reset()
        {
            MinatoPlayer = null;
            MinatoTarget = null;
            SealButton = null;
            TeleportButton = null;
            DeadBodyLocations.Clear();
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

        public static PlayerControl FindClosestTarget(PlayerControl player)
        {
            PlayerControl result = null;
            float num = GameOptionsData.KillDistances[Mathf.Clamp(PlayerControl.GameOptions.KillDistance, 0, 2)];
            Vector2 truePosition = player.GetTruePosition();
            List<GameData.PlayerInfo> allPlayers = GameData.Instance.AllPlayers.ToArray().ToList();
            for (int i = 0; i < allPlayers.Count; i++)
            {
                GameData.PlayerInfo playerInfo = allPlayers[i];
                if (!playerInfo.Disconnected && playerInfo.PlayerId != player.PlayerId && !playerInfo.IsDead)
                {
                    PlayerControl target = playerInfo.Object;
                    if (target)
                    {
                        Vector2 vector = target.GetTruePosition() - truePosition;
                        float magnitude = vector.magnitude;
                        if (magnitude <= num && !PhysicsHelpers.AnyNonTriggersBetween(truePosition, vector.normalized, magnitude, Constants.ShipAndObjectsMask))
                        {
                            result = target;
                            num = magnitude;
                        }
                    }
                }
            }
            return result;
        }
    }    
}
