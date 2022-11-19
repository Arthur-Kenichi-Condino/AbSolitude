#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static AKCondinoO.Voxels.VoxelSystem;
using static AKCondinoO.Voxels.Water.Editing.VoxelWaterEditing;
using static AKCondinoO.Voxels.Water.MarchingCubes.MarchingCubesWater;
namespace AKCondinoO.Voxels.Water.Editing{
    internal class VoxelWaterEditingContainer:BackgroundContainer{
     internal readonly Queue<WaterEditRequest>requests=new Queue<WaterEditRequest>();
    }
    internal class VoxelWaterEditingMultithreaded:BaseMultithreaded<VoxelWaterEditingContainer>{
        internal struct WaterEditOutputData{
         public double density;
         public double absorbing;public bool absorbingB,absorbingT,absorbingW,absorbingE,absorbingS,absorbingN;
         public double spreading;public bool spreadingB,spreadingT,spreadingW,spreadingE,spreadingS,spreadingN;
        }
     internal readonly Queue<Dictionary<Vector3Int,WaterEditOutputData>>waterEditOutputDataPool=new Queue<Dictionary<Vector3Int,WaterEditOutputData>>();
     readonly Dictionary<Vector2Int,Dictionary<Vector3Int,WaterEditOutputData>>dataFromFileToMerge=new();
     readonly Dictionary<Vector2Int,Dictionary<Vector3Int,WaterEditOutputData>>dataForSavingToFile=new();
     readonly StringBuilder stringBuilder=new StringBuilder();
        protected override void Execute(){
         Log.DebugMessage("VoxelWaterEditingMultithreaded:Execute()");
         while(container.requests.Count>0){
          WaterEditRequest editRequest=container.requests.Dequeue();
          Vector3    center    =editRequest.center;
          Vector2Int cCoord1=vecPosTocCoord(center );
          Vector2Int cnkRgn1=cCoordTocnkRgn(cCoord1);
          int cnkIdx1=GetcnkIdx(cCoord1.x,cCoord1.y);
          Vector3Int vCoord1=vecPosTovCoord(center );
          int vxlIdx1=GetvxlIdx(vCoord1.x,vCoord1.y,vCoord1.z);
         }
         VoxelSystem.Concurrent.waterFileData_rwl.EnterWriteLock();
         try{
          //  salvar dados em arquivos
         }catch{
          throw;
         }finally{
          VoxelSystem.Concurrent.waterFileData_rwl.ExitWriteLock();
         }
        }
    }
}