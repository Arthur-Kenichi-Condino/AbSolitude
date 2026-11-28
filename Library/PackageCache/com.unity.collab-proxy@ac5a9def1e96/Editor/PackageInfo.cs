using System;
using System.Reflection;

using Codice.LogWrapper;

using PackageManager = UnityEditor.PackageManager;

namespace Unity.PlasticSCM.Editor
{
    internal static class PackageInfo
    {
        internal interface IPackageManagerClient
        {
            void SearchLatestCompatibleVersionAsync(string packageName, Action<string> onComplete);
        }

        internal class VersionData
        {
            internal readonly string Version;

            internal string LatestVersion { get; private set; }

            internal static VersionData Default = new VersionData("0.0.0", "0.0.0");

            internal VersionData(string version, string latestVersion)
            {
                Version = version;
                LatestVersion = latestVersion;
            }

            internal void SetLatestVersion(string latestVersion)
            {
                LatestVersion = latestVersion;
            }

            internal bool IsLatestVersion()
            {
                if (string.IsNullOrEmpty(LatestVersion) ||
                    LatestVersion == Default.LatestVersion)
                    return true;

                return Version == LatestVersion;
            }
        }

        internal static void Initialize()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            PackageManager.PackageInfo packageInfo = FindPackageInfo(assembly);

            if (packageInfo == null)
                return;

            Data = new VersionData(packageInfo.version, VersionData.Default.LatestVersion);

            PackageManagerClient.SearchLatestCompatibleVersionAsync(NAME, OnSearchVersionCompleted);
        }

        static void OnSearchVersionCompleted(string latestCompatibleVersion)
        {
            Data.SetLatestVersion(latestCompatibleVersion);
        }

        internal static VersionData Data = VersionData.Default;
        internal static IPackageManagerClient PackageManagerClient = new PackageManagerClient();
        internal const string NAME = "com.unity.collab-proxy";

        static PackageManager.PackageInfo FindPackageInfo(Assembly assembly)
        {
            PackageManager.PackageInfo packageInfo =
                PackageManager.PackageInfo.FindForAssembly(assembly);

            if (packageInfo == null)
            {
                mLog.DebugFormat("No package found for {0} (dev env plugin)", assembly);
                return null;
            }

            if (packageInfo.name != NAME)
            {
                mLog.ErrorFormat("Package {0} doesn't match with {1}", packageInfo.name, NAME);
                return null;
            }

            mLog.DebugFormat("Package {0} version: {1}", NAME, packageInfo.version);
            return packageInfo;
        }

        static readonly ILog mLog = PlasticApp.GetLogger("PackageInfo");
    }
}
