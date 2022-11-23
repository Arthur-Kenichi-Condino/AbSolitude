#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
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
         public double previousDensity;
         public bool sleeping;
            internal WaterEditOutputData(double density,double previousDensity,bool sleeping){
             this.density=density;this.previousDensity=previousDensity;this.sleeping=sleeping;
            }
            public override string ToString(){
             return string.Format(CultureInfoUtil.en_US,"waterEditOutputData={{ density={0} , previousDensity={1} , sleeping={2} , }}",density,previousDensity,sleeping);
            }
            internal static WaterEditOutputData Parse(string s){
             WaterEditOutputData result=new WaterEditOutputData();
             double density=0d;
             double previousDensity=0d;
             bool sleeping=false;
             int densityStringStart=s.IndexOf("density=");
             if(densityStringStart>=0){
                densityStringStart+=8;
              int densityStringEnd=s.IndexOf(" , ",densityStringStart);
              string densityString=s.Substring(densityStringStart,densityStringEnd-densityStringStart);
              //Log.DebugMessage("densityString:"+densityString);
              density=double.Parse(densityString,NumberStyles.Any,CultureInfoUtil.en_US);
             }
             int previousDensityStringStart=s.IndexOf("previousDensity=");
             if(previousDensityStringStart>=0){
                previousDensityStringStart+=16;
              int previousDensityStringEnd=s.IndexOf(" , ",previousDensityStringStart);
              string previousDensityString=s.Substring(previousDensityStringStart,previousDensityStringEnd-previousDensityStringStart);
              previousDensity=double.Parse(previousDensityString,NumberStyles.Any,CultureInfoUtil.en_US);
             }
             int sleepingStringStart=s.IndexOf("sleeping=");
             if(sleepingStringStart>=0){
                sleepingStringStart+=9;
              int sleepingStringEnd=s.IndexOf(" , ",sleepingStringStart);
              string sleepingString=s.Substring(sleepingStringStart,sleepingStringEnd-sleepingStringStart);
              sleeping=bool.Parse(sleepingString);
             }
             result.density=density;
             result.previousDensity=previousDensity;
             result.sleeping=sleeping;
             return result;
            }
        }
     internal readonly Queue<Dictionary<Vector3Int,WaterEditOutputData>>waterEditOutputDataPool=new Queue<Dictionary<Vector3Int,WaterEditOutputData>>();
     readonly Dictionary<Vector2Int,Dictionary<Vector3Int,WaterEditOutputData>>dataFromFileToMerge=new();
     readonly Dictionary<Vector2Int,Dictionary<Vector3Int,WaterEditOutputData>>dataForSavingToFile=new();
     readonly StringBuilder stringBuilder=new StringBuilder();
        protected override void Cleanup(){
         foreach(var editData in dataFromFileToMerge){editData.Value.Clear();waterEditOutputDataPool.Enqueue(editData.Value);}
         dataFromFileToMerge.Clear();
         foreach(var editData in dataForSavingToFile){editData.Value.Clear();waterEditOutputDataPool.Enqueue(editData.Value);}
         dataForSavingToFile.Clear();
        }
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
         void MergeEdits(Vector2Int cCoord,Vector3Int vCoord,Vector2Int cnkRgn,double resultDensity,double previousDensity,bool sleeping){
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