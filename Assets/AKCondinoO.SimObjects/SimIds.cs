using System;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.SimObjects{
    internal class SimIds{
     private readonly Dictionary<(Type type,string variant),ulong>counters=new();
        internal ulong Next((Type type,string variant)key){
         counters.TryGetValue(key,out ulong id);
         id++;
         counters[key]=id;
         return id;
        }
    }
}