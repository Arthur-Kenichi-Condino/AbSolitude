#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Gameplaying;
using AKCondinoO.Sims.Actors;
using AKCondinoO.Sims.Actors.Skills.SkillBuffs;
using AKCondinoO.Sims.Inventory;
using AKCondinoO.Voxels;
using AKCondinoO.Voxels.Terrain;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations;
using static AKCondinoO.Voxels.VoxelSystem;
namespace AKCondinoO.Sims{
    internal partial class SimObject{
     internal PersistentData persistentData;
        internal struct PersistentData{
         public Quaternion rotation;
         public Vector3    position;
         public Vector3    localScale;
         public(Type simObjectType,ulong idNumber)?masterId;
         public bool isInventoryItem;
         public(Type simInventoryType,ulong idNumber)?containerSimInventoryId;
         //  Não salvar lista de inventários aqui; ela é salva em uma pasta própria, por tipo e id de sim object
            internal void UpdateData(SimObject simObject){
             rotation=simObject.transform.rotation;
             position=simObject.transform.position;
             localScale=simObject.transform.localScale;
             masterId=simObject.masterId;
             isInventoryItem=(simObject.asInventoryItem!=null);
            }
            public override string ToString(){
             return string.Format(CultureInfoUtil.en_US,"persistentData={{ position={0} , rotation={1} , localScale={2} , masterId={3} , isInventoryItem={4} , containerSimInventoryId={5} , }}",position,rotation,localScale,masterId,isInventoryItem,containerSimInventoryId);
            }
            internal static PersistentData Parse(string s){
             PersistentData persistentData=new PersistentData();
             int positionStringStart=s.IndexOf("position=(");
             if(positionStringStart>=0){
                positionStringStart+=10;
              int positionStringEnd=s.IndexOf(") , ",positionStringStart);
              string positionString=s.Substring(positionStringStart,positionStringEnd-positionStringStart);
              string[]xyzString=positionString.Split(',');
              float x=float.Parse(xyzString[0].Replace(" ",""),NumberStyles.Any,CultureInfoUtil.en_US);
              float y=float.Parse(xyzString[1].Replace(" ",""),NumberStyles.Any,CultureInfoUtil.en_US);
              float z=float.Parse(xyzString[2].Replace(" ",""),NumberStyles.Any,CultureInfoUtil.en_US);
              persistentData.position=new Vector3(x,y,z);
             }
             int rotationStringStart=s.IndexOf("rotation=(");
             if(rotationStringStart>=0){
                rotationStringStart+=10;
              int rotationStringEnd=s.IndexOf(") , ",rotationStringStart);
              string rotationString=s.Substring(rotationStringStart,rotationStringEnd-rotationStringStart);
              string[]xyzwString=rotationString.Split(',');
              float x=float.Parse(xyzwString[0].Replace(" ",""),NumberStyles.Any,CultureInfoUtil.en_US);
              float y=float.Parse(xyzwString[1].Replace(" ",""),NumberStyles.Any,CultureInfoUtil.en_US);
              float z=float.Parse(xyzwString[2].Replace(" ",""),NumberStyles.Any,CultureInfoUtil.en_US);
              float w=float.Parse(xyzwString[3].Replace(" ",""),NumberStyles.Any,CultureInfoUtil.en_US);
              persistentData.rotation=new Quaternion(x,y,z,w);
             }
             int localScaleStringStart=s.IndexOf("localScale=(");
             if(localScaleStringStart>=0){
                localScaleStringStart+=12;
              int localScaleStringEnd=s.IndexOf(") , ",localScaleStringStart);
              string localScaleString=s.Substring(localScaleStringStart,localScaleStringEnd-localScaleStringStart);
              string[]xyzString=localScaleString.Split(',');
              float x=float.Parse(xyzString[0].Replace(" ",""),NumberStyles.Any,CultureInfoUtil.en_US);
              float y=float.Parse(xyzString[1].Replace(" ",""),NumberStyles.Any,CultureInfoUtil.en_US);
              float z=float.Parse(xyzString[2].Replace(" ",""),NumberStyles.Any,CultureInfoUtil.en_US);
              persistentData.localScale=new Vector3(x,y,z);
             }
             if(persistentData.localScale.x<=0f||
                persistentData.localScale.y<=0f||
                persistentData.localScale.z<=0f){
              persistentData.localScale=Vector3.one;
             }
             int masterIdStringStart=s.IndexOf("masterId=(");
             if(masterIdStringStart>=0){
                masterIdStringStart+=10;
              int masterIdStringEnd=s.IndexOf(") , ",masterIdStringStart);
              string masterIdString=s.Substring(masterIdStringStart,masterIdStringEnd-masterIdStringStart);
              string[]simObjectTypeIdNumberString=masterIdString.Split(',');
              Type simObjectType=Type.GetType(simObjectTypeIdNumberString[0].Replace(" ",""));
              //Log.DebugMessage("masterId:simObjectType:"+simObjectType);
              ulong idNumber=ulong.Parse(simObjectTypeIdNumberString[1].Replace(" ",""),NumberStyles.Any,CultureInfoUtil.en_US);
              (Type simObjectType,ulong idNumber)masterId=(simObjectType,idNumber);
              persistentData.masterId=masterId;
             }
             int isInventoryItemStringStart=s.IndexOf("isInventoryItem=");
             if(isInventoryItemStringStart>=0){
                isInventoryItemStringStart+=16;
              int isInventoryItemStringEnd=s.IndexOf(" , ",isInventoryItemStringStart);
              string isInventoryItemString=s.Substring(isInventoryItemStringStart,isInventoryItemStringEnd-isInventoryItemStringStart);
              bool isInventoryItem=bool.Parse(isInventoryItemString);
              persistentData.isInventoryItem=isInventoryItem;
             }
             return persistentData;
            }
        }
    }
}