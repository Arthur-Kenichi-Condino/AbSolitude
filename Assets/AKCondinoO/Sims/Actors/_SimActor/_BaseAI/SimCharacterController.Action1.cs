#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Inventory;
using AKCondinoO.Sims.Weapons;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AKCondinoO.InputHandler;
namespace AKCondinoO.Sims.Actors{
    internal partial class SimCharacterController{
        internal void OnAction1(){
         if(Enabled.ACTION_1.curState){
          if(actor is BaseAI baseAI){
           if(MainCamera.singleton.toFollowActor==actor){
            if(actor.isAiming){
             if(actor.itemsEquipped!=null){
              if(actor.itemsEquipped.Value.forAction1 is SimWeapon simWeapon){
               Log.DebugMessage("OnAction1():Shoot");
               simWeapon.TryStartShootingAction(simAiming:actor);
              }
             }
            }
           }
          }
         }
        }
    }
}