#if UNITY_EDITOR
#if UNITY_MP_TOOLS_DEV
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Multiplayer.Tools.Common.Visualization
{
    /// <summary>
    /// This class provides functionality for visualizing and generating color-blind friendly color palettes.
    /// </summary>
    /// <remarks>
    /// We have a good list of color-blind friendly colors from the 24-color palette here:
    /// http://mkweb.bcgsc.ca/colorblind/palettes.mhtml.
    ///
    /// However, these colors are ordered in such a way that there is little contrast between adjacent colors in the palette.
    /// Thus this original ordering is really bad for our use cases, as we never know in advance how many different colors will be used,
    /// as this depends on the number of clients, variables, etc. that the user is visualizing.
    ///
    /// When visualizing N categories of data we will only use the first N colors, so we want to reorder these colors to
    /// get as much contrast out of the first N colors as possible, including for users with color blindness.
    /// </remarks>
    class CategoricalColorPaletteVisualizer : EditorWindow
    {
        [MenuItem("Window/Multiplayer/Multiplayer Tools Dev/Color Palette")]
        static void ShowColorPaletteGenerator()
        {
            var window = EditorWindow.GetWindow<CategoricalColorPaletteVisualizer>();
            window.titleContent = new GUIContent("MP Tools Color Palette Visualizer");

            // Can become very hard to find a window on a multi-monitor setup if it accidentally becomes 1x1
            window.minSize = new Vector2(200, 200);
            window.maxSize = new Vector2(3840, 2160);
        }

        /// <summary>
        /// Color names in their original order from the 24-color palette here: http://mkweb.bcgsc.ca/colorblind/palettes.mhtml
        /// </summary>
        public static readonly string[] k_ColorNames_OriginalOrder =
        {
            "sherwood green",
            "mulberry",
            "deep opal",
            "french plum",
            "robin hood",
            "jazberry jam",
            "elf green",
            "magenta",
            "jeepers creepers",
            "persian rose",
            "aquamarine",
            "barbie pink",
            "vivid opal",
            "amaranth pink",
            "light turquoise",
            "azalea",

            "madison",
            "christalle",
            "tory blue",
            "purple heart",
            "royal blue",
            "french violet",
            "azure",
            "electric purple",
            "bleu de france",
            "psychedelic purple",
            "capri",
            "fuchsia",
            "aqua blue",
            "violet",
            "electric blue",
            "pale mauve",

            "british racing green",
            "rosewood",
            "san felix",
            "hot chile",
            "bilbao",
            "alabama crimson",
            "india green",
            "amaranth red",
            "kelly green",
            "carmine",
            "vivid harlequin",
            "burning orange",
            "radioactive green",
            "frenzee",
            "lime",
            "gargoyle gas",
        };

        /// <summary>
        /// Colors in their original order from the 24-color palette here: http://mkweb.bcgsc.ca/colorblind/palettes.mhtml
        /// </summary>
        public static readonly Color[] k_ColorPalette_OriginalOrder =
        {
            new(  0 / 255f,  61 / 255f,  48 / 255f), // sherwood green
            new( 90 / 255f,  10 / 255f,  51 / 255f), // mulberry
            new(  0 / 255f,  87 / 255f,  69 / 255f), // deep opal
            new(129 / 255f,  13 / 255f,  73 / 255f), // french plum
            new(  0 / 255f, 115 / 255f,  92 / 255f), // robin hood
            new(171 / 255f,  13 / 255f,  97 / 255f), // jazberry jam
            new(  0 / 255f, 145 / 255f, 117 / 255f), // elf green
            new(216 / 255f,  13 / 255f, 123 / 255f), // magenta
            new(  0 / 255f, 175 / 255f, 142 / 255f), // jeepers creepers
            new(255 / 255f,  46 / 255f, 149 / 255f), // persian rose
            new(  0 / 255f, 203 / 255f, 167 / 255f), // aquamarine
            new(255 / 255f, 120 / 255f, 173 / 255f), // barbie pink
            new(  0 / 255f, 235 / 255f, 193 / 255f), // vivid opal
            new(255 / 255f, 172 / 255f, 198 / 255f), // amaranth pink
            new(134 / 255f, 255 / 255f, 222 / 255f), // light turquoise
            new(255 / 255f, 215 / 255f, 225 / 255f), // azalea
            new(  0 / 255f,  48 / 255f, 111 / 255f), // madison
            new( 70 / 255f,  11 / 255f, 112 / 255f), // christalle
            new(  0 / 255f,  72 / 255f, 158 / 255f), // tory blue
            new(107 / 255f,   6 / 255f, 159 / 255f), // purple heart
            new(  0 / 255f,  95 / 255f, 204 / 255f), // royal blue
            new(142 / 255f,   6 / 255f, 205 / 255f), // french violet
            new(  0 / 255f, 121 / 255f, 250 / 255f), // azure
            new(180 / 255f,  10 / 255f, 252 / 255f), // electric purple
            new(  0 / 255f, 159 / 255f, 250 / 255f), // bleu de france
            new(237 / 255f,  13 / 255f, 253 / 255f), // psychedelic purple
            new(  0 / 255f, 194 / 255f, 249 / 255f), // capri
            new(255 / 255f, 102 / 255f, 253 / 255f), // fuchsia
            new(  0 / 255f, 229 / 255f, 248 / 255f), // aqua blue
            new(255 / 255f, 163 / 255f, 252 / 255f), // violet
            new(124 / 255f, 255 / 255f, 250 / 255f), // electric blue
            new(255 / 255f, 213 / 255f, 253 / 255f), // pale mauve
            new(  0 / 255f,  64 / 255f,   2 / 255f), // british racing green
            new( 95 / 255f,   9 / 255f,  20 / 255f), // rosewood
            new(  0 / 255f,  90 / 255f,   1 / 255f), // san felix
            new(134 / 255f,   8 / 255f,  28 / 255f), // hot chile
            new(  0 / 255f, 119 / 255f,   2 / 255f), // bilbao
            new(178 / 255f,   7 / 255f,  37 / 255f), // alabama crimson
            new(  0 / 255f, 149 / 255f,   3 / 255f), // india green
            new(222 / 255f,  13 / 255f,  46 / 255f), // amaranth red
            new(  0 / 255f, 180 / 255f,   8 / 255f), // kelly green
            new(255 / 255f,  66 / 255f,  53 / 255f), // carmine
            new(  0 / 255f, 211 / 255f,   2 / 255f), // vivid harlequin
            new(255 / 255f, 135 / 255f,  53 / 255f), // burning orange
            new(  0 / 255f, 244 / 255f,   7 / 255f), // radioactive green
            new(255 / 255f, 185 / 255f,  53 / 255f), // frenzee
            new(175 / 255f, 255 / 255f,  42 / 255f), // lime
            new(255 / 255f, 226 / 255f,  57 / 255f), // gargoyle gas
        };

        public static int ColorCount => k_ColorPalette_OriginalOrder.Length;

        enum ColorOrder
        {
            /// <summary>
            /// Default color ordering from http://mkweb.bcgsc.ca/colorblind/palettes.mhtml
            /// </summary>
            Original,

            /// <summary>
            /// Zig-zag ordering to try to get more contrast out of the first N colors
            /// </summary>
            ZigZag,
        }
        static readonly string[] k_ColorOrderNames = EnumUtil.GetNames<ColorOrder>();

        static int GetColorIndex(ColorOrder order, int index)
        {
            return order switch
            {
                ColorOrder.Original => index,
                ColorOrder.ZigZag => ZigZagOrdering(index),
                _ => throw new ArgumentOutOfRangeException(nameof(order), order, null)
            };
        }

        /// <summary>
        /// Method to reorder the indices to maximize the contrast between the first N entries,
        /// unlike the original order in which adjacent colors are very similar.
        /// </summary>
        static int ZigZagOrdering(int indexInNewOrder)
        {
            var i = (ColorCount - 1) - indexInNewOrder;

            var series = i % 3;

            var redOrGreenShift = ((i & 8) != 0 ? 1 : 0);

            var redOrGreen = (i + redOrGreenShift) & 1;

            var next = -i;

            var lightness =
                ((next & 0b001) != 0 ? 0b100 : 0) |
                ((next & 0b010) != 0 ? 0b010 : 0) |
                ((next & 0b100) != 0 ? 0b001 : 0);

            var indexInOriginalOrder = series * 16 + lightness * 2 + redOrGreen;

            return indexInOriginalOrder;
        }

        enum ColorVisionType
        {
            /// <summary>
            /// Normal vision
            /// </summary>
            Normal,

            /// <summary>
            /// Deuteranomaly and deuteranopia: type of color blindness that affects 6% of males,
            /// missing or anomalous cones to see green light
            /// </summary>
            Deuteranopia,

            /// <summary>
            /// Protanomaly and protanopia: type of color blindness that affects 2% of males;
            /// missing or anomalous cones to see red light
            /// </summary>
            Protanopia,

            /// <summary>
            /// Monochromacy: total color blindness, incredibly rare but included for comparison.
            /// Also how a color palette or visualization would look if printed out in greyscale.
            /// </summary>
            Monochromacy,

            Count,
        }

        static string ColorVisionDescription(ColorVisionType visionType)
        {
            return visionType switch
            {
                ColorVisionType.Normal => "Normal vision",
                ColorVisionType.Deuteranopia => "Affecting green cones",
                ColorVisionType.Protanopia => "Affecting red cones",
                ColorVisionType.Monochromacy => "Total color blindness",
                _ => throw new ArgumentOutOfRangeException(nameof(visionType), visionType, null)
            };
        }

        static Color ColorTransform(ColorVisionType visionType, Color inputColor)
        {
            return visionType switch
            {
                ColorVisionType.Normal => inputColor,
                ColorVisionType.Deuteranopia => DeuteranopiaColorTransform(inputColor),
                ColorVisionType.Protanopia => ProtanopiaColorTransform(inputColor),
                ColorVisionType.Monochromacy => MonochromacyColorTransform(inputColor),
                _ => throw new ArgumentOutOfRangeException(nameof(visionType), visionType, null)
            };
        }

        /// <summary>
        /// Simulation of color blindness missing cone to see green light
        /// </summary>
        /// <remarks>
        /// Transform is from here: http://mkweb.bcgsc.ca/colorblind/math.mhtml
        /// </remarks>
        static Color DeuteranopiaColorTransform(Color inputColor)
        {
            var c = inputColor;
            return new Color(
                0.33066007f * c.r + 0.66933993f * c.g + 0f * c.b,
                0.33066007f * c.r + 0.66933993f * c.g + 0f * c.b,
               -0.02785538f * c.r + 0.02785538f * c.g + 1f * c.b);
        }

        /// <summary>
        /// Simulation of color blindness missing cone to see red light
        /// </summary>
        /// <remarks>
        /// Transform is from here: http://mkweb.bcgsc.ca/colorblind/math.mhtml
        /// </remarks>
        static Color ProtanopiaColorTransform(Color inputColor)
        {
            var c = inputColor;
            return new Color(
                0.170556992f * c.r + 0.829443014f * c.g + 0f * c.b,
                0.170556991f * c.r + 0.829443008f * c.g + 0f * c.b,
               -0.004517144f * c.r + 0.004517144f * c.g + 1f * c.b);
        }

        /// <summary>
        /// Simulation of total color blindness
        /// </summary>
        /// <remarks>
        /// Transform is from here: http://mkweb.bcgsc.ca/colorblind/math.mhtml
        /// </remarks>
        static Color MonochromacyColorTransform(Color inputColor)
        {
            var c = inputColor;
            return new Color(
                0.2126f * c.r + 0.7152f * c.g + 0.0722f * c.b,
                0.2126f * c.r + 0.7152f * c.g + 0.0722f * c.b,
                0.2126f * c.r + 0.7152f * c.g + 0.0722f * c.b);
        }

        ColorOrder m_Order = ColorOrder.ZigZag;

        [NonSerialized]
        VisualElement m_ColorVisualizationContainer;

        public void CreateGUI()
        {
            var root = rootVisualElement;

            var windowTitle = new Label("MP Tools Categorical Color Palette");
            windowTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
            windowTitle.style.fontSize = 24;
            root.Add(windowTitle);

            var windowDescription = new Label(
                "Window for visualizing the color palette used in the Multiplayer Tools package as it would appear " +
                "with various kinds of color blindness.");
            root.Add(windowDescription);

            var colorOrderDropdown = new DropdownField(
                label: "Color Order",
                choices: k_ColorOrderNames.ToList(),
                defaultIndex: (int)m_Order);
            root.Add(colorOrderDropdown);
            colorOrderDropdown.style.alignSelf = Align.FlexStart;
            colorOrderDropdown.RegisterCallback<ChangeEvent<string>>(evt =>
            {
                m_Order = (ColorOrder)colorOrderDropdown.index;
                RegenerateColorPaletteVisualization();
            });

            var outputColorsToConsoleButton = new Button(() => LogColorsToConsole(m_Order));
            outputColorsToConsoleButton.text = "Output Colors to Console";
            root.Add(outputColorsToConsoleButton);

            outputColorsToConsoleButton.style.alignSelf = Align.FlexStart;

            root.Add(m_ColorVisualizationContainer ??= new());
            RegenerateColorPaletteVisualization();
        }

        void RegenerateColorPaletteVisualization()
        {
            m_ColorVisualizationContainer.Clear();
            m_ColorVisualizationContainer.Add(CreateColorPaletteVisualization(m_Order));
        }

        static VisualElement CreateColorPaletteVisualization(ColorOrder order)
        {
            var colorColumns = new VisualElement();
            colorColumns.style.flexDirection = FlexDirection.Row;

            for (var visionType = ColorVisionType.Normal;
                 (int)visionType < (int)ColorVisionType.Count;
                 visionType = (ColorVisionType)((int)visionType + 1))
            {
                var column = new VisualElement();
                column.style.flexDirection = FlexDirection.Column;
                colorColumns.Add(column);

                var columnTitle = new Label(visionType.ToString());
                columnTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
                columnTitle.style.fontSize = 16;
                column.Add(columnTitle);

                var description = new Label($"({ColorVisionDescription(visionType)})");
                column.Add(description);

                for (var i = 0; i < ColorCount; ++i)
                {
                    var index = GetColorIndex(order, i);
                    var colorName = k_ColorNames_OriginalOrder[index];
                    var color = k_ColorPalette_OriginalOrder[index];
                    var colorSimulated = ColorTransform(visionType, color);
                    var label = new Label(colorName);
                    label.style.color = Color.white;
                    label.style.backgroundColor = colorSimulated;
                    label.style.paddingLeft = 10;
                    label.style.paddingBottom = 2;
                    label.style.paddingTop = 2;
                    label.style.unityFontStyleAndWeight = FontStyle.Bold;
                    column.Add(label);
                }
            }
            return colorColumns;
        }

        static void LogColorsToConsole(ColorOrder order)
        {
            byte Round(float x) => (byte)Math.Round(255f * x);

            HashSet<int> existingEntries = new();

            StringBuilder colors = new();
            for (var i = 0; i < CategoricalColorPalette.ColorCount; ++i)
            {
                var index = GetColorIndex(order, i);

                var name = k_ColorNames_OriginalOrder[index];
                var color = k_ColorPalette_OriginalOrder[index];

                if (existingEntries.Contains(index))
                {
                    Debug.LogWarning($"Color order {order} contains duplicate entries for color {name} corresponding to index {index}");
                }
                existingEntries.Add(index);

                colors.AppendLine($"new({Round(color.r),3} / 255f, {Round(color.g),3} / 255f, {Round(color.b),3} / 255f), // {name}");
            }
            Debug.Log(colors.ToString());
        }
    }
}
#endif // UNITY_MP_TOOLS_DEV
#endif // UNITY_EDITOR