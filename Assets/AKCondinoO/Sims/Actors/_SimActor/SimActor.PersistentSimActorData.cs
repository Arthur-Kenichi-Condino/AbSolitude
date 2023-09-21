#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors.Skills;
using AKCondinoO.Sims.Actors.Combat;
using AKCondinoO.Sims.Inventory;
using AKCondinoO.Sims.Weapons.Rifle.SniperRifle;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UMA;
using UMA.CharacterSystem;
using UnityEngine;
using UnityEngine.AI;
using static AKCondinoO.GameMode;
using static AKCondinoO.InputHandler;
using static AKCondinoO.Sims.Actors.SimActor.PersistentSimActorData;
using static AKCondinoO.Voxels.VoxelSystem;
namespace AKCondinoO.Sims.Actors{
    internal abstract partial class SimActor:SimObject{
     internal PersistentSimActorData persistentSimActorData;
        //  [https://stackoverflow.com/questions/945664/can-structs-contain-fields-of-reference-types]
        internal struct PersistentSimActorData{
         public ListWrapper<SkillData>skills;
            public struct SkillData{
             public Type skill;public int level;
            }
         public ListWrapper<SlaveData>slaves;
            public struct SlaveData{
             public Type simObjectType;public ulong idNumber;
            }
         public float timerToRandomMove;
            internal void UpdateData(SimActor simActor){
             skills=new ListWrapper<SkillData>(simActor.skills.Select(kvp=>{return new SkillData{skill=kvp.Key,level=kvp.Value.level};}).ToList());
             slaves=new ListWrapper<SlaveData>(simActor.slaves.Select(v  =>{return new SlaveData{simObjectType=v.simObjectType,idNumber=v.idNumber};}).ToList());
            }
         private static readonly ConcurrentQueue<StringBuilder>stringBuilderPool=new ConcurrentQueue<StringBuilder>();
            public override string ToString(){
             if(!stringBuilderPool.TryDequeue(out StringBuilder stringBuilder)){
              stringBuilder=new StringBuilder();
             }
             stringBuilder.Clear();
             stringBuilder.AppendFormat(CultureInfoUtil.en_US,"skills={{ ");
             skills.Reset();
             while(skills.MoveNext()){
              SkillData skill=skills.Current;
              stringBuilder.AppendFormat(CultureInfoUtil.en_US,"[{0},{1}], ",skill.skill,skill.level);
             }
             stringBuilder.AppendFormat(CultureInfoUtil.en_US,"}} , ");
             stringBuilder.AppendFormat(CultureInfoUtil.en_US,"slaves={{ ");
             slaves.Reset();
             while(slaves.MoveNext()){
              SlaveData slave=slaves.Current;
              stringBuilder.AppendFormat(CultureInfoUtil.en_US,"[{0},{1}], ",slave.simObjectType,slave.idNumber);
             }
             stringBuilder.AppendFormat(CultureInfoUtil.en_US,"}} , ");
             string result=string.Format(CultureInfoUtil.en_US,"persistentSimActorData={{ {0}, }}",stringBuilder.ToString());
             stringBuilderPool.Enqueue(stringBuilder);
             return result;
            }
         private static readonly ConcurrentQueue<List<SkillData>>parsingSkillListPool=new ConcurrentQueue<List<SkillData>>();
         private static readonly ConcurrentQueue<List<SlaveData>>parsingSlaveListPool=new ConcurrentQueue<List<SlaveData>>();
            internal static PersistentSimActorData Parse(string s){
             PersistentSimActorData persistentSimActorData=new PersistentSimActorData();
             if(!parsingSkillListPool.TryDequeue(out List<SkillData>skillList)){
              skillList=new List<SkillData>();
             }
             skillList.Clear();
             if(!parsingSlaveListPool.TryDequeue(out List<SlaveData>slaveList)){
              slaveList=new List<SlaveData>();
             }
             slaveList.Clear();
             //Log.DebugMessage("s:"+s);
             int skillsStringStart=s.IndexOf("skills={");
             if(skillsStringStart>=0){
                skillsStringStart+=8;
              int skillsStringEnd=s.IndexOf("} , ",skillsStringStart);
              string skillsString=s.Substring(skillsStringStart,skillsStringEnd-skillsStringStart);
              int skillStringStart=0;
              while((skillStringStart=skillsString.IndexOf("[",skillStringStart))>=0){
               int skillAssetTypeStringStart=skillStringStart+1;
               int skillAssetTypeStringEnd  =skillsString.IndexOf(",",skillAssetTypeStringStart);
               Type skillAssetType=Type.GetType(skillsString.Substring(skillAssetTypeStringStart,skillAssetTypeStringEnd-skillAssetTypeStringStart));
               int skillLevelStringStart=skillAssetTypeStringEnd+1;
               int skillLevelStringEnd  =skillsString.IndexOf("],",skillLevelStringStart);
               int skillLevel=int.Parse(skillsString.Substring(skillLevelStringStart,skillLevelStringEnd-skillLevelStringStart));
               //Log.DebugMessage("skillType:"+skillType+";skillLevel:"+skillLevel);
               SkillData skill=new SkillData(){
                skill=skillAssetType,
                level=skillLevel,
               };
               skillList.Add(skill);
               skillStringStart=skillLevelStringEnd+2;
              }
             }
             int slavesStringStart=s.IndexOf("slaves={");
             if(slavesStringStart>=0){
                slavesStringStart+=8;
              int slavesStringEnd=s.IndexOf("} , ",slavesStringStart);
              string slavesString=s.Substring(slavesStringStart,slavesStringEnd-slavesStringStart);
              //Log.DebugMessage("slavesString:"+slavesString);
              int slaveStringStart=0;
              while((slaveStringStart=slavesString.IndexOf("[",slaveStringStart))>=0){
               int slaveSimObjectTypeStringStart=slaveStringStart+1;
               int slaveSimObjectTypeStringEnd  =slavesString.IndexOf(",",slaveSimObjectTypeStringStart);
               Type slaveSimObjectType=Type.GetType(slavesString.Substring(slaveSimObjectTypeStringStart,slaveSimObjectTypeStringEnd-slaveSimObjectTypeStringStart));
               int slaveIdNumberStringStart=slaveSimObjectTypeStringEnd+1;
               int slaveIdNumberStringEnd  =slavesString.IndexOf("],",slaveIdNumberStringStart);
               ulong slaveIdNumber=ulong.Parse(slavesString.Substring(slaveIdNumberStringStart,slaveIdNumberStringEnd-slaveIdNumberStringStart));
               SlaveData slave=new SlaveData(){
                simObjectType=slaveSimObjectType,
                idNumber=slaveIdNumber,
               };
               slaveList.Add(slave);
               slaveStringStart=slaveIdNumberStringEnd+2;
              }
             }
             persistentSimActorData.skills=new ListWrapper<SkillData>(skillList);
             persistentSimActorData.slaves=new ListWrapper<SlaveData>(slaveList);
             parsingSkillListPool.Enqueue(skillList);
             parsingSlaveListPool.Enqueue(slaveList);
             return persistentSimActorData;
            }
        }
    }
}