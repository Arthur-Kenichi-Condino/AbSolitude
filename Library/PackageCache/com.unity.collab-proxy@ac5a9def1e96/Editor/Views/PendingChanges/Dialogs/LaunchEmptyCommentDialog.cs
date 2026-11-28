using Codice.CM.Common;

namespace Unity.PlasticSCM.Editor.Views.PendingChanges.Dialogs
{
    internal class LaunchEmptyCommentDialog : PendingChangesTab.IEmptyCommentDialog
    {
        internal LaunchEmptyCommentDialog(
            IGetUVCSWindow parentWindow,
            WorkspaceInfo wkInfo)
        {
            mParentWindow = parentWindow;
            mWkInfo = wkInfo;
        }

        bool PendingChangesTab.IEmptyCommentDialog.ShouldContinueWithCheckin()
        {
            return EmptyCommentDialog.ShouldContinueWithCheckin(mParentWindow.Get(), mWkInfo);
        }

        bool PendingChangesTab.IEmptyCommentDialog.ShouldContinueWithShelve()
        {
            return EmptyCommentDialog.ShouldContinueWithShelve(mParentWindow.Get(), mWkInfo);
        }

        readonly IGetUVCSWindow mParentWindow;
        readonly WorkspaceInfo mWkInfo;
    }
}
