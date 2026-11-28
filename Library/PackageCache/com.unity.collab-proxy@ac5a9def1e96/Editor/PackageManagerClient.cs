using System;
using System.Linq;

using UnityEditor;

using Codice.LogWrapper;

using PackageManager = UnityEditor.PackageManager;

namespace Unity.PlasticSCM.Editor
{
    internal class PackageManagerClient : PackageInfo.IPackageManagerClient
    {
        void PackageInfo.IPackageManagerClient.SearchLatestCompatibleVersionAsync(string packageName, Action<string> onComplete)
        {
            SearchPackageAsync(PackageManager.Client.Search(packageName), onComplete);
        }

        static void SearchPackageAsync(PackageManager.Requests.SearchRequest searchRequest, Action<string> onCompleted)
        {
            EditorApplication.update += PollSearchRequest;

            void PollSearchRequest()
            {
                if (!searchRequest.IsCompleted)
                    return;

                EditorApplication.update -= PollSearchRequest;

                if (searchRequest.Status != PackageManager.StatusCode.Success)
                {
                    mLog.ErrorFormat("Search failed: {0}", searchRequest.Error.message);
                    return;
                }

                // As we are requesting the data of a single package,
                // we should only ever get a single result, but the API returns a list
                PackageManager.PackageInfo packageInfo = searchRequest.Result.FirstOrDefault();

                if (packageInfo == null)
                {
                    mLog.ErrorFormat("Search returned no results for package {0}", PackageInfo.NAME);
                    return;
                }

                if (packageInfo.name != PackageInfo.NAME)
                {
                    mLog.ErrorFormat("Package {0} doesn't match with {1}", packageInfo.name, PackageInfo.NAME);
                    return;
                }

                onCompleted(packageInfo.versions.latestCompatible);
            }
        }

        static readonly ILog mLog = PlasticApp.GetLogger("PackageManagerClient");
    }
}
