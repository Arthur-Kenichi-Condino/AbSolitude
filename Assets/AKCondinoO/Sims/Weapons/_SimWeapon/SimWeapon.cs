#if UNITY_EDITOR
#define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Weapons{
    internal class SimWeapon:SimObject{
        protected override void Awake(){
         base.Awake();
        }
        internal override void OnActivated(){
         base.OnActivated();
         ammo=startingAmmo;
        }
     [SerializeField]internal float startingAmmo=0f;
     internal float ammo=0f;
      [SerializeField]internal float ammoPerMagazine=0f;
       internal float ammoLoaded=0f;
        internal bool Reload(){
         float ammoToLoad=ammoPerMagazine-ammoLoaded;
         if(ammoToLoad>0f){
          Log.DebugMessage("ammoToLoad:"+ammoToLoad);
          ammoToLoad=Math.Min(ammoToLoad,ammo);
          if(ammoToLoad>0f){
           Log.DebugMessage("reloading ammo:"+ammoToLoad);
           ammoLoaded=ammoToLoad;
           ammo-=ammoToLoad;
           Log.DebugMessage("remaining ammo:"+ammo);
           return true;
          }
         }
         return false;
        }
     [SerializeField]internal float shootDis=900f;
     [SerializeField]internal Transform[]magazines;
     [SerializeField]internal Transform[]cartridges;
     [SerializeField]internal Transform[]bullets;
     [SerializeField]internal Transform muzzle;
     [NonSerialized]RaycastHit[]shootHits=new RaycastHit[4];
        internal bool TryStartShootingAction(SimObject simAiming){
         if(simAiming is BaseAI baseAI){
          if(baseAI.DoShooting(this)){
           return true;
          }
         }
         return false;
        }
        internal void OnShoot(SimObject simAiming){
         if(ammoLoaded>0f){
          OnShootGetHits(simAiming,ref shootHits,out int shootHitsLength);
          if(shootHitsLength>0){
           for(int i=0;i<shootHitsLength;++i){
            RaycastHit shootHit=shootHits[i];
            Log.DebugMessage("shootHit:"+shootHit.collider.name+",of:"+shootHit.collider.transform.root.name);
           }
          }
         }
        }
        internal void OnShootGetHits(SimObject holder,ref RaycastHit[]shootHits,out int shootHitsLength){
         shootHitsLength=0;
         if(muzzle!=null){
          if(holder is SimActor actor&&actor.simActorCharacterController!=null){
           Vector3 shootDir=(actor.simActorCharacterController.aimingAt-muzzle.transform.position).normalized;
           Ray shootRay=new Ray(muzzle.transform.position,shootDir);
           _GetShootHits:{
            shootHitsLength=Physics.RaycastNonAlloc(shootRay,shootHits,shootDis,PhysUtil.shootingHitsLayer,QueryTriggerInteraction.Collide);
           }
           if(shootHitsLength>0){
            if(shootHitsLength>=shootHits.Length){
             Array.Resize(ref shootHits,shootHitsLength*2);
             goto _GetShootHits;
            }
           }
           for(int i=shootHits.Length-1;i>=0;--i){
            if(i>=shootHitsLength){
             shootHits[i]=default(RaycastHit);
             continue;
            }
            RaycastHit hit=shootHits[i];
            if(hit.collider.transform.root==this.transform.root||hit.collider.transform.root==holder.transform.root){
             shootHits[i]=default(RaycastHit);
             shootHitsLength--;
            }
           }
           Array.Sort(shootHits,ShootGetHitsArraySortComparer);
          }
         }
        }
        private int ShootGetHitsArraySortComparer(RaycastHit a,RaycastHit b){//  ordena 'a' relativo a 'b'
         if(a.collider==null&&b.collider==null){
          return 0;
         }
         if(a.collider==null&&b.collider!=null){
          return 1;
         }
         if(a.collider!=null&&b.collider==null){
          return -1;
         }
         return Vector3.Distance(muzzle.position,a.point).CompareTo(Vector3.Distance(muzzle.position,b.point));
        }
    }
}