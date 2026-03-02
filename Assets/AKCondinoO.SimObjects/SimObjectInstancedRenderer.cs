using AKCondinoO.Utilities;
using UnityEngine;
using UnityEngine.Rendering;
namespace AKCondinoO.SimObjects{
    internal class SimObjectInstancedRenderer{
     private readonly Mesh mesh;
     private readonly int subMeshCount;
     private readonly RenderParams[]renderParams;
     private Matrix4x4[]matrices;
     private int count;
        internal SimObjectInstancedRenderer(Mesh mesh,Material[]material,int layer,int preallocate=0){
         this.mesh=mesh;
         subMeshCount=mesh.subMeshCount;
         renderParams=new RenderParams[subMeshCount];
         for(int i=0;i<subMeshCount;i++){
          RenderParams rParams=new(material[i]);
          rParams.layer=layer;
          rParams.shadowCastingMode=ShadowCastingMode.On;
          rParams.receiveShadows=true;
          renderParams[i]=rParams;
         }
         matrices=Pool.RentArray<Matrix4x4>(preallocate);
         count=0;
        }
        internal void DrawAll(){
         const int batchSize=1023;
         for(int i=0;i<subMeshCount;i++){
          RenderParams rParams=renderParams[i];
          for(int j=0;j<count;j+=1023){
           int size=Mathf.Min(count-j,batchSize);
           Graphics.RenderMeshInstanced(
            in rParams,mesh,i,matrices,size,j
           );
          }
         }
        }
    }
}