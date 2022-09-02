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
        internal struct TerrainEditData{
         internal double density;
         internal MaterialId material;
        }
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
                //  calcular valores para suavização
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
                          ValidateCoord(ref cnkRgn3,ref vCoord3);
                          cCoord3=cnkRgnTocCoord(cnkRgn3);
                         }
                         int cnkIdx3=GetcnkIdx(cCoord3.x,cCoord3.y);
                         sqrt_xz_2=Mathf.Sqrt(Mathf.Pow(x,2)+Mathf.Pow(z,2));
                         sqrt_zy_2=Mathf.Sqrt(Mathf.Pow(z,2)+Mathf.Pow(y,2));
                         double resultDensity;
                         if(y>=size.y||x>=size.x||z>=size.z){
                          if(y>=size.y&&x>=size.x&&z>=size.z){
                           float sqrt_yx_xz_2=Mathf.Sqrt(Mathf.Pow(sqrt_yx_2,2)+Mathf.Pow(sqrt_xz_2,2));
                            float sqrt_yx_xz_zy_2=Mathf.Sqrt(Mathf.Pow(sqrt_yx_xz_2,2)+Mathf.Pow(sqrt_zy_2,2));
                           resultDensity=density*(1f-(sqrt_yx_xz_zy_2-sqrt_yx_xz_1)/(sqrt_yx_xz_zy_2));
                          }else if(y>=size.y&&x>=size.x){resultDensity=density*(1f-(sqrt_yx_2-sqrt_yx_1)/(sqrt_yx_2));
                          }else if(x>=size.x&&z>=size.z){resultDensity=density*(1f-(sqrt_xz_2-sqrt_xz_1)/(sqrt_xz_2));
                          }else if(z>=size.z&&y>=size.y){resultDensity=density*(1f-(sqrt_zy_2-sqrt_zy_1)/(sqrt_zy_2));
                          }else if(y>=size.y){resultDensity=density*(1f-(y-size.y)/(float)y)*1.414f;//  raiz quadrada de 2
                          }else if(x>=size.x){resultDensity=density*(1f-(x-size.x)/(float)x)*1.414f;
                          }else if(z>=size.z){resultDensity=density*(1f-(z-size.z)/(float)z)*1.414f;
                          }else{
                           resultDensity=0d;
                          }
                         }else{
                          resultDensity=density;
                         }
                         //  TO DO: get current file data to merge
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