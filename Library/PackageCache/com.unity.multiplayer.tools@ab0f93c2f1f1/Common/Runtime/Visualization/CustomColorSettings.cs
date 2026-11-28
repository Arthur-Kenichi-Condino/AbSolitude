#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Unity.Multiplayer.Tools.Common.Visualization
{
    /// <summary>
    /// CustomColorSettings is a singleton class that stores and synchronizes custom Colors for the NetVis tool between the MPPM instances.
    /// It is a synchronized scriptable singleton saved to the project settings at the path specified by the FilePath attribute,
    /// and it is automatically updated whenever the file is changed by an MPPM instance, therefore always reflecting the latest Colors set by the user everywhere.
    /// </summary>
    [FilePath("ProjectSettings/Packages/com.unity.multiplayer.tools/CustomColorSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    class CustomColorSettings : SyncedSingleton<CustomColorSettings>
    {
        // Nested class to handle saving settings when assets are saved
        class SaveAssetsProcessor : AssetModificationProcessor
        {
            static string[] OnWillSaveAssets(string[] paths)
            {
                instance.SaveIfDirty();
                return paths;
            }
        }

        [SerializeField]
        SerializedDictionary<int, Color> colors = new ();

        /// <summary>
        /// Method to set a custom Color for a specific ID, automatically serialized to disk and synchronized between MPPM instances.
        /// </summary>
        /// <param name="colorID">Color ID we want to overwrite with a custom Color, usually the same as the Client ID</param>
        /// <param name="color">The custom Color we want to set for the Color ID</param>
        public static void SetColor(int colorID, Color color)
        {
            instance.colors[colorID] = color;
            EditorUtility.SetDirty(instance);
            instance.SaveIfDirty();
        }
        
        /// <summary>
        /// Sets multiple custom colors at once.
        /// </summary>
        /// <param name="colorDictionary">Dictionary containing color IDs mapped to their custom colors</param>
        public static void SetColors(Dictionary<int, Color> colorDictionary)
        {
            foreach (var kvp in colorDictionary)
            {
                instance.colors[kvp.Key] = kvp.Value;
            }
            EditorUtility.SetDirty(instance);
            instance.SaveIfDirty();
        }

        /// <summary>
        /// Checks if the Color is already set to custom for the given Color ID.
        /// </summary>
        /// <param name="colorID">Color ID we want to check, usually the same as the Client ID</param>
        /// <returns>True if it has a custom Color, false otherwise</returns>
        public static bool HasColor(int colorID)
        {
            return instance.colors.ContainsKey(colorID);
        }

        /// <summary>
        /// Retrieves the custom Color associated with the given Color ID.
        /// </summary>
        /// <param name="colorID">Color ID of the Color we want to retrieve, usually the same as the Client ID</param>
        /// <returns>The custom Color for the specified Color ID</returns>
        public static Color GetColor(int colorID)
        {
            return instance.colors[colorID];
        }
        
        /// <summary>
        /// Retrieves all currently defined custom colors.
        /// </summary>
        /// <returns>Dictionary containing all color IDs mapped to their custom colors</returns>
        public static Dictionary<int, Color> GetColors()
        {
            return new Dictionary<int, Color>(instance.colors);
        }
        
        /// <summary>
        /// Removes the custom Color associated with the given Color ID and synchronizes the changes across instances.
        /// </summary>
        /// <param name="colorID">Color ID to remove</param>
        public static void RemoveColor(int colorID)
        {
            instance.colors.Remove(colorID);
            EditorUtility.SetDirty(instance);
            instance.SaveIfDirty();
        }

        /// <summary>
        /// Clears all custom Colors, resetting the custom Color configuration to an empty state, therefore the
        /// original palette will be used for all Colors.
        /// </summary>
        public static void ClearColors()
        {
            instance.colors.Clear();
            EditorUtility.SetDirty(instance);
            instance.SaveIfDirty();
        }

        /// <summary>
        /// Checks if there are any unsaved changes to the Color settings and saves them if necessary.
        /// </summary>
        internal void SaveIfDirty()
        {
            if (EditorUtility.IsDirty(this))
            {
                Save(true);
            }
        }
    }
}
#endif