using System;

using UnityEditor;

using PlasticGui;

namespace Unity.PlasticSCM.Editor.UI.Progress
{
    internal class ProgressControlsForWindow : IProgressControls
    {
        internal class Data
        {
            internal bool IsOperationRunning;
            internal float ProgressPercent;
            internal string ProgressMessage;
        }

        internal Data ProgressData { get { return mData; } }

        internal void UpdateProgress(EditorWindow parentWindow)
        {
            if (IsOperationRunning() || mRequestedRepaint)
            {
                parentWindow.Repaint();

                mRequestedRepaint = false;
            }
        }

        void IProgressControls.HideProgress()
        {
            mData.IsOperationRunning = false;
            mData.ProgressMessage = string.Empty;

            mRequestedRepaint = true;
        }

        void IProgressControls.ShowProgress(string message)
        {
            mData.IsOperationRunning = true;
            mData.ProgressMessage = message;
            mData.ProgressPercent = -1;

            mRequestedRepaint = true;
        }

        void IProgressControls.ShowError(string message)
        {
            UnityEngine.Debug.LogError(message);
        }

        void IProgressControls.ShowNotification(string message)
        {
            UnityEngine.Debug.Log(message);
        }

        void IProgressControls.ShowSuccess(string message)
        {
            UnityEngine.Debug.Log(message);
        }

        void IProgressControls.ShowWarning(string message)
        {
            UnityEngine.Debug.LogWarning(message);
        }

        bool IsOperationRunning()
        {
            return mData.IsOperationRunning;
        }

        Data mData = new Data();

        bool mRequestedRepaint;
    }
}
