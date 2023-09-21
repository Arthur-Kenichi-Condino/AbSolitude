#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors.Combat;
using AKCondinoO.Sims.Actors.Homunculi.Vanilmirth;
using AKCondinoO.Sims.Actors.Skills;
using AKCondinoO.Sims.Inventory;
using AKCondinoO.Sims.Weapons.Rifle.SniperRifle;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static AKCondinoO.InputHandler;
using static AKCondinoO.Voxels.VoxelSystem;
namespace AKCondinoO.Sims.Actors{
    internal partial class BaseAI:SimActor{
     [SerializeField]bool DEBUG_ACTIVATE_THIRD_PERSON_CAM_TO_FOLLOW_THIS=false;
     [SerializeField]bool DEBUG_TOGGLE_CROUCHING=false;
     [SerializeField]bool        DEBUG_TOGGLE_HOLSTER_WEAPON=false;
     [SerializeField]WeaponTypes DEBUG_TOGGLE_HOLSTER_WEAPON_TYPE=WeaponTypes.SniperRifle;
     [SerializeField]float AFKTimeToUseAI=30f;
      float AFKTimerToUseAI;
     bool?wasCrouchingBeforeShouldCrouch;
        internal override int ManualUpdate(bool doValidationChecks){
         int result=0;
         if((result=base.ManualUpdate(doValidationChecks))!=0){
          DisableNavMeshAgent();
          return result;
         }
         bool shouldCrouch=false;//  is crouching required?
         if(Core.singleton.isServer){
          if(IsOwner){
           if(DEBUG_ACTIVATE_THIRD_PERSON_CAM_TO_FOLLOW_THIS){
              DEBUG_ACTIVATE_THIRD_PERSON_CAM_TO_FOLLOW_THIS=false;
            OnThirdPersonCamFollow();
           }
           if(MainCamera.singleton.toFollowActor==this){
            //Log.DebugMessage("following this:"+this);
            if(InputHandler.singleton.activityDetected&&!Enabled.RELEASE_MOUSE.curState){
             isUsingAI=false;
             AFKTimerToUseAI=AFKTimeToUseAI;
             //Log.DebugMessage("start using manual control:"+this);
            }
           }else{
            if(!isUsingAI){
             isUsingAI=true;
             AFKTimerToUseAI=0f;
             Log.DebugMessage("camera stopped following, use AI:"+this);
            }
           }
           if(!isUsingAI){
            if(AFKTimerToUseAI>0f){
             AFKTimerToUseAI-=Time.deltaTime;
            }
            if(AFKTimerToUseAI<=0f){
             isUsingAI=true;
             Log.DebugMessage("AFK for too long, use AI:"+this);
            }
           }
           if(DEBUG_TOGGLE_HOLSTER_WEAPON){
              DEBUG_TOGGLE_HOLSTER_WEAPON=false;
            if(DEBUG_TOGGLE_HOLSTER_WEAPON_TYPE==WeaponTypes.SniperRifle){
             if(SimObjectSpawner.singleton.simInventoryItemsInContainerSettings.allSettings.TryGetValue(typeof(RemingtonModel700BDL),out SimInventoryItemsInContainerSettings.InContainerSettings simInventoryItemSettings)){
              if(inventoryItemsSpawnData!=null&&inventoryItemsSpawnData.dequeued){
               inventoryItemsSpawnData.at.Add((Vector3.zero,Vector3.zero,Vector3.one,typeof(RemingtonModel700BDL),null,new PersistentData()));
               inventoryItemsSpawnData.asInventoryItemOwnerIds[inventoryItemsSpawnData.at.Count-1]=id.Value;
               inventoryItemsSpawnData.dequeued=false;
               SimObjectSpawner.singleton.OnSpecificSpawnRequestAt(inventoryItemsSpawnData);
              }
             }
            }else if(DEBUG_TOGGLE_HOLSTER_WEAPON_TYPE==WeaponTypes.None){
             //  TO DO: release items
            }
           }
           if(aiSensor){
            if(canSense){
             if(!aiSensor.isActiveAndEnabled){
              aiSensor.Activate();
             }
             if(aiSensor.isActiveAndEnabled){
              if(rightEye){
               if(leftEye){
                aiSensor.transform.position=(leftEye.transform.position+rightEye.transform.position)/2f;
                aiSensor.transform.rotation=leftEye.transform.rotation;
               }else{
                aiSensor.transform.position=rightEye.transform.position;
                aiSensor.transform.rotation=rightEye.transform.rotation;
               }
              }else if(leftEye){
               aiSensor.transform.position=leftEye.transform.position;
               aiSensor.transform.rotation=leftEye.transform.rotation;
              }else if(head){
               if(aiSensor.zIsUp){
                aiSensor.transform.position=head.transform.position;
                aiSensor.transform.rotation=Quaternion.LookRotation(head.up,head.forward);
               }else{
                aiSensor.transform.position=head.transform.position;
                aiSensor.transform.rotation=Quaternion.Euler(0f,head.transform.eulerAngles.y,0f);
               }
              }
             }
            }
           }
           HitHurtBoxesUpdate();
           if(isUsingAI){
            EnableNavMeshAgent();
            if(!navMeshAgent.isOnNavMesh){
             DisableNavMeshAgent();
            }
            if(navMeshAgent.enabled){
             if(navMeshAgent.isStopped!=navMeshAgentShouldBeStopped){
              navMeshAgent.isStopped=navMeshAgentShouldBeStopped;
             }
             AI();
            }
           }else{
            DisableNavMeshAgent();
            if(characterController!=null){
               characterController.ManualUpdate();
             transform.position+=characterController.moveDelta;
             characterController.character.transform.position-=characterController.moveDelta;
             OnCharacterControllerUpdated();
            }
           }
          }else{
           DisableNavMeshAgent();
          }
         }
         if(transform.hasChanged){
          GetCollidersTouchingNonAlloc(instantCheck:true);
         }
         if(gotCollidersTouchingFromInstantCheck){
          for(int i=0;i<collidersTouchingUpperCount;++i){
           Collider colliderTouchingUpper=collidersTouchingUpper[i];
           if(colliderTouchingUpper.transform.root!=transform.root){//  it's not myself
            shouldCrouch=true;
           }
          }
          for(int i=0;i<collidersTouchingMiddleCount;++i){
           Collider colliderTouchingMiddle=collidersTouchingMiddle[i];
           if(colliderTouchingMiddle.transform.root!=transform.root){//  it's not myself
            shouldCrouch=true;
           }
          }
         }else{
          if(simCollisionsTouchingUpper !=null){
           foreach(Collider colliderTouchingUpper  in simCollisionsTouchingUpper .simObjectColliders){
            shouldCrouch=true;
           }
          }
          if(simCollisionsTouchingMiddle!=null){
           foreach(Collider colliderTouchingMiddle in simCollisionsTouchingMiddle.simObjectColliders){
            shouldCrouch=true;
           }
          }
         }
         if(Core.singleton.isServer){
          if(IsOwner){
           if(shouldCrouch){
            if(wasCrouchingBeforeShouldCrouch==null){
               wasCrouchingBeforeShouldCrouch=crouching;
            }
            if(!crouching){
             OnToggleCrouching();
            }
           }else{
            if(wasCrouchingBeforeShouldCrouch!=null){
             if(!wasCrouchingBeforeShouldCrouch.Value){
              if(crouching){
               OnToggleCrouching();
              }
             }else{
              if(!crouching){
               OnToggleCrouching();
              }
             }
               wasCrouchingBeforeShouldCrouch=null;
            }
            if(DEBUG_TOGGLE_CROUCHING){
               DEBUG_TOGGLE_CROUCHING=false;
             OnToggleCrouching();
            }
           }
          }
         }
         UpdateGetters();
         if(animatorController!=null){
            animatorController.ManualUpdate();
         }
         lastForward=transform.forward;
         teleportedMove=false;
         return result;
        }
    }
}