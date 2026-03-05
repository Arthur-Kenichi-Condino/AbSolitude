using AKCondinoO.Bootstrap;
using AKCondinoO.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
namespace AKCondinoO.SimObjects{
    internal class SimObjectInstancedRenderer{
     private Mesh mesh;
     private readonly int subMeshCount;
     private readonly RenderParams[]renderParams;
     private Matrix4x4[]matrices;
     private int count;
     private readonly Dictionary<int,SimObject>simObjects=new();//  TO DO: usar array pois o índice é conhecido
        internal SimObjectInstancedRenderer(Mesh mesh,Material[]materials,int layer,int preallocate=0){
         this.mesh=mesh;
         subMeshCount=mesh.subMeshCount;
         renderParams=new RenderParams[subMeshCount];
         for(int i=0;i<subMeshCount;i++){
          var material=materials[i];
          material.enableInstancing=true;
          RenderParams rParams=new(material);
          rParams.layer=layer;
          rParams.shadowCastingMode=ShadowCastingMode.On;
          rParams.receiveShadows=true;
          renderParams[i]=rParams;
         }
         matrices=new Matrix4x4[preallocate];
         count=0;
        }
        internal int AddInstance(SimObject simObject){
         int index=count++;
         if(matrices.Length<=index){
          int newSize=matrices.Length==0?4:matrices.Length*2;
          Array.Resize(ref matrices,newSize);
         }
         Matrix4x4 matrix=simObject.transform.localToWorldMatrix;
         matrices[index]=matrix;
         simObject.instancedRenderingIndex=index;
         simObjects[index]=simObject;
         return index;
        }
        internal void RemoveAtSwapBack(int index){
         simObjects.Remove(index,out SimObject simObject);
         simObject.instancedRenderingIndex=-1;
         count--;
         if(simObjects.Remove(count,out simObject)){
          matrices[index]=matrices[count];
          simObject.instancedRenderingIndex=index;
          simObjects[index]=simObject;
         }
        }
        internal void Clear(bool destroy=false){
         count=0;
         if(destroy){
          this.mesh=null;
         }
         simObjects.Clear();
        }
        internal void DrawAll(){
         //Logs.Message(Logs.LogType.Debug,"mesh.name:"+mesh.name+";subMeshCount:"+subMeshCount);
         const int batchSize=1023;
         for(int i=0;i<subMeshCount;i++){
          RenderParams rParams=renderParams[i];
          for(int j=0;j<count;j+=1023){
           int size=Mathf.Min(count-j,batchSize);
           //Logs.Message(Logs.LogType.Debug,"size:"+size);
           Graphics.RenderMeshInstanced(
            in rParams,mesh,i,matrices,size,j
           );
          }
         }
        }
    }
}