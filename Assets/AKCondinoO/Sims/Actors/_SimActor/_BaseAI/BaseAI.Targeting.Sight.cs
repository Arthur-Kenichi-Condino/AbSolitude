#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace AKCondinoO.Sims.Actors{
    internal partial class BaseAI{
        internal enum AggressionMode:int{
         Defensive=0,
         AggressiveToPotentialEnemies=1,
         AggressiveToAll=2,
        }
     [SerializeField]protected AggressionMode MyAggressionMode=AggressionMode.Defensive;internal AggressionMode aggression{get{return MyAggressionMode;}}
        internal virtual void OnSimObjectIsInSight(SimObject simObject){
         ApplyAggressionModeForThenAddTarget(simObject,null,false);
        }
        internal virtual void OnSimObjectIsOutOfSight(SimObject simObject){
         SetTargetToBeRemoved(simObject);
        }
     [NonSerialized]internal readonly Dictionary<SimObject,BaseAI>inAudioRange=new();
        internal virtual void OnSimObjectIsInAudioRange(SimObject simObject,BaseAI ai){
         //Log.DebugMessage("OnSimObjectIsInAudioRange:ai:"+ai);
         inAudioRange[simObject]=ai;
        }
        internal virtual void OnSimObjectIsOutOfAudioRange(SimObject simObject,BaseAI ai){
         inAudioRange.Remove(simObject);
        }
    }
}