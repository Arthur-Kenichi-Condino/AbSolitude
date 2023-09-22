#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors;
using AKCondinoO.Sims.Inventory;
using AKCondinoO.Sims.Weapons;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Animations;
using static AKCondinoO.Sims.Actors.BaseAI;
namespace AKCondinoO.Sims{
    internal partial class SimObject{
     protected InContainerTransformData inContainerTransformData=null;
        internal void SetAsInventoryItemTransform(){
         //Log.DebugMessage("SetAsInventoryItemTransform:"+id);
         if(asInventoryItem.container is SimHands simHands){
          if(simHands.leftHand!=null&&simHands.rightHand!=null){
           Debug.DrawLine(simHands.leftHand.transform.position,simHands.rightHand.transform.position,Color.blue);
           Vector3 lineBetweenHandsDir=(simHands.leftHand.transform.position-simHands.rightHand.transform.position).normalized;
           Quaternion lineBetweenHandsRot=Quaternion.LookRotation(lineBetweenHandsDir,asInventoryItem.container.asSimObject.transform.up);
           SimInventoryItemsInContainerSettings.InContainerSettings settings=asInventoryItem.settings;
           if(asInventoryItem.container.asSimObject is BaseAI containerAsBaseAI){
            if(containerAsBaseAI.animatorController!=null){
             if(containerAsBaseAI.animatorController.currentWeaponAimLayerIndex!=null){
              string layerName=null;
              int layerIndex=containerAsBaseAI.animatorController.currentWeaponAimLayerIndex.Value;
              GetLayerAtIndex(layerIndex,out layerName);
              if(layerName==null){//  Aim layer has higher priority
               layerIndex=containerAsBaseAI.animatorController.currentWeaponLayerIndex.Value;
               GetLayerAtIndex(layerIndex,out layerName);
              }
              void GetLayerAtIndex(int layerIndex,out string layerName){
               if(containerAsBaseAI.animatorController.layerIndexToName.TryGetValue(layerIndex,out layerName)){
                if(containerAsBaseAI.animatorController.layerTargetWeight[layerIndex]!=1f){
                 layerName=null;
                }
               }
              }
              if(layerName!=null){
               //Log.DebugMessage("currentWeaponAimLayerIndex.Value:layerName:"+layerName);
               if(settings.transformSettings.TryGetValue(typeof(SimHands),out var transformSettingsForSimHands)){
                (Type containerSimType,ActorMotion?containerSimMotion,string layer)key=(containerAsBaseAI.GetType(),containerAsBaseAI.motion,layerName);
                if(transformSettingsForSimHands.TryGetValue(key,out var transformSettingsForParentBodyPartName)){
                 int priority=int.MaxValue;
                 Transform bodyPart=null;
                 InContainerTransformData inContainerTransformData=null;
                 foreach(var kvp in transformSettingsForParentBodyPartName){
                  string parentBodyPartName=kvp.Key;
                  if(containerAsBaseAI.nameToBodyPart.TryGetValue(parentBodyPartName,out Transform parentBodyPart)){
                   int layerPriority=kvp.Value.layerPriority;
                   if(layerPriority<=priority){
                    priority=layerPriority;
                    bodyPart=parentBodyPart;
                    inContainerTransformData=kvp.Value;
                    //Log.DebugMessage("SetAsInventoryItemTransform:layerName:"+layerName);
                   }
                  }
                 }
                 if(bodyPart!=null){
                  if(inContainerTransformData!=this.inContainerTransformData){
                   Log.DebugMessage("set ParentConstraint:"+layerName);
                   this.inContainerTransformData=inContainerTransformData;
                   if(parentConstraint!=null){
                    parentConstraint.locked=false;
                    for(int i=0;i<parentConstraint.sourceCount;++i){
                     parentConstraint.RemoveSource(i);
                    }
                    ConstraintSource source=new ConstraintSource{
                     sourceTransform=bodyPart,
                     weight=1f,
                    };
                    parentConstraint.AddSource(source);
                    Log.DebugMessage("set ParentConstraint:localRotation:"+inContainerTransformData.transform.localRotation.eulerAngles);
                    parentConstraint.SetRotationOffset   (parentConstraint.sourceCount-1,inContainerTransformData.transform.localRotation.eulerAngles);
                    Log.DebugMessage("set ParentConstraint:localPosition:"+inContainerTransformData.transform.localPosition);
                    parentConstraint.SetTranslationOffset(parentConstraint.sourceCount-1,inContainerTransformData.transform.localPosition);
                    parentConstraint.locked=true;
                    parentConstraint.constraintActive=true;
                   }
                  }
                 }
                }
               }
              }
             }
            }
           }
          }
         }
        }
    }
}