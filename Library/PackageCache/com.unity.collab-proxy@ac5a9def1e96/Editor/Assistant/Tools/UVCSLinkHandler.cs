#if AIA_PRESENT
using System;
using Codice.CM.Common;
using Unity.AI.Assistant;
using Unity.PlasticSCM.Editor.Links;
using UnityEngine;

namespace Unity.PlasticSCM.Editor.Assistant.Tools
{
    // AIA deeplink handler, process uvcs:// links coming from the AIA window
    //
    // uvcs://show/changeset/555465
    // uvcs://merge/branch/main/task001
    [LinkHandler(UVCS_PREFIX)]
    internal class UVCSLinkHandler : ILinkHandler
    {
        const string UVCS_PREFIX = "uvcs";

        void ILinkHandler.Handle(ILinkHandler.Context context, string prefix, string url)
        {
            if (prefix != UVCS_PREFIX)
                return;

            HandleLink(url);
        }

        internal static void HandleLink(string url)
        {
            FireAndForget(async () =>
            {
                WorkspaceInfo wkInfo = await UVCSToolContext.GetCurrentWorkspaceAsync();

                ProcessUVCSLink.Execute(url, wkInfo, UnityVersionControlApiProvider.Instance);
            });
        }

        static async void FireAndForget(Func<System.Threading.Tasks.Task> action)
        {
            try
            {
                await action();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
    }
}
#endif
