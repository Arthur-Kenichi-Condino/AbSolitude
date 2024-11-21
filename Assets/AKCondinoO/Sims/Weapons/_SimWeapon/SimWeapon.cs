#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors;
using AKCondinoO.Sims.Actors.Combat;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AKCondinoO.Sims.Actors.AnimationEventsHandler;
namespace AKCondinoO.Sims.Weapons{
    internal class SimWeapon:SimObject{
     [SerializeField]internal float shootDis=900f;
     [SerializeField]internal float gracePeriod=.01f;
     [SerializeField]internal Transform[]magazines;
     [SerializeField]internal Transform[]cartridges;
     [SerializeField]internal Transform[]bullets;
     [SerializeField]internal Transform muzzle;
      internal SimWeaponOnShootVisualEffect simWeaponVisualEffect;
        protected override void Awake(){
         base.Awake();
         if(muzzle!=null){
          simWeaponVisualEffect=muzzle.GetComponent<SimWeaponOnShootVisualEffect>();
         }
        }
        internal override void OnActivated(){
         base.OnActivated();
         ammo=startingAmmo;
        }
        internal bool TryStartReloadingAction(SimObject simAiming){
         if(simAiming is BaseAI baseAI){
          if(OnWillReloadChecks(out _)){
           if(baseAI.DoReloadingOnAnimationEventUsingWeapon(this)){
            return true;
           }
          }
         }
         return false;
        }
     [SerializeField]internal float startingAmmo=0f;
     internal float ammo=0f;
      [SerializeField]internal float ammoPerMagazine=0f;
       internal float ammoLoaded=0f;
        internal bool OnWillReloadChecks(out float ammoToLoad){
         ammoToLoad=ammoPerMagazine-ammoLoaded;
         if(ammoToLoad>0f){
          ammoToLoad=Math.Min(ammoToLoad,ammo);
          if(ammoToLoad>0f){
           //Log.DebugMessage("OnWillReloadChecks():ammoToLoad:"+ammoToLoad);
           return true;
          }
         }
         ammoToLoad=0f;
         return false;
        }
        internal void OnReload(SimObject simAiming){
         if(simAiming is BaseAI baseAI&&baseAI.characterController!=null){
          baseAI.characterController.OnReloadEvent();
         }
        }
        internal bool Reload(SimObject simAiming){
         if(OnWillReloadChecks(out float ammoToLoad)){
          Log.DebugMessage("reloading ammo:"+ammoToLoad);
          ammoLoaded=ammoToLoad;
          ammo-=ammoToLoad;
          Log.DebugMessage("remaining ammo:"+ammo);
          return true;
         }
         return false;
        }
        internal bool TryStartShootingAction(SimObject simAiming){
         if(simAiming is BaseAI baseAI){
          if(baseAI.DoShootingOnAnimationEventUsingWeapon(this)){
           return true;
          }
         }
         return false;
        }
     [NonSerialized]RaycastHit[]shootHits=new RaycastHit[4];
        internal void OnShoot(SimObject simAiming){
         if(ammoLoaded>0f){
          OnShootGetHits(simAiming,ref shootHits,out int shootHitsLength);
          if(shootHitsLength>0){
           for(int i=0;i<shootHitsLength;++i){
            RaycastHit shootHit=shootHits[i];
            Log.DebugMessage("shootHit:"+shootHit.collider.name+",of:"+shootHit.collider.transform.root.name);
            GameObject colliderGameObject=shootHit.collider.gameObject;
            SimObject simObjectHit=null;
            BaseAI actorHit=null;
            Hurtboxes hurtbox=null;
            if(LayerMask.LayerToName(colliderGameObject.layer)=="Hurtbox"){
             Log.DebugMessage("shootHit:layer:Hurtbox");
             hurtbox=colliderGameObject.GetComponent<Hurtboxes>();
             if(hurtbox!=null){
              simObjectHit=actorHit=hurtbox.actor;
             }
            }
            if(actorHit!=null){
             actorHit.OnShotByWeapon(this,hurtbox);
             break;
            }
           }
          }
          if(simWeaponVisualEffect!=null){
           simWeaponVisualEffect.OnShot();
          }
         }else{
          Log.DebugMessage("on shoot:no ammo");
          if(simWeaponVisualEffect!=null){
           simWeaponVisualEffect.OnShotDry();
          }
         }
        }
        internal virtual void OnShootGetHits(SimObject holder,ref RaycastHit[]shootHits,out int shootHitsLength){
         shootHitsLength=0;
         if(muzzle!=null){
          if(holder is BaseAI actor&&actor.characterController!=null){
           //  TO DO: este valor está errado: shootDir
           Vector3 shootDir=(actor.characterController.aimingAtRaw-muzzle.transform.position).normalized;
           Ray shootRay=new Ray(muzzle.transform.position,shootDir);
           Debug.DrawRay(shootRay.origin,shootRay.direction*shootDis,Color.white,1f);
           Debug.DrawLine(shootRay.origin,shootRay.origin+(shootRay.direction*shootDis),Color.blue,5f);
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
            Log.DebugMessage("shootHits[i]:"+shootHits[i].transform?.name);
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
        private int ShootGetHitsArraySortComparer(RaycastHit a,RaycastHit b){//  ordena 'a' relativo a 'b', e retorna 'a' antes de 'b' se 'a' for menor que 'b'
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