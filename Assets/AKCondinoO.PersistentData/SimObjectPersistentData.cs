using AKCondinoO.SimObjects;
using AKCondinoO.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static AKCondinoO.PersistentData.PersistentDataSerialization;
namespace AKCondinoO.PersistentData{
    internal class SimObjectPersistentData{
     static readonly Dictionary<(Type,string),ObjectPoolBase>pool=new(){
      {(typeof(SimObjectPersistentData),""),Pool.GetPool<SimObjectPersistentData>("",()=>new(),(SimObjectPersistentData item)=>{item.OnReturnToPoolRecycle();},true)},
     };
     internal static Dictionary<Type,Type>poolIds=new(){
      {typeof(SimObject),typeof(SimObjectPersistentData)},
     };
        internal static SimObjectPersistentData RentFor(Type simObjectType){
         Type poolId;
         switch(simObjectType){
          default:{
           poolId=poolIds[typeof(SimObject)];
           break;
          }
         }
         return(SimObjectPersistentData)pool[(poolId,"")].ObjectRent();
        }
        internal static void Return(Type poolId,SimObjectPersistentData persistentData){
         pool[(poolId,"")].ObjectReturn(persistentData);
        }
        protected virtual void OnReturnToPoolRecycle(){
         //  limpar aqui
         readFromFile=false;
        }
     public bool readFromFile;
     public Quaternion readRot;
     public Vector3 readPos;
     public Vector3 readScale;
     public Quaternion rot;
     public Vector3 pos;
     public Vector3 scale;
        internal virtual void Update(SimObject simObject,SimObjectPersistentData snapshot){
         snapshot.readFromFile=readFromFile;
         snapshot.readRot=readRot;
         snapshot.readPos=readPos;
         snapshot.readScale=readScale;
         var transform=simObject.transform;
         rot=snapshot.rot=transform.rotation;
         pos=snapshot.pos=transform.position;
         scale=snapshot.scale=transform.localScale;
        }
        internal virtual int CalculateSerializedSize(int version,int supportedVersion){
         return
          sizeof(float)*4+//  rot
          sizeof(float)*3+//  pos
          sizeof(float)*3;//  scale
        }
        internal virtual void Write(BinaryWriter writer,int requestedVersion,int supportedVersion){
         WriteQuaternion(writer,rot);
         WriteVector3(writer,pos);
         WriteVector3(writer,scale);
        }
        internal virtual void Read(BinaryReader reader,int savedVersion,int supportedVersion){
         readFromFile=true;
         readRot=ReadQuaternion(reader);
         readPos=ReadVector3(reader);
         readScale=ReadVector3(reader);
         rot=readRot;
         pos=readPos;
         scale=readScale;
        }
     internal static readonly Dictionary<Type,IPersistentDataSerializer>serializers=new(){
      {typeof(SimObjectPersistentData),new SimObjectPersistentDataSerializer()},
     };
        internal class SimObjectPersistentDataSerializer:PersistentDataSerializer<SimObjectPersistentData>{
            protected override int OnCalculateSerializedSize(SimObjectPersistentData value,int version,int supportedVersion){
             switch(supportedVersion){
              default:{
               return
                value.CalculateSerializedSize(version,supportedVersion);
              }
             }
            }
            protected override void OnWriteTo(BinaryWriter writer,SimObjectPersistentData value,int requestedVersion,int supportedVersion){
             switch(supportedVersion){
              default:{
               value.Write(writer,requestedVersion,supportedVersion);
               break;
              }
             }
            }
            protected override void OnReadFromInto(BinaryReader reader,SimObjectPersistentData persistentData,int savedVersion,int supportedVersion){
             switch(supportedVersion){
              default:{
               persistentData.Read(reader,savedVersion,supportedVersion);
               break;
              }
             }
            }
        }
    }
}