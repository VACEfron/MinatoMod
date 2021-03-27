using System.Reflection;
using Reactor.Extensions;
using UnityEngine;

namespace MinatoMod
{
    public static class AssetBundleHandler
    {
        private static AssetBundle AssetsBundle;

        public static void LoadAssetBundle()
        {
            byte[] bundleRead = Assembly.GetCallingAssembly().GetManifestResourceStream("MinatoMod.src.Assets.bundle").ReadFully();
            AssetsBundle = AssetBundle.LoadFromMemory(bundleRead);
        }

        public static Object LoadAssetFromBundle(string name)
        {
            return AssetsBundle.LoadAsset(name);
        }
    }
}
