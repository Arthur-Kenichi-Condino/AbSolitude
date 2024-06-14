#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims{
    internal class SimFloor:SimConstruction{
        protected override void Awake(){
         base.Awake();
         if(!customSnapping.IsSubclassOf(typeof(SimFloor))){
          foreach(Collider collider in volumeColliders){
           GetSnappingRays(collider,snappingRays);
          }
         }
        }
        internal override void GetSnappingRays(Collider collider,Dictionary<Collider,Ray[]>snappingRays){
         if(collider is BoxCollider floorBoxCollider){
          Vector3 extents=floorBoxCollider.bounds.extents;
          Log.DebugMessage("extents:"+extents);
          Ray[]rays=new Ray[4]{
           new Ray(new Vector3( extents.x,-extents.y, extents.z),Vector3.down),
           new Ray(new Vector3(-extents.x,-extents.y, extents.z),Vector3.down),
           new Ray(new Vector3( extents.x,-extents.y,-extents.z),Vector3.down),
           new Ray(new Vector3(-extents.x,-extents.y,-extents.z),Vector3.down)
          };
          snappingRays.Add(floorBoxCollider,rays);
         }
        }
    }
}