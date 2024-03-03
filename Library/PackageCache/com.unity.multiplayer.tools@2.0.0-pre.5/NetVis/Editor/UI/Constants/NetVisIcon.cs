using System.Threading.Tasks;
using Unity.Multiplayer.Tools.Editor;
using UnityEngine;

namespace Unity.Multiplayer.Tools.NetVis.Editor.UI
{
    enum NetVisIcon
    {
        NetSceneVis,
        Bandwidth,
        Ownership,
        Settings,
    }

    enum EditorTheme
    {
        Dark,
        Light,
    }

    static class IconExtensions
    {
        // This is the naming scheme that is required for compatibility with Unity's [Icon] attribute,
        // which is used for the collapsed state of the overlay. If icons are not named according to this
        // naming scheme, then Unity will be able to find the correct icon based on the editor theme and
        // selection state.
        const string k_DarkIconPrefix = "d_";
        const string k_SelectedIconSuffix = "_On";

        /// <returns> The path without a file extension </returns>
        public static string GetPath(this NetVisIcon icon, EditorTheme theme, bool selected = false)
        {
            return NetVisEditorPaths.k_IconsRoot +
                   (theme == EditorTheme.Dark ? k_DarkIconPrefix : "") +
                   icon +
                   (selected ? k_SelectedIconSuffix : "");
        }

        /// <returns> The path with a file extension </returns>
        public static string GetPathWithExtension(this NetVisIcon icon, EditorTheme theme, bool selected = false)
            => GetPath(icon, theme, selected) + ".png";

        public static async Task<Texture2D> LoadAsync(this NetVisIcon icon, EditorTheme theme, bool selected = false)
        {
            var iconPath = icon.GetPathWithExtension(theme, selected);

            if (string.IsNullOrEmpty(iconPath))
            {
                return null;
            }
            
            var iconTexture = await AssetDatabaseHelper.LoadAssetAtPathAsync<Texture2D>(iconPath);
#if UNITY_MP_TOOLS_DEV
            if (iconTexture == null)
            {
                Debug.LogError($"Unable to load NetVis icon at {iconPath}");
            }
#endif
            return iconTexture;
        }
    }
}
