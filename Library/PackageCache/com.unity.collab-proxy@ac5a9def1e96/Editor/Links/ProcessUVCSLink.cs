using System;
using System.IO;
using System.Threading.Tasks;

using UnityEngine;

using Codice.CM.Common;
using PlasticGui;
using Unity.PlasticSCM.Editor.Api;

namespace Unity.PlasticSCM.Editor.Links
{
    // show/changeset/<id>            → Open Changesets view, select changeset
    // show/branch/<branchName>       → Open Branches view, select branch
    // show/shelve/<id>               → Open Shelves view, select shelve
    // show/history/<filePath>        → Open file history view
    //                                  (filePath is project-relative, e.g. Assets/Foo.cs)
    //
    // open/<ViewType>                → Open a UVCS view tab (uses ViewType enum names)
    // open/BranchExplorerView        → Open the Branch Explorer window
    //
    // refresh/<ViewType>             → Refresh a UVCS view
    //
    // merge/branch/<branchName>      → Launch merge from branch
    // merge/changeset/<id>           → Launch merge from changeset
    // merge/shelve/<id>              → Launch apply shelve UI

    internal static class ProcessUVCSLink
    {
        internal const string MALFORMED_LINK_MSG =
            @"Invalid UVCS link ""{0}"": the link format is not recognized.";

        internal static bool IsMergeLink(string linkPath)
        {
            if (string.IsNullOrEmpty(linkPath))
                return false;

            return SplitOnce(linkPath)[0] == ACTION_MERGE;
        }

        internal static void Execute(
            string linkPath,
            WorkspaceInfo wkInfo,
            IUnityVersionControlApi api)
        {
            string[] parts = SplitOnce(linkPath);
            if (parts.Length < 2)
            {
                LogMalformedLink(linkPath);
                return;
            }

            switch (parts[0])
            {
                case ACTION_SHOW:
                    ExecuteShow(parts[1], linkPath, wkInfo, api);
                    return;
                case ACTION_OPEN:
                    ExecuteOpen(parts[1], linkPath, api);
                    return;
                case ACTION_REFRESH:
                    ExecuteRefresh(parts[1], linkPath, api);
                    return;
                case ACTION_MERGE:
                    ExecuteMerge(parts[1], linkPath, wkInfo, api);
                    return;
                default:
                    LogMalformedLink(linkPath);
                    return;
            }
        }

        static void ExecuteShow(
            string remainder,
            string linkPath,
            WorkspaceInfo wkInfo,
            IUnityVersionControlApi api)
        {
            string[] parts = SplitOnce(remainder);
            if (parts.Length < 2 || string.IsNullOrEmpty(parts[1]))
            {
                LogMalformedLink(linkPath);
                return;
            }

            string entityValue = parts[1];

            switch (parts[0])
            {
                case ENTITY_CHANGESET:
                    long changesetId;
                    if (!long.TryParse(entityValue, out changesetId))
                    {
                        LogMalformedLink(linkPath);
                        return;
                    }

                    LogIfFaulted(api.ShowChangesetsView(wkInfo, changesetId));
                    return;
                case ENTITY_BRANCH:
                    LogIfFaulted(api.ShowBranchesView(wkInfo, BRANCH_SEPARATOR + entityValue));
                    return;
                case ENTITY_SHELVE:
                    long shelveId;
                    if (!long.TryParse(entityValue, out shelveId))
                    {
                        LogMalformedLink(linkPath);
                        return;
                    }

                    LogIfFaulted(api.ShowShelvesView(wkInfo, shelveId));
                    return;
                case ENTITY_HISTORY:
                    LogIfFaulted(api.ShowFileHistory(wkInfo, GetFullPath(entityValue)));
                    return;
                default:
                    LogMalformedLink(linkPath);
                    return;
            }
        }

        static void ExecuteOpen(string remainder, string linkPath, IUnityVersionControlApi api)
        {
            ViewType viewType;
            if (!TryParseViewType(SplitOnce(remainder)[0], out viewType))
            {
                LogMalformedLink(linkPath);
                return;
            }

            if (viewType == ViewType.BranchExplorerView)
            {
                LogIfFaulted(api.ShowBranchExplorer());
                return;
            }

            LogIfFaulted(api.ShowView(viewType));
        }

        static void ExecuteRefresh(string remainder, string linkPath, IUnityVersionControlApi api)
        {
            ViewType viewType;
            if (!TryParseViewType(remainder, out viewType))
            {
                LogMalformedLink(linkPath);
                return;
            }

            LogIfFaulted(api.RefreshView(viewType));
        }

        static void ExecuteMerge(
            string remainder,
            string linkPath,
            WorkspaceInfo wkInfo,
            IUnityVersionControlApi api)
        {
            string[] parts = SplitOnce(remainder);
            if (parts.Length < 2 || string.IsNullOrEmpty(parts[1]))
            {
                LogMalformedLink(linkPath);
                return;
            }

            string entityValue = parts[1];

            switch (parts[0])
            {
                case ENTITY_BRANCH:
                    LogIfFaulted(api.LaunchMergeFromBranchUI(
                        wkInfo, BRANCH_SEPARATOR + entityValue));
                    return;
                case ENTITY_CHANGESET:
                    long changesetId;
                    if (!long.TryParse(entityValue, out changesetId))
                    {
                        LogMalformedLink(linkPath);
                        return;
                    }

                    LogIfFaulted(api.LaunchMergeFromChangesetUI(wkInfo, changesetId));
                    return;
                case ENTITY_SHELVE:
                    long shelveId;
                    if (!long.TryParse(entityValue, out shelveId))
                    {
                        LogMalformedLink(linkPath);
                        return;
                    }

                    LogIfFaulted(api.LaunchApplyShelveUI(wkInfo, shelveId));
                    return;
                default:
                    LogMalformedLink(linkPath);
                    return;
            }
        }

        static bool TryParseViewType(string value, out ViewType viewType)
        {
            return Enum.TryParse(value, out viewType)
                && Enum.IsDefined(typeof(ViewType), viewType);
        }

        static string GetFullPath(string projectRelativePath)
        {
            string projectPath = Path.GetDirectoryName(ApplicationDataPath.Get());

            return Path.GetFullPath(Path.Combine(projectPath, projectRelativePath));
        }

        static string[] SplitOnce(string value)
        {
            return value.Split(new[] { '/' }, 2);
        }

        static void LogMalformedLink(string linkPath)
        {
            Debug.LogWarningFormat(MALFORMED_LINK_MSG, linkPath);
        }

        // The API moves its own work off the UI thread, so nothing here needs to wait on the
        // task. The continuation exists only so a failure is reported instead of being swallowed
        // by an unobserved task.
        static void LogIfFaulted(Task task)
        {
            task.ContinueWith(
                faultedTask => Debug.LogException(faultedTask.Exception),
                TaskContinuationOptions.OnlyOnFaulted);
        }

        const string ACTION_SHOW = "show";
        const string ACTION_OPEN = "open";
        const string ACTION_REFRESH = "refresh";
        const string ACTION_MERGE = "merge";

        const string ENTITY_CHANGESET = "changeset";
        const string ENTITY_BRANCH = "branch";
        const string ENTITY_SHELVE = "shelve";
        const string ENTITY_HISTORY = "history";

        const string BRANCH_SEPARATOR = "/";
    }
}
