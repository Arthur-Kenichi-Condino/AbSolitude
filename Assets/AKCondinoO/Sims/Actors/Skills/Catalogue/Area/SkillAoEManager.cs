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
        }
        public void OnDestroyingCoreEvent(object sender,EventArgs e){
         Log.DebugMessage("SkillAoEManager:OnDestroyingCoreEvent");
        }
    }
}