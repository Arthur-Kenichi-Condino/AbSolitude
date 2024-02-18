#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors.Humanoid.Human.ArthurCondino;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors.Skills{
    internal class SkillAoEManager:MonoBehaviour,ISingletonInitialization{
     internal static SkillAoEManager singleton{get;set;}
     internal readonly Dictionary<Type,LinkedList<SkillAoE>>pool=new Dictionary<Type,LinkedList<SkillAoE>>();
        void Awake(){
         if(singleton==null){singleton=this;}else{DestroyImmediate(this);return;}
        }
     internal readonly Dictionary<Type,GameObject>skillAoEPrefabs=new Dictionary<Type,GameObject>();
        public void Init(){
         foreach(var o in Resources.LoadAll("AKCondinoO/Prefabs/Skills/SkillAoE/",typeof(GameObject))){
          Log.DebugMessage("loading GameObject: o.name:"+o.name);
          GameObject gameObject=(GameObject)o;
          SkillAoE skillAoE=gameObject.GetComponent<SkillAoE>();
          if(skillAoE==null)continue;
          Log.DebugMessage("GameObject is SkillAoE["+o.name);
          Type t=skillAoE.GetType();
          skillAoEPrefabs.Add(t,gameObject);
          pool.Add(t,new LinkedList<SkillAoE>());
         }
        }
        public void OnDestroyingCoreEvent(object sender,EventArgs e){
         Log.DebugMessage("SkillAoEManager:OnDestroyingCoreEvent");
        }
        internal(GameObject skillAoEGameObject,SkillAoE skillAoE)SpawnSkillAoEGameObject(Type skillAoEType,Skill skill){
         GameObject skillAoEGameObject;
         var pool=this.pool[skillAoEType];
         SkillAoE skillAoE;
         if(pool.Count>0){
          skillAoE=pool.First.Value;
          pool.RemoveFirst();
          skillAoE.pooled=null;
          skillAoEGameObject=skillAoE.gameObject;
         }else{
          skillAoEGameObject=Instantiate(skillAoEPrefabs[skillAoEType]);
          skillAoE=skillAoEGameObject.GetComponent<SkillAoE>();
         }
         skillAoE.skill=skill;
         skillAoE.OnSpawned();
         return(skillAoEGameObject,skillAoE);
        }
        internal void Pool(Type skillAoEType,SkillAoE skillAoE){
         skillAoE.OnPool();
         skillAoE.pooled=pool[skillAoEType].AddLast(skillAoE);
        }
    }
}