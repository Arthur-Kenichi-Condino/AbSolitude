#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors{
    internal partial class BaseAI{
        internal enum GotTargetMode:int{
         FromOwner=0,
         FromFriends=1,
         Defensively=2,
         Aggressively=3,
        }
        internal enum EnemyPriority:int{
         High=0,
         Medium=1,
         Low=2,
        }
     internal readonly SortedList<GotTargetMode,
                        SortedList<EnemyPriority,
                         Dictionary<(Type simType,ulong number),(float dis,float timeout)>
                        >
                       >targetsGotten=new();
        internal virtual void UpdateEnemiesAndAllies(){
        }
    }
}