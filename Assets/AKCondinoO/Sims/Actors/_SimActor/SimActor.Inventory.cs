#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors{
    internal partial class SimActor{
        internal enum HandsUsage:int{
         None=0,
         OneHanded=1,
         TwoHanded=2,
        }
    }
}