using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Main{
    internal static class Log{
     private static readonly HashSet<string>enabledAt=new();
        internal static void Enable (string className)=>enabledAt.Add   (className);
        internal static void Disable(string className)=>enabledAt.Remove(className);
    }
}