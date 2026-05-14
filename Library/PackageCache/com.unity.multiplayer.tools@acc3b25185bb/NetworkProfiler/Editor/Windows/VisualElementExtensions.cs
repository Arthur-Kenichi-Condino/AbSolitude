using UnityEngine.UIElements;

namespace Unity.Multiplayer.Tools.NetworkProfiler.Editor
{
    static class VisualElementExtensions
    {
        public static void SetRotate(this VisualElement element, float rotation)
        {
            element.style.rotate = new StyleRotate(new Rotate(rotation));
        }
    }
}
