using AKCondinoO.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.SimActors.SimInteractions{
    internal static class InteractionDefinitions{
     private static readonly Dictionary<Type,List<SimInteractionDefinition>>definitions=new();
     private static readonly Dictionary<Type,SimInteractionDefinition>defaultDefinition=new();
     internal static readonly Dictionary<Type,ObjectPoolBase>instancing=new();
        internal static void RegisterInteraction<TTarget,TInstance>(Func<SimInteractionDefinition>def,bool isDefault=false)
        where TInstance:SimInteractionInstance,new(){
         var targetType=typeof(TTarget);
         RegisterInteraction<TInstance>(targetType,def,isDefault);
        }
        internal static void RegisterInteraction<TInstance>(Type targetType,Func<SimInteractionDefinition>def,bool isDefault=false)
        where TInstance:SimInteractionInstance,new(){
         if(!definitions.TryGetValue(targetType,out var list)){
          definitions.Add(targetType,list=new());
         }
         list.Add(def());
         if(isDefault){
          defaultDefinition[targetType]=def();
         }
         var instanceType=typeof(TInstance);
         if(!instancing.ContainsKey(instanceType)){
          var pool=Pool.GetPool<TInstance>(
           "",
           ()=>new TInstance(),
           (TInstance item)=>item.OnReturnToPoolRecycle()
          );
          instancing.Add(instanceType,pool);
         }
        }
        internal static void GetFor(IInteractable target,List<SimInteractionDefinition>interactionDefinitions){
         var type=target.GetType();
         while(type!=null){
          if(definitions.TryGetValue(type,out var list)){
           interactionDefinitions.AddRange(list);
          }
          type=type.BaseType;
         }
        }
        internal static SimInteractionDefinition GetDefaultFor(IInteractable target){
         var type=target.GetType();
         while(type!=null){
          if(defaultDefinition.TryGetValue(type,out var def)){
           return def;
          }
          type=type.BaseType;
         }
         return null;
        }
        internal static void UnregisterAll(){
         definitions      .Clear();
         defaultDefinition.Clear();
         instancing       .Clear();
        }
    }
    internal abstract class InteractableInteractions:ScriptableObject{
        internal abstract void Register();
    }
}