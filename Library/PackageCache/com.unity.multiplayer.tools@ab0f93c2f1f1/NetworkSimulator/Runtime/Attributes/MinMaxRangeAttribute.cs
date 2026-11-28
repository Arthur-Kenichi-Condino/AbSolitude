using UnityEngine;

namespace Unity.Multiplayer.Tools.NetworkSimulator.Runtime
{
    /// <summary>
    /// Specifies a value range as float or int
    /// </summary>
    public class MinMaxRangeAttribute : PropertyAttribute
    {
        /// <summary>
        /// The min value
        /// </summary>
        public readonly float Min;
        /// <summary>
        /// The max value
        /// </summary>
        public readonly float Max;
        /// <summary>
        /// If rounding to int is wanted, set to true 
        /// </summary>
        public readonly bool RoundToInt = false;
        /// <summary>
        /// Constructor for the Attribute, default is a float range
        /// </summary>
        /// <param name="min">Minimum</param>
        /// <param name="max">Maximum</param>
        /// <param name="roundToInt">Round to Integer</param>
        public MinMaxRangeAttribute(float min, float max, bool roundToInt = false)
        {
            Min = min;
            Max = max;
            RoundToInt = roundToInt;
        }
    }
}
