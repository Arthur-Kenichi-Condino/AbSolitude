using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Multiplayer.Tools.NetworkProfiler.Editor.NoDataView
{
    /// <summary>
    /// A VisualElement for Profiler frames without data
    /// </summary>
    public class ProfilerNoData : VisualElement
    {
        const string k_NoInfoString = "No data for this frame. Select another one in the frame chart.";

        /// <summary>
        /// The constructor
        /// </summary>
        public ProfilerNoData()
        {
            this.style.flexGrow = 1.0f;
            this.style.backgroundColor = EditorGUIUtility.isProSkin ? new Color(0.22f, 0.22f, 0.22f) : new Color(0.76f, 0.76f, 0.76f);
            this.style.unityTextAlign = TextAnchor.MiddleCenter;
            var m_NoDataLabel = new Label(k_NoInfoString)
            {
                style =
                {
                    flexGrow = 1.0f,
                    fontSize = 12,
                }
            };
            Add(m_NoDataLabel); // Adding the label to the VisualElement's children.
        }
    }
}
