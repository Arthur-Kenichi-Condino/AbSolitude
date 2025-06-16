#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
namespace AKCondinoO.Sims{
    internal class SimConstruction:SimObject{
     internal static int constructionLayer;
     [SerializeField]internal ParentConstraint[]snappingConstraints;
     internal readonly Dictionary<Collider,Ray[]>snappingRays=new();
      protected Type customSnapping=typeof(SimConstruction);
     internal float snapTimer=0f;
     internal SimObject snappedTo;
     internal readonly Dictionary<SimObject,float>connections=new();
        protected override void Awake(){
         base.Awake();
        }
        internal virtual void GetSnappingRays(Collider collider,Dictionary<Collider,Ray[]>snappingRays){
        }
        internal override int ManualUpdate(bool doValidationChecks){
         int result=0;
         if(transform.hasChanged){
          if(!SimObjectManager.singleton.disableSnappingToSlots){
          }
         }
         if((result=base.ManualUpdate(doValidationChecks))!=0){
          return result;
         }
         return result;
        }
     [SerializeField]bool DEBUG_DRAW_COLLIDERS=true;
        #if UNITY_EDITOR
            protected override void OnDrawGizmos(){
             base.OnDrawGizmos();
             if(DEBUG_DRAW_COLLIDERS){
              DrawColliders();
             }
            }
            void DrawColliders(){
             if(interactionsEnabled){
              foreach(Collider collider in volumeColliders){
               if(collider is BoxCollider box){
                Gizmos.color=Color.gray;
                Gizmos.matrix=Matrix4x4.TRS(transform.position+box.center,transform.rotation,transform.localScale);
                Gizmos.DrawCube(Vector3.zero,box.size);
               }
              }
              Gizmos.matrix=Matrix4x4.identity;
             }
            }
        #endif
    }
}