using AKCondinoO.Bootstrap;
using AKCondinoO.Utilities;
using System;
using UnityEngine;
using UnityEngine.Rendering;
namespace AKCondinoO.SimObjects{
    internal class SimObjectInstancedRenderer{
     private readonly Mesh mesh;
     private readonly int subMeshCount;
     private readonly RenderParams[]renderParams;
     private Matrix4x4[]matrices;
     private int count;
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
          rParams.worldBounds=new Bounds(new(),new(100000,100000,100000));
          renderParams[i]=rParams;
         }
         matrices=new Matrix4x4[preallocate];
         count=0;
        }
        internal int AddInstance(Matrix4x4 matrix){
         int index=count;
         if(matrices.Length<=index){
          int newSize=matrices.Length==0?4:matrices.Length*2;
          Array.Resize(ref matrices,newSize);
         }
         matrices[count++]=matrix;
         return index;
        }
        internal void RemoveAtSwapBack(int index){
         count--;
         matrices[index]=matrices[count];
        }
        internal void Clear(){
         count=0;
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