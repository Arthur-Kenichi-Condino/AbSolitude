#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors.Skills.SkillVisualEffects{
    internal class SkillVisualEffectsManager:MonoBehaviour,ISingletonInitialization{
     internal static SkillVisualEffectsManager singleton{get;set;}
     internal readonly Dictionary<Type,LinkedList<SkillVisualEffect>>pool=new Dictionary<Type,LinkedList<SkillVisualEffect>>();
        private void Awake(){
         if(singleton==null){singleton=this;}else{DestroyImmediate(this);return;}
        }
     internal readonly Dictionary<Type,GameObject>skillVisualEffectPrefabs=new Dictionary<Type,GameObject>();
        public void Init(){
         foreach(var o in Resources.LoadAll("AKCondinoO/Prefabs/Skills/SkillVisualEffects/",typeof(GameObject))){
          Log.DebugMessage("loading GameObject: o.name:"+o.name);
          GameObject gameObject=(GameObject)o;
          SkillVisualEffect skillVisualEffect=gameObject.GetComponent<SkillVisualEffect>();
          if(skillVisualEffect==null)continue;
          Log.DebugMessage("GameObject is SkillVisualEffect["+o.name);
          Type t=skillVisualEffect.GetType();
          skillVisualEffectPrefabs.Add(t,gameObject);
          pool.Add(t,new LinkedList<SkillVisualEffect>());
         }
        }
        public void OnDestroyingCoreEvent(object sender,EventArgs e){
         Log.DebugMessage("SkillVisualEffectsManager:OnDestroyingCoreEvent");
        }
    }
}