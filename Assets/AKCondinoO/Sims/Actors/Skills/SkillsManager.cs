#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors.Skills{
    internal class SkillsManager:MonoBehaviour,ISingletonInitialization{
     internal static SkillsManager singleton{get;set;}
     internal readonly Dictionary<Type,LinkedList<Skill>>pool=new Dictionary<Type,LinkedList<Skill>>();
        private void Awake(){
         if(singleton==null){singleton=this;}else{DestroyImmediate(this);return;}
        }
     internal readonly Dictionary<Type,GameObject>skillPrefabs=new Dictionary<Type,GameObject>();
        public void Init(){
         foreach(var o in Resources.LoadAll("AKCondinoO/Sims/Actors/Skills/",typeof(GameObject))){
          GameObject gameObject=(GameObject)o;
          Skill skill=gameObject.GetComponent<Skill>();
          if(skill==null)continue;
          Log.DebugMessage("skill prefab:"+skill.name);
          Type t=skill.GetType();
          skillPrefabs.Add(t,gameObject);
          pool.Add(t,new LinkedList<Skill>());
         }
        }
        public void OnDestroyingCoreEvent(object sender,EventArgs e){
         Log.DebugMessage("SkillsManager:OnDestroyingCoreEvent");
        }
        internal GameObject SpawnSkillGameObject(Type skillType){
         GameObject skillGameObject;
         var pool=this.pool[skillType];
         if(pool.Count>0){
          Skill skill=pool.First.Value;
          pool.RemoveFirst();
          skill.pooled=null;
          skillGameObject=skill.gameObject;
         }else{
          skillGameObject=Instantiate(skillPrefabs[skillType]);
         }
         return skillGameObject;
        }
    }
}