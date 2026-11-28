using System;

using UnityEditor;
using UnityEngine;

using Codice.CM.Common;
using PlasticGui;
using Unity.PlasticSCM.Editor.Api;
using Unity.PlasticSCM.Editor.Links;

namespace Unity.PlasticSCM.Editor.Hub
{
    // Hub link handler, process com.unity.editor:// links coming from the Hub
    //
    // com.unity.editor://editor/Unity.PlasticSCM.Editor/show/changeset/555465?repo=codice@codice@cloud
    // com.unity.editor://editor/Unity.PlasticSCM.Editor/merge/branch/main/task001?repo=codice@codice@cloud

    // NOTE: hub links are like:
    // unityhub://editor/project/<cloudProjectId>/Unity.PlasticSCM.Editor/show/changeset/277200?repo=codice@codice@cloud
    // and are transformed by the Hub to com.unity.editor:// links (see above)
    internal static class ProcessHubUVCSLink
    {
        // Needs to be public and static
        // EditorDeeplink discovers handlers with BindingFlags.Public | BindingFlags.Static
        // Only available in 6000.0.75f1+ and 6000.3.0b1+
#if DEEPLINK_HANDLER_AVAILABLE 
        [DeeplinkHandler(UVCS_LINK_NAMESPACE)]
#endif
        public static void Execute(Uri uri)
        {
            string linkPath;
            if (!TryGetLinkPath(uri, out linkPath))
            {
                Debug.LogWarningFormat(ProcessUVCSLink.MALFORMED_LINK_MSG, uri);
                return;
            }

            WorkspaceInfo wkInfo = FindWorkspace.InfoForApplicationPath(
                ApplicationDataPath.Get(), PlasticGui.Plastic.API);

            if (wkInfo == null)
            {
                Debug.LogFormat(NO_WORKSPACE_MSG, uri);
                return;
            }

            string linkRepository = GetQueryValue(uri, REPOSITORY_QUERY_KEY);
            bool bIsTargetRepositoryConfirmed = IsCurrentRepository(
                linkRepository, PlasticGui.Plastic.API.GetRepositorySpec(wkInfo));

            if (!bIsTargetRepositoryConfirmed && ProcessUVCSLink.IsMergeLink(linkPath))
            {
                Debug.LogErrorFormat(MERGE_REFUSED_MSG, uri, linkRepository);
                return;
            }

            if (!bIsTargetRepositoryConfirmed && !string.IsNullOrEmpty(linkRepository))
                Debug.LogWarningFormat(REPOSITORY_MISMATCH_MSG, linkRepository);

            ProcessUVCSLink.Execute(linkPath, wkInfo, new UnityVersionControlApi());
        }

        internal static bool TryGetLinkPath(Uri uri, out string linkPath)
        {
            linkPath = null;

            string[] segments = uri.Segments;
            if (segments.Length < 3)
                return false;

            string[] linkSegments = new string[segments.Length - 2];
            for (int i = 2; i < segments.Length; i++)
            {
                linkSegments[i - 2] = Uri.UnescapeDataString(segments[i].Trim('/'));
            }

            linkPath = string.Join("/", linkSegments);

            return !string.IsNullOrEmpty(linkPath);
        }

        internal static bool IsCurrentRepository(string linkRepository, RepositorySpec wkRepSpec)
        {
            if (string.IsNullOrEmpty(linkRepository) || wkRepSpec == null)
                return false;

            // A repository spec is "name@server" and the server itself can contain '@'
            // (for example "codice@codice@cloud"), so the first separator is the boundary
            int separatorIndex = linkRepository.IndexOf('@');
            if (separatorIndex <= 0 || separatorIndex == linkRepository.Length - 1)
                return false;

            string name = linkRepository.Substring(0, separatorIndex);
            string server = linkRepository.Substring(separatorIndex + 1);

            RepositorySpec linkRepSpec =
                RepositorySpec.BuildFromNameAndResolvedServer(name, server);

            return wkRepSpec.Equals(linkRepSpec);
        }

        internal static string GetQueryValue(Uri uri, string key)
        {
            string query = uri.Query;
            if (string.IsNullOrEmpty(query))
                return null;

            foreach (string pair in query.TrimStart('?').Split('&'))
            {
                int separatorIndex = pair.IndexOf('=');
                if (separatorIndex <= 0)
                    continue;

                string pairKey = pair.Substring(0, separatorIndex);
                if (!string.Equals(pairKey, key, StringComparison.OrdinalIgnoreCase))
                    continue;

                return Uri.UnescapeDataString(pair.Substring(separatorIndex + 1));
            }

            return null;
        }

        internal const string NO_WORKSPACE_MSG =
            @"Cannot process the UVCS link ""{0}"": this project is not in a UVCS workspace.";

        internal const string REPOSITORY_MISMATCH_MSG =
            @"This UVCS link targets the repository ""{0}"", which is not the one this workspace " +
            "is using. Continuing anyway, so the result may not be what the link intended.";

        internal const string MERGE_REFUSED_MSG =
            @"Refused to process the UVCS merge link ""{0}"": it targets the repository ""{1}"", " +
            "which does not match this workspace.";

        // The namespace segment the Hub forwards in com.unity.editor://editor/<namespace>/...
        // It has to stay a prefix of this type's AssemblyQualifiedName or dispatch silently fails.
        const string UVCS_LINK_NAMESPACE = "Unity.PlasticSCM.Editor";

        const string REPOSITORY_QUERY_KEY = "repo";
    }
}
