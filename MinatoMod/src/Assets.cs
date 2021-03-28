using System.Reflection;
using Reactor.Extensions;
using UnityEngine;

namespace MinatoMod
{
    public class Assets
    {
        private static AssetBundle AssetBundle;

        public static void LoadAssetBundle()
        {
            byte[] bundleRead = Assembly.GetCallingAssembly().GetManifestResourceStream("MinatoMod.src.Assets.bundle").ReadFully();
            AssetBundle = AssetBundle.LoadFromMemory(bundleRead);
        }

        public static Object LoadAsset(string name)
        {
            return AssetBundle.LoadAsset(name);
        }
    }
}

