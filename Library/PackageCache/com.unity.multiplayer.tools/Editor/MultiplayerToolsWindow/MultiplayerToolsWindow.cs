using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Multiplayer.Tools.Editor.MultiplayerToolsWindow
{
    /// <summary>
    /// The Multiplayer Tools Editor Window provides quick access to the different Tools in this package.
    /// </summary>
    public class MultiplayerToolsWindow : EditorWindow
    {
        const string k_PathInPackage = "Packages/com.unity.multiplayer.tools/Editor/MultiplayerToolsWindow";
        const string k_MainUxmlPath = k_PathInPackage + "/UI/MultiplayerToolsWindow.uxml";
        const string k_SectionUxmlPath = k_PathInPackage + "/UI/MultiplayerToolsWindowSection.uxml";

        /// <summary>
        /// The menu item hook to show the Multiplayer Tools Editor Window
        /// </summary>
        [MenuItem("Window/Multiplayer/Multiplayer Tools")]
        public static void Open()
        {
            GetWindow<MultiplayerToolsWindow>("Multiplayer Tools");
        }

        void OnEnable()
        {
            rootVisualElement.Clear();
            var theme = EditorGUIUtility.isProSkin ? "dark" : "light";
            rootVisualElement.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>($"{k_PathInPackage}/UI/{theme}.uss"));
            var windowTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(k_MainUxmlPath);
            var sectionTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(k_SectionUxmlPath);
            var root = windowTree.CloneTree();
            var sectionRoot = root.Q("SectionsRoot");
            sectionRoot.Add(InstantiateSection(sectionTree, new NetSceneVis()));
            sectionRoot.Add(InstantiateSection(sectionTree, new Rnsm()));
            sectionRoot.Add(InstantiateSection(sectionTree, new NetworkSimulatorFeature()));
            sectionRoot.Add(InstantiateSection(sectionTree, new NetworkProfiler()));
            sectionRoot.Add(InstantiateSection(sectionTree, new Decorator()));
            AddPackageVersions(root.Q("PackageRoot"));
            root.Q<Button>("OpenPackageManagerButton").clickable.clicked += OpenPackageManager;
            rootVisualElement.Add(root);
        }

        static TemplateContainer InstantiateSection(VisualTreeAsset tree, IMultiplayerToolsFeature feature)
        {
            var root = tree.CloneTree();
            var titleTemplate = root.Q<TextElement>("EnabledFeaturesTitle");
            titleTemplate.tooltip = feature.ToolTip;
            titleTemplate.text = feature.Name;
            var content = root.Q<TextElement>("EnabledFeaturesText");
            content.text = feature.AvailabilityMessage;
            var statusIcon = root.Q("StatusIcon");
            statusIcon.AddToClassList(feature.IsAvailable ? "okIcon" : "logIcon");
#if UNITY_2022_1_OR_NEWER
            statusIcon.style.backgroundPositionY = new StyleBackgroundPosition(new BackgroundPosition(BackgroundPositionKeyword.Top));
#endif
            var docIcon = root.Q("DocButton");
            docIcon.tooltip = $"Open documentation for {feature.Name}";
            docIcon.style.display = feature.DocumentationUrl == null ? DisplayStyle.None : DisplayStyle.Flex;
            var clickable = new Clickable(() => Application.OpenURL(feature.DocumentationUrl));
            docIcon.AddManipulator(clickable);
            var button = root.Q<Button>("OpenButton");
            button.text = feature.ButtonText;
            if (feature.IsAvailable)
                button.clicked += feature.Open;
            else
                button.SetEnabled(false);

            if (feature is Decorator)
                button.clicked += () => button.text = feature.ButtonText;

            return root;
        }

        static void OpenPackageManager()
        {
            try
            {
                UnityEditor.PackageManager.UI.Window.Open("UnityRegistry");
            }
            catch (Exception)
            {
                // Hide the error in the PackageManager API until the team fixes it
                // Debug.Log("Error opening Package Manager: " + e.Message);
            }
        }

        static void AddPackageVersions(VisualElement parent)
        {
            AddOnePackageVersion(parent, "com.unity.netcode.gameobjects", "Netcode for GameObjects", "https://docs-multiplayer.unity3d.com/netcode/current/installation/");
            AddOnePackageVersion(parent, "com.unity.transport", "Unity Transport", "https://docs-multiplayer.unity3d.com/transport/current/install/");
            AddOnePackageVersion(parent, "com.unity.multiplayer.tools", "Multiplayer Tools", "https://docs-multiplayer.unity3d.com/tools/current/install-tools/");
        }

        static void AddOnePackageVersion(VisualElement parent, string packageId, string packageName, string docUrl)
        {
            var info = GetInfoString(packageId, packageName);
            var line = new VisualElement() {style = {flexDirection = FlexDirection.Row}};
            var textElement = new Label() {text = info};
            var docButton = new VisualElement();
            docButton.AddToClassList("docIcon");
            docButton.tooltip = $"Installation documentation for {packageName}";
            docButton.AddManipulator(new Clickable(() => Application.OpenURL(docUrl)));
            line.Add(textElement);
            line.Add(docButton);
            parent.Add(line);
        }

        static string GetInfoString(string packageId, string packageName)
        {
#if UNITY_2023_2_OR_NEWER
            var info = UnityEditor.PackageManager.PackageInfo.FindForPackageName(packageId);
#else
            var allPkgs = UnityEditor.PackageManager.PackageInfo.GetAllRegisteredPackages();
            var info = Array.Find(allPkgs, p => p.name == packageId);
#endif
            return info == null ? $"• {packageName}: package not found" : $"• {packageName} version: {info.version}";
        }
    }

    /// <summary> Description of a feature that can be enabled in the Multiplayer Tools window </summary>
    interface IMultiplayerToolsFeature
    {
        /// <summary> Name of the feature, displayed as a title in the UI </summary>
        string Name { get; }

        /// <summary> Short summary of the feature, displayed in a tooltip </summary>
        string ToolTip { get; }

        /// <summary> Text to display on the button that opens the feature </summary>
        string ButtonText { get; }

        /// <summary> URL to the documentation for this feature </summary>
        string DocumentationUrl { get; }

        /// <summary> Whether the feature is available in the current setup (Unity version and package version) </summary>
        bool IsAvailable { get; }

        /// <summary> If the feature is not available, this message will be displayed in the UI </summary>
        string AvailabilityMessage { get; }

        /// <summary> Open the window / overlay, or add the relevant object to the scene.</summary>
        void Open();
    }
}
