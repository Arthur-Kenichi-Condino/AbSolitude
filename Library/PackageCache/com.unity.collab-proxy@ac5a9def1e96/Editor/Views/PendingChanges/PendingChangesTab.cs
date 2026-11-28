using System;
using System.Collections.Generic;
using System.IO;

using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

using Codice.Client.BaseCommands;
using Codice.Client.BaseCommands.Config;
using Codice.Client.Commands;
using Codice.Client.Common;
using Codice.Client.Common.FsNodeReaders;
using Codice.Client.Common.Threading;
using Codice.CM.Common;
using Codice.CM.Common.Merge;
using Codice.CM.Common.Mount;
using Codice.LogWrapper;
using GluonGui;
using GluonGui.WorkspaceWindow.Views.Checkin.Operations;
using GluonGui.WorkspaceWindow.Views.Shelves;
using PlasticGui;
using PlasticGui.WorkspaceWindow;
using PlasticGui.WorkspaceWindow.Diff;
using PlasticGui.WorkspaceWindow.Items;
using PlasticGui.WorkspaceWindow.Open;
using PlasticGui.WorkspaceWindow.PendingChanges;
using PlasticGui.WorkspaceWindow.PendingChanges.Changelists;
using Unity.PlasticSCM.Editor.AssetsOverlays.Cache;
using Unity.PlasticSCM.Editor.AssetUtils;
using Unity.PlasticSCM.Editor.AssetUtils.Processor;
using Unity.PlasticSCM.Editor.Settings;
using Unity.PlasticSCM.Editor.StatusBar;
using Unity.PlasticSCM.Editor.Tool;
using Unity.PlasticSCM.Editor.UI;
using Unity.PlasticSCM.Editor.UI.Errors;
using Unity.PlasticSCM.Editor.UI.Progress;
using Unity.PlasticSCM.Editor.UI.Tree;
using Unity.PlasticSCM.Editor.UI.UIElements;
using Unity.PlasticSCM.Editor.UI.UIElements.Tree;
using Unity.PlasticSCM.Editor.Views.PendingChanges.Dialogs;
using Unity.PlasticSCM.Editor.Views.PendingChanges.PendingMergeLinks;
using Unity.PlasticSCM.Editor.Views.Changesets;

using Unity.PlasticSCM.Editor.Diff;

namespace Unity.PlasticSCM.Editor.Views.PendingChanges
{
    internal partial class PendingChangesTab :
        IRefreshableView,
        PendingChangesOptionsFoldout.IAutoRefreshView,
        IPendingChangesView,
        CheckinUIOperation.ICheckinView,
        ShelveOperations.ICheckinView,
        PendingChangesViewPendingChangeMenu.IMetaMenuOperations,
        IPendingChangesMenuOperations,
        IChangelistMenuOperations,
        IOpenMenuOperations,
        PendingChangesViewPendingChangeMenu.IAdvancedUndoMenuOperations,
        IFilesFilterPatternsMenuOperations,
        PendingChangesViewMenu.IGetSelectedNodes,
        ChangesetsTab.IRevertToChangesetListener,
        CommentArea.IPendingChangesTabOperations
    {
        internal interface IEmptyCommentDialog
        {
            bool ShouldContinueWithCheckin();
            bool ShouldContinueWithShelve();
        }
        internal string EmptyStateMessage { get { return mEmptyStatePanel.Text; } }
        internal CommentArea CommentArea { get { return mCommentArea; } }
        internal PlasticTree Table { get { return mTreeControl; } }
        internal IProgressControls ProgressControls { get { return mProgressControls; } }
        internal ToolbarButton RefreshButton { get { return mRefreshButton; } }
        internal ToolbarSearchField SearchField { get { return mSearchField; } }
        internal Label GluonWarningLabel { get { return mGluonWarningLabel; } }

        internal bool IsVisible
        {
            get { return mbIsVisible; }
            set
            {
                mbIsVisible = value;

                mSplitView.SetVisible(value);
                mContentElement.SetVisible(value);
            }
        }

        internal void SetChangesForTesting(List<ChangeInfo> changes)
        {
            UpdateChangesTree(changes);
        }

        internal void SelectChangesForTesting(List<ChangeInfo> changes)
        {
            PendingChangesSelection.SelectChanges(
                mTreeControl, mPendingChangesTree, changes);
        }

        internal void SetMergeLinksForTesting(
            IDictionary<MountPoint, IList<PendingMergeLink>> mergeLinks)
        {
            mPendingMergeLinks = mergeLinks;

            UpdateMergeLinksList();
        }

        internal void SetDrawOperationSuccessForTesting(IDrawOperationSuccess drawOperationSuccess)
        {
            mDrawOperationSuccess = drawOperationSuccess;
            mIsOperationSuccessPendingToDraw = true;
        }

        internal void SetLaunchToolForTesting(
            LaunchTool.IShowDownloadPlasticExeWindow showDownloadPlasticExeWindow,
            LaunchTool.IProcessExecutor processExecutor)
        {
            mShowDownloadPlasticExeWindow = showDownloadPlasticExeWindow;
            mProcessExecutor = processExecutor;
        }

        internal void SetEmptyCommentDialogForTesting(IEmptyCommentDialog emptyCommentDialog)
        {
            mEmptyCommentDialog = emptyCommentDialog;
        }

        internal PendingChangesTab(
            WorkspaceInfo wkInfo,
            RepositorySpec repSpec,
            ViewHost viewHost,
            WorkspaceWindow workspaceWindow,
            IViewSwitcher viewSwitcher,
            IShowChangesetInView showChangesetInView,
            IShowShelveInView showShelveInView,
            IMergeViewLauncher mergeViewLauncher,
            IHistoryViewLauncher historyViewLauncher,
            IAssetStatusCache assetStatusCache,
            ISaveAssets saveAssets,
            LaunchTool.IShowDownloadPlasticExeWindow showDownloadPlasticExeWindow,
            LaunchTool.IProcessExecutor processExecutor,
            WorkspaceOperationsMonitor workspaceOperationsMonitor,
            INewChangesInWk newChangesInWk,
            IPendingChangesUpdater pendingChangesUpdater,
            IIncomingChangesUpdater developerIncomingChangesUpdater,
            IIncomingChangesUpdater gluonIncomingChangesUpdater,
            IShelvedChangesUpdater shelvedChangesUpdater,
            CheckPendingChanges.IUpdatePendingChanges updatePendingChanges,
            WindowStatusBar windowStatusBar,
            IGetUVCSWindow parentWindow,
            bool isGluonMode)
        {
            mWkInfo = wkInfo;
            mRepSpec = repSpec;
            mViewHost = viewHost;
            mWorkspaceWindow = workspaceWindow;
            mViewSwitcher = viewSwitcher;
            mShowChangesetInView = showChangesetInView;
            mShowShelveInView = showShelveInView;
            mHistoryViewLauncher = historyViewLauncher;
            mAssetStatusCache = assetStatusCache;
            mSaveAssets = saveAssets;
            mShowDownloadPlasticExeWindow = showDownloadPlasticExeWindow;
            mProcessExecutor = processExecutor;
            mWorkspaceOperationsMonitor = workspaceOperationsMonitor;
            mNewChangesInWk = newChangesInWk;
            mPendingChangesUpdater = pendingChangesUpdater;
            mDeveloperIncomingChangesUpdater = developerIncomingChangesUpdater;
            mGluonIncomingChangesUpdater = gluonIncomingChangesUpdater;
            mShelvedChangesUpdater = shelvedChangesUpdater;
            mUpdatePendingChanges = updatePendingChanges;
            mWindowStatusBar = windowStatusBar;
            mParentWindow = parentWindow;
            mIsGluonMode = isGluonMode;

            mGuiMessage = new UnityPlasticGuiMessage();
            mCheckedStateManager = new PendingChangesViewCheckedStateManager();
            mEmptyStatePanel = new EmptyStatePanel(mParentWindow.Repaint);

            BuildComponents(isGluonMode);

            mProgressControls = new ProgressControlsForViews();

            workspaceWindow.RegisterPendingChangesProgressControls(mProgressControls);

            mPendingChangesOperations = new PendingChangesOperations(
                wkInfo,
                workspaceWindow,
                viewSwitcher,
                mergeViewLauncher,
                this,
                mProgressControls,
                workspaceWindow,
                pendingChangesUpdater,
                isGluonMode ?
                    gluonIncomingChangesUpdater :
                    developerIncomingChangesUpdater,
                shelvedChangesUpdater,
                null,
                null);

            mEmptyCommentDialog = new LaunchEmptyCommentDialog(mParentWindow, wkInfo);

            Refresh();
        }

        internal void AutoRefresh()
        {
            if (!PlasticGuiConfig.Get().Configuration.CommitAutoRefresh)
                return;

            if (mNewChangesInWk != null && !mNewChangesInWk.Detected())
                return;

            Refresh();
        }

        internal void Refresh(PendingChangesStatus pendingChangesStatus = null)
        {
            if (IsOperationRunning())
                return;

            if (mDeveloperIncomingChangesUpdater != null)
                mDeveloperIncomingChangesUpdater.Update(DateTime.Now);

            if (mGluonIncomingChangesUpdater != null)
                mGluonIncomingChangesUpdater.Update(DateTime.Now);

            if (mShelvedChangesUpdater != null)
                mShelvedChangesUpdater.Update(DateTime.Now);

            FillPendingChanges(mNewChangesInWk, pendingChangesStatus);
        }

        internal void ClearIsCommentWarningNeeded()
        {
            mIsEmptyCheckinCommentWarningNeeded = false;
            mIsEmptyShelveCommentWarningNeeded = false;
        }

        internal void UpdateIsCheckinCommentWarningNeeded(string comment)
        {
            mIsEmptyCheckinCommentWarningNeeded =
                string.IsNullOrEmpty(comment) &&
                PlasticGuiConfig.Get().Configuration.ShowEmptyCommentWarning;

            mNeedsToShowEmptyCheckinCommentDialog = mIsEmptyCheckinCommentWarningNeeded;
        }

        internal void UpdateIsShelveCommentWarningNeeded(string comment)
        {
            mIsEmptyShelveCommentWarningNeeded =
                string.IsNullOrEmpty(comment) &&
                PlasticGuiConfig.Get().Configuration.ShowEmptyShelveCommentWarning;

            mNeedsToShowEmptyShelveCommentDialog = mIsEmptyShelveCommentWarningNeeded;
        }

        internal void OnEnable()
        {

            mTreeControl.OnEnable();

            RegisterHeaderEvents();
            RegisterTreeEvents();

            mCommentArea.OnEnable();

            if (mContentElement.parent == null)
                mParentWindow.Get().rootVisualElement.Add(mContentElement);
        }

        internal void OnDisable()
        {
            List<IPlasticTreeNode> allNodes = mPendingChangesTree.GetAllNodes();

            TreeCheckedStatesSessionState.Save(
                allNodes,
                UnityConstants.PENDING_CHANGES_CHECKED_ITEMS_KEY_NAME,
                UnityConstants.PENDING_CHANGES_UNCHECKED_ITEMS_KEY_NAME);

            SaveCollapsedNodeKeys(allNodes);

            mTreeControl.OnDisable();

            UnRegisterHeaderEvents();
            UnRegisterTreeEvents();

            if (mErrorsPanel != null)
                mErrorsPanel.OnDisable();

            mCommentArea.OnDisable();

            mContentElement.RemoveFromHierarchy();

            mShownDiffChangePath = null;
            GetWindowIfOpened.Diff()?.ClearIfShownFrom(DiffSource.PendingChanges);
        }

        void RegisterTreeEvents()
        {
            mTreeControl.TreeView.selectionChanged += OnTreeSelectionChanged;
            mTreeControl.TreeView.itemsChosen += OnTreeItemsChosen;
            mTreeControl.RegisterCallback<KeyDownEvent>(OnTreeKeyDown);
            mTreeControl.RegisterCallback<ContextClickEvent>(OnTreeContextClick);
            mTreeControl.RegisterCallback<FocusInEvent>(OnTreeFocusIn);
        }

        void UnRegisterTreeEvents()
        {
            mTreeControl.TreeView.selectionChanged -= OnTreeSelectionChanged;
            mTreeControl.TreeView.itemsChosen -= OnTreeItemsChosen;
            mTreeControl.UnregisterCallback<KeyDownEvent>(OnTreeKeyDown);
            mTreeControl.UnregisterCallback<ContextClickEvent>(OnTreeContextClick);
            mTreeControl.UnregisterCallback<FocusInEvent>(OnTreeFocusIn);
        }

        void RegisterHeaderEvents()
        {
            mSearchField.RegisterValueChangedCallback(SearchField_OnValueChanged);
            mSearchField.RegisterCallback<KeyDownEvent>(
                SearchField_OnKeyDown, TrickleDown.TrickleDown);
            mContentElement.RegisterCallback<KeyDownEvent>(ContentElement_OnKeyDown);
        }

        void UnRegisterHeaderEvents()
        {
            mSearchField.UnregisterValueChangedCallback(SearchField_OnValueChanged);
            mSearchField.UnregisterCallback<KeyDownEvent>(
                SearchField_OnKeyDown, TrickleDown.TrickleDown);
            mContentElement.UnregisterCallback<KeyDownEvent>(ContentElement_OnKeyDown);
        }

        internal void Update()
        {
            mProgressControls.UpdateProgress(mParentWindow.Get());

            UpdateProgressOverlay();

            mCommentArea.UpdateOperationRunningState(IsOperationRunning());

            UpdateErrorsPanelVisibility();

            ShowPendingOperationSuccessInStatusBar();

            // Display the empty comment dialog here, otherwise it causes errors in the OnGUI method
            if (mNeedsToShowEmptyCheckinCommentDialog)
            {
                mNeedsToShowEmptyCheckinCommentDialog = false;

                mHasPendingCheckinFromPreviousUpdate =
                    mEmptyCommentDialog.ShouldContinueWithCheckin();

                mIsEmptyCheckinCommentWarningNeeded = !mHasPendingCheckinFromPreviousUpdate;
            }

            if (mNeedsToShowEmptyShelveCommentDialog)
            {
                mNeedsToShowEmptyShelveCommentDialog = false;

                mHasPendingShelveFromPreviousUpdate =
                    mEmptyCommentDialog.ShouldContinueWithShelve();

                mIsEmptyShelveCommentWarningNeeded = !mHasPendingShelveFromPreviousUpdate;
            }

            if (mHasPendingCheckinFromPreviousUpdate)
            {
                mHasPendingCheckinFromPreviousUpdate = false;
                CheckinForMode(mIsGluonMode, mCommentArea.KeepItemsLocked);
            }

            if (mHasPendingShelveFromPreviousUpdate)
            {
                mHasPendingShelveFromPreviousUpdate = false;
                ShelveForMode(mIsGluonMode, mCommentArea.KeepItemsLocked);
            }
        }

        // The tab content is a UI Toolkit element hosted in the window's
        // rootVisualElement; the IMGUI pass only reserves the layout rect the
        // element is positioned over.
        internal void OnGUI(ResolvedUser currentUser)
        {
            mCommentArea.SetCurrentUser(currentUser);

            Rect reservedRect = GUILayoutUtility.GetRect(
                0, 100000, 0, 100000,
                GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            if (Event.current.type == EventType.Repaint)
                UpdateContentElementRect(reservedRect);

            ExecuteAfterOnGUIAction();
        }

        void UpdateContentElementRect(Rect reservedRect)
        {
            mContentElement.style.right = StyleKeyword.Auto;
            mContentElement.style.bottom = StyleKeyword.Auto;
            mContentElement.style.left = reservedRect.x;
            mContentElement.style.top = reservedRect.y;
            mContentElement.style.width = reservedRect.width;
            mContentElement.style.height = reservedRect.height;
        }

        void DoEmptyStateArea()
        {
            Rect rect = GUILayoutUtility.GetRect(0, 100000, 0, 100000);

            if (IsOperationRunning())
                return;

            DrawEmptyState(rect, mEmptyStatePanel, mDrawOperationSuccess);
        }

        void DoErrorsArea()
        {
            mErrorsPanel.OnGUI();
        }

        void DoMergeLinksAreaIfNeeded()
        {
            if (!HasPendingMergeLinks() || mHasPendingMergeLinksFromRevert)
                return;

            Rect availableWidthRect = GUILayoutUtility.GetRect(0, 1, GUILayout.ExpandWidth(true));
            DoMergeLinksArea(mMergeLinksListView, availableWidthRect.width);
        }

        void DoNotificationArea()
        {
            if (!mProgressControls.HasNotification())
                return;

            DrawProgressForViews.ForNotificationArea(mProgressControls.ProgressData);
        }

        void IPendingChangesView.SetDefaultComment(string defaultComment)
        {
        }

        void IPendingChangesView.ClearComments()
        {
            mCommentArea.ClearComments();
        }

        void IRefreshableView.Refresh()
        {
            Refresh();
        }

        void PendingChangesOptionsFoldout.IAutoRefreshView.ForceRefresh()
        {
            ((IRefreshableView)this).Refresh();
        }

        void IPendingChangesView.ClearChangesToCheck(List<string> changes)
        {
            mCheckedStateManager.ClearChangesToCheck(changes);

            mParentWindow.Repaint();
        }

        void IPendingChangesView.CleanCheckedElements(List<ChangeInfo> checkedChanges)
        {
            mCheckedStateManager.Clean(checkedChanges);

            mParentWindow.Repaint();
        }

        void IPendingChangesView.CheckChanges(List<string> changesToCheck)
        {
            mCheckedStateManager.SetChangesToCheck(changesToCheck);

            mParentWindow.Repaint();
        }

        bool IPendingChangesView.IncludeDependencies(
            IList<ChangeDependencies> changesDependencies,
            string operation)
        {
            return DependenciesDialog.IncludeDependencies(
                mWkInfo, changesDependencies, operation, mParentWindow.Get());
        }

        SearchMatchesData IPendingChangesView.AskForMatches(string changePath)
        {
            throw new NotImplementedException();
        }

        void IPendingChangesView.CleanLinkedTasks()
        {
        }

        void CheckinUIOperation.ICheckinView.CollapseWarningMessagePanel()
        {
            UpdateGluonWarningLabel(string.Empty);
        }

        void CheckinUIOperation.ICheckinView.ExpandWarningMessagePanel(string text)
        {
            UpdateGluonWarningLabel(text);
        }

        void CheckinUIOperation.ICheckinView.ClearComments()
        {
            mCommentArea.ClearComments();
        }

        void ShelveOperations.ICheckinView.OnShelvesetApplied(List<ErrorMessage> errorMessages)
        {
            mViewSwitcher.ShowPendingChanges();
            mErrorsPanel.UpdateErrorsList(errorMessages);
        }

        bool PendingChangesViewPendingChangeMenu.IMetaMenuOperations.SelectionHasMeta()
        {
            ChangeInfo selectedChange = PendingChangesSelection.GetSelectedChange(mTreeControl);

            if (selectedChange == null)
                return false;

            return mPendingChangesTree.HasMeta(selectedChange);
        }

        void PendingChangesViewPendingChangeMenu.IMetaMenuOperations.DiffMeta()
        {
            ChangeInfo selectedChange = PendingChangesSelection.GetSelectedChange(mTreeControl);
            ChangeInfo selectedChangeMeta = mPendingChangesTree.GetMetaChange(
                selectedChange);

            ChangeInfo changedForMoved = mPendingChangesTree.GetChangedForMoved(selectedChange);
            ChangeInfo changedForMovedMeta = (changedForMoved == null) ?
                null : mPendingChangesTree.GetMetaChange(changedForMoved);

            IUnityDiffWindow diffWindow = ShowWindow.Diff();

            diffWindow.ShowDiffFromChange(
                selectedChangeMeta,
                changedForMovedMeta,
                showDiffInDesktopApp: () => DiffInDesktopApp(
                    selectedChangeMeta, changedForMovedMeta));

            mShownDiffChangePath = selectedChangeMeta?.Path;
        }

        void PendingChangesViewPendingChangeMenu.IMetaMenuOperations.HistoryMeta()
        {
            ChangeInfo selectedChange = PendingChangesSelection.GetSelectedChange(mTreeControl);
            ChangeInfo selectedChangeMeta = mPendingChangesTree.GetMetaChange(
                selectedChange);

            mHistoryViewLauncher.ShowHistoryView(
                selectedChangeMeta.RepositorySpec,
                selectedChangeMeta.RevInfo.ItemId,
                selectedChangeMeta.Path,
                selectedChangeMeta.IsDirectory);
        }

        void PendingChangesViewPendingChangeMenu.IMetaMenuOperations.AnnotateMeta()
        {
            ChangeInfo selectedChange = PendingChangesSelection
                .GetSelectedChange(mTreeControl);
            ChangeInfo selectedChangeMeta = mPendingChangesTree.GetMetaChange(
                selectedChange);

            if (!AnnotateItemCheck.CanBeAnnotated(
                    GlobalConfig.Instance,
                    mWkInfo,
                    string.Empty,
                    Path.GetFileName(selectedChangeMeta.Path),
                    Path.GetExtension(selectedChangeMeta.Path),
                    selectedChangeMeta.RevInfo))
            {
                AnnotateItemCheck.ShowCantAnnotateAlert();
                return;
            }

            LaunchTool.OpenAnnotate(
                mShowDownloadPlasticExeWindow,
                mProcessExecutor,
                mWkInfo,
                selectedChangeMeta.RepositorySpec,
                selectedChangeMeta.RevInfo,
                WorkspacePath.ClientToCM(selectedChangeMeta.Path, mWkInfo.ClientPath));
        }

        void PendingChangesViewPendingChangeMenu.IMetaMenuOperations.OpenMeta()
        {
            List<string> selectedPaths = PendingChangesSelection.GetSelectedMetaPaths(
                mTreeControl, mPendingChangesTree);

            FileSystemOperation.Open(selectedPaths);
        }

        void PendingChangesViewPendingChangeMenu.IMetaMenuOperations.OpenMetaWith()
        {
            List<string> selectedPaths = PendingChangesSelection.GetSelectedMetaPaths(
                mTreeControl, mPendingChangesTree);

            OpenOperation.OpenWith(
                FileSystemOperation.GetExePath(),
                selectedPaths);
        }

        void PendingChangesViewPendingChangeMenu.IMetaMenuOperations.OpenMetaInExplorer()
        {
            List<string> selectedPaths = PendingChangesSelection.GetSelectedMetaPaths(
                mTreeControl, mPendingChangesTree);

            if (selectedPaths.Count < 1)
                return;

            FileSystemOperation.OpenInExplorer(selectedPaths[0]);
        }

        SelectedChangesGroupInfo IPendingChangesMenuOperations.GetSelectedChangesGroupInfo()
        {
            return PendingChangesSelection.GetSelectedChangesGroupInfo(
                mWkInfo.ClientPath, mTreeControl);
        }

        void IPendingChangesMenuOperations.Diff()
        {
            ChangeInfo selectedChange = PendingChangesSelection.GetSelectedChange(mTreeControl);
            ChangeInfo changedForMoved = mPendingChangesTree.GetChangedForMoved(selectedChange);

            IUnityDiffWindow diffWindow = ShowWindow.Diff();
            diffWindow.ShowDiffFromChange(
                selectedChange,
                changedForMoved,
                BuildDiffInDesktopAppAction(selectedChange, changedForMoved));

            mShownDiffChangePath = selectedChange?.Path;
        }

        void DiffInDesktopApp(ChangeInfo change, ChangeInfo changedForMoved)
        {
            DiffOperation.DiffWorkspaceContent(
                mWkInfo,
                change,
                changedForMoved,
                mProgressControls,
                PlasticExeLauncher.BuildForDiffWorkspaceContent(mWkInfo, mIsGluonMode, mShowDownloadPlasticExeWindow),
                null);
        }

        void IPendingChangesMenuOperations.UndoChanges()
        {
            List<ChangeInfo> changesToUndo = PendingChangesSelection.GetSelectedChanges(
                mTreeControl, mPendingChangesTree, bExcludePrivates: true);

            List<ChangeInfo> dependenciesCandidates =
                mPendingChangesTree.GetDependenciesCandidates(changesToUndo, true);

            UndoChangesForMode(mIsGluonMode, false, changesToUndo, dependenciesCandidates);
        }

        void IPendingChangesMenuOperations.SearchMatches()
        {
            ChangeInfo selectedChange = PendingChangesSelection.GetSelectedChange(mTreeControl);

            if (selectedChange == null)
                return;

            SearchMatchesOperation operation = new SearchMatchesOperation(
                mWkInfo,
                mWorkspaceWindow,
                this,
                mProgressControls,
                mPendingChangesUpdater,
                mDeveloperIncomingChangesUpdater,
                null);

            operation.SearchMatches(
                selectedChange,
                mPendingChangesTree.GetAllChanges(),
                null);
        }

        void IPendingChangesMenuOperations.ApplyLocalChanges()
        {
            List<ChangeInfo> selectedChanges = PendingChangesSelection.GetSelectedChanges(
                mTreeControl, mPendingChangesTree);

            if (selectedChanges.Count == 0)
                return;

            ApplyLocalChangesOperation operation = new ApplyLocalChangesOperation(
                mWkInfo,
                mWorkspaceWindow,
                this,
                mProgressControls,
                mPendingChangesUpdater,
                mDeveloperIncomingChangesUpdater,
                null);

            operation.ApplyLocalChanges(
                selectedChanges,
                mPendingChangesTree.GetAllChanges(),
                null);
        }

        void IPendingChangesMenuOperations.CopyFilePath(bool relativePath)
        {
            EditorGUIUtility.systemCopyBuffer = GetFilePathList.FromSelectedPaths(
                PendingChangesSelection.GetSelectedPathsWithoutMeta(mTreeControl),
                relativePath,
                mWkInfo.ClientPath);
        }

        void IPendingChangesMenuOperations.Delete()
        {
            List<string> privateDirectoriesToDelete;
            List<string> privateFilesToDelete;

            if (!PendingChangesSelection.GetSelectedPathsToDelete(
                    mTreeControl,
                    mPendingChangesTree,
                    out privateDirectoriesToDelete,
                    out privateFilesToDelete))
                return;

            DeleteOperation.Delete(
                mWorkspaceWindow,
                mProgressControls,
                mWkInfo,
                privateDirectoriesToDelete,
                privateFilesToDelete,
                mPendingChangesUpdater,
                mIsGluonMode ?
                    mGluonIncomingChangesUpdater :
                    mDeveloperIncomingChangesUpdater,
                mShelvedChangesUpdater,
                () => RefreshAsset.UnityAssetDatabase(mAssetStatusCache));
        }

        void IPendingChangesMenuOperations.Annotate()
        {
            ChangeInfo selectedChange = PendingChangesSelection.
                GetSelectedChange(mTreeControl);

            if (!AnnotateItemCheck.CanBeAnnotated(
                    GlobalConfig.Instance,
                    mWkInfo,
                    string.Empty,
                    Path.GetFileName(selectedChange.Path),
                    Path.GetExtension(selectedChange.Path),
                    selectedChange.RevInfo))
            {
                AnnotateItemCheck.ShowCantAnnotateAlert();
                return;
            }

            LaunchTool.OpenAnnotate(
                mShowDownloadPlasticExeWindow,
                mProcessExecutor,
                mWkInfo,
                selectedChange.RepositorySpec,
                selectedChange.RevInfo,
                WorkspacePath.ClientToCM(selectedChange.Path, mWkInfo.ClientPath));
        }

        void IPendingChangesMenuOperations.History()
        {
            ChangeInfo selectedChange = PendingChangesSelection.GetSelectedChange(mTreeControl);

            mHistoryViewLauncher.ShowHistoryView(
                selectedChange.RepositorySpec,
                selectedChange.RevInfo.ItemId,
                selectedChange.Path,
                selectedChange.IsDirectory);
        }

        SelectedChangesGroupInfo IChangelistMenuOperations.GetSelectedChangesGroupInfo()
        {
            return PendingChangesSelection.GetSelectedChangesGroupInfo(
                mWkInfo.ClientPath, mTreeControl);
        }

        List<ChangeListInfo> IChangelistMenuOperations.GetSelectedChangelistInfos()
        {
            return PendingChangesSelection.GetSelectedChangeListInfos(mTreeControl);
        }

        void IChangelistMenuOperations.Checkin()
        {
            List<ChangeInfo> changesToCheckin;
            List<ChangeInfo> dependenciesCandidates;

            mPendingChangesTree.GetCheckedChanges(
                PendingChangesSelection.GetSelectedChangelistNodes(mTreeControl),
                false, out changesToCheckin, out dependenciesCandidates);

            CheckinChangesForMode(
                changesToCheckin,
                dependenciesCandidates,
                mIsGluonMode,
                mCommentArea.KeepItemsLocked);
        }

        void IChangelistMenuOperations.Shelve()
        {
            List<ChangeInfo> changesToShelve;
            List<ChangeInfo> dependenciesCandidates;

            mPendingChangesTree.GetCheckedChanges(
                PendingChangesSelection.GetSelectedChangelistNodes(mTreeControl),
                false, out changesToShelve, out dependenciesCandidates);

            ShelveChangesForMode(
                changesToShelve,
                dependenciesCandidates,
                mIsGluonMode,
                mCommentArea.KeepItemsLocked);
        }

        void IChangelistMenuOperations.Undo()
        {
            List<ChangeInfo> changesToUndo;
            List<ChangeInfo> dependenciesCandidates;

            mPendingChangesTree.GetCheckedChanges(
                PendingChangesSelection.GetSelectedChangelistNodes(mTreeControl),
                true, out changesToUndo, out dependenciesCandidates);

            UndoChangesForMode(mIsGluonMode, false, changesToUndo, dependenciesCandidates);
        }

        void IChangelistMenuOperations.CreateNew()
        {
            ChangelistCreationData changelistCreationData =
                CreateChangelistDialog.CreateChangelist(mWkInfo, mParentWindow.Get());

            ChangelistOperations.CreateNew(mWkInfo, this, changelistCreationData);
        }

        void IChangelistMenuOperations.MoveToNewChangelist(List<ChangeInfo> changes)
        {
            ChangelistCreationData changelistCreationData =
                CreateChangelistDialog.CreateChangelist(mWkInfo, mParentWindow.Get());

            if (!changelistCreationData.Result)
                return;

            ChangelistOperations.CreateNew(mWkInfo, this, changelistCreationData);

            ChangelistOperations.MoveToChangelist(
                mWkInfo, this, changes,
                changelistCreationData.ChangelistInfo.Name);
        }

        void IChangelistMenuOperations.Edit()
        {
            ChangeListInfo changelistToEdit = PendingChangesSelection.GetSelectedChangeListInfo(mTreeControl);

            ChangelistCreationData changelistCreationData = CreateChangelistDialog.EditChangelist(
                mWkInfo,
                changelistToEdit,
                mParentWindow.Get());

            ChangelistOperations.Edit(mWkInfo, this, changelistToEdit, changelistCreationData);
        }

        void IChangelistMenuOperations.Delete()
        {
            ChangelistOperations.Delete(
                mWkInfo,
                this,
                PendingChangesSelection.GetSelectedChangelistNodes(mTreeControl));
        }

        void IChangelistMenuOperations.MoveToChangelist(
            List<ChangeInfo> changes,
            string targetChangelist)
        {
            ChangelistOperations.MoveToChangelist(
                mWkInfo,
                this,
                changes,
                targetChangelist);
        }

        void IOpenMenuOperations.Open()
        {
            List<string> selectedPaths = PendingChangesSelection.GetSelectedPathsWithoutMeta(mTreeControl);

            FileSystemOperation.Open(selectedPaths);
        }

        void IOpenMenuOperations.OpenWith()
        {
            List<string> selectedPaths = PendingChangesSelection.GetSelectedPathsWithoutMeta(mTreeControl);

            OpenOperation.OpenWith(
                FileSystemOperation.GetExePath(),
                selectedPaths);
        }

        void IOpenMenuOperations.OpenWithCustom(string exePath, string args)
        {
            List<string> selectedPaths = PendingChangesSelection.GetSelectedPathsWithoutMeta(mTreeControl);

            OpenOperation.OpenWith(exePath, selectedPaths);
        }

        void IOpenMenuOperations.OpenInExplorer()
        {
            List<string> selectedPaths = PendingChangesSelection.GetSelectedPathsWithoutMeta(mTreeControl);

            if (selectedPaths.Count < 1)
                return;

            FileSystemOperation.OpenInExplorer(selectedPaths[0]);
        }

        void IFilesFilterPatternsMenuOperations.AddFilesFilterPatterns(
            FilterTypes type, FilterActions action, FilterOperationType operation)
        {
            List<string> selectedPaths = PendingChangesSelection.GetSelectedPaths(
                mTreeControl, mPendingChangesTree);

            string[] rules = FilterRulesGenerator.GenerateRules(
                selectedPaths, mWkInfo.ClientPath, action, operation);

            bool isApplicableToAllWorkspaces = !mIsGluonMode;
            bool isAddOperation = operation == FilterOperationType.Add;

            FilterRulesConfirmationData filterRulesConfirmationData =
                FilterRulesConfirmationDialog.AskForConfirmation(
                    rules, isAddOperation, isApplicableToAllWorkspaces, mParentWindow.Get());

            AddFilesFilterPatternsOperation.Run(
                mWkInfo,
                mWorkspaceWindow,
                type,
                operation,
                filterRulesConfirmationData,
                mPendingChangesUpdater);
        }

        void PendingChangesViewPendingChangeMenu.IAdvancedUndoMenuOperations.UndoUnchanged()
        {
            UndoUnchangedChanges(PendingChangesSelection.GetSelectedChanges(
                mTreeControl, mPendingChangesTree, bExcludePrivates: true));
        }

        void PendingChangesViewPendingChangeMenu.IAdvancedUndoMenuOperations.UndoCheckoutsKeepingChanges()
        {
            UndoCheckoutChangesKeepingLocalChanges(
                PendingChangesSelection.GetSelectedChanges(
                    mTreeControl, mPendingChangesTree, bExcludePrivates: true));
        }

        List<IPlasticTreeNode> PendingChangesViewMenu.IGetSelectedNodes.GetSelectedNodes()
        {
            return mTreeControl.GetAllSelection();
        }

        void ChangesetsTab.IRevertToChangesetListener.OnSuccessOperation()
        {
            mHasPendingMergeLinksFromRevert = true;
        }

        void SearchField_OnValueChanged(ChangeEvent<string> evt)
        {
            mSearchText = evt.newValue;
            mDelayedFilterAction.Run();
        }

        void SearchField_OnKeyDown(KeyDownEvent evt)
        {
            if (KeyboardEvents.IsEscapePressed(evt) && !string.IsNullOrEmpty(mSearchField.value))
            {
                evt.StopPropagation();

                mSearchField.value = string.Empty;
                return;
            }

            if (evt.keyCode != KeyCode.UpArrow && evt.keyCode != KeyCode.DownArrow)
                return;

            evt.StopPropagation();

            SearchField_OnDownOrUpArrowKeyPressed();
        }

        void ContentElement_OnKeyDown(KeyDownEvent evt)
        {
            if (!KeyboardEvents.IsFindShortcutPressed(evt))
                return;

            evt.StopPropagation();

            mSearchField.Q<TextField>().Focus();
        }

        void SearchField_OnDownOrUpArrowKeyPressed()
        {
            mTreeControl.FocusTree();

            if (mTreeControl.HasSelectedRows())
                return;

            PendingChangesSelection.SelectChanges(
                mTreeControl, mPendingChangesTree, null);
        }

        void ClearOperationSuccess()
        {
            if (mIsOperationSuccessPendingToDraw)
                return;

            mDrawOperationSuccess = null;
        }

        void FillPendingChanges(
            INewChangesInWk newChangesInWk, PendingChangesStatus pendingChangesStatus)
        {
            if (mIsRefreshing)
                return;

            mIsRefreshing = true;

            ClearOperationSuccess();

            List<ChangeInfo> changesToSelect = PendingChangesSelection.GetChangesToFocus(
                mTreeControl, mPendingChangesTree);

            ((IProgressControls)mProgressControls).ShowProgress(PlasticLocalization.
                GetString(PlasticLocalization.Name.LoadingPendingChanges));

            bool bHasToUpdatePendingChangesNotification = pendingChangesStatus == null;

            IThreadWaiter waiter = ThreadWaiter.GetWaiter();
            waiter.Execute(
                /*threadOperationDelegate*/ delegate
                {
                    FilterManager.Get().Reload(mWkInfo);

                    if (pendingChangesStatus != null)
                        return;

                    if (newChangesInWk != null)
                        newChangesInWk.Detected();

                    pendingChangesStatus = new PendingChangesStatus(
                        GetStatus.ForWorkspace(
                            mWkInfo,
                            GetStatus.DefaultOptions(),
                            PendingChangesOptions.GetMovedMatchingOptions()),
                        PlasticGui.Plastic.API.GetPendingMergeLinks(mWkInfo));
                },
                /*afterOperationDelegate*/ delegate
                {
                    try
                    {
                        if (waiter.Exception != null)
                        {
                            if (!IsControlledException(waiter.Exception))
                                ExceptionsHandler.DisplayException(waiter.Exception);

                            return;
                        }

                        mPendingMergeLinks = pendingChangesStatus.PendingMergeLinks;

                        UpdateChangesTree(pendingChangesStatus.WorkspaceStatusResult.Changes);

                        ClearDiffWindowIfShownChangeIsGone(
                            pendingChangesStatus.WorkspaceStatusResult.Changes);

                        UpdateMergeLinksList();

                        SelectChangesWithoutNotify(changesToSelect);

                        RefreshAsset.VersionControlCache(mAssetStatusCache);

                        if (bHasToUpdatePendingChangesNotification)
                        {
                            CheckPendingChanges.UpdateNotification(
                                mWkInfo, mUpdatePendingChanges, pendingChangesStatus);
                        }
                    }
                    finally
                    {
                        ((IProgressControls)mProgressControls).HideProgress();

                        UpdateNotificationPanel();

                        mIsRefreshing = false;
                    }
                });
        }

        bool IsControlledException(Exception ex)
        {
            // This condition covers the scenario of a hanging refresh operation
            // that fails after the workspace has been removed
            return ex is CmException &&
                   NOT_EXISTING_WORKSPACE_MESSAGE_ID.Equals(((CmException) ex).KeyMessage);
        }

        void ClearDiffWindowIfShownChangeIsGone(List<ChangeInfo> newChanges)
        {
            if (mShownDiffChangePath == null)
                return;

            foreach (ChangeInfo change in newChanges)
            {
                if (change.Path == mShownDiffChangePath)
                    return;
            }

            mShownDiffChangePath = null;
            GetWindowIfOpened.Diff()?.ClearIfShownFrom(DiffSource.PendingChanges);
        }

        void SelectChangesWithoutNotify(List<ChangeInfo> changesToSelect)
        {
            mbIsSelectionChangedEventDisabled = true;

            try
            {
                PendingChangesSelection.SelectChanges(
                    mTreeControl, mPendingChangesTree, changesToSelect);
            }
            finally
            {
                mbIsSelectionChangedEventDisabled = false;
            }
        }

        void UpdateChangesTree(List<ChangeInfo> changes)
        {
            mPendingChangesTree.BuildChangeCategories(
                mWkInfo, changes, mCheckedStateManager);

            RestoreCheckedStatesFromSessionState();

            FillTree();
        }

        void RestoreCheckedStatesFromSessionState()
        {
            if (!mbRestoreCheckedStatesFromSessionState)
                return;

            List<IPlasticTreeNode> allNodes = mPendingChangesTree.GetAllNodes();

            if (allNodes.Count == 0)
                return;

            mbRestoreCheckedStatesFromSessionState = false;

            TreeCheckedStatesSessionState.Restore(
                allNodes,
                UnityConstants.PENDING_CHANGES_CHECKED_ITEMS_KEY_NAME,
                UnityConstants.PENDING_CHANGES_UNCHECKED_ITEMS_KEY_NAME);
        }

        void FillTree()
        {
            Filter filter = new Filter(mSearchText);

            mbIsSelectionChangedEventDisabled = true;

            try
            {
                mTreeControl.FillItemsAndSelectRows(
                    mPendingChangesTree.InnerTree,
                    null,
                    filter);

                if (filter.IsEmpty)
                    RestoreCollapsedNodesBeforeSearch();
            }
            finally
            {
                mbIsSelectionChangedEventDisabled = false;
            }

            UpdateItemColumnHeader();

            UpdateContentVisibility();
        }

        void OnFilterApplied(Filter filter)
        {
            if (filter.IsEmpty)
                return;

            if (mCollapsedNodesBeforeSearch == null)
            {
                mCollapsedNodesBeforeSearch = mExpandedNodesCache.GetCollapsedNodes();
            }

            // Filtered matches must be visible even inside collapsed categories
            ExpandNodes(mPendingChangesTree.GetNodes());
        }

        void RestoreCollapsedNodesBeforeSearch()
        {
            if (mCollapsedNodesBeforeSearch == null)
                return;

            List<IPlasticTreeNode> allNodes = mPendingChangesTree.GetAllNodes();

            if (allNodes.Count == 0)
                return;

            HashSet<string> collapsedNodes = new HashSet<string>(mCollapsedNodesBeforeSearch);
            mCollapsedNodesBeforeSearch = null;

            foreach (IPlasticTreeNode node in allNodes)
            {
                if (collapsedNodes.Contains(((IPlasticTreeNodeKey)node).GetKey()))
                {
                    mExpandedNodesCache.CollapseNode(node);
                    mTreeControl.CollapseNode(node);
                    continue;
                }

                mExpandedNodesCache.ExpandNode(node);
                mTreeControl.ExpandNode(node);
            }
        }

        void ExpandNodes(IEnumerable<IPlasticTreeNode> nodes)
        {
            if (nodes == null)
                return;

            foreach (IPlasticTreeNode node in nodes)
            {
                mExpandedNodesCache.ExpandNode(node);

                ExpandNodes(node.GetChildren());
            }
        }

        void OnExpandedStateChanged(IPlasticTreeNode node, bool bIsExpanded)
        {
            if (bIsExpanded)
            {
                mExpandedNodesCache.ExpandNode(node);
                return;
            }

            mExpandedNodesCache.CollapseNode(node);
        }

        void SaveCollapsedNodeKeys(List<IPlasticTreeNode> allNodes)
        {
            if (allNodes.Count == 0)
                return;

            ExpandedNodesCache collapsedStateToSave = mCollapsedNodesBeforeSearch == null ?
                mExpandedNodesCache :
                new ExpandedNodesCache(mCollapsedNodesBeforeSearch);

            PendingChangesTreeSettings.SaveCollapsedNodeKeys(
                PlasticGuiConfig.GetConfigFile(),
                mWkInfo.Id,
                collapsedStateToSave.GetCollapsedNodes(allNodes));
        }

        void OnTreeSelectionChanged(IEnumerable<object> selectedItems)
        {
            mDelayedSelectionRunner.Run();

            UpdateItemColumnHeader();

            if (mbIsSelectionChangedEventDisabled)
                return;

            SelectAssetsOfSelectedChanges();
        }

        void OnTreeFocusIn(FocusInEvent evt)
        {
            VisualElement previousFocusedElement = evt.relatedTarget as VisualElement;

            if (previousFocusedElement != null && mTreeControl.Contains(previousFocusedElement))
                return;

            mTreeControl.schedule.Execute(ReselectAssetsOfSelectedChanges);
        }

        void ReselectAssetsOfSelectedChanges()
        {
            if (!mTreeControl.HasSelectedRows())
                return;

            SelectAssetsOfSelectedChanges();
        }

        void SelectAssetsOfSelectedChanges()
        {
            List<UnityEngine.Object> assets = new List<UnityEngine.Object>();

            foreach (PendingChangeInfo changeInfo
                in mTreeControl.GetSelectedNodes<PendingChangeInfo>())
            {
                if (!changeInfo.IsChangedNode())
                    continue;

                UnityEngine.Object asset = LoadAsset.FromChangeInfo(changeInfo.ChangeInfo);

                if (asset == null)
                    continue;

                assets.Add(asset);
            }

            UnityEditor.Selection.objects = assets.ToArray();

            if (assets.Count == 1)
                EditorGUIUtility.PingObject(assets[0]);
        }

        void OnTreeItemsChosen(IEnumerable<object> items)
        {
            if (mTreeControl.GetSelectedRowsCount() != 1)
                return;

            ChangeInfo selectedChange = PendingChangesSelection.GetSelectedChange(mTreeControl);

            if (selectedChange != null)
            {
                ((IPendingChangesMenuOperations)this).Diff();
                return;
            }

            IPlasticTreeNode selectedNode = mTreeControl.GetSelectedNode<IPlasticTreeNode>();

            if (selectedNode == null)
                return;

            if (mTreeControl.IsExpanded(selectedNode))
                mTreeControl.CollapseNode(selectedNode);
            else
                mTreeControl.ExpandNode(selectedNode);
        }

        void OnTreeKeyDown(KeyDownEvent evt)
        {
            if (KeyboardEvents.IsSpacebarPressed(evt))
            {
                ToggleSelectedChanges();
                evt.imguiEvent?.Use();
                evt.StopPropagation();
                return;
            }

            if (KeyboardEvents.IsSpacebarCharacter(evt))
            {
                evt.imguiEvent?.Use();
                evt.StopPropagation();
                return;
            }

            if (evt.keyCode == KeyCode.None)
                return;

            Event keyEvent = new Event()
            {
                type = EventType.KeyDown,
                keyCode = evt.keyCode,
                modifiers = evt.modifiers
            };

            if (!mMenu.ProcessKeyActionIfNeeded(keyEvent))
                return;

            evt.StopPropagation();
        }

        void ToggleSelectedChanges()
        {
            if (!mTreeControl.HasSelectedRows())
                return;

            mTreeControl.SelectionActivator.OnSelectionActivated(
                mTreeControl.GetAllSelection()[0]);
        }

        void OnTreeContextClick(ContextClickEvent evt)
        {
            if (PlasticTree.IsClickedOnHeader(evt))
                return;

            if (!mTreeControl.HasSelectedRows())
                return;

            mMenu.Popup();

            evt.StopPropagation();
        }

        void DelayedSearchChanged()
        {
            UpdateEmptyStateMessage();

            FillTree();
        }

        void DelayedSelectionChanged()
        {
            if (!mTreeControl.HasSelectedRows())
                return;

            OnDelayedSelectionChanged();
        }

        void UpdateItemColumnHeader()
        {
            mTreeControl.SetColumnTitle(
                PendingChangesTreeDefinition.ITEM_COLUMN_NAME,
                PendingChangesTreeDefinition.BuildItemColumnTitle(
                    GetCheckedItemCount(), GetTotalItemCount()));
        }

        void UpdateContentVisibility()
        {
            mShouldShowEmptyState = GetTotalItemCount() == 0;

            mTreeControl.SetVisible(!mShouldShowEmptyState);
            mEmptyStateContainer.SetVisible(mShouldShowEmptyState);

            mMergeLinksContainer.SetVisible(
                HasPendingMergeLinks() && !mHasPendingMergeLinksFromRevert);
        }

        void UpdateProgressOverlay()
        {
            bool bIsOperationRunning = IsOperationRunning();

            mTreeControl.SetEnabled(!bIsOperationRunning);

            mProgressOverlay.SetVisible(bIsOperationRunning);

            if (!bIsOperationRunning)
                return;

            mProgressOverlayBar.title = mProgressControls.ProgressData.ProgressMessage;
            mProgressOverlayBar.value = mProgressControls.ProgressData.ProgressPercent * 100f;
        }

        void UpdateErrorsPanelVisibility()
        {
            if (mErrorsContainer == null)
                return;

            mErrorsContainer.SetVisible(mErrorsPanel.IsVisible);
        }

        void ShowPendingOperationSuccessInStatusBar()
        {
            if (mDrawOperationSuccess == null || mShouldShowEmptyState)
                return;

            mDrawOperationSuccess.InStatusBar(mWindowStatusBar);
            mDrawOperationSuccess = null;
            mIsOperationSuccessPendingToDraw = false;
        }

        int GetCheckedItemCount()
        {
            return CheckableItems.GetCheckedChildNodeCount(mPendingChangesTree.GetNodes());
        }

        int GetTotalItemCount()
        {
            return CheckableItems.GetTotalChildNodeCount(mPendingChangesTree.GetNodes());
        }

        void UpdateGluonWarningLabel(string text)
        {
            mGluonWarningLabel.text = text;
            mGluonWarningLabel.SetVisible(!string.IsNullOrEmpty(text));
        }

        void UpdateMergeLinksList()
        {
            mMergeLinksListView.BuildModel(mPendingMergeLinks);

            mMergeLinksListView.Reload();

            if (!HasPendingMergeLinks())
                mHasPendingMergeLinksFromRevert = false;
        }

        void UpdateNotificationPanel()
        {
            if (PlasticGui.Plastic.API.IsFsReaderWatchLimitReached(mWkInfo))
            {
                ((IProgressControls)mProgressControls).ShowWarning(PlasticLocalization.
                    GetString(PlasticLocalization.Name.NotifyLinuxWatchLimitWarning));
                return;
            }
        }

        void ExecuteAfterOnGUIAction()
        {
            if (IsOperationRunning())
                return;

            if (mAfterOnGUIAction == null)
                return;

            mAfterOnGUIAction();
            mAfterOnGUIAction = null;
        }

        void DrawEmptyState(
            Rect rect,
            EmptyStatePanel emptyStatePanel,
            IDrawOperationSuccess drawOperationSuccess)
        {
            if (drawOperationSuccess == null)
            {
                emptyStatePanel.OnGUI(rect);
                return;
            }

            drawOperationSuccess.InEmptyState(rect);
            mIsOperationSuccessPendingToDraw = false;
        }

        static string GetEmptyStateMessage(string searchString)
        {
            if (!string.IsNullOrEmpty(searchString))
                return PlasticLocalization.Name.NoPendingChangesMatchingFilters.GetString();

            return PlasticLocalization.Name.NoPendingChangesFoundUnityVCS.GetString();
        }

        bool HasPendingMergeLinks()
        {
            if (mPendingMergeLinks == null)
                return false;

            return mPendingMergeLinks.Count > 0;
        }

        bool IsOperationRunning()
        {
            return mProgressControls.IsOperationRunning() || mPendingChangesUpdater.IsRunning();
        }

        static void DoMergeLinksArea(
            MergeLinksListView mergeLinksListView, float width)
        {
            GUILayout.Label(
                PlasticLocalization.GetString(
                    PlasticLocalization.Name.MergeLinkDescriptionColumn),
                EditorStyles.boldLabel);

            float desiredTreeHeight = mergeLinksListView.DesiredHeight;

            Rect treeRect = GUILayoutUtility.GetRect(
                0,
                width,
                desiredTreeHeight,
                desiredTreeHeight);

            mergeLinksListView.OnGUI(treeRect);
        }

        void OnDelayedSelectionChanged()
        {
            if (!IsVisible)
                return;

            if (mTreeControl.GetSelectedRowsCount() != 1)
                return;

            ChangeInfo selectedChange = PendingChangesSelection.GetSelectedChange(mTreeControl);

            if (selectedChange == null)
                return;

            IUnityDiffWindow diffWindow = GetWindowIfOpened.Diff();

            if (diffWindow == null)
                return;

            ChangeInfo changedForMoved = mPendingChangesTree.GetChangedForMoved(selectedChange);

            diffWindow.ShowDiffFromChange(
                selectedChange,
                changedForMoved,
                BuildDiffInDesktopAppAction(selectedChange, changedForMoved));

            mShownDiffChangePath = selectedChange?.Path;
        }

        void UpdateEmptyStateMessage()
        {
            mEmptyStatePanel.UpdateContent(GetEmptyStateMessage(mSearchText));
        }

        Action BuildDiffInDesktopAppAction(
            ChangeInfo selectedChange,
            ChangeInfo changedForMoved)
        {
            if (!DiffOperation.IsApplicableDiffWorkspaceContent(selectedChange))
                return null;

            return () => DiffInDesktopApp(selectedChange, changedForMoved);
        }

        void BuildComponents(bool isGluonMode)
        {
            mCommentArea = new CommentArea(
                this, isGluonMode, ClearIsCommentWarningNeeded);

            mMenu = new PendingChangesViewMenu(
                mWkInfo, this, this, this, this, this, this, this, isGluonMode);

            mPendingChangesTree = new UnityPendingChangesTree();
            mExpandedNodesCache = new ExpandedNodesCache(
                PendingChangesTreeSettings.GetCollapsedNodeKeys(
                    PlasticGuiConfig.GetConfigFile(), mWkInfo.Id));

            mTreeControl = new PlasticTree(
                PendingChangesTreeDefinition.BuildColumns(isGluonMode),
                PendingChangesTreeDefinition.BuildCellRenderFunction(
                    mPendingChangesTree, isGluonMode, mAssetStatusCache),
                PendingChangesTreeDefinition.BuildExpandFunction(mExpandedNodesCache),
                SelectionType.Multiple,
                UnityConstants.PENDING_CHANGES_TREE_NAME,
                legacySettingsName: UnityConstants.PENDING_CHANGES_LEGACY_TABLE_SETTINGS_NAME,
                legacySettingsColumnNamesByIndex:
                    PendingChangesTreeDefinition.LEGACY_COLUMN_NAMES_BY_INDEX,
                defaultSelectionFunc:
                    PendingChangesTreeDefinition.BuildDefaultSelectionFunction(),
                defaultSortColumnName: PendingChangesTreeDefinition.ITEM_COLUMN_NAME,
                checkedStatesChangedAction: UpdateItemColumnHeader,
                afterFilterAppliedAction: OnFilterApplied,
                afterExpandChangedAction: OnExpandedStateChanged);

            RegisterTreeEvents();

            mDelayedFilterAction = new DelayedActionBySecondsRunner(
                DelayedSearchChanged, UnityConstants.SEARCH_DELAYED_INPUT_ACTION_INTERVAL);

            mDelayedSelectionRunner = new DelayedActionBySecondsRunner(
                DelayedSelectionChanged, UnityConstants.SELECTION_DELAYED_INPUT_ACTION_INTERVAL);

            UpdateEmptyStateMessage();

            mMergeLinksListView = new MergeLinksListView();
            mMergeLinksListView.Reload();

            if (isGluonMode)
                mErrorsPanel = new ErrorsPanel(
                    PlasticLocalization.Name.ChangesCannotBeApplied.GetString(),
                    UnityConstants.PENDING_CHANGES_ERRORS_TABLE_SETTINGS_NAME,
                    UnityConstants.PENDING_CHANGES_ERRORS_PANEL_SPLITTER_SETTINGS_NAME);

            mContentElement = BuildContentElement();
            mContentElement.SetVisible(false);

            RegisterHeaderEvents();

            mParentWindow.Get().rootVisualElement.Add(mContentElement);
        }

        VisualElement BuildContentElement()
        {
            mEmptyStateContainer = new IMGUIContainer(DoEmptyStateArea);
            mEmptyStateContainer.style.flexGrow = 1;
            mEmptyStateContainer.SetVisible(false);

            if (mErrorsPanel != null)
            {
                mErrorsContainer = new IMGUIContainer(DoErrorsArea);
                mErrorsContainer.style.height = Length.Percent(ERRORS_AREA_HEIGHT_PERCENT);
                mErrorsContainer.SetVisible(false);
            }

            mMergeLinksContainer = new IMGUIContainer(DoMergeLinksAreaIfNeeded);
            mMergeLinksContainer.SetVisible(false);

            mProgressOverlay = BuildProgressOverlay();

            VisualElement changesContent = EntriesPacker.CreateVPanel(
                mTreeControl,
                mEmptyStateContainer,
                mErrorsContainer,
                mMergeLinksContainer,
                new IMGUIContainer(DoNotificationArea),
                mProgressOverlay);
            changesContent.style.flexGrow = 1;

            VisualElement changesPane = EntriesPacker.CreateVPanel(
                BuildHeader(),
                changesContent);
            changesPane.style.flexGrow = 1;
            changesPane.style.minWidth = SPLIT_PANE_MIN_WIDTH;

            mCommentArea.style.minWidth = SPLIT_PANE_MIN_WIDTH;

            mSplitView = new TwoPaneSplitView(
                1, COMMENT_AREA_INITIAL_WIDTH, TwoPaneSplitViewOrientation.Horizontal);
            mSplitView.viewDataKey =
                UnityConstants.PENDING_CHANGES_COMMENTS_SPLITTER_VIEW_DATA_KEY;
            mSplitView.style.flexGrow = 1;
            mSplitView.Add(changesPane);
            mSplitView.Add(mCommentArea);

            // The absolute-fill panel covers the window until the first
            // repaint syncs the exact tab-content rect reserved by IMGUI
            return EntriesPacker.CreateAbsoluteFillPanel(mSplitView);
        }

        VisualElement BuildHeader()
        {
            mGluonWarningLabel = ControlBuilder.Label.CreateWarningMessageLabel();

            mRefreshButton = ControlBuilder.Toolbar.CreateImageButtonLeft(
                Images.GetRefreshIcon(),
                PlasticLocalization.Name.RefreshButton.GetString(),
                () => Refresh());

            ToolbarSpacer spacer = new ToolbarSpacer();
            spacer.style.flexGrow = 1;

            mSearchField = ControlBuilder.Toolbar.CreateSearchField();
            mSearchField.style.width = UnityConstants.SEARCH_FIELD_WIDTH / 2f;

            UnityEditor.UIElements.Toolbar toolbar = ControlBuilder.Toolbar.Create();
            toolbar.Add(mRefreshButton);
            toolbar.Add(spacer);
            toolbar.Add(mSearchField);

            return EntriesPacker.CreateVPanel(mGluonWarningLabel, toolbar);
        }

        VisualElement BuildProgressOverlay()
        {
            mProgressOverlayBar = new ProgressBar();
            mProgressOverlayBar.style.width = PROGRESS_OVERLAY_BAR_WIDTH;

            return ControlBuilder.Overlay.CreateAbsoluteCenteredOverlay(
                mProgressOverlayBar);
        }

        bool mShouldShowEmptyState;
        bool mIsRefreshing;
        bool mIsEmptyCheckinCommentWarningNeeded = false;
        bool mNeedsToShowEmptyCheckinCommentDialog = false;
        bool mHasPendingCheckinFromPreviousUpdate = false;
        bool mIsEmptyShelveCommentWarningNeeded = false;
        bool mNeedsToShowEmptyShelveCommentDialog = false;
        bool mHasPendingShelveFromPreviousUpdate = false;
        bool mHasPendingMergeLinksFromRevert = false;
        bool mbIsVisible;
        bool mbIsSelectionChangedEventDisabled;
        bool mbRestoreCheckedStatesFromSessionState = true;
        string mShownDiffChangePath;
        string mSearchText = string.Empty;
        Action mAfterOnGUIAction;
        IDictionary<MountPoint, IList<PendingMergeLink>> mPendingMergeLinks;

        IDrawOperationSuccess mDrawOperationSuccess;
        bool mIsOperationSuccessPendingToDraw = false;

        PlasticTree mTreeControl;
        UnityPendingChangesTree mPendingChangesTree;
        ExpandedNodesCache mExpandedNodesCache;
        List<string> mCollapsedNodesBeforeSearch;
        PendingChangesViewMenu mMenu;
        DelayedActionBySecondsRunner mDelayedFilterAction;
        DelayedActionBySecondsRunner mDelayedSelectionRunner;
        MergeLinksListView mMergeLinksListView;
        ErrorsPanel mErrorsPanel;

        VisualElement mContentElement;
        TwoPaneSplitView mSplitView;
        Label mGluonWarningLabel;
        ToolbarButton mRefreshButton;
        ToolbarSearchField mSearchField;
        IMGUIContainer mEmptyStateContainer;
        IMGUIContainer mErrorsContainer;
        IMGUIContainer mMergeLinksContainer;
        VisualElement mProgressOverlay;
        ProgressBar mProgressOverlayBar;

        CommentArea mCommentArea;
        readonly IGetUVCSWindow mParentWindow;

        readonly PendingChangesOperations mPendingChangesOperations;
        IEmptyCommentDialog mEmptyCommentDialog;

        readonly ProgressControlsForViews mProgressControls;
        readonly EmptyStatePanel mEmptyStatePanel;
        readonly PendingChangesViewCheckedStateManager mCheckedStateManager;
        readonly GuiMessage.IGuiMessage mGuiMessage;
        readonly bool mIsGluonMode;
        readonly WindowStatusBar mWindowStatusBar;
        readonly INewChangesInWk mNewChangesInWk;
        readonly IPendingChangesUpdater mPendingChangesUpdater;
        readonly IIncomingChangesUpdater mDeveloperIncomingChangesUpdater;
        readonly IIncomingChangesUpdater mGluonIncomingChangesUpdater;
        readonly IShelvedChangesUpdater mShelvedChangesUpdater;
        readonly CheckPendingChanges.IUpdatePendingChanges mUpdatePendingChanges;
        readonly WorkspaceOperationsMonitor mWorkspaceOperationsMonitor;
        LaunchTool.IShowDownloadPlasticExeWindow mShowDownloadPlasticExeWindow;
        LaunchTool.IProcessExecutor mProcessExecutor;
        readonly ISaveAssets mSaveAssets;
        readonly IAssetStatusCache mAssetStatusCache;
        readonly IHistoryViewLauncher mHistoryViewLauncher;
        readonly IShowShelveInView mShowShelveInView;
        readonly IShowChangesetInView mShowChangesetInView;
        readonly IViewSwitcher mViewSwitcher;
        readonly WorkspaceWindow mWorkspaceWindow;
        readonly ViewHost mViewHost;
        readonly RepositorySpec mRepSpec;
        readonly WorkspaceInfo mWkInfo;

        const string NOT_EXISTING_WORKSPACE_MESSAGE_ID = "WK_DOESNT_EXIST";
        const int COMMENT_AREA_INITIAL_WIDTH = 300;
        const int SPLIT_PANE_MIN_WIDTH = 160;
        const int ERRORS_AREA_HEIGHT_PERCENT = 40;
        const int PROGRESS_OVERLAY_BAR_WIDTH = 300;

        static readonly ILog mLog = PlasticApp.GetLogger("PendingChangesTab");
    }
}
