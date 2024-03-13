#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors{
    internal partial class BaseAI{
        internal void Move(Vector3 dest,bool setMyDest=true){
         if(setMyDest)MyDest=dest;
         navMeshAgent.destination=dest;
         GetAStarPath(dest);
        }
        internal void MoveStop(bool setMyDest=true){
         Vector3 dest=navMeshAgent.transform.position;
         if(setMyDest)MyDest=dest;
         navMeshAgent.destination=dest;
        }
    }
}