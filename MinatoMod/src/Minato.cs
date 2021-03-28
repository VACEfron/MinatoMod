using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnhollowerBaseLib;
using UnityEngine;
using static GameData;

namespace MinatoMod
{
    public static class Minato
    {
        public static CooldownButton SealButton;
        public static CooldownButton TeleportButton;

        private static AudioClip SealSfx;
        private static AudioClip TeleportSfx;

        public static Vector2 ButtonsPosition = new Vector2(3.22f, -1.1f);

        public static float SealCooldown = 15f;
        public static float TeleportCooldown = 22.5f;        

        // Make a random impostor Minato.
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSetInfected))]
        public static class SetRandomMinato
        {
            public static void Postfix([HarmonyArgument(0)] Il2CppReferenceArray<PlayerInfo> infected)
            {
                var random = new System.Random();

                PlayerControl GetRandomFromList(List<PlayerControl> x)
                    => x[random.Next(x.Count)];

                var minato = GetRandomFromList(infected.Select(x => x.Object).ToList());

                Utils.MinatoPlayer = minato;
                SetMinatoButtons();

                CustomRpc.HandleCustomRpc(minato.PlayerId, CustomRpc.CustomRpcType.SetMinato);
            }
        }

        // Change intro sequence for Minato.
        [HarmonyPatch(typeof(IntroCutscene.CoBegin__d), nameof(IntroCutscene.CoBegin__d.MoveNext))]
        public class IntroScreen
        {
            public static void Postfix(IntroCutscene.CoBegin__d __instance)
            {
                if (GetMinato() == null)
                    return;

                if (PlayerControl.LocalPlayer.Data.IsImpostor)
                    Utils.MinatoPlayer.nameText.Color = Utils.MinatoColor;

                if (!PlayerControl.LocalPlayer.IsMinato())
                    return;

                var scene = __instance.__this;
                scene.Title.Text = "Minato";
                scene.Title.Color = Utils.MinatoColor;
                scene.ImpostorText.gameObject.SetActive(true);
                scene.ImpostorText.Text = "Use your seal to teleport and [FF0000FF]kill[]";
                scene.BackgroundBar.material.color = Utils.MinatoColor;
            }
        }

        // Change task objective for Minato.
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetTasks))]
        public static class Role
        {
            public static void Postfix(PlayerControl __instance)
            {
                if (!__instance.IsMinato())
                    return;

                var task = new GameObject("MinatoTask").AddComponent<ImportantTextTask>();
                task.transform.SetParent(__instance.transform, false);
                task.Text = "[FFD642FF]Use your seal to teleport and kill crewmates\nFake tasks:[]";
                __instance.myTasks[0] = task;
            }
        }

        // Change HUD name color for Minato.
        [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
        public class ColorChange
        {
            public static void Postfix(HudManager __instance)
            {
                var meetingHud = MeetingHud.Instance;

                if (meetingHud != null && PlayerControl.LocalPlayer.Data.IsImpostor)
                    meetingHud.playerStates.FirstOrDefault(x => x.NameText.Text == Utils.MinatoPlayer.nameText.Text).NameText.Color = Utils.MinatoColor; 
            }
        }

        // Reset target & cooldown after meetings.
        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Close))]
        public static class MeetingEnd
        {
            public static void Postfix(MeetingHud __instance)
            {
                SetMinatoTarget(null);
                SealButton.Timer = SealCooldown;
                TeleportButton.Timer = TeleportCooldown;
            }
        }

        public static void SetMinatoButtons()
        {
            TeleportSfx = Assets.LoadAsset("sfx_teleport").Cast<AudioClip>();
            SealSfx = Assets.LoadAsset("sfx_seal").Cast<AudioClip>();

            SealButton = new CooldownButton(
                onClick: () => SetMinatoTarget(PlayerControl.LocalPlayer.FindClosestTarget()),
                cooldown: SealCooldown,
                firstCooldown: 10,
                needsTarget: true,
                sprite: Assets.LoadAsset("SealButton").Cast<GameObject>().GetComponent<SpriteRenderer>().sprite,
                positionOffset: ButtonsPosition,
                useTester: () => !PlayerControl.LocalPlayer.Data.IsDead && PlayerControl.LocalPlayer.IsMinato() && Utils.MinatoTarget == null,
                hudManager: HudManager.Instance
            );
        }

        public static void SetMinatoTarget(PlayerControl target)
        {
            if (target != null)
                SoundManager.Instance.PlaySound(SealSfx, false, .9f);

            Utils.MinatoTarget = target;
            CustomRpc.HandleCustomRpc(target?.PlayerId, CustomRpc.CustomRpcType.SetMinatoTarget);

            if (TeleportButton == null)
            {
                TeleportButton = new CooldownButton(
                    onClick: () => TeleportToTarget(Utils.MinatoTarget),
                    cooldown: TeleportCooldown,
                    firstCooldown: TeleportCooldown,
                    needsTarget: false,
                    sprite: Assets.LoadAsset("TeleportButton").Cast<GameObject>().GetComponent<SpriteRenderer>().sprite,
                    positionOffset: ButtonsPosition,
                    useTester: () => !PlayerControl.LocalPlayer.Data.IsDead && PlayerControl.LocalPlayer.IsMinato() && Utils.MinatoTarget != null,
                    hudManager: HudManager.Instance
                );
            }
        }

        public static void TeleportToTarget(PlayerControl target)
        {
            if (!PlayerControl.LocalPlayer.CanMove)
                return;

            SoundManager.Instance.PlaySound(TeleportSfx, false, 1.5f);

            Utils.MinatoPlayer.transform.position = Utils.DeadBodyLocations.TryGetValue(target.PlayerId, out Vector3 position) ? position : target.transform.position;
            SetMinatoTarget(null);
        }

        public static PlayerControl GetMinato()
        {
            return PlayerControl.AllPlayerControls.ToArray().FirstOrDefault(x => x.PlayerId == Utils.MinatoPlayer?.PlayerId);
        }

        public static bool IsMinato(this PlayerControl player)
        {
            return Utils.MinatoPlayer != null && player.PlayerId == Utils.MinatoPlayer.PlayerId;
        }
    }
}
