using UnityEngine;

namespace Unity.Multiplayer.Tools.Common.Visualization
{
    /// <summary>
    /// A color-blind-friendly color palette for visualization of categorical data
    /// </summary>
    // [InitializeOnLoad]
    static class CategoricalColorPalette
    {
        /// <summary>
        /// The number of available distinct colors in the palette.
        /// Neighboring integer Color IDs up to ColorCount are guaranteed to be distinct from each other.
        /// </summary>
        public static int ColorCount => k_ColorPalette.Length;

        /// <summary>
        /// Retrieves a color from the categorical color palette associated with the given ID.
        /// </summary>
        /// <param name="id">The ID used to select a color from the palette.</param>
        /// <returns>A color from the palette corresponding to the given ID.</returns>
        public static Color GetColor(int id) => k_ColorPalette[(id % ColorCount + ColorCount) % ColorCount];    // Euclidean modulo operation

        /// <remarks>
        /// List generated using <see cref="CategoricalColorPaletteVisualizer"/>
        /// </remarks>
        static readonly Color[] k_ColorPalette =
        {
            new(  0 / 255f, 180 / 255f,   8 / 255f), // kelly green
            new(142 / 255f,   6 / 255f, 205 / 255f), // french violet
            new(  0 / 255f, 235 / 255f, 193 / 255f), // vivid opal
            new(134 / 255f,   8 / 255f,  28 / 255f), // hot chile
            new(  0 / 255f, 194 / 255f, 249 / 255f), // capri
            new(216 / 255f,  13 / 255f, 123 / 255f), // magenta
            new(175 / 255f, 255 / 255f,  42 / 255f), // lime
            new( 70 / 255f,  11 / 255f, 112 / 255f), // christalle
            new(255 / 255f,  46 / 255f, 149 / 255f), // persian rose
            new(  0 / 255f, 119 / 255f,   2 / 255f), // bilbao
            new(255 / 255f, 163 / 255f, 252 / 255f), // violet
            new(  0 / 255f,  87 / 255f,  69 / 255f), // deep opal
            new(255 / 255f, 135 / 255f,  53 / 255f), // burning orange
            new(  0 / 255f, 121 / 255f, 250 / 255f), // azure
            new(255 / 255f, 215 / 255f, 225 / 255f), // azalea
            new(  0 / 255f,  64 / 255f,   2 / 255f), // british racing green
            new(  0 / 255f, 159 / 255f, 250 / 255f), // bleu de france
            new(171 / 255f,  13 / 255f,  97 / 255f), // jazberry jam
            new(  0 / 255f, 244 / 255f,   7 / 255f), // radioactive green
            new(107 / 255f,   6 / 255f, 159 / 255f), // purple heart
            new(  0 / 255f, 203 / 255f, 167 / 255f), // aquamarine
            new(222 / 255f,  13 / 255f,  46 / 255f), // amaranth red
            new(124 / 255f, 255 / 255f, 250 / 255f), // electric blue
            new( 90 / 255f,  10 / 255f,  51 / 255f), // mulberry
            new(255 / 255f,  66 / 255f,  53 / 255f), // carmine
            new(  0 / 255f,  95 / 255f, 204 / 255f), // royal blue
            new(255 / 255f, 172 / 255f, 198 / 255f), // amaranth pink
            new(  0 / 255f,  90 / 255f,   1 / 255f), // san felix
            new(255 / 255f, 102 / 255f, 253 / 255f), // fuchsia
            new(  0 / 255f, 145 / 255f, 117 / 255f), // elf green
            new(255 / 255f, 226 / 255f,  57 / 255f), // gargoyle gas
            new(  0 / 255f,  48 / 255f, 111 / 255f), // madison
            new(  0 / 255f, 175 / 255f, 142 / 255f), // jeepers creepers
            new(178 / 255f,   7 / 255f,  37 / 255f), // alabama crimson
            new(  0 / 255f, 229 / 255f, 248 / 255f), // aqua blue
            new(129 / 255f,  13 / 255f,  73 / 255f), // french plum
            new(  0 / 255f, 211 / 255f,   2 / 255f), // vivid harlequin
            new(180 / 255f,  10 / 255f, 252 / 255f), // electric purple
            new(134 / 255f, 255 / 255f, 222 / 255f), // light turquoise
            new( 95 / 255f,   9 / 255f,  20 / 255f), // rosewood
            new(237 / 255f,  13 / 255f, 253 / 255f), // psychedelic purple
            new(  0 / 255f, 115 / 255f,  92 / 255f), // robin hood
            new(255 / 255f, 185 / 255f,  53 / 255f), // frenzee
            new(  0 / 255f,  72 / 255f, 158 / 255f), // tory blue
            new(255 / 255f, 120 / 255f, 173 / 255f), // barbie pink
            new(  0 / 255f, 149 / 255f,   3 / 255f), // india green
            new(255 / 255f, 213 / 255f, 253 / 255f), // pale mauve
            new(  0 / 255f,  61 / 255f,  48 / 255f), // sherwood green
        };
    }
}
