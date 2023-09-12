#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
namespace AKCondinoO.Sims.Actors.Skills.SkillBuffs{
    internal partial class SkillBuff{
     #region pooling
         internal static readonly Dictionary<Type,Queue<SkillBuff>>pool=new Dictionary<Type,Queue<SkillBuff>>();
         internal static readonly Dictionary<Type,List<SkillBuff>>allActiveBuffs=new Dictionary<Type,List<SkillBuff>>();
         static readonly Type[]skillBuffBaseCtorParamsTypes=new Type[]{};
          static readonly object[]skillBuffBaseCtorParams=new object[]{};
           static readonly Dictionary<Type,(ConstructorInfo ctorInfo,object[]ctorParams)>ctorCache=new Dictionary<Type,(ConstructorInfo,object[])>();
            internal static SkillBuff Dequeue(Type buffType){
             Log.DebugMessage("SkillBuff Dequeue buffType:"+buffType);
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
             }else{
              if(!allActiveBuffs.TryGetValue(buffType,out List<SkillBuff>activeBuffsList)){
               allActiveBuffs.Add(buffType,activeBuffsList=new List<SkillBuff>());
              }
              activeBuffsList.Add(skillBuff);
             }
             return skillBuff;
            }
            internal static void Pool(SkillBuff skillBuff,bool gameExiting=false){
             Log.DebugMessage("SkillBuff Pool skillBuff:"+skillBuff);
             skillBuff.OnUnapply(true,gameExiting);
             skillBuff.OnReset();
             Type buffType=skillBuff.GetType();
             if(allActiveBuffs.ContainsKey(buffType)){
              allActiveBuffs[buffType].Remove(skillBuff);
             }
             if(!SkillBuff.pool.TryGetValue(buffType,out Queue<SkillBuff>pool)){
              SkillBuff.pool.Add(buffType,pool=new Queue<SkillBuff>());
             }
             pool.Enqueue(skillBuff);
            }
     #endregion
    }
}