using AKCondinoO.Sims.Actors;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims{
    internal static class SimObjectUtil{   
		      internal static bool IsSimActor(Type t){
			      return t==typeof(SimActor)||t.IsSubclassOf(typeof(SimActor));
			     }
    }
}