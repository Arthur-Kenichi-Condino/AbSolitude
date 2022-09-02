#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using static AKCondinoO.Voxels.VoxelSystem;
using static AKCondinoO.Voxels.Terrain.Editing.VoxelTerrainEditing;
using static AKCondinoO.Voxels.Terrain.MarchingCubes.MarchingCubesTerrain;
namespace AKCondinoO.Voxels.Terrain.Editing{
    internal class VoxelTerrainEditingContainer:BackgroundContainer{
     internal object[]terrainSynchronization;
     internal readonly Queue<TerrainEditRequest>requests=new Queue<TerrainEditRequest>();
     internal readonly HashSet<int>dirty=new HashSet<int>();
    }
    internal class VoxelTerrainEditingMultithreaded:BaseMultithreaded<VoxelTerrainEditingContainer>{
        protected override void Execute(){
         Log.DebugMessage("VoxelTerrainEditingMultithreaded:Execute()");
         container.dirty.Clear();
         while(container.requests.Count>0){
          TerrainEditRequest editRequest=container.requests.Dequeue();
          Vector3    center    =editRequest.center;
          Vector3Int size      =editRequest.size;
          double     density   =editRequest.density;
          MaterialId material  =editRequest.material;
          int        smoothness=editRequest.smoothness;
          switch(editRequest.mode){
           case(EditMode.PlaceCube):{
            Log.DebugMessage("EditMode.PlaceCube");
                float sqrt_yx_1=Mathf.Sqrt(Mathf.Pow(size.y,2)+Mathf.Pow(size.x,2));
                float sqrt_xz_1=Mathf.Sqrt(Mathf.Pow(size.x,2)+Mathf.Pow(size.z,2));
                float sqrt_zy_1=Mathf.Sqrt(Mathf.Pow(size.z,2)+Mathf.Pow(size.y,2));
                 float sqrt_yx_xz_1=Mathf.Sqrt(Mathf.Pow(sqrt_yx_1,2)+Mathf.Pow(sqrt_xz_1,2));
                  float sqrt_yx_xz_zy_1=Mathf.Sqrt(Mathf.Pow(sqrt_yx_xz_1,2)+Mathf.Pow(sqrt_zy_1,2));
                float sqrt_yx_2;
                float sqrt_xz_2;
                float sqrt_zy_2;
                Vector2Int cCoord1=vecPosTocCoord(center ),        cCoord3;
                Vector2Int cnkRgn1=cCoordTocnkRgn(cCoord1),        cnkRgn3;
                Vector3Int vCoord1=vecPosTovCoord(center ),vCoord2,vCoord3;
                //  y
                for(int y=0;y<size.y+smoothness;++y){for(vCoord2=new Vector3Int(vCoord1.x,vCoord1.y-y,vCoord1.z);vCoord2.y<=vCoord1.y+y;vCoord2.y+=y*2){
                        if(vCoord2.y>=0&&vCoord2.y<Height){
                //  x
                for(int x=0;x<size.x+smoothness;++x){for(vCoord2.x=vCoord1.x-x                                  ;vCoord2.x<=vCoord1.x+x;vCoord2.x+=x*2){
                         sqrt_yx_2=Mathf.Sqrt(Mathf.Pow(y,2)+Mathf.Pow(x,2));
                //  z
                for(int z=0;z<size.z+smoothness;++z){for(vCoord2.z=vCoord1.z-z                                  ;vCoord2.z<=vCoord1.z+z;vCoord2.z+=z*2){
                         cCoord3=cCoord1;
                         cnkRgn3=cnkRgn1;
                         vCoord3=vCoord2;
                         if(vCoord3.x<0||vCoord3.x>=Width||
                            vCoord3.z<0||vCoord3.z>=Depth
                         ){
                         }
                 if(z==0){break;}
                }}
                 if(x==0){break;}
                }}
                        }
                 if(y==0){break;}
                }}
            break;
           }
          }
         }
         foreach(object syn in container.terrainSynchronization){
          Monitor.Enter(syn);
         }
         try{
         }catch{
          throw;
         }finally{
          foreach(object syn in container.terrainSynchronization){
           Monitor.Exit(syn);
          }
         }
        }
    }
}