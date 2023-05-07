#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
namespace AKCondinoO.Sims.Actors.Skills.SkillBuffs{
    internal class SkillBuff{
     #region pooling
         internal static readonly Dictionary<Type,Queue<SkillBuff>>pool=new Dictionary<Type,Queue<SkillBuff>>();
         internal static readonly Dictionary<Type,List<SkillBuff>>allActiveBuffs=new Dictionary<Type,List<SkillBuff>>();
         static readonly Type[]skillBuffBaseCtorParamsTypes=new Type[]{};
          static readonly object[]skillBuffBaseCtorParams=new object[]{};
           static readonly Dictionary<Type,(ConstructorInfo ctorInfo,object[]ctorParams)>ctorCache=new Dictionary<Type,(ConstructorInfo,object[])>();
            internal static SkillBuff Dequeue(Type buffType){
             if(SkillBuff.pool.TryGetValue(buffType,out Queue<SkillBuff>pool)&&pool.TryDequeue(out SkillBuff skillBuff)){
              return skillBuff;
             }
             skillBuff=null;
             if(ctorCache.TryGetValue(buffType,out(ConstructorInfo ctorInfo,object[]ctorParams)ctorCached)){
              //Log.DebugMessage(simInventoryType+":using ctorCached:"+ctorCached);
              skillBuff=(SkillBuff)ctorCached.ctorInfo.Invoke(ctorCached.ctorParams);
              return skillBuff;
             }
             ConstructorInfo ctor=buffType.GetConstructor(BindingFlags.Instance|BindingFlags.NonPublic,null,skillBuffBaseCtorParamsTypes,null);
             if(ctor!=null){
              object[]cacheCtorParams=new object[skillBuffBaseCtorParams.Length];
              Array.Copy(skillBuffBaseCtorParams,cacheCtorParams,skillBuffBaseCtorParams.Length);
              ctorCache[buffType]=(ctor,cacheCtorParams);
              skillBuff=(SkillBuff)ctor.Invoke(cacheCtorParams);
              return skillBuff;
             }
             if(skillBuff==null){
              Log.Warning("dequeue skill buff "+buffType+" error: type's Constructor could not be handled");
             }
             return skillBuff;
            }
            internal static void Pool(SkillBuff skillBuff){
             skillBuff.OnUnapply(true);
             skillBuff.OnReset();
             Type buffType=skillBuff.GetType();
             if(!SkillBuff.pool.TryGetValue(buffType,out Queue<SkillBuff>pool)){
              SkillBuff.pool.Add(buffType,pool=new Queue<SkillBuff>());
             }
             pool.Enqueue(skillBuff);
            }
     #endregion
     internal Skill activatedBySkill;
     internal SkillBuffEffectsState applyingEffectsOn;
        internal SkillBuff(){
        }
        internal virtual void OnReset(){
         activatedBySkill=null;
         applyingEffectsOn=null;
         duration=0f;
         delay=0f;
         elapsedTime=0f;
         expired=false;
         applied=false;
         hasBeenUpdatedThisFrame=false;
        }
     internal float duration;
      internal float delay;
       internal float elapsedTime;
     internal bool expired=false;
     internal bool applied=false;
     internal bool hasBeenUpdatedThisFrame=false;
        internal virtual void ManualUpdate(float deltaTime){
         if(elapsedTime>=delay){
          if(!expired){
           OnApply();
          }
          if(applied){
           if(elapsedTime>=duration){
            expired=true;
           }
          }
          if(expired){
           OnUnapply();
          }
         }
         elapsedTime+=deltaTime;
         hasBeenUpdatedThisFrame=true;
        }
        internal virtual void OnApply(){
         if(!applied){
          applied=true;
         }
        }
        internal virtual void OnUnapply(bool pooling=false){
         if(pooling){
          if(!expired){
           //  should the skill be applied immediately
           OnApply();
          }
         }
         if(applied){
          applied=false;
         }
        }
    }
}