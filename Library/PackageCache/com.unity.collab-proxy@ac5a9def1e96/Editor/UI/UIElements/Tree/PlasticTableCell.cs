using UnityEngine;

namespace Unity.PlasticSCM.Editor.UI.UIElements.Tree
{
    internal class PlasticTableCell
    {
        internal enum Alignment
        {
            Left,
            Center,
            Right
        }

        internal enum TextTrimming
        {
            None,
            Path
        }

        internal enum StyleType
        {
            Normal,
            Lighter
        }

        internal string Text;
        internal string Tooltip;
        internal Texture Icon;
        internal Texture OverlayIcon;
        internal Alignment TextAlignment;
        internal TextTrimming Trimming;
        internal StyleType Style;
    }
}
