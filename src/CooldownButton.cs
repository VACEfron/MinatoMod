// Yoinked and modified this from here: https://gist.github.com/Pandapip1/b306f77187ce2f25e73be2560da4aa5f#file-cooldownbutton-cs

using System;
using System.Collections.Generic;
using HarmonyLib;
using Reactor.Extensions;
using UnityEngine;

namespace MinatoMod
{
    public class CooldownButton
    {
        public static List<CooldownButton> Buttons = new List<CooldownButton>();
        public KillButtonManager KillButtonManager;
        public Action OnClick;
        public HudManager HudManager;
        public float MaxTimer = 0f;
        public float Timer = 0f;

        private bool _needsTarget = true;
        private readonly bool _enabled = true;
        private Func<bool> _useTester;
        private readonly Dictionary<string, Texture2D> _cache = new Dictionary<string, Texture2D>();        
        private Vector2 _positionOffset = Vector2.zero;

        private Sprite _sprite;
        private bool _canUse;

        public CooldownButton(Action onClick, float cooldown, float firstCooldown, bool needsTarget, byte[] image, Vector2 positionOffset, Func<bool> useTester, HudManager hudManager)
        {
            SetVars(onClick, cooldown, firstCooldown, needsTarget, image, positionOffset, useTester, hudManager);
            Update();
        }

        private void SetVars(Action onClick, float cooldown, float firstCooldown, bool needsTarget, byte[] image, Vector2 positionOffset, Func<bool> useTester, HudManager hudManager)
        {
            Buttons.Add(this);
            HudManager = hudManager;
            OnClick = onClick;
            _positionOffset = positionOffset;
            _useTester = useTester;
            MaxTimer = cooldown;
            Timer = firstCooldown;
            _needsTarget = needsTarget;
            SetImage(image);
            KillButtonManager = UnityEngine.Object.Instantiate(HudManager.KillButton, HudManager.transform);
            Update();
        }

        private bool CanUse()
        {
            if (PlayerControl.LocalPlayer == null || PlayerControl.LocalPlayer.Data == null)
                return false;
            _canUse = _useTester();
            return true;
        }

        private bool HasTarget()
        {            
            return PlayerControl.LocalPlayer.FindClosestTarget() != null || !_needsTarget;
        }

        public  static void HudUpdate()
        {
            Buttons.RemoveAll(item => item.KillButtonManager == null);
            for (int i = 0; i < Buttons.Count; i++)
            {
                if (Buttons[i].CanUse())
                    Buttons[i].Update();
            }
        }

        private void Update()
        {
            KillButtonManager.transform.localPosition = _positionOffset;
            PassiveButton button = KillButtonManager.GetComponent<PassiveButton>();
            button.OnClick.RemoveAllListeners();
            button.OnClick.AddListener((UnityEngine.Events.UnityAction)listener);
            void listener()
            {
                if (Timer < 0f && _canUse && _enabled && HasTarget() && PlayerControl.LocalPlayer.CanMove)
                {
                    KillButtonManager.renderer.color = new Color(1f, 1f, 1f, 0.3f);
                    Timer = MaxTimer;
                    OnClick();
                }
            }
            KillButtonManager.renderer.sprite = _sprite;
            if (Timer < 0f)
            {
                if (_enabled && HasTarget() && PlayerControl.LocalPlayer.CanMove)
                    KillButtonManager.renderer.color = new Color(1f, 1f, 1f, 1f);
                else
                    KillButtonManager.renderer.color = new Color(1f, 1f, 1f, .3f);
            }
            else
            {
                if (_canUse && PlayerControl.LocalPlayer.CanMove)
                    Timer -= Time.deltaTime;
                KillButtonManager.renderer.color = new Color(1f, 1f, 1f, 0.3f);
                KillButtonManager.renderer.material.SetFloat("_Desat", 0f);
            }
            KillButtonManager.gameObject.SetActive(_canUse && MeetingHud.Instance == null);
            KillButtonManager.renderer.enabled = _canUse && MeetingHud.Instance == null;
            if (_canUse)
            {
                KillButtonManager.renderer.material.SetFloat("_Desat", 0f);
                KillButtonManager.SetCoolDown(Timer, MaxTimer);
            }
        }

        private static string GetHashSHA1(byte[] data)
        {
            using (var sha1 = new System.Security.Cryptography.SHA1CryptoServiceProvider())
            {
                return string.Concat(sha1.ComputeHash(data).Join(x => x.ToString("X2")));
            }
        }

        private static Texture2D ScaleTexture(Texture2D source, int targetWidth, int targetHeight)
        {
            Texture2D result = new Texture2D(targetWidth, targetHeight, source.format, true);
            Color[] rpixels = result.GetPixels(0);
            float incX = (1.0f / targetWidth);
            float incY = (1.0f / targetHeight);
            for (int px = 0; px < rpixels.Length; px++)
            {
                rpixels[px] = source.GetPixelBilinear(incX * ((float)px % targetWidth), incY * ((float)Mathf.Floor(px / targetWidth)));
            }
            result.SetPixels(rpixels, 0);
            result.Apply();
            return result;
        }

        private void SetImage(byte[] value)
        {
            string sha = GetHashSHA1(value);
            if (_cache.ContainsKey(sha))
            {
                _sprite = GUIExtensions.CreateSprite(_cache[sha]);
                return;
            }
            Texture2D tex = GUIExtensions.CreateEmptyTexture();
            ImageConversion.LoadImage(tex, value, false);
            tex = ScaleTexture(tex, 125, 125);
            _cache[sha] = tex;
            _sprite = GUIExtensions.CreateSprite(tex);
        }
    }

    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
    public static class ButtonUpdatePatch
    {
        public static void Postfix(HudManager __instance)
        {
            CooldownButton.HudUpdate();
        }
    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
    public static class MeetingStart
    {
        public static void Postfix(MeetingHud __instance)
        {
            CooldownButton.Buttons.RemoveAll(item => item.KillButtonManager == null);
            for (int i = 0; i < CooldownButton.Buttons.Count; i++)
                if (CooldownButton.Buttons[i] is CooldownButton button)
                    button.Timer = button.MaxTimer;
        }
    }    
}