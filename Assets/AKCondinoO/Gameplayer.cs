using AKCondinoO.Voxels;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AKCondinoO.Voxels.VoxelSystem;
namespace AKCondinoO{
    internal class Gameplayer:MonoBehaviour{
     internal static Gameplayer main;
     internal Vector2Int cCoord,cCoord_Previous;
     internal Vector2Int cnkRgn;
     internal Bounds activeWorldBounds;
        void Awake(){
        }
        internal void Init(){
         activeWorldBounds=new Bounds(Vector3.zero,
          new Vector3(
           (instantiationDistance.x*2+1)*Width,
           Height,
           (instantiationDistance.y*2+1)*Depth
          )
         );
         cCoord_Previous=cCoord=vecPosTocCoord(transform.position);
         OnCoordinatesChanged();
        }
     bool pendingCoordinatesUpdate=true;
        void Update(){
         transform.position=Camera.main.transform.position;
         if(transform.hasChanged){
            transform.hasChanged=false;
          pendingCoordinatesUpdate=true;
         }
         if(pendingCoordinatesUpdate){
            pendingCoordinatesUpdate=false;
          cCoord_Previous=cCoord;
          cCoord=vecPosTocCoord(transform.position);
          if(cCoord!=cCoord_Previous){
           OnCoordinatesChanged();
          }
         }
        }
        void OnCoordinatesChanged(){
         cnkRgn=cCoordTocnkRgn(cCoord);
         activeWorldBounds.center=new Vector3(cnkRgn.x,0,cnkRgn.y);
         VoxelSystem.singleton.generationRequests.Add(this);
        }
    }
}