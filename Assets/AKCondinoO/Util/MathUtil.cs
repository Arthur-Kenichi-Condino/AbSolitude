using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO{
    internal static class MathUtil{
        internal static double NextDouble(this System.Random random,double minimum,double maximum){
         return GenerateRandomDouble(random,minimum,maximum);
        }
        /// <summary>
        ///  [https://stackoverflow.com/questions/1064901/random-number-between-2-double-numbers]
        /// </summary>
        /// <param name="random"></param>
        /// <param name="minimum"></param>
        /// <param name="maximum"></param>
        /// <returns></returns>
        internal static double GenerateRandomDouble(System.Random random,double minimum,double maximum){
         return random.NextDouble()*(maximum-minimum)+minimum;
        }
    }
}