#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims;
using AKCondinoO.Voxels.Biomes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Pipes;
using System.Text;
using Unity.Collections;
using UnityEngine;
using static AKCondinoO.Voxels.VoxelSystem;
using static AKCondinoO.Voxels.Biomes.BaseBiomeSimObjectsSpawnSettings;
using static AKCondinoO.Voxels.Terrain.SimObjectsPlacing.VoxelTerrainSurfaceSimObjectsPlacerContainer;
using static AKCondinoO.Sims.SimObject;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace AKCondinoO.Voxels.Terrain.SimObjectsPlacing{
    internal struct SpawnMapInfo{
     internal bool isBlocked;
     internal SpawnedTypes type;
    }
    internal struct SpawnCandidateData{
     public(Type simObject,SimObjectSettings simObjectSettings)simObjectPicked;
     public SimObjectSettings simObjectSettings;
     public SimObjectSpawnModifiers modifiers;
     public Vector3 size;
     public Bounds bounds;
     public int priority;
     public Quaternion rotation;
     public bool priorityOverWest, priorityOverEast, priorityOverBothX;
     public bool priorityOverSouth,priorityOverNorth,priorityOverBothZ;
     public double selectionValue;
     public SpawnPickingLayer pickingLayer;
     public int halfSeqSize;
    }
    internal class VoxelTerrainSurfaceSimObjectsPlacerContainer:BackgroundContainer{
     //internal Vector3 maxSpawnSize;
     //internal Vector3 margin;
     internal Vector2Int cCoord;
     internal Vector2Int cnkRgn;
     internal        int cnkIdx;
     internal bool surfaceSimObjectsHadBeenAdded;
     internal NativeList<RaycastCommand>GetGroundRays;
     internal NativeList<RaycastHit    >GetGroundHits;
     internal readonly Dictionary<int,RaycastHit?>gotGroundHits=new Dictionary<int,RaycastHit?>(Width*Depth);
     internal readonly Dictionary<SpawnedTypes,bool[]>blocked=new Dictionary<SpawnedTypes,bool[]>();
     internal Dictionary<int,Dictionary<Vector3Int,bool>>state=new();
     internal Dictionary<int,Dictionary<Vector3Int,SpawnCandidateData>>hasData=new();
      internal Dictionary<int,HashSet<Vector3Int>>hasNoData=new();
     internal readonly(Color color,Bounds bounds,Vector3 scale,Quaternion rotation)[]testArray=new(Color,Bounds,Vector3,Quaternion)[FlattenOffset];
     internal readonly SpawnData spawnData=new SpawnData(FlattenOffset);
        internal enum Execution{
         ReserveBounds,
         GetGround,
         FillSpawnData,
         SaveStateToFile,
        }
     internal Execution execution;
        internal VoxelTerrainSurfaceSimObjectsPlacerContainer(){
         foreach(SpawnedTypes type in Enum.GetValues(typeof(SpawnedTypes))){
          blocked[type]=new bool[FlattenOffset];
         }
        }
        protected override void Dispose(bool disposing){
         if(disposed)return;
         if(disposing){//  free managed resources here
         }
         //  free unmanaged resources here
         base.Dispose(disposing);
        }
    }
    internal class VoxelTerrainSurfaceSimObjectsPlacerMultithreaded:BaseMultithreaded<VoxelTerrainSurfaceSimObjectsPlacerContainer>{
     internal FileStream chunkStateFileStream;
      internal StreamWriter chunkStateFileStreamWriter;
      internal StreamReader chunkStateFileStreamReader;
       readonly StringBuilder stringBuilder=new StringBuilder();
        readonly StringBuilder lineStringBuilder=new StringBuilder();
     //readonly Dictionary<int,SpawnMapInfo[]>spawnMaps=new();
        protected override void Cleanup(){
         //spawnMaps.Clear();
        }
        //bool TryReserveBoundsAt(Vector3 pos,int recursionDepth,out(Type simObject,SimObjectSettings simObjectSettings)?simObjectPicked,out double selectionValue){
        // Vector3Int vCoord=vecPosTovCoord(pos,out Vector2Int cnkRgn);
        // return TryReserveBoundsAt(vCoord,cnkRgn,pos,recursionDepth,out simObjectPicked,out selectionValue);
        //}
        //bool TryReserveBoundsAt(Vector3Int vCoord,Vector2Int cnkRgn,Vector3 pos,int recursionDepth,out(Type simObject,SimObjectSettings simObjectSettings)?simObjectPicked,out double selectionValue){
        // Vector3Int noiseInput=vCoord;noiseInput.x+=cnkRgn.x;
        //                              noiseInput.z+=cnkRgn.y;
        // simObjectPicked=VoxelSystem.biome.biomeSpawnSettings.TryGetSettingsToSpawnSimObject(noiseInput,out selectionValue);
        // return TryReserveBoundsFor(
        //  simObjectPicked,selectionValue,
        //  noiseInput,
        //  vCoord,cnkRgn,pos,
        //  recursionDepth
        // );
        //}
        //bool TryReserveBoundsFor((Type simObject,SimObjectSettings simObjectSettings)?simObjectPicked,double selectionValue,Vector3Int noiseInput,Vector3Int vCoord,Vector2Int cnkRgn,Vector3 pos,int recursionDepth){
        // if(simObjectPicked==null){
        //  return false;
        // }
        // recursionDepth++;
        // bool canSpawn=true;
        // //  criar bounds para teste
        // SimObjectSettings simObjectSettings=simObjectPicked.Value.simObjectSettings;
        // Vector3 size=simObjectSettings.size;
        // //  primeiro verifica se qualquer coisa iria spawn'ar diretamente dentro do tamanho do objeto (bounds), já descarta o objeto
        // bool priorityOverWest =MathUtil.AlternatingSequence(vCoord.x+cnkRgn.x,container.maxSpawnSize.x,0)==0;
        // bool priorityOverSouth=MathUtil.AlternatingSequence(vCoord.z+cnkRgn.y,container.maxSpawnSize.z,0)==0;
        // Vector3Int coord=new Vector3Int(0,Height/2-1,0);
        // for(
        //  coord.x=priorityOverWest ?Mathf.CeilToInt(size.x):Mathf.FloorToInt(-size.x);
        //  priorityOverWest ?(coord.x>0):(coord.x<0);
        //  coord.x+=priorityOverWest ?-1:1
        // ){
        // for(
        //  coord.z=priorityOverSouth?Mathf.CeilToInt(size.z):Mathf.FloorToInt(-size.z);
        //  priorityOverSouth?(coord.z>0):(coord.z<0);
        //  coord.z+=priorityOverSouth?-1:1
        // ){
        //  Vector3 otherPos=pos+new Vector3(coord.x,0,coord.z);
        //  if(
        //   TryReserveBoundsAt(
        //    otherPos,recursionDepth,out(Type simObject,SimObjectSettings simObjectSettings)?otherSimObjectPicked,out double otherSelectionValue
        //   )
        //  ){
        //   canSpawn=false;
        //   goto _Break;
        //  }
        // }
        // }
        // _Break:{}
        // return canSpawn;
        //}
        //bool TryReserveBoundsAt(Vector3Int vCoord1,Vector2Int cnkRgn1,out(Type simObject,SimObjectSettings simObjectSettings)?simObjectPicked1,out double selectionValue1){
        // bool canSpawn=true;
        // Vector3Int noiseInput1=vCoord1;noiseInput1.x+=cnkRgn1.x;
        //                                noiseInput1.z+=cnkRgn1.y;
        // simObjectPicked1=VoxelSystem.biome.biomeSpawnSettings.TryGetSettingsToSpawnSimObject(noiseInput1,out selectionValue1);
        // if(simObjectPicked1==null){
        //  return false;
        // }
        // //  criar bounds para teste
        // SimObjectSettings simObjectSettings1=simObjectPicked1.Value.simObjectSettings;
        // Vector3 size1=simObjectSettings1.size;
        // bool priorityOverWest1 =MathUtil.AlternatingSequence(vCoord1.x+cnkRgn1.x,container.maxSpawnSize.x,0)==0;
        // bool priorityOverEast1 =!priorityOverWest1 ;
        // bool priorityOverSouth1=MathUtil.AlternatingSequence(vCoord1.z+cnkRgn1.y,container.maxSpawnSize.z,0)==0;
        // bool priorityOverNorth1=!priorityOverSouth1;
        // Vector3Int coord2=new Vector3Int(0,Height/2-1,0);
        // for(coord2.x=Mathf.FloorToInt(-size1.x);coord2.x<Mathf.CeilToInt(size1.x);coord2.x++){
        // for(coord2.z=Mathf.FloorToInt(-size1.z);coord2.z<Mathf.CeilToInt(size1.z);coord2.z++){
        //  if(
        //   coord2.x==0&&
        //   coord2.z==0
        //  ){
        //   continue;
        //  }
        //  Log.DebugMessage("coord2:"+coord2,container.cnkRgn.x==0&&container.cnkRgn.y==0);
        //  Vector3 pos2=noiseInput1+coord2;
        //  Vector3Int vCoord2=vecPosTovCoord(pos2,out Vector2Int cnkRgn2);
        //  bool priorityOverWest2 =MathUtil.AlternatingSequence(vCoord2.x+cnkRgn2.x,container.maxSpawnSize.x,0)==0;
        //  bool priorityOverEast2 =!priorityOverWest2 ;
        //  bool priorityOverSouth2=MathUtil.AlternatingSequence(vCoord2.z+cnkRgn2.y,container.maxSpawnSize.z,0)==0;
        //  bool priorityOverNorth2=!priorityOverSouth2;
        //  bool trySimObject2=false;
        //  if(coord2.x<0){
        //   if(priorityOverEast2){
        //    if(priorityOverEast1){
        //     trySimObject2=true;
        //    }
        //    if(priorityOverWest1){
        //    }
        //   }
        //   if(priorityOverWest2){
        //    if(priorityOverEast1){
        //     trySimObject2=true;
        //    }
        //    if(priorityOverWest1){
        //    }
        //   }
        //  }
        //  if(coord2.x>0){
        //   if(priorityOverEast2){
        //    if(priorityOverEast1){
        //    }
        //    if(priorityOverWest1){
        //     trySimObject2=true;
        //    }
        //   }
        //   if(priorityOverWest2){
        //    if(priorityOverEast1){
        //    }
        //    if(priorityOverWest1){
        //    }
        //   }
        //  }
        //  if(trySimObject2){
        //   canSpawn=!TryReserveBoundsAt(vCoord2,cnkRgn2,out(Type simObject,SimObjectSettings simObjectSettings)?simObjectPicked2,out double selectionValue2);
        //   if(!canSpawn){
        //    goto _Break;
        //   }
        //  }
        //  //if(coord2.x>0){         //  
        //  // if(priorityOverWest1){ //  
        //  //  if(priorityOverWest2){
        //  //   //canSpawn=false;
        //  //   //goto _Break;
        //  //  }
        //  //  if(priorityOverEast2){
        //  //  }
        //  // }
        //  // if(priorityOverEast1){
        //  //  if(priorityOverWest2){
        //  //  }
        //  //  if(priorityOverEast2){
        //  //  }
        //  // }
        //  //}
        //  //if(coord2.x<0){
        //  // if(priorityOverWest1){
        //  //  if(priorityOverWest2){
        //  //  }
        //  //  if(priorityOverEast2){
        //  //  }
        //  // }
        //  // if(priorityOverEast1){
        //  //  if(priorityOverWest2){
        //  //  }
        //  //  if(priorityOverEast2){
        //  //  }
        //  // }
        //  //}
        // }}
        // _Break:{}
        // return canSpawn;
        //}
        //bool TryReserveBoundsAt(Vector3Int vCoord1,Vector2Int cnkRgn1,out(Type simObject,SimObjectSettings simObjectSettings)?simObjectPicked1,out double selectionValue1){
        // bool canSpawn=true;
        // Vector3Int noiseInput1=vCoord1;noiseInput1.x+=cnkRgn1.x;
        //                                noiseInput1.z+=cnkRgn1.y;
        // simObjectPicked1=VoxelSystem.biome.biomeSpawnSettings.TryGetSettingsToSpawnSimObject(noiseInput1,out selectionValue1);
        // if(simObjectPicked1==null){
        //  return false;
        // }
        // SimObjectSettings simObjectSettings1=simObjectPicked1.Value.simObjectSettings;
        // Vector3 size1=simObjectSettings1.size;
        // bool priorityOverWest1 =MathUtil.AlternatingSequence(vCoord1.x+cnkRgn1.x,container.maxSpawnSize.x,0)==0;
        // bool priorityOverEast1 =!priorityOverWest1 ;
        // bool priorityOverSouth1=MathUtil.AlternatingSequence(vCoord1.z+cnkRgn1.y,container.maxSpawnSize.z,0)==0;
        // bool priorityOverNorth1=!priorityOverSouth1;
        // Vector3Int coord2=new Vector3Int(0,Height/2-1,0);
        // for(coord2.x=Mathf.FloorToInt(-size1.x);coord2.x<Mathf.CeilToInt(size1.x);coord2.x++){
        // for(coord2.z=Mathf.FloorToInt(-size1.z);coord2.z<Mathf.CeilToInt(size1.z);coord2.z++){
        //  if(
        //   coord2.x==0&&
        //   coord2.z==0
        //  ){
        //   continue;
        //  }
        //  Vector3 pos2=noiseInput1+coord2;
        //  Vector3Int vCoord2=vecPosTovCoord(pos2,out Vector2Int cnkRgn2);
        //  bool priorityOverWest2 =MathUtil.AlternatingSequence(vCoord2.x+cnkRgn2.x,container.maxSpawnSize.x,0)==0;
        //  bool priorityOverEast2 =!priorityOverWest2 ;
        //  bool priorityOverSouth2=MathUtil.AlternatingSequence(vCoord2.z+cnkRgn2.y,container.maxSpawnSize.z,0)==0;
        //  bool priorityOverNorth2=!priorityOverSouth2;
        //  if(
        //   Conflicts(coord2,
        //    priorityOverWest1,priorityOverEast1,
        //    priorityOverWest2,priorityOverEast2
        //   )
        //  ){
        //   canSpawn=false;
        //   goto _Break;
        //  }
        // }}
        // _Break:{}
        // return canSpawn;
        //}
        //bool Conflicts(Vector3Int coord2,
        // bool priorityOverWest1,bool priorityOverEast1,
        // bool priorityOverWest2,bool priorityOverEast2
        //){
        // if(coord2.x<0){
        //  if(priorityOverWest1){
        //   return true;
        //  }
        // }
        // //if(coord2.x>0){
        // // if(priorityOverEast1){
        // //  if(priorityOverWest2){
        // //   return false;
        // //  }
        // //  return true;
        // // }
        // //}
        // return false;
        // //if(priorityOverWest1&&coord2.x<0)return true;//  objeto vence / candidato perde
        // //if(priorityOverEast1&&coord2.x>0)return true;//  objeto vence / candidato perde
        // ////  Caso especial: prioridades opostas, permitir que um lado sempre vença
        // //if(priorityOverEast1&&priorityOverWest2)return true;//  empate -> objeto vence / candidato perde
        // //return false;//  objeto perde / candidato vence
        //}
//        void ResolveSpawnConflict(
//         Vector3 size1,int priority1,
//         Vector3 size2,int priority2,
//         Vector3Int coord,
//         double selectionValue1,
//         double selectionValue2,
//         bool priorityOverWest1 ,bool priorityOverEast1 ,bool priorityOverBothX1,bool priorityOverSouth1,bool priorityOverNorth1,bool priorityOverBothZ1,
//         bool priorityOverWest2 ,bool priorityOverEast2 ,bool priorityOverBothX2,bool priorityOverSouth2,bool priorityOverNorth2,bool priorityOverBothZ2,
//         ref bool canSpawn//InX//,
//         //ref bool canSpawnInZ
//        ){
//         //if(!priorityOverBothX1){
//          //if(!priorityOverWest1){if(coord.x<0){canSpawn=false;return;}}
//          //if(!priorityOverEast1){if(coord.x>0){canSpawn=false;return;}}
//          //if(coord.x<0){
//          // if(priorityOverEast2){
//          // }
//          //}
//          //if(coord.x>0){
//          // if(priorityOverWest2){
//          // }
//          //}
////          if(coord.x<0){//  obj2 está a oeste
////           bool obj1=priorityOverWest1||priorityOverBothX1;
////           bool obj2=priorityOverEast2||priorityOverBothX2;
////           if(obj2&&!obj1){//  obj2 vence
////            canSpawn=false;return;
////           }
////           if(obj1&&!obj2)return;// obj1 vence
////           if(obj1&&obj2){
////           }
////    return "Nenhum";
////}
////else if (obj2.x > obj1.x) // obj2 está a leste
////{
////    bool obj1Wins = priorityOverEast1 || priorityOverBothX1;
////    bool obj2Wins = priorityOverWest2 || priorityOverBothX2;

////    if (obj1Wins && !obj2Wins) return "Obj1 vence";
////    if (obj2Wins && !obj1Wins) return "Obj2 vence";
////    if (obj1Wins && obj2Wins) return "Empate";
////    return "Nenhum";
////}
////else
////{
////    return "Mesma posição em X";
////}
//    //# 1. Prioridade absoluta
//    //if obj1.priority > obj2.priority:
//    //    return obj1
//    //if obj2.priority > obj1.priority:
//    //    return obj2

//    //# 2. Tamanho (qualquer dimensão maior já define)
//    //if obj1.size.x > obj2.size.x or obj1.size.y > obj2.size.y or obj1.size.z > obj2.size.z:
//    //    return obj1
//    //if obj2.size.x > obj1.size.x or obj2.size.y > obj1.size.y or obj2.size.z > obj1.size.z:
//    //    return obj2

//    //# 3. Tabela de prioridades (X e Z)
//    //dx = obj2.coord.x - obj1.coord.x
//    //dz = obj2.coord.z - obj1.coord.z

//    //if dx < 0:  # obj2 a oeste de obj1
//    //    if obj1.priorityOverWest1 or obj1.priorityOverBothX1: return obj1
//    //    if obj2.priorityOverEast2 or obj2.priorityOverBothX2: return obj2
//    //if dx > 0:  # obj2 a leste de obj1
//    //    if obj1.priorityOverEast1 or obj1.priorityOverBothX1: return obj1
//    //    if obj2.priorityOverWest2 or obj2.priorityOverBothX2: return obj2

//    //if dz < 0:  # obj2 ao sul de obj1
//    //    if obj1.priorityOverSouth1 or obj1.priorityOverBothZ1: return obj1
//    //    if obj2.priorityOverNorth2 or obj2.priorityOverBothZ2: return obj2
//    //if dz > 0:  # obj2 ao norte de obj1
//    //    if obj1.priorityOverNorth1 or obj1.priorityOverBothZ1: return obj1
//    //    if obj2.priorityOverSouth2 or obj2.priorityOverBothZ2: return obj2

//    //# 4. Nenhum vencedor ? empate (nenhum criado)
//    //return null
//          //canSpawn=false;
//          //return;
//          //if(priorityOverBothX2){
//          // //  empate
//          //}else{
//          //}
//         //}
//         ////  TO DO: adicionar prioridade por valor ou tamanho opcional ao encontrar priority over both
//         ////  TO DO: e prioridade para sul e oeste no empate priority over both
//         ////canSpawnInX=false;
//         ////canSpawnInZ=false;
//         ////if(!priorityOverBothX1){
//         //// if(coord.x<0){
//         ////  if(!priorityOverWest1){
//         ////   canSpawnInX=false;
//         ////   return;
//         ////  }
//         //// }
//         //// if(coord.x>0){
//         ////  if(!priorityOverEast1){
//         ////   canSpawnInX=false;
//         ////   return;
//         ////  }
//         //// }
//         //// if(!priorityOverBothZ1){
//         ////  if(coord.z<0){
//         ////   if(!priorityOverSouth1){
//         ////    canSpawnInZ=false;
//         ////    return;
//         ////   }
//         ////  }
//         ////  if(coord.z>0){
//         ////   if(!priorityOverNorth1){
//         ////    canSpawnInZ=false;
//         ////    return;
//         ////   }
//         ////  }
//         //// }
//         ////}else{
//         ////}
//         ////if(selectionValue2>selectionValue1){
//         //// if(
//         ////  size2.z>size1.z||
//         ////  size2.x>size1.x
//         //// ){
//         ////  canSpawnInX=false;
//         ////  canSpawnInZ=false;
//         ////  return;
//         //// }
//         ////}
//         //if(!priorityOverBothX1){
//         // if(coord.x<0){
//         //  if(priorityOverBothX2){
//         //   canSpawnInX=false;
//         //   return;
//         //  }
//         //  if(!priorityOverWest1){
//         //   canSpawnInX=false;
//         //   return;
//         //  }
//         // }
//         // if(coord.x>0){
//         //  if(priorityOverBothX2){
//         //   canSpawnInX=false;
//         //   return;
//         //  }
//         //  if(!priorityOverEast1){
//         //   canSpawnInX=false;
//         //   return;
//         //  }
//         // }
//         // if(coord.x==0){
//         //  if(!priorityOverBothZ1){
//         //   if(coord.z<0){
//         //    if(priorityOverBothZ2){
//         //     canSpawnInZ=false;
//         //     return;
//         //    }
//         //    if(!priorityOverSouth1){
//         //     canSpawnInZ=false;
//         //     return;
//         //    }
//         //   }
//         //   if(coord.z>0){
//         //    if(priorityOverBothZ2){
//         //     canSpawnInZ=false;
//         //     return;
//         //    }
//         //    if(!priorityOverNorth1){
//         //     canSpawnInZ=false;
//         //     return;
//         //    }
//         //   }
//         //  }else{
//         //   if(priorityOverBothZ2){//  empate
//         //    if(priority2>priority1){
//         //     canSpawnInZ=false;
//         //     return;
//         //    }else{
//         //     if(size2.z>=size1.z){
//         //      canSpawnInZ=false;
//         //      return;
//         //     }
//         //    }
//         //   }
//         //  }
//         // }
//         //}else{
//         // if(coord.x!=0){
//         //  if(priorityOverBothX2){//  empate
//         //   if(priority2>priority1){
//         //    canSpawnInX=false;
//         //    return;
//         //   }else{
//         //    if(size2.x>=size1.x){
//         //     canSpawnInX=false;
//         //     return;
//         //    }
//         //   }
//         //  }
//         // }
//         // if(coord.x==0){
//         //  if(!priorityOverBothZ1){
//         //   if(coord.z<0){
//         //    if(priorityOverBothZ2){
//         //     canSpawnInZ=false;
//         //     return;
//         //    }
//         //    if(!priorityOverSouth1){
//         //     canSpawnInZ=false;
//         //     return;
//         //    }
//         //   }
//         //   if(coord.z>0){
//         //    if(priorityOverBothZ2){
//         //     canSpawnInZ=false;
//         //     return;
//         //    }
//         //    if(!priorityOverNorth1){
//         //     canSpawnInZ=false;
//         //     return;
//         //    }
//         //   }
//         //  }else{
//         //   if(priorityOverBothZ2){//  empate
//         //    if(priority2>priority1){
//         //     canSpawnInZ=false;
//         //     return;
//         //    }else{
//         //     if(size2.z>=size1.z){
//         //      canSpawnInZ=false;
//         //      return;
//         //     }
//         //    }
//         //   }
//         //  }
//         // }
//         //}
//        }
//     Vector3Int[]getCoordsOutputArray=new Vector3Int[0];
//      readonly HashSet<Vector3Int>getCoordsOutputHashSet=new();
//     //Vector3Int[]getCoordsOutputArray3=new Vector3Int[0];
//        void GetCoords(
//         SimObjectSpawnModifiers modifiers,Bounds bounds,Quaternion rotation,
//         ref Vector3Int[]getCoordsOutputArray,out int length,HashSet<Vector3Int>getCoordsIgnoredHashSet=null
//        ){
//         Vector3 margin=Vector3.one;
//         int getCoordsOutputArraySize=PhysUtil.GetCoordsInsideBoundsMinArraySize(bounds,modifiers.scale,margin);
//         if(getCoordsOutputArraySize>getCoordsOutputArray.Length){
//          Array.Resize(ref getCoordsOutputArray,getCoordsOutputArraySize);
//         }
//         length=PhysUtil.GetCoordsInsideBoundsUsingParallelFor(bounds,modifiers.scale,rotation,margin,
//          false,getCoordsOutputArray,getCoordsIgnoredHashSet
//         );
//        }
//        bool GetSimObjectSettings(Vector3 margin,int layer,Vector3 pos,Vector3Int noiseInput,
//         out(Type simObject,SimObjectSettings simObjectSettings)?simObjectPicked,
//         out SimObjectSettings simObjectSettings,out SimObjectSpawnModifiers modifiers,
//         out Vector3 size,out Bounds bounds,out int priority,
//         out Quaternion rotation,
//         out bool priorityOverWest ,out bool priorityOverEast ,out bool priorityOverBothX,
//         out bool priorityOverSouth,out bool priorityOverNorth,out bool priorityOverBothZ,
//         out double selectionValue,out SpawnPickingLayer pickingLayer
//        ){
//         //  TO DO: rotação levando em conta o solo
//         simObjectPicked=VoxelSystem.biome.biomeSpawnSettings.TryGetSettingsToSpawnSimObject(layer,noiseInput,out selectionValue,out pickingLayer);
//         if(simObjectPicked==null){
//          simObjectSettings=default;modifiers=default;
//          size=default;bounds=default;priority=default;
//          rotation=default;
//          priorityOverWest =default;priorityOverEast =default;priorityOverBothX=default;
//          priorityOverSouth=default;priorityOverNorth=default;priorityOverBothZ=default;
//          return false;
//         }
//         simObjectSettings=simObjectPicked.Value.simObjectSettings;
//         modifiers=VoxelSystem.biome.biomeSpawnSettings.GetSimObjectSpawnModifiers(noiseInput,simObjectSettings);
//         size=simObjectSettings.size;
//         bounds=new Bounds(Vector3.zero,size);
//         priority=simObjectSettings.priority;
//         rotation=Quaternion.AngleAxis(modifiers.rotation,Vector3.up);
//         Bounds maxScaledBounds=new Bounds(Vector3.zero,Vector3.Scale(size,simObjectSettings.maxScale));
//         Vector3 maxScaledExtents=maxScaledBounds.extents;
//         int seqResultX=MathUtil.AlternatingSequenceWithSeparator((int)pos.x,(Mathf.CeilToInt(maxScaledExtents.x)+Mathf.CeilToInt(margin.x))*2,0);
//         int seqResultZ=MathUtil.AlternatingSequenceWithSeparator((int)pos.z,(Mathf.CeilToInt(maxScaledExtents.z)+Mathf.CeilToInt(margin.z))*2,0);
//         priorityOverWest =seqResultX==0;
//         priorityOverEast =seqResultX==1;
//         priorityOverBothX=seqResultX==2;
//         priorityOverSouth=seqResultZ==0;
//         priorityOverNorth=seqResultZ==1;
//         priorityOverBothZ=seqResultZ==2;
//         return true;
//        }
//        //bool TryReserveBoundsAt(Vector3Int pos1,Vector3Int noiseInput1,out Bounds bounds1,out SimObjectSpawnModifiers modifiers1,out Color debugColor){
//        // debugColor=new Color(0,0,0,0);
//        // if(
//        //  GetSimObjectSettings(pos1,noiseInput1,
//        //   out(Type simObject,SimObjectSettings simObjectSettings)?simObjectPicked1,
//        //   out SimObjectSettings simObjectSettings1,out modifiers1,
//        //   out Vector3 size1,out bounds1,out int priority1,
//        //   out Quaternion rotation1,
//        //   out bool priorityOverWest1 ,out bool priorityOverEast1 ,out bool priorityOverBothX1,
//        //   out bool priorityOverSouth1,out bool priorityOverNorth1,out bool priorityOverBothZ1,
//        //   out double selectionValue1
//        //  )
//        // ){
//        //  debugColor=Color.gray;
//        //  bool canSpawn=true;
//        //  GetCoords(
//        //   modifiers1,bounds1,rotation1,
//        //   ref getCoordsOutputArray,out int length
//        //  );
//        //  var parallelForResult=Parallel.For(0,length,(i,parallelForState)=>{
//        //   Vector3Int coord=getCoordsOutputArray[i];
//        //   if(coord.x==0&&coord.z==0){return;}
//        //   Vector3 pos2=pos1;
//        //   pos2.x+=coord.x;
//        //   pos2.z+=coord.z;
//        //   Vector3Int vCoord2=vecPosTovCoord(pos2,out Vector2Int cnkRgn2);
//        //   Vector3Int noiseInput2=vCoord2;noiseInput2.x+=cnkRgn2.x;
//        //                                  noiseInput2.z+=cnkRgn2.y;
//        //   if(
//        //    GetSimObjectSettings(pos2,noiseInput2,
//        //     out(Type simObject,SimObjectSettings simObjectSettings)?simObjectPicked2,
//        //     out SimObjectSettings simObjectSettings2,out SimObjectSpawnModifiers modifiers2,
//        //     out Vector3 size2,out Bounds bounds2,out int priority2,
//        //     out Quaternion rotation2,
//        //     out bool priorityOverWest2 ,out bool priorityOverEast2 ,out bool priorityOverBothX2,
//        //     out bool priorityOverSouth2,out bool priorityOverNorth2,out bool priorityOverBothZ2,
//        //     out double selectionValue2
//        //    )
//        //   ){//  can we still spawn simObjectPicked1 if there's a simObjectPicked2 here?
//        //    bool local_canSpawn=true;
//        //    ResolveSpawnConflict(
//        //     size1,priority1,
//        //     size2,priority2,
//        //     coord,
//        //     selectionValue1,
//        //     selectionValue2,
//        //     priorityOverWest1 ,priorityOverEast1 ,priorityOverBothX1,priorityOverSouth1,priorityOverNorth1,priorityOverBothZ1,
//        //     priorityOverWest2 ,priorityOverEast2 ,priorityOverBothX2,priorityOverSouth2,priorityOverNorth2,priorityOverBothZ2,
//        //     ref local_canSpawn
//        //    );
//        //    if(!local_canSpawn){
//        //     //  try to spawn 2, if cannot spawn 2, 1 still can spawn
//        //     lock(getCoordsOutputArray){
//        //      canSpawn=false;
//        //      parallelForState.Stop();
//        //      return;
//        //     }
//        //    }
//        //   }
//        //  });
//        //  if(canSpawn){
//        //   if(priorityOverBothX1){
//        //    debugColor=Color.cyan;
//        //   }else if(priorityOverWest1){
//        //    debugColor=Color.blue;
//        //   }else{
//        //    debugColor=Color.green;
//        //   }
//        //   return true;
//        //  }
//        // }
//        // return false;
//        //}
//        bool TryReserveBoundsAtRecursively(Vector3 margin,int layer,Vector3Int pos1,Vector3Int noiseInput1,ref bool recursion){
//         if(stateX.TryGetValue(pos1,out bool result1)){
//          return result1;
//         }
//         result1=false;
//         if(
//          GetSimObjectSettings(margin,layer,pos1,noiseInput1,
//           out(Type simObject,SimObjectSettings simObjectSettings)?simObjectPicked1,
//           out SimObjectSettings simObjectSettings1,out SimObjectSpawnModifiers modifiers1,
//           out Vector3 size1,out Bounds bounds1,out int priority1,
//           out Quaternion rotation1,
//           out bool priorityOverWest1 ,out bool priorityOverEast1 ,out bool priorityOverBothX1,
//           out bool priorityOverSouth1,out bool priorityOverNorth1,out bool priorityOverBothZ1,
//           out double selectionValue1,out SpawnPickingLayer pickingLayer1
//          )
//         ){
//          result1=true;
//          Dictionary<Vector3Int,bool>candidates            =new();
//          Dictionary<Vector3Int,bool>candidatesWithPriority=new();
//          int max=Mathf.CeilToInt(Mathf.Max(pickingLayer1.maxDimensions.x,pickingLayer1.maxDimensions.z))+Mathf.CeilToInt(margin.x);
//          Vector3Int coord2=new Vector3Int(0,Height/2-1,0);
//          for(coord2.x=-max;coord2.x<=max;coord2.x++){if(coord2.x==0&&coord2.z==0){continue;}
//           Vector3Int pos2=pos1;
//           pos2.x+=coord2.x;
//           pos2.z+=coord2.z;
//           Vector3Int vCoord2=vecPosTovCoord(pos2,out Vector2Int cnkRgn2);
//           Vector3Int noiseInput2=vCoord2;noiseInput2.x+=cnkRgn2.x;
//                                          noiseInput2.z+=cnkRgn2.y;
//           if(
//            GetSimObjectSettings(margin,layer,pos2,noiseInput2,
//             out(Type simObject,SimObjectSettings simObjectSettings)?simObjectPicked2,
//             out SimObjectSettings simObjectSettings2,out SimObjectSpawnModifiers modifiers2,
//             out Vector3 size2,out Bounds bounds2,out int priority2,
//             out Quaternion rotation2,
//             out bool priorityOverWest2 ,out bool priorityOverEast2 ,out bool priorityOverBothX2,
//             out bool priorityOverSouth2,out bool priorityOverNorth2,out bool priorityOverBothZ2,
//             out double selectionValue2,out SpawnPickingLayer pickingLayer2
//            )
//           ){
//            Bounds bounds1a=bounds1;bounds1a.center=pos1;
//            Bounds bounds2a=bounds2;bounds2a.center=pos2;
//            if(bounds2a.Intersects(bounds1a)){
//             if(priorityOverBothX2){
//              //if(priorityOverBothX1){
//              // recursion=false;
//              // result1=false;
//              // break;
//              //}else{
//              // candidatesWithPriority.Add(pos2,true);
//              //}
//              if(size1.x<=size2.x&&size1.z<=size2.z){
//               recursion=false;
//               result1=false;
//               break;
//              }
//             }
//             candidates.Add(pos2,true);
//            }
//           }
//          }
//          //foreach(var kvp in candidatesWithPriority){
//          // recursion=false;
//          // result1=false;
//          // break;
//          //}
//          foreach(var kvp in candidates){
//           if(recursion){
//            Vector3Int pos2=kvp.Key;
//            Vector3Int vCoord2=vecPosTovCoord(pos2,out Vector2Int cnkRgn2);
//            Vector3Int noiseInput2=vCoord2;noiseInput2.x+=cnkRgn2.x;
//                                           noiseInput2.z+=cnkRgn2.y;
//            if(stateX.TryGetValue(pos2,out bool result2)){
//             if(result2){
//              result1=false;
//             }
//             break;
//            }
//            if(TryReserveBoundsAtRecursively(margin,layer,pos2,noiseInput2,ref recursion)){
//             result1=false;
//             break;
//            }else{
//             recursion=true;
//            }
//           }
//          }

//    //vai de x -6 até x 6
//    // pega todos os candidatos na lista candidates
//    // e candidateswithpriorityoverbothx
//    //para cada candidato, 
//    // se pelo menos 1 em candidateswithpriorityoverbothx
//    //  ele terá prioridade, então não testa mais nada
//    // caso contrário testa os outros de forma recursiva
        

//          ////for(int x=1;x<pickingLayer1.maxDimensions.x+Mathf.CeilToInt(margin.x);++x){
//          ////}
//          //Vector3Int coord2=new Vector3Int(0,Height/2-1,0);
//          //for(coord2.x=-();coord2.x<=Mathf.CeilToInt(Mathf.Max(pickingLayer1.maxDimensions.x,pickingLayer1.maxDimensions.z))+Mathf.CeilToInt(margin.x);coord2.x++){
//          // if(coord2.x==0&&coord2.z==0){continue;}
//          // Vector3Int pos2=pos1;
//          // pos2.x+=coord2.x;
//          // pos2.z+=coord2.z;
//          // Vector3Int vCoord2=vecPosTovCoord(pos2,out Vector2Int cnkRgn2);
//          // Vector3Int noiseInput2=vCoord2;noiseInput2.x+=cnkRgn2.x;
//          //                                noiseInput2.z+=cnkRgn2.y;
//          // if(
//          //  GetSimObjectSettings(margin,layer,pos2,noiseInput2,
//          //   out(Type simObject,SimObjectSettings simObjectSettings)?simObjectPicked2,
//          //   out SimObjectSettings simObjectSettings2,out SimObjectSpawnModifiers modifiers2,
//          //   out Vector3 size2,out Bounds bounds2,out int priority2,
//          //   out Quaternion rotation2,
//          //   out bool priorityOverWest2 ,out bool priorityOverEast2 ,out bool priorityOverBothX2,
//          //   out bool priorityOverSouth2,out bool priorityOverNorth2,out bool priorityOverBothZ2,
//          //   out double selectionValue2,out SpawnPickingLayer pickingLayer2
//          //  )
//          // ){
//          //  Bounds bounds1a=bounds1;bounds1a.center=pos1;
//          //  Bounds bounds2a=bounds2;bounds2a.center=pos2;
//          //  if(state.TryGetValue(pos2,out bool result2)){
//          //   if(result2){
//          //    if(bounds2a.Intersects(bounds1a)){
//          //     result1=false;
//          //    }
//          //   }
//          //   goto _AfterRecursion2;
//          //  }
//          //  if(
//          //   size1.x<size2.x||
//          //   size1.z<size2.z
//          //  ){
//          //  }else if(
//          //   size1.x>size2.x||
//          //   size1.z>size2.z
//          //  ){
//          //   goto _AfterRecursion2;
//          //  }else{
//          //   if(priorityOverBothX2){
//          //    if(bounds2a.Intersects(bounds1a)){
//          //     result1=false;
//          //     goto _AfterRecursion2;
//          //    }
//          //   }
//          //   //if(!priorityOverBothX1){
//          //   // if(!priorityOverWest1){
//          //   //  if(coord2.x<0){
//          //   //   result1=false;
//          //   //   goto _AfterRecursion2;
//          //   //  }
//          //   // }
//          //   // if(!priorityOverEast1){
//          //   //  if(coord2.x>0){
//          //   //   result1=false;
//          //   //   goto _AfterRecursion2;
//          //   //  }
//          //   // }
//          //   //}else{
//          //   // goto _AfterRecursion2;
//          //   //}
//          //  }
//          //  if(bounds2a.Intersects(bounds1a)){
//          //   if(TryReserveBoundsAtRecursively(margin,layer,pos2,noiseInput2)){
//          //    result1=false;
//          //    goto _AfterRecursion2;
//          //   }
//          //  }
//          //  _AfterRecursion2:{}
//          //  if(result1==false){
//          //   goto _End2;
//          //  }
//          // }
//          //}
//          //_End2:{}
//         }
//         stateX.Add(pos1,result1);
//         return result1;
//        }
//        protected struct SpawnCandidateData{
//         public(Type simObject,SimObjectSettings simObjectSettings)simObjectPicked;
//         public SimObjectSettings simObjectSettings;
//         public SimObjectSpawnModifiers modifiers;
//         public Vector3 size;
//         public Bounds bounds;
//         public int priority;
//         public Quaternion rotation;
//         public bool priorityOverWest, priorityOverEast, priorityOverBothX;
//         public bool priorityOverSouth,priorityOverNorth,priorityOverBothZ;
//         public double selectionValue;
//         public SpawnPickingLayer pickingLayer;
//         public int max;
//        }
//     //  TO DO: fazer hasNoData, e fazer hasData, também, aplicando layer
//     readonly Dictionary<Vector3Int,SpawnCandidateData>hasData=new();
//      readonly HashSet<Vector3Int>hasNoData=new();
//        protected bool GetCandidateData(int layer,Vector3 margin,Vector3Int pos1,Vector3Int noiseInput1,
//         out SpawnCandidateData spawnCandidateData1,out SpawnPickingLayer pickingLayer,out int max
//        ){
//         if(hasNoData.Contains(pos1)){
//          spawnCandidateData1=default;
//          pickingLayer=null;
//          max=default;
//          return false;
//         }
//         if(hasData.TryGetValue(pos1,out spawnCandidateData1)){
//          pickingLayer=spawnCandidateData1.pickingLayer;
//          max=spawnCandidateData1.max;
//          return true;
//         }
//         (Type simObject,SimObjectSettings simObjectSettings)?simObjectPicked1=VoxelSystem.biome.biomeSpawnSettings.TryGetSettingsToSpawnSimObject(layer,noiseInput1,out double selectionValue1,out pickingLayer);
//         if(simObjectPicked1==null){
//          spawnCandidateData1=default;
//          max=default;
//          hasNoData.Add(pos1);
//          return false;
//         }
//         SimObjectSettings simObjectSettings1=simObjectPicked1.Value.simObjectSettings;
//         SimObjectSpawnModifiers modifiers1=VoxelSystem.biome.biomeSpawnSettings.GetSimObjectSpawnModifiers(noiseInput1,simObjectSettings1);
//         Vector3 size1=simObjectSettings1.size;
//         Bounds bounds1=new Bounds(Vector3.zero,size1);
//         int priority1=simObjectSettings1.priority;
//         //  TO DO: aplicar rotação do chão na rotação final
//         Quaternion rotation1=Quaternion.AngleAxis(modifiers1.rotation,Vector3.up);
//         Bounds maxScaledBounds1=new Bounds(Vector3.zero,Vector3.Scale(size1,simObjectSettings1.maxScale));
//         Vector3 maxScaledExtents1=maxScaledBounds1.extents;
//         //  seqResult tem que levar o tamanho único, porque tem possibilidade de rotação
//         max=Mathf.CeilToInt(Mathf.Max(pickingLayer.maxDimensions.x,pickingLayer.maxDimensions.z))+Mathf.CeilToInt(Mathf.Max(margin.x,margin.z));
//         int seqResultX1=MathUtil.AlternatingSequenceWithSeparator(pos1.x,max*2,0);
//         int seqResultZ1=MathUtil.AlternatingSequenceWithSeparator(pos1.z,max*2,0);//  seqStart:pickingLayer1.maxDimensions.z
//         spawnCandidateData1=new(){
//          simObjectPicked=simObjectPicked1.Value,
//          simObjectSettings=simObjectSettings1,
//          modifiers=modifiers1,
//          size=size1,
//          bounds=bounds1,
//          priority=priority1,
//          rotation=rotation1,
//          priorityOverWest =(seqResultX1==0),priorityOverEast =(seqResultX1==1),priorityOverBothX=(seqResultX1==2),
//          priorityOverSouth=(seqResultZ1==0),priorityOverNorth=(seqResultZ1==1),priorityOverBothZ=(seqResultZ1==2),
//          selectionValue=selectionValue1,
//          pickingLayer=pickingLayer,
//          max=max,
//         };
//         hasData.Add(pos1,spawnCandidateData1);
//         return true;
//        }
//class Vector2DescendingComparer : IComparer<Vector2>
//{
//    public int Compare(Vector2 a, Vector2 b)
//    {
//        // Calcula “tamanho” como x + z
//        float sizeA = a.x + a.y;
//        float sizeB = b.x + b.y;

//        // Ordem decrescente
//        int cmp = sizeB.CompareTo(sizeA);
//        if(cmp != 0) return cmp;

//        // Se mesmo tamanho, desempata por x
//        cmp = b.x.CompareTo(a.x);
//        if(cmp != 0) return cmp;

//        // Se ainda empate, desempata por y
//        return b.y.CompareTo(a.y);
//    }
//}
//     readonly Dictionary<Vector3Int,bool>state=new();
//        protected bool RecursivelyTryReserveBoundsAt(int layer,Vector3 margin,Vector3Int pos1,Vector3Int noiseInput1,
//         out SpawnCandidateData spawnCandidateData1
//        ){
//         SpawnPickingLayer pickingLayer1;
//         int max1;
//         if(state.TryGetValue(pos1,out bool result1)){
//          if(result1){
//           GetCandidateData(layer,margin,pos1,noiseInput1,out spawnCandidateData1,out pickingLayer1,out max1);
//          }else{
//           spawnCandidateData1=default;
//          }
//          return result1;
//         }
//         result1=true;
//         if(!GetCandidateData(layer,margin,pos1,noiseInput1,out spawnCandidateData1,out pickingLayer1,out max1)){
//          result1=false;
//         }
//         if(result1){
//          result1=RecursivelyTryReserveBoundsBySizeAt(layer,margin,pos1,noiseInput1,
//           spawnCandidateData1,pickingLayer1,max1
//          );
//         }
//         state.Add(pos1,result1);
//         return result1;
//        }
//        protected bool RecursivelyTryReserveBoundsBySizeAt(int layer,Vector3 margin,Vector3Int pos1,Vector3Int noiseInput1,
//         SpawnCandidateData spawnCandidateData1,SpawnPickingLayer pickingLayer1,int max1
//        ){
//         bool result1=true;
//         Vector2 size1=new Vector2(
//          spawnCandidateData1.size.x,
//          spawnCandidateData1.size.z
//         );
//         var candidatesThatConflictBySize=new SortedDictionary<Vector2,Dictionary<Vector3Int,SpawnCandidateData>>(
//          new Vector2DescendingComparer()//  ordem decrescente
//         );
//         {
//          Vector3Int coord2=new Vector3Int(0,Height/2-1,0);
//          for(coord2.x=-max1;coord2.x<=max1;coord2.x++){
//          for(coord2.z=-max1;coord2.z<=max1;coord2.z++){if(coord2.x==0&&coord2.z==0){continue;}
//           Vector3Int pos2=pos1;
//           pos2.x+=coord2.x;
//           pos2.z+=coord2.z;
//           Vector3Int vCoord2=vecPosTovCoord(pos2,out Vector2Int cnkRgn2);
//           Vector3Int noiseInput2=vCoord2;noiseInput2.x+=cnkRgn2.x;
//                                          noiseInput2.z+=cnkRgn2.y;
//           if(GetCandidateData(layer,margin,pos2,noiseInput2,out SpawnCandidateData spawnCandidateData2,out SpawnPickingLayer pickingLayer2,out int max2)){
//            Vector2 size2=new Vector2(
//             spawnCandidateData2.size.x,
//             spawnCandidateData2.size.z
//            );
//            if(
//             size2.x<size1.x&&
//             size2.y<size1.y
//            ){
//             continue;
//            }
//            Bounds bounds1=spawnCandidateData1.bounds;
//            Bounds bounds2=spawnCandidateData2.bounds;
//            Bounds worldBounds1=bounds1;worldBounds1.center=pos1;
//            Bounds worldBounds2=bounds2;worldBounds2.center=pos2;
//            if(!worldBounds2.Intersects(worldBounds1)){
//             continue;
//            }
//            if(
//             !candidatesThatConflictBySize.TryGetValue(size2,out var candidatesThatConflict)
//            ){
//             candidatesThatConflictBySize.Add(size2,candidatesThatConflict=new());
//            }
//            candidatesThatConflict.Add(pos2,spawnCandidateData2);
//           }
//          }}
//         }
//         Dictionary<Vector3Int,SpawnCandidateData>candidatesThatConflictWithSameSize=new();
//         foreach(var kvp in candidatesThatConflictBySize){
//          Vector2 size2=kvp.Key;//  from bigger to smaller
//          var candidatesThatConflict=kvp.Value;
//          if(
//           size2.x<size1.x&&
//           size2.y<size1.y
//          ){
//           continue;
//          }
//          if(
//           size2.x>size1.x||
//           size2.y>size1.y
//          ){
//           //  candidate2 is bigger, so if it wins in a recursion with its neighbours of same size, or loses to a bigger with its recursion, there'll be no need to do a recursion with the smaller sizes: break instantly,
//           // and by that, I mean goto _IteratingCandidatesThatConflictBreak
//           //  [não coloquei a recursão ainda e só fiz objetos de mesmo tamanho aparecerem, vou testar desempate com objetos do mesmo tamanho primeiro somente]
//           result1=false;
//           goto _IteratingCandidatesThatConflictBreak;
//          }
//          foreach(var posSpawnCandidateDataPair2 in candidatesThatConflict){
//           Vector3Int pos2=posSpawnCandidateDataPair2.Key;
//           Vector3Int vCoord2=vecPosTovCoord(pos2,out Vector2Int cnkRgn2);
//           Vector3Int noiseInput2=vCoord2;noiseInput2.x+=cnkRgn2.x;
//                                          noiseInput2.z+=cnkRgn2.y;
//           SpawnCandidateData spawnCandidateData2=posSpawnCandidateDataPair2.Value;
//           //  there'll be a recursion here only if the candidate2 wins, and by that, it should ignore pos1 in its next recursion
//           if(pos2.x!=pos1.x){
//            if(
//             spawnCandidateData2.priorityOverBothX&&
//             spawnCandidateData1.priorityOverBothX
//            ){
//             result1=false;
//             goto _IteratingCandidatesThatConflictBreak;
//            }
//            if(
//             spawnCandidateData2.priorityOverEast&&
//             spawnCandidateData1.priorityOverWest
//            ){
//             result1=false;
//             goto _IteratingCandidatesThatConflictBreak;
//            }
//            if(
//             spawnCandidateData2.priorityOverWest&&
//             spawnCandidateData1.priorityOverEast
//            ){
//             result1=false;
//             goto _IteratingCandidatesThatConflictBreak;
//            }
//            if(spawnCandidateData2.priorityOverBothX){
//             candidatesThatConflictWithSameSize.Add(pos2,spawnCandidateData2);//  there'll be a for this pos2, after the foreach(var kvp in candidatesThatConflictBySize) loop ends
//             continue;
//            }
//            if(spawnCandidateData1.priorityOverBothX){
//             continue;
//            }
//            if(spawnCandidateData2.priorityOverWest&&pos2.x>pos1.x){
//             candidatesThatConflictWithSameSize.Add(pos2,spawnCandidateData2);//  there'll be a for this pos2, after the foreach(var kvp in candidatesThatConflictBySize) loop ends
//             continue;
//            }
//            if(spawnCandidateData2.priorityOverEast&&pos2.x<pos1.x){
//             candidatesThatConflictWithSameSize.Add(pos2,spawnCandidateData2);//  there'll be a for this pos2, after the foreach(var kvp in candidatesThatConflictBySize) loop ends
//             continue;
//            }
//           }else{//  mesmo X, desempata com Z
//            if(
//             spawnCandidateData2.priorityOverBothZ&&
//             spawnCandidateData1.priorityOverBothZ
//            ){
//             result1=false;
//             goto _IteratingCandidatesThatConflictBreak;
//            }
//            if(
//             spawnCandidateData2.priorityOverNorth&&
//             spawnCandidateData1.priorityOverSouth
//            ){
//             result1=false;
//             goto _IteratingCandidatesThatConflictBreak;
//            }
//            if(
//             spawnCandidateData2.priorityOverSouth&&
//             spawnCandidateData1.priorityOverNorth
//            ){
//             result1=false;
//             goto _IteratingCandidatesThatConflictBreak;
//            }
//            if(spawnCandidateData2.priorityOverBothZ){
//             candidatesThatConflictWithSameSize.Add(pos2,spawnCandidateData2);//  there'll be a for this pos2, after the foreach(var kvp in candidatesThatConflictBySize) loop ends
//             continue;
//            }
//            if(spawnCandidateData1.priorityOverBothZ){
//             continue;
//            }
//            if(spawnCandidateData2.priorityOverSouth&&pos2.z>pos1.z){
//             candidatesThatConflictWithSameSize.Add(pos2,spawnCandidateData2);//  there'll be a for this pos2, after the foreach(var kvp in candidatesThatConflictBySize) loop ends
//             continue;
//            }
//            if(spawnCandidateData2.priorityOverNorth&&pos2.z<pos1.z){
//             candidatesThatConflictWithSameSize.Add(pos2,spawnCandidateData2);//  there'll be a for this pos2, after the foreach(var kvp in candidatesThatConflictBySize) loop ends
//             continue;
//            }
//           }
//          }
//          candidatesThatConflict.Clear();
//         }
//         if(result1){
//          foreach(var posSpawnCandidateDataPair2 in candidatesThatConflictWithSameSize){
//           Vector3Int pos2=posSpawnCandidateDataPair2.Key;
//           Vector3Int vCoord2=vecPosTovCoord(pos2,out Vector2Int cnkRgn2);
//           Vector3Int noiseInput2=vCoord2;noiseInput2.x+=cnkRgn2.x;
//                                          noiseInput2.z+=cnkRgn2.y;
//           SpawnCandidateData spawnCandidateData2=posSpawnCandidateDataPair2.Value;
//           if(RecursivelyTryReserveBoundsAt(layer,margin,pos2,noiseInput2,out spawnCandidateData2)){
//            result1=false;
//            break;
//           }
//          }
//         }
//         candidatesThatConflictWithSameSize.Clear();
//         _IteratingCandidatesThatConflictBreak:{}
//         return result1;
//        }



         //SpawnPickingLayer pickingLayer1=spawnCandidateData1.pickingLayer;
         //int max=Mathf.CeilToInt(Mathf.Max(pickingLayer1.maxDimensions.x,pickingLayer1.maxDimensions.z))+Mathf.CeilToInt(Mathf.Max(margin.x,margin.z));
         //Vector2 size1=new Vector2(
         // spawnCandidateData1.size.x,
         // spawnCandidateData1.size.z
         //);
         //var candidatesThatConflictBySize=new SortedDictionary<Vector2,Dictionary<Vector3Int,SpawnCandidateData>>(
         // new Vector2DescendingComparer()//  ordem decrescente
         //);
         //Vector3Int coord2=new Vector3Int(0,Height/2-1,0);
         //for(coord2.x=-max;coord2.x<=max;coord2.x++){
         //for(coord2.z=-max;coord2.z<=max;coord2.z++){if(coord2.x==0&&coord2.z==0){continue;}
         // Vector3Int pos2=pos1;
         // pos2.x+=coord2.x;
         // pos2.z+=coord2.z;
         // Vector3Int vCoord2=vecPosTovCoord(pos2,out Vector2Int cnkRgn2);
         // Vector3Int noiseInput2=vCoord2;noiseInput2.x+=cnkRgn2.x;
         //                                noiseInput2.z+=cnkRgn2.y;
         // if(GetCandidateData(layer,margin,pos2,noiseInput2,out SpawnCandidateData spawnCandidateData2)){
         //  Vector2 size2=new Vector2(
         //   spawnCandidateData2.size.x,
         //   spawnCandidateData2.size.z
         //  );
         //  if(
         //   size2.x<=size1.x&&
         //   size2.y<=size1.y
         //  ){
         //   continue;
         //  }
         //  Bounds bounds1=spawnCandidateData1.bounds;
         //  Bounds bounds2=spawnCandidateData2.bounds;
         //  Bounds worldBounds1=bounds1;worldBounds1.center=pos1;
         //  Bounds worldBounds2=bounds2;worldBounds2.center=pos2;
         //  if(!worldBounds2.Intersects(worldBounds1)){
         //   continue;
         //  }
         // }
         //}}
         //if(state.TryGetValue(pos1,out bool result1)){
         // if(result1){
         //  GetCandidateData(layer,margin,pos1,noiseInput1,out spawnCandidateData1);
         // }else{
         //  spawnCandidateData1=default;
         // }
         // return result1;
         //}
         //result1=true;
         //SpawnPickingLayer pickingLayer1=null;
         //int max=0;
         //if(!GetCandidateData(layer,margin,pos1,noiseInput1,out spawnCandidateData1)){
         // result1=false;
         //}else{
         // pickingLayer1=spawnCandidateData1.pickingLayer;
         // max=Mathf.CeilToInt(Mathf.Max(pickingLayer1.maxDimensions.x,pickingLayer1.maxDimensions.z))+Mathf.CeilToInt(Mathf.Max(margin.x,margin.z));
         //}
         //var candidatesThatConflictBySize=new SortedDictionary<Vector2,Dictionary<Vector3Int,SpawnCandidateData>>(
         // new Vector2DescendingComparer()//  ordem decrescente
         //);
         //if(result1){
         // Vector3Int coord2=new Vector3Int(0,Height/2-1,0);
         // for(coord2.x=-max;coord2.x<=max;coord2.x++){
         // for(coord2.z=-max;coord2.z<=max;coord2.z++){if(coord2.x==0&&coord2.z==0){continue;}
         //  Vector3Int pos2=pos1;
         //  pos2.x+=coord2.x;
         //  pos2.z+=coord2.z;
         //  Vector3Int vCoord2=vecPosTovCoord(pos2,out Vector2Int cnkRgn2);
         //  Vector3Int noiseInput2=vCoord2;noiseInput2.x+=cnkRgn2.x;
         //                                 noiseInput2.z+=cnkRgn2.y;
         //  if(GetCandidateData(layer,margin,pos2,noiseInput2,out SpawnCandidateData spawnCandidateData2)){
         //   if(
         //    spawnCandidateData2.size.x<spawnCandidateData1.size.x&&
         //    spawnCandidateData2.size.z<spawnCandidateData1.size.z
         //   ){
         //    continue;
         //   }
         //   Bounds bounds1=spawnCandidateData1.bounds;
         //   Bounds bounds2=spawnCandidateData2.bounds;
         //   Bounds worldBounds1=bounds1;worldBounds1.center=pos1;
         //   Bounds worldBounds2=bounds2;worldBounds2.center=pos2;
         //   if(!worldBounds2.Intersects(worldBounds1)){
         //    continue;
         //   }
         //   if(
         //    spawnCandidateData2.size.x>spawnCandidateData1.size.x||
         //    spawnCandidateData2.size.z>spawnCandidateData1.size.z
         //   ){
         //    Vector2 size=new Vector2(spawnCandidateData2.size.x,spawnCandidateData2.size.z);
         //    if(
         //     !candidatesThatConflictBySize.TryGetValue(size,out var candidatesThatConflict)
         //    ){
         //     candidatesThatConflictBySize.Add(size,candidatesThatConflict=new());
         //    }
         //    candidatesThatConflict.Add(pos2,spawnCandidateData2);
         //    continue;
         //   }
         //  }
         // }}
         //}
         //if(result1){
         // foreach(var kvp in candidatesThatConflictBySize){
         //  Vector2 size=kvp.Key;
         //  var candidatesThatConflict=kvp.Value;
         //  if(candidatesThatConflict.Count>0){
         //   foreach(var posSpawnCandidateDataPair2 in candidatesThatConflict){
         //    Vector3Int pos2=posSpawnCandidateDataPair2.Key;
         //    Vector3Int vCoord2=vecPosTovCoord(pos2,out Vector2Int cnkRgn2);
         //    Vector3Int noiseInput2=vCoord2;noiseInput2.x+=cnkRgn2.x;
         //                                   noiseInput2.z+=cnkRgn2.y;
         //    SpawnCandidateData spawnCandidateData2=posSpawnCandidateDataPair2.Value;
         //    if(RecursivelyTryReserveBoundsAt(layer,margin,pos2,noiseInput2,out spawnCandidateData2)){
         //     result1=false;
         //     goto _IteratingCandidatesThatConflictBreak;
         //    }
         //   }
         //   candidatesThatConflict.Clear();
         //  }
         // }
         // _IteratingCandidatesThatConflictBreak:{}
         //}
         //Dictionary<Vector3Int,SpawnCandidateData>candidatesThatConflictWithSameSize=new();
         //if(result1){
         // Vector3Int coord2=new Vector3Int(0,Height/2-1,0);
         // for(coord2.x=-max;coord2.x<=max;coord2.x++){
         // for(coord2.z=-max;coord2.z<=max;coord2.z++){if(coord2.x==0&&coord2.z==0){continue;}
         //  Vector3Int pos2=pos1;
         //  pos2.x+=coord2.x;
         //  pos2.z+=coord2.z;
         //  Vector3Int vCoord2=vecPosTovCoord(pos2,out Vector2Int cnkRgn2);
         //  Vector3Int noiseInput2=vCoord2;noiseInput2.x+=cnkRgn2.x;
         //                                 noiseInput2.z+=cnkRgn2.y;
         //  if(GetCandidateData(layer,margin,pos2,noiseInput2,out SpawnCandidateData spawnCandidateData2)){
         //   if(
         //    spawnCandidateData2.size.x!=spawnCandidateData1.size.x||
         //    spawnCandidateData2.size.z!=spawnCandidateData1.size.z
         //   ){
         //    continue;
         //   }
         //   if(
         //    recursionSizeLimit!=null&&
         //    spawnCandidateData2.size.x<=recursionSizeLimit.Value.x&&
         //    spawnCandidateData2.size.z<=recursionSizeLimit.Value.z
         //   ){
         //    continue;
         //   }
         //   Bounds bounds1=spawnCandidateData1.bounds;
         //   Bounds bounds2=spawnCandidateData2.bounds;
         //   Bounds worldBounds1=bounds1;worldBounds1.center=pos1;
         //   Bounds worldBounds2=bounds2;worldBounds2.center=pos2;
         //   if(!worldBounds2.Intersects(worldBounds1)){
         //    continue;
         //   }
         //   if(coord2.x!=0){
         //    if(
         //     spawnCandidateData2.priorityOverBothX&&
         //     spawnCandidateData1.priorityOverBothX
         //    ){
         //     result1=false;
         //     goto _GetCandidatesThatConflictBreak;
         //    }
         //    if(
         //     spawnCandidateData2.priorityOverEast&&
         //     spawnCandidateData1.priorityOverWest
         //    ){
         //     result1=false;
         //     goto _GetCandidatesThatConflictBreak;
         //    }
         //    if(
         //     spawnCandidateData2.priorityOverWest&&
         //     spawnCandidateData1.priorityOverEast
         //    ){
         //     result1=false;
         //     goto _GetCandidatesThatConflictBreak;
         //    }
         //    if(spawnCandidateData2.priorityOverBothX){
         //     candidatesThatConflictWithSameSize.Add(pos2,spawnCandidateData2);
         //     continue;
         //    }
         //    if(spawnCandidateData1.priorityOverBothX){
         //     continue;
         //    }
         //    if(spawnCandidateData2.priorityOverWest&&coord2.x>0){
         //     candidatesThatConflictWithSameSize.Add(pos2,spawnCandidateData2);
         //     continue;
         //    }
         //    if(spawnCandidateData2.priorityOverEast&&coord2.x<0){
         //     candidatesThatConflictWithSameSize.Add(pos2,spawnCandidateData2);
         //     continue;
         //    }
         //   }else{//  mesmo X, desempata com Z
         //    if(
         //     spawnCandidateData2.priorityOverBothZ&&
         //     spawnCandidateData1.priorityOverBothZ
         //    ){
         //     result1=false;
         //     goto _GetCandidatesThatConflictBreak;
         //    }
         //    if(
         //     spawnCandidateData2.priorityOverNorth&&
         //     spawnCandidateData1.priorityOverSouth
         //    ){
         //     result1=false;
         //     goto _GetCandidatesThatConflictBreak;
         //    }
         //    if(
         //     spawnCandidateData2.priorityOverSouth&&
         //     spawnCandidateData1.priorityOverNorth
         //    ){
         //     result1=false;
         //     goto _GetCandidatesThatConflictBreak;
         //    }
         //    if(spawnCandidateData2.priorityOverBothZ){
         //     candidatesThatConflictWithSameSize.Add(pos2,spawnCandidateData2);
         //     continue;
         //    }
         //    if(spawnCandidateData1.priorityOverBothZ){
         //     continue;
         //    }
         //    if(spawnCandidateData2.priorityOverSouth&&coord2.z>0){
         //     candidatesThatConflictWithSameSize.Add(pos2,spawnCandidateData2);
         //     continue;
         //    }
         //    if(spawnCandidateData2.priorityOverNorth&&coord2.z<0){
         //     candidatesThatConflictWithSameSize.Add(pos2,spawnCandidateData2);
         //     continue;
         //    }
         //   }
         //  }
         // }}
         // _GetCandidatesThatConflictBreak:{}
         //}
         //if(result1){
         // if(candidatesThatConflictWithSameSize.Count>0){
         //  foreach(var posSpawnCandidateDataPair2 in candidatesThatConflictWithSameSize){
         //   Vector3Int pos2=posSpawnCandidateDataPair2.Key;
         //   Vector3Int vCoord2=vecPosTovCoord(pos2,out Vector2Int cnkRgn2);
         //   Vector3Int noiseInput2=vCoord2;noiseInput2.x+=cnkRgn2.x;
         //                                  noiseInput2.z+=cnkRgn2.y;
         //   SpawnCandidateData spawnCandidateData2=posSpawnCandidateDataPair2.Value;
         //   if(RecursivelyTryReserveBoundsAt(layer,margin,pos2,noiseInput2,out spawnCandidateData2,spawnCandidateData2.size)){
         //    result1=false;
         //    goto _IteratingCandidatesThatConflictBreak;
         //   }
         //  }
         //  _IteratingCandidatesThatConflictBreak:{}
         // }
         //}
         //candidatesThatConflictWithSameSize.Clear();
         //state.Add(pos1,result1);
         //return result1;



         ////  fazer concurrent queue para usar listas necessárias sem ter que criar várias vezes
         //Dictionary<Vector3Int,SpawnCandidateData>candidatesThatConflict=new();
         //Dictionary<Vector3Int,SpawnCandidateData>candidatesThatUltimatelyConflict=new();
         //if(result1){
         // Vector3Int coord2=new Vector3Int(0,Height/2-1,0);
         // for(coord2.x=-max;coord2.x<=max;coord2.x++){
         // for(coord2.z=-max;coord2.z<=max;coord2.z++){if(coord2.x==0&&coord2.z==0){continue;}
         //  Vector3Int pos2=pos1;
         //  pos2.x+=coord2.x;
         //  pos2.z+=coord2.z;
         //  Vector3Int vCoord2=vecPosTovCoord(pos2,out Vector2Int cnkRgn2);
         //  Vector3Int noiseInput2=vCoord2;noiseInput2.x+=cnkRgn2.x;
         //                                 noiseInput2.z+=cnkRgn2.y;
         //  if(GetCandidateData(layer,margin,pos2,noiseInput2,out SpawnCandidateData spawnCandidateData2)){
         //   if(spawnCandidateData2.priority<spawnCandidateData1.priority){
         //    continue;
         //   }
         //   if(
         //    spawnCandidateData2.size.x<spawnCandidateData1.size.x&&
         //    spawnCandidateData2.size.z<spawnCandidateData1.size.z
         //   ){
         //    continue;
         //   }
         //   Bounds bounds1=spawnCandidateData1.bounds;
         //   Bounds bounds2=spawnCandidateData2.bounds;
         //   Bounds worldBounds1=bounds1;worldBounds1.center=pos1;
         //   Bounds worldBounds2=bounds2;worldBounds2.center=pos2;
         //   if(!worldBounds2.Intersects(worldBounds1)){
         //    continue;
         //   }
         //   if(spawnCandidateData2.priority>spawnCandidateData1.priority){
         //    candidatesThatUltimatelyConflict.Add(pos2,spawnCandidateData2);
         //    continue;
         //   }
         //   if(
         //    spawnCandidateData2.size.x>spawnCandidateData1.size.x||
         //    spawnCandidateData2.size.z>spawnCandidateData1.size.z
         //   ){
         //    candidatesThatUltimatelyConflict.Add(pos2,spawnCandidateData2);
         //    continue;
         //   }
         //   if(coord2.x!=0){
         //    if(
         //     spawnCandidateData2.priorityOverBothX&&
         //     spawnCandidateData1.priorityOverBothX
         //    ){
         //     result1=false;
         //     goto _GetCandidatesThatConflictBreak;
         //    }
         //    if(
         //     spawnCandidateData2.priorityOverEast&&
         //     spawnCandidateData1.priorityOverWest
         //    ){
         //     result1=false;
         //     goto _GetCandidatesThatConflictBreak;
         //    }
         //    if(
         //     spawnCandidateData2.priorityOverWest&&
         //     spawnCandidateData1.priorityOverEast
         //    ){
         //     result1=false;
         //     goto _GetCandidatesThatConflictBreak;
         //    }
         //    if(spawnCandidateData2.priorityOverBothX){
         //     candidatesThatConflict.Add(pos2,spawnCandidateData2);
         //     continue;
         //    }
         //    if(spawnCandidateData1.priorityOverBothX){
         //     continue;
         //    }
         //    if(spawnCandidateData2.priorityOverWest&&coord2.x>0){
         //     candidatesThatConflict.Add(pos2,spawnCandidateData2);
         //     continue;
         //    }
         //    if(spawnCandidateData2.priorityOverEast&&coord2.x<0){
         //     candidatesThatConflict.Add(pos2,spawnCandidateData2);
         //     continue;
         //    }
         //   }else{//  mesmo X, desempata com Z
         //    if(
         //     spawnCandidateData2.priorityOverBothZ&&
         //     spawnCandidateData1.priorityOverBothZ
         //    ){
         //     result1=false;
         //     goto _GetCandidatesThatConflictBreak;
         //    }
         //    if(
         //     spawnCandidateData2.priorityOverNorth&&
         //     spawnCandidateData1.priorityOverSouth
         //    ){
         //     result1=false;
         //     goto _GetCandidatesThatConflictBreak;
         //    }
         //    if(
         //     spawnCandidateData2.priorityOverSouth&&
         //     spawnCandidateData1.priorityOverNorth
         //    ){
         //     result1=false;
         //     goto _GetCandidatesThatConflictBreak;
         //    }
         //    if(spawnCandidateData2.priorityOverBothZ){
         //     candidatesThatConflict.Add(pos2,spawnCandidateData2);
         //     continue;
         //    }
         //    if(spawnCandidateData1.priorityOverBothZ){
         //     continue;
         //    }
         //    if(spawnCandidateData2.priorityOverSouth&&coord2.z>0){
         //     candidatesThatConflict.Add(pos2,spawnCandidateData2);
         //     continue;
         //    }
         //    if(spawnCandidateData2.priorityOverNorth&&coord2.z<0){
         //     candidatesThatConflict.Add(pos2,spawnCandidateData2);
         //     continue;
         //    }
         //   }
         //  }
         // }}
         // _GetCandidatesThatConflictBreak:{}
         //}
         //if(result1){
         // foreach(var posSpawnCandidateDataPair2 in candidatesThatUltimatelyConflict){
         //  Vector3Int pos2=posSpawnCandidateDataPair2.Key;
         //  Vector3Int vCoord2=vecPosTovCoord(pos2,out Vector2Int cnkRgn2);
         //  Vector3Int noiseInput2=vCoord2;noiseInput2.x+=cnkRgn2.x;
         //                                 noiseInput2.z+=cnkRgn2.y;
         //  SpawnCandidateData spawnCandidateData2=posSpawnCandidateDataPair2.Value;
         //  if(RecursivelyTryReserveBoundsAt(layer,margin,pos2,noiseInput2,out spawnCandidateData2)){
         //   result1=false;
         //   break;
         //  }
         // }
         //}
         //if(result1){
         // foreach(var posSpawnCandidateDataPair2 in candidatesThatConflict){
         //  Vector3Int pos2=posSpawnCandidateDataPair2.Key;
         //  Vector3Int vCoord2=vecPosTovCoord(pos2,out Vector2Int cnkRgn2);
         //  Vector3Int noiseInput2=vCoord2;noiseInput2.x+=cnkRgn2.x;
         //                                 noiseInput2.z+=cnkRgn2.y;
         //  SpawnCandidateData spawnCandidateData2=posSpawnCandidateDataPair2.Value;
         //  if(RecursivelyTryReserveBoundsAt(layer,margin,pos2,noiseInput2,out spawnCandidateData2)){
         //   result1=false;
         //   break;
         //  }
         // }
         //}
         //candidatesThatUltimatelyConflict.Clear();
         //candidatesThatConflict.Clear();
         //return RecursivelyTryReserveBoundsAtX(layer,margin,pos1,noiseInput1,out spawnCandidateData1)&&
         //       RecursivelyTryReserveBoundsAtZ(layer,margin,pos1,noiseInput1,out spawnCandidateData1);
         //return RecursivelyTryReserveBoundsAtXZ(layer,margin,pos1,noiseInput1,out spawnCandidateData1);
         //if(state.TryGetValue(pos1,out bool result1)){
         // if(result1){
         //  GetCandidateData(layer,margin,pos1,noiseInput1,out spawnCandidateData1);
         // }else{
         //  spawnCandidateData1=default;
         // }
         // return result1;
         //}
         //result1=true;
         //SpawnPickingLayer pickingLayer1=null;
         //int max=0;
         //if(!RecursivelyTryReserveBoundsAtX(layer,margin,pos1,noiseInput1,out spawnCandidateData1)){
         // result1=false;
         //}else{
         // pickingLayer1=spawnCandidateData1.pickingLayer;
         // max=Mathf.CeilToInt(Mathf.Max(pickingLayer1.maxDimensions.x,pickingLayer1.maxDimensions.z))+Mathf.CeilToInt(Mathf.Max(margin.x,margin.z));
         //}
         ////  fazer concurrent queue para usar listas necessárias sem ter que criar várias vezes
         //Dictionary<Vector3Int,SpawnCandidateData>candidatesThatConflict=new();
         //if(result1){
         // Vector3Int coord2=new Vector3Int(0,Height/2-1,0);
         // for(coord2.x=-max;coord2.x<=max;coord2.x++){
         // for(coord2.z=-max;coord2.z<=max;coord2.z++){if(coord2.z==0){continue;}
         //  Vector3Int pos2=pos1;
         //  pos2.x+=coord2.x;
         //  pos2.z+=coord2.z;
         //  Vector3Int vCoord2=vecPosTovCoord(pos2,out Vector2Int cnkRgn2);
         //  Vector3Int noiseInput2=vCoord2;noiseInput2.x+=cnkRgn2.x;
         //                                 noiseInput2.z+=cnkRgn2.y;
         //  if(RecursivelyTryReserveBoundsAtX(layer,margin,pos2,noiseInput2,out SpawnCandidateData spawnCandidateData2)){
         //   candidatesThatConflict.Add(pos2,spawnCandidateData2);
         //  }
         // }}
         // //if(!spawnCandidateData1.priorityOverBothZ){
         // // result1=false;
         // //}
         //}
         //if(result1){
         // foreach(var posSpawnCandidateDataPair2 in candidatesThatConflict){
         //  Vector3Int pos2=posSpawnCandidateDataPair2.Key;
         //  Vector3Int vCoord2=vecPosTovCoord(pos2,out Vector2Int cnkRgn2);
         //  Vector3Int noiseInput2=vCoord2;noiseInput2.x+=cnkRgn2.x;
         //                                 noiseInput2.z+=cnkRgn2.y;
         //  SpawnCandidateData spawnCandidateData2=posSpawnCandidateDataPair2.Value;
         //  Bounds bounds1=spawnCandidateData1.bounds;
         //  Bounds bounds2=spawnCandidateData2.bounds;
         //  Bounds worldBounds1=bounds1;worldBounds1.center=pos1;
         //  Bounds worldBounds2=bounds2;worldBounds2.center=pos2;
         //  if(!worldBounds2.Intersects(worldBounds1)){
         //   continue;
         //  }
         //  if(spawnCandidateData2.priorityOverBothZ){
         //   result1=false;
         //   goto _IterateCandidatesThatConflictBreak;
         //  }
         //  if(spawnCandidateData1.priorityOverBothZ){
         //   continue;
         //  }
         //  if(spawnCandidateData2.priorityOverSouth&&pos2.z>pos1.z){
         //   result1=false;
         //   goto _IterateCandidatesThatConflictBreak;
         //  }
         //  if(spawnCandidateData2.priorityOverNorth&&pos2.z<pos1.z){
         //   result1=false;
         //   goto _IterateCandidatesThatConflictBreak;
         //  }
         // }
         // _IterateCandidatesThatConflictBreak:{}
         //}
         //candidatesThatConflict.Clear();
         //state.Add(pos1,result1);
         //return result1;
         //if(state.TryGetValue(pos1,out bool result1)){
         // if(result1){
         //  GetCandidateData(layer,margin,pos1,noiseInput1,out spawnCandidateData1);
         // }else{
         //  spawnCandidateData1=default;
         // }
         // return result1;
         //}
         ////  fazer concurrent queue para usar listas necessárias sem ter que criar várias vezes
         //Dictionary<Vector3Int,SpawnCandidateData>candidatesThatConflict=new();
         //if(result1){
         // Vector3Int coord2=new Vector3Int(0,Height/2-1,0);
         // //for(coord2.x=-max;coord2.x<=max;coord2.x++){
         // for(coord2.z=-max;coord2.z<=max;coord2.z++){if(coord2.z==0){continue;}
         //  Vector3Int pos2=pos1;
         //  pos2.x+=coord2.x;
         //  pos2.z+=coord2.z;
         //  Vector3Int vCoord2=vecPosTovCoord(pos2,out Vector2Int cnkRgn2);
         //  Vector3Int noiseInput2=vCoord2;noiseInput2.x+=cnkRgn2.x;
         //                                 noiseInput2.z+=cnkRgn2.y;
         //  if(RecursivelyTryReserveBoundsAtX(layer,margin,pos2,noiseInput2,out SpawnCandidateData spawnCandidateData2)){
         //   candidatesThatConflict.Add(pos2,spawnCandidateData2);
         //   //Log.DebugMessage("candidatesThatConflict:coord2:"+coord2);
         //  }
         // }//}
         // Log.DebugMessage("candidatesThatConflict:"+candidatesThatConflict.Count);
         //}
         //Dictionary<Vector3Int,SpawnCandidateData>candidatesThatUltimatelyConflict=new();
         //if(result1){
         // foreach(var posSpawnCandidateDataPair2 in candidatesThatConflict){
         //  Vector3Int pos2=posSpawnCandidateDataPair2.Key;
         //  Vector3Int vCoord2=vecPosTovCoord(pos2,out Vector2Int cnkRgn2);
         //  Vector3Int noiseInput2=vCoord2;noiseInput2.x+=cnkRgn2.x;
         //                                 noiseInput2.z+=cnkRgn2.y;
         //  SpawnCandidateData spawnCandidateData2=posSpawnCandidateDataPair2.Value;
         //  Bounds bounds1=spawnCandidateData1.bounds;
         //  Bounds bounds2=spawnCandidateData2.bounds;
         //  Bounds worldBounds1=bounds1;worldBounds1.center=pos1;
         //  Bounds worldBounds2=bounds2;worldBounds2.center=pos2;
         //  if(!worldBounds2.Intersects(worldBounds1)){
         //   continue;
         //  }

         //  if(!spawnCandidateData1.priorityOverBothZ){
         //   candidatesThatUltimatelyConflict.Add(pos2,spawnCandidateData2);
         //  }

         //   //if(candidatesThatConflict.TryGetValue(pos2,out SpawnCandidateData spawnCandidateData2)){
         //   // if(
         //   //  spawnCandidateData2.priorityOverBothZ&&
         //   //  spawnCandidateData1.priorityOverBothZ
         //   // ){
         //   //  result1=false;
         //   //  goto _IterateCandidatesThatConflictBreak;
         //   // }
         //   // if(
         //   //  spawnCandidateData2.priorityOverNorth&&
         //   //  spawnCandidateData1.priorityOverSouth
         //   // ){
         //   //  result1=false;
         //   //  goto _IterateCandidatesThatConflictBreak;
         //   // }
         //   // if(
         //   //  spawnCandidateData2.priorityOverSouth&&
         //   //  spawnCandidateData1.priorityOverNorth
         //   // ){
         //   //  result1=false;
         //   //  goto _IterateCandidatesThatConflictBreak;
         //   // }
         //   //}
         //   //if(spawnCandidateData2.priorityOverBothZ){
         //   // candidatesThatUltimatelyConflict.Add(pos2,spawnCandidateData2);
         //   // continue;
         //   //}
         //   //if(spawnCandidateData1.priorityOverBothZ){
         //   // continue;
         //   //}
         //   //if(spawnCandidateData2.priorityOverSouth&&pos2.z>pos1.z){
         //   // candidatesThatUltimatelyConflict.Add(pos2,spawnCandidateData2);
         //   // continue;
         //   //}
         //   //if(spawnCandidateData2.priorityOverNorth&&pos2.z<pos1.z){
         //   // candidatesThatUltimatelyConflict.Add(pos2,spawnCandidateData2);
         //   // continue;
         //   //}

         // }
         // _IterateCandidatesThatConflictBreak:{}



         //}
         //if(result1){
         // if(candidatesThatUltimatelyConflict.Count>0){
         //  result1=false;
         // }
         //}
         //candidatesThatUltimatelyConflict.Clear();
         //candidatesThatConflict.Clear();
         //if(!spawnCandidateData1.priorityOverBothZ){
         // result1=false;
         //}
         //state[pos1]=result1;

     //readonly Dictionary<Vector3Int,bool>stateXZ=new();
        //protected bool RecursivelyTryReserveBoundsAtXZ(int layer,Vector3 margin,Vector3Int pos1,Vector3Int noiseInput1,
        // out SpawnCandidateData spawnCandidateData1
        //){
        // if(stateXZ.TryGetValue(pos1,out bool result1)){
        //  if(result1){
        //   GetCandidateData(layer,margin,pos1,noiseInput1,out spawnCandidateData1);
        //  }else{
        //   spawnCandidateData1=default;
        //  }
        //  return result1;
        // }
        // result1=true;
        // SpawnPickingLayer pickingLayer1=null;
        // int max=0;
        // if(!RecursivelyTryReserveBoundsAtX(layer,margin,pos1,noiseInput1,out spawnCandidateData1)){
        //  result1=false;
        // }
        // if(result1){
        //  if(!RecursivelyTryReserveBoundsAtZ(layer,margin,pos1,noiseInput1,out spawnCandidateData1)){
        //   result1=false;
        //  }
        // }
        // if(result1){
        //  pickingLayer1=spawnCandidateData1.pickingLayer;
        //  max=Mathf.CeilToInt(Mathf.Max(pickingLayer1.maxDimensions.x,pickingLayer1.maxDimensions.z))+Mathf.CeilToInt(Mathf.Max(margin.x,margin.z));
        // }
        // //  fazer concurrent queue para usar listas necessárias sem ter que criar várias vezes
        // Dictionary<Vector3Int,SpawnCandidateData>candidatesThatConflict=new();
        // if(result1){
        //  Vector3Int coord2=new Vector3Int(0,Height/2-1,0);
        //  for(coord2.x=-max;coord2.x<=max;coord2.x++){
        //  for(coord2.z=-max;coord2.z<=max;coord2.z++){if(coord2.x==0&&coord2.z==0){continue;}
        //   Vector3Int pos2=pos1;
        //   pos2.x+=coord2.x;
        //   pos2.z+=coord2.z;
        //   Vector3Int vCoord2=vecPosTovCoord(pos2,out Vector2Int cnkRgn2);
        //   Vector3Int noiseInput2=vCoord2;noiseInput2.x+=cnkRgn2.x;
        //                                  noiseInput2.z+=cnkRgn2.y;
        //   if(!RecursivelyTryReserveBoundsAtX(layer,margin,pos2,noiseInput2,out SpawnCandidateData spawnCandidateData2)){
        //    continue;//  will be blocked in X by something
        //   }
        //   if(!RecursivelyTryReserveBoundsAtZ(layer,margin,pos2,noiseInput2,out                    spawnCandidateData2)){
        //    continue;//  will be blocked in Z by something
        //   }
        //   candidatesThatConflict.Add(pos2,spawnCandidateData2);
        //  }}
        // }
        // Dictionary<Vector3Int,SpawnCandidateData>candidatesThatUltimatelyConflict=new();
        // if(result1){
        //  if(candidatesThatConflict.Count>0){
        //   //Log.DebugMessage("candidatesThatConflict:"+candidatesThatConflict.Count);
        //   foreach(var posSpawnCandidateDataPair2 in candidatesThatConflict){
        //    Vector3Int pos2=posSpawnCandidateDataPair2.Key;
        //    Vector3Int vCoord2=vecPosTovCoord(pos2,out Vector2Int cnkRgn2);
        //    Vector3Int noiseInput2=vCoord2;noiseInput2.x+=cnkRgn2.x;
        //                                   noiseInput2.z+=cnkRgn2.y;
        //    SpawnCandidateData spawnCandidateData2=posSpawnCandidateDataPair2.Value;
        //    if(spawnCandidateData2.priority<spawnCandidateData1.priority){
        //     continue;
        //    }
        //    if(
        //     spawnCandidateData2.size.x<spawnCandidateData1.size.x&&
        //     spawnCandidateData2.size.z<spawnCandidateData1.size.z
        //    ){
        //     continue;
        //    }
        //    Bounds bounds1=spawnCandidateData1.bounds;
        //    Bounds bounds2=spawnCandidateData2.bounds;
        //    Bounds worldBounds1=bounds1;worldBounds1.center=pos1;
        //    Bounds worldBounds2=bounds2;worldBounds2.center=pos2;
        //    if(!worldBounds2.Intersects(worldBounds1)){
        //     continue;
        //    }
        //    if(spawnCandidateData2.priority>spawnCandidateData1.priority){
        //     candidatesThatUltimatelyConflict.Add(pos2,spawnCandidateData2);
        //     continue;
        //    }
        //    if(
        //     spawnCandidateData2.size.x>spawnCandidateData1.size.x||
        //     spawnCandidateData2.size.z>spawnCandidateData1.size.z
        //    ){
        //     candidatesThatUltimatelyConflict.Add(pos2,spawnCandidateData2);
        //     continue;
        //    }
        //    if(
        //     spawnCandidateData2.priorityOverBothX&&
        //     spawnCandidateData1.priorityOverBothX
        //    ){
        //     result1=false;
        //     goto _IteratingCandidatesThatConflictBreak;
        //    }
        //    if(
        //     spawnCandidateData2.priorityOverEast&&
        //     spawnCandidateData1.priorityOverWest
        //    ){
        //     result1=false;
        //     goto _IteratingCandidatesThatConflictBreak;
        //    }
        //    if(
        //     spawnCandidateData2.priorityOverWest&&
        //     spawnCandidateData1.priorityOverEast
        //    ){
        //     result1=false;
        //     goto _IteratingCandidatesThatConflictBreak;
        //    }
        //    if(spawnCandidateData2.priorityOverBothX){
        //     candidatesThatUltimatelyConflict.Add(pos2,spawnCandidateData2);
        //     continue;
        //    }
        //    if(spawnCandidateData1.priorityOverBothX){
        //     continue;
        //    }
        //    if(spawnCandidateData2.priorityOverWest&&pos2.x>pos1.x){
        //     candidatesThatUltimatelyConflict.Add(pos2,spawnCandidateData2);
        //     continue;
        //    }
        //    if(spawnCandidateData2.priorityOverEast&&pos2.x<pos1.x){
        //     candidatesThatUltimatelyConflict.Add(pos2,spawnCandidateData2);
        //     continue;
        //    }
        //   }
        //   _IteratingCandidatesThatConflictBreak:{}
        //  }
        // }
        // candidatesThatConflict.Clear();
        // if(result1){
        //  if(candidatesThatUltimatelyConflict.Count>0){
        //   //Log.DebugMessage("candidatesThatUltimatelyConflict:"+candidatesThatUltimatelyConflict.Count);
        //   foreach(var posSpawnCandidateDataPair2 in candidatesThatUltimatelyConflict){
        //    Vector3Int pos2=posSpawnCandidateDataPair2.Key;
        //    Vector3Int vCoord2=vecPosTovCoord(pos2,out Vector2Int cnkRgn2);
        //    Vector3Int noiseInput2=vCoord2;noiseInput2.x+=cnkRgn2.x;
        //                                   noiseInput2.z+=cnkRgn2.y;
        //    SpawnCandidateData spawnCandidateData2=posSpawnCandidateDataPair2.Value;
        //    if(RecursivelyTryReserveBoundsAtXZ(layer,margin,pos2,noiseInput2,out spawnCandidateData2)){
        //     result1=false;
        //     break;
        //    }
        //   }
        //   result1=false;
        //  }
        // }
        // candidatesThatUltimatelyConflict.Clear();
        // stateXZ.Add(pos1,result1);
        // return result1;
        //}
     //readonly Dictionary<Vector3Int,bool>stateX=new();
   //     protected bool RecursivelyTryReserveBoundsAtX(int layer,Vector3 margin,Vector3Int pos1,Vector3Int noiseInput1,
   //      out SpawnCandidateData spawnCandidateData1
   //     ){
   //      if(stateX.TryGetValue(pos1,out bool result1)){
   //       if(result1){
   //        GetCandidateData(layer,margin,pos1,noiseInput1,out spawnCandidateData1);
   //       }else{
   //        spawnCandidateData1=default;
   //       }
   //       return result1;
   //      }
   //      result1=true;
   //      SpawnPickingLayer pickingLayer1=null;
   //      int max=0;
   //      if(!GetCandidateData(layer,margin,pos1,noiseInput1,out spawnCandidateData1)){
   //       result1=false;
   //      }else{
   //       pickingLayer1=spawnCandidateData1.pickingLayer;
   //       max=Mathf.CeilToInt(Mathf.Max(pickingLayer1.maxDimensions.x,pickingLayer1.maxDimensions.z))+Mathf.CeilToInt(Mathf.Max(margin.x,margin.z));
   //      }



   //      //  fazer concurrent queue para usar listas necessárias sem ter que criar várias vezes
   //      Dictionary<Vector3Int,SpawnCandidateData>candidatesThatConflict=new();
   //      if(result1){



   //       Vector3Int coord2=new Vector3Int(0,Height/2-1,0);
   //       for(coord2.x=-max;coord2.x<=max;coord2.x++){
   //       /*for(coord2.z=-max;coord2.z<=max;coord2.z++){*/if(coord2.x==0){continue;}
   //        Vector3Int pos2=pos1;
   //        pos2.x+=coord2.x;
   //        pos2.z+=coord2.z;
   //        Vector3Int vCoord2=vecPosTovCoord(pos2,out Vector2Int cnkRgn2);
   //        Vector3Int noiseInput2=vCoord2;noiseInput2.x+=cnkRgn2.x;
   //                                       noiseInput2.z+=cnkRgn2.y;
   //        if(GetCandidateData(layer,margin,pos2,noiseInput2,out SpawnCandidateData spawnCandidateData2)){
   //         if(spawnCandidateData2.priority<spawnCandidateData1.priority){
   //          continue;
   //         }
   //         if(
   //          spawnCandidateData2.size.x<spawnCandidateData1.size.x&&
   //          spawnCandidateData2.size.z<spawnCandidateData1.size.z
   //         ){
   //          continue;
   //         }
   //         Bounds bounds1=spawnCandidateData1.bounds;
   //         Bounds bounds2=spawnCandidateData2.bounds;
   //         Bounds worldBounds1=bounds1;worldBounds1.center=pos1;
   //         Bounds worldBounds2=bounds2;worldBounds2.center=pos2;
   //         if(!worldBounds2.Intersects(worldBounds1)){
   //          continue;
   //         }
   //         if(spawnCandidateData2.priority>spawnCandidateData1.priority){
   //          candidatesThatConflict.Add(pos2,spawnCandidateData2);
   //          continue;
   //         }
   //         if(
   //          spawnCandidateData2.size.x>spawnCandidateData1.size.x||
   //          spawnCandidateData2.size.z>spawnCandidateData1.size.z
   //         ){
   //          candidatesThatConflict.Add(pos2,spawnCandidateData2);
   //          continue;
   //         }
   //          //  todos os candidatos farão verificação recursiva, então aqui deve ter desempate relativo total
   //          // quando há possibilidade de conflito, para não dar stack overflow:
   //          // - se priority maior
   //          // - se size maior
   //          // - priorityOver maior
   //         if(
   //          spawnCandidateData2.priorityOverBothX&&
   //          spawnCandidateData1.priorityOverBothX
   //         ){
   //          result1=false;
   //          goto _GetCandidatesThatConflictBreak;
   //         }
   //         if(
   //          spawnCandidateData2.priorityOverEast&&
   //          spawnCandidateData1.priorityOverWest
   //         ){
   //          result1=false;
   //          goto _GetCandidatesThatConflictBreak;
   //         }
   //         if(
   //          spawnCandidateData2.priorityOverWest&&
   //          spawnCandidateData1.priorityOverEast
   //         ){
   //          result1=false;
   //          goto _GetCandidatesThatConflictBreak;
   //         }
   //         if(spawnCandidateData2.priorityOverBothX){
   //          candidatesThatConflict.Add(pos2,spawnCandidateData2);
   //          continue;
   //         }
   //         if(spawnCandidateData1.priorityOverBothX){
   //          continue;
   //         }
   //         if(spawnCandidateData2.priorityOverWest&&coord2.x>0){
   //          candidatesThatConflict.Add(pos2,spawnCandidateData2);
   //          continue;
   //         }
   //         if(spawnCandidateData2.priorityOverEast&&coord2.x<0){
   //          candidatesThatConflict.Add(pos2,spawnCandidateData2);
   //          continue;
   //         }
   //        }
   //       }//}
   //       _GetCandidatesThatConflictBreak:{}



   //      }
   //      if(result1){



   //       foreach(var posSpawnCandidateDataPair2 in candidatesThatConflict){
   //        Vector3Int pos2=posSpawnCandidateDataPair2.Key;
   //        Vector3Int vCoord2=vecPosTovCoord(pos2,out Vector2Int cnkRgn2);
   //        Vector3Int noiseInput2=vCoord2;noiseInput2.x+=cnkRgn2.x;
   //                                       noiseInput2.z+=cnkRgn2.y;
   //        if(RecursivelyTryReserveBoundsAtX(layer,margin,pos2,noiseInput2,out SpawnCandidateData spawnCandidateData2)){
   //         result1=false;
   //         break;
   //        }
   //       }



   //      }
   //      candidatesThatConflict.Clear();
   //      stateX.Add(pos1,result1);
   //      return result1;
   ////  when to stop recursion and result false:
   ////  when found object bigger
   ////  when found object with same size and higher priority
   ////           found priority<<<<<   stop tests to the left
   ////           priority>>>>>found priority   stop tests to the right
   //      //for(){
   //      //}
   //      //return false;
   //        //if(GetCandidateData(layer,margin,pos2,noiseInput2,out SpawnCandidateData spawnCandidateData2)){
   //        // if(spawnCandidateData2.priority<spawnCandidateData1.priority){
   //        //  continue;
   //        // }
   //        // if(
   //        //  spawnCandidateData2.size.x<spawnCandidateData1.size.x&&
   //        //  spawnCandidateData2.size.z<spawnCandidateData1.size.z
   //        // ){
   //        //  continue;
   //        // }
   //        // Bounds bounds1=spawnCandidateData1.bounds;
   //        // Bounds bounds2=spawnCandidateData2.bounds;
   //        // Bounds worldBounds1=bounds1;worldBounds1.center=pos1;
   //        // Bounds worldBounds2=bounds2;worldBounds2.center=pos2;
   //        // if(!worldBounds2.Intersects(worldBounds1)){
   //        //  continue;
   //        // }
   //        // if(spawnCandidateData2.priority>spawnCandidateData1.priority){
   //        //  candidatesThatConflict.Add(pos2,spawnCandidateData2);
   //        //  continue;
   //        // }
   //        // if(
   //        //  spawnCandidateData2.size.x>spawnCandidateData1.size.x||
   //        //  spawnCandidateData2.size.z>spawnCandidateData1.size.z
   //        // ){
   //        //  candidatesThatConflict.Add(pos2,spawnCandidateData2);
   //        //  continue;
   //        // }
   //        // if(
   //        //  spawnCandidateData2.priorityOverBothZ&&
   //        //  spawnCandidateData1.priorityOverBothZ
   //        // ){
   //        //  result1=false;
   //        //  goto _GetCandidatesThatConflictBreak;
   //        // }
   //        // if(
   //        //  spawnCandidateData2.priorityOverNorth&&
   //        //  spawnCandidateData1.priorityOverSouth
   //        // ){
   //        //  result1=false;
   //        //  goto _GetCandidatesThatConflictBreak;
   //        // }
   //        // if(
   //        //  spawnCandidateData2.priorityOverSouth&&
   //        //  spawnCandidateData1.priorityOverNorth
   //        // ){
   //        //  result1=false;
   //        //  goto _GetCandidatesThatConflictBreak;
   //        // }
   //        // if(spawnCandidateData2.priorityOverBothZ){
   //        //  candidatesThatConflict.Add(pos2,spawnCandidateData2);
   //        //  continue;
   //        // }
   //        // if(spawnCandidateData1.priorityOverBothZ){
   //        //  continue;
   //        // }
   //        // if(spawnCandidateData2.priorityOverSouth&&coord2.z>0){
   //        //  candidatesThatConflict.Add(pos2,spawnCandidateData2);
   //        //  continue;
   //        // }
   //        // if(spawnCandidateData2.priorityOverNorth&&coord2.z<0){
   //        //  candidatesThatConflict.Add(pos2,spawnCandidateData2);
   //        //  continue;
   //        // }
   //        //}
   //      //if(limitRecursionAt<=0){
   //      // if(result1){
   //      //  Vector3Int coord2=new Vector3Int(0,Height/2-1,0);
   //      //  for(coord2.x=-max;coord2.x<=max;coord2.x++){
   //      //  for(coord2.z=-max;coord2.z<=max;coord2.z++){if(coord2.z==0){continue;}
   //      //   Vector3Int pos2=pos1;
   //      //   pos2.x+=coord2.x;
   //      //   pos2.z+=coord2.z;
   //      //   Vector3Int vCoord2=vecPosTovCoord(pos2,out Vector2Int cnkRgn2);
   //      //   Vector3Int noiseInput2=vCoord2;noiseInput2.x+=cnkRgn2.x;
   //      //                                  noiseInput2.z+=cnkRgn2.y;
   //      //   if(RecursivelyTryReserveBoundsAt(layer,margin,pos2,noiseInput2,out SpawnCandidateData spawnCandidateData2,1)){
   //      //    candidatesThatConflict.Add(pos2,spawnCandidateData2);
   //      //   }
   //      //  }}
   //      // }
   //      // if(result1){
   //      //  Log.DebugMessage("candidatesThatConflict:"+candidatesThatConflict.Count);
   //      //  Vector3Int coord2=new Vector3Int(0,Height/2-1,0);
   //      //  for(coord2.x=-max;coord2.x<=max;coord2.x++){
   //      //  for(coord2.z=-max;coord2.z<=max;coord2.z++){if(coord2.z==0){continue;}
   //      //   Vector3Int pos2=pos1;
   //      //   pos2.x+=coord2.x;
   //      //   pos2.z+=coord2.z;
   //      //   if(candidatesThatConflict.TryGetValue(pos2,out SpawnCandidateData spawnCandidateData2)){
   //      //    Vector3Int vCoord2=vecPosTovCoord(pos2,out Vector2Int cnkRgn2);
   //      //    Vector3Int noiseInput2=vCoord2;noiseInput2.x+=cnkRgn2.x;
   //      //                                   noiseInput2.z+=cnkRgn2.y;
   //      //    if(spawnCandidateData2.priority<spawnCandidateData1.priority){
   //      //     continue;
   //      //    }
   //      //    if(
   //      //     spawnCandidateData2.size.x<spawnCandidateData1.size.x&&
   //      //     spawnCandidateData2.size.z<spawnCandidateData1.size.z
   //      //    ){
   //      //     continue;
   //      //    }
   //      //    Bounds bounds1=spawnCandidateData1.bounds;
   //      //    Bounds bounds2=spawnCandidateData2.bounds;
   //      //    Bounds worldBounds1=bounds1;worldBounds1.center=pos1;
   //      //    Bounds worldBounds2=bounds2;worldBounds2.center=pos2;
   //      //    if(!worldBounds2.Intersects(worldBounds1)){
   //      //     continue;
   //      //    }
   //      //    if(spawnCandidateData2.priority>spawnCandidateData1.priority){
   //      //     //candidatesThatConflict.Add(pos2,spawnCandidateData2);
   //      //     continue;
   //      //    }
   //      //    if(
   //      //     spawnCandidateData2.size.x>spawnCandidateData1.size.x||
   //      //     spawnCandidateData2.size.z>spawnCandidateData1.size.z
   //      //    ){
   //      //     //candidatesThatConflict.Add(pos2,spawnCandidateData2);
   //      //     continue;
   //      //    }
   //      //   }
   //      //  }}
   //      //  _GetCandidatesThatConflictBreak:{}
   //      // }
   //      // candidatesThatConflict.Clear();
   //      //}
   //     }
     //readonly Dictionary<Vector3Int,bool>stateZ=new();
        //protected bool RecursivelyTryReserveBoundsAtZ(int layer,Vector3 margin,Vector3Int pos1,Vector3Int noiseInput1,
        // out SpawnCandidateData spawnCandidateData1
        //){
        // if(stateZ.TryGetValue(pos1,out bool result1)){
        //  if(result1){
        //   GetCandidateData(layer,margin,pos1,noiseInput1,out spawnCandidateData1);
        //  }else{
        //   spawnCandidateData1=default;
        //  }
        //  return result1;
        // }
        // result1=true;
        // SpawnPickingLayer pickingLayer1=null;
        // int max=0;
        // if(!GetCandidateData(layer,margin,pos1,noiseInput1,out spawnCandidateData1)){
        //  result1=false;
        // }else{
        //  pickingLayer1=spawnCandidateData1.pickingLayer;
        //  max=Mathf.CeilToInt(Mathf.Max(pickingLayer1.maxDimensions.x,pickingLayer1.maxDimensions.z))+Mathf.CeilToInt(Mathf.Max(margin.x,margin.z));
        // }



        // //  fazer concurrent queue para usar listas necessárias sem ter que criar várias vezes
        // Dictionary<Vector3Int,SpawnCandidateData>candidatesThatConflict=new();
        // if(result1){



        //  Vector3Int coord2=new Vector3Int(0,Height/2-1,0);
        //  //for(coord2.x=-max;coord2.x<=max;coord2.x++){
        //  for(coord2.z=-max;coord2.z<=max;coord2.z++){if(coord2.z==0){continue;}
        //   Vector3Int pos2=pos1;
        //   pos2.x+=coord2.x;
        //   pos2.z+=coord2.z;
        //   Vector3Int vCoord2=vecPosTovCoord(pos2,out Vector2Int cnkRgn2);
        //   Vector3Int noiseInput2=vCoord2;noiseInput2.x+=cnkRgn2.x;
        //                                  noiseInput2.z+=cnkRgn2.y;
        //   if(GetCandidateData(layer,margin,pos2,noiseInput2,out SpawnCandidateData spawnCandidateData2)){
        //    if(spawnCandidateData2.priority<spawnCandidateData1.priority){
        //     continue;
        //    }
        //    if(
        //     spawnCandidateData2.size.x<spawnCandidateData1.size.x&&
        //     spawnCandidateData2.size.z<spawnCandidateData1.size.z
        //    ){
        //     continue;
        //    }
        //    Bounds bounds1=spawnCandidateData1.bounds;
        //    Bounds bounds2=spawnCandidateData2.bounds;
        //    Bounds worldBounds1=bounds1;worldBounds1.center=pos1;
        //    Bounds worldBounds2=bounds2;worldBounds2.center=pos2;
        //    if(!worldBounds2.Intersects(worldBounds1)){
        //     continue;
        //    }
        //    if(spawnCandidateData2.priority>spawnCandidateData1.priority){
        //     candidatesThatConflict.Add(pos2,spawnCandidateData2);
        //     continue;
        //    }
        //    if(
        //     spawnCandidateData2.size.x>spawnCandidateData1.size.x||
        //     spawnCandidateData2.size.z>spawnCandidateData1.size.z
        //    ){
        //     candidatesThatConflict.Add(pos2,spawnCandidateData2);
        //     continue;
        //    }
        //     //  todos os candidatos farão verificação recursiva, então aqui deve ter desempate relativo total
        //     // quando há possibilidade de conflito, para não dar stack overflow:
        //     // - se priority maior
        //     // - se size maior
        //     // - priorityOver maior
        //    if(
        //     spawnCandidateData2.priorityOverBothZ&&
        //     spawnCandidateData1.priorityOverBothZ
        //    ){
        //     result1=false;
        //     goto _GetCandidatesThatConflictBreak;
        //    }
        //    if(
        //     spawnCandidateData2.priorityOverNorth&&
        //     spawnCandidateData1.priorityOverSouth
        //    ){
        //     result1=false;
        //     goto _GetCandidatesThatConflictBreak;
        //    }
        //    if(
        //     spawnCandidateData2.priorityOverSouth&&
        //     spawnCandidateData1.priorityOverNorth
        //    ){
        //     result1=false;
        //     goto _GetCandidatesThatConflictBreak;
        //    }
        //    if(spawnCandidateData2.priorityOverBothZ){
        //     candidatesThatConflict.Add(pos2,spawnCandidateData2);
        //     continue;
        //    }
        //    if(spawnCandidateData1.priorityOverBothZ){
        //     continue;
        //    }
        //    if(spawnCandidateData2.priorityOverSouth&&coord2.z>0){
        //     candidatesThatConflict.Add(pos2,spawnCandidateData2);
        //     continue;
        //    }
        //    if(spawnCandidateData2.priorityOverNorth&&coord2.z<0){
        //     candidatesThatConflict.Add(pos2,spawnCandidateData2);
        //     continue;
        //    }
        //   }
        //  }//}
        //  _GetCandidatesThatConflictBreak:{}



        // }
        // if(result1){



        //  foreach(var posSpawnCandidateDataPair2 in candidatesThatConflict){
        //   Vector3Int pos2=posSpawnCandidateDataPair2.Key;
        //   Vector3Int vCoord2=vecPosTovCoord(pos2,out Vector2Int cnkRgn2);
        //   Vector3Int noiseInput2=vCoord2;noiseInput2.x+=cnkRgn2.x;
        //                                  noiseInput2.z+=cnkRgn2.y;
        //   if(RecursivelyTryReserveBoundsAtZ(layer,margin,pos2,noiseInput2,out SpawnCandidateData spawnCandidateData2)){
        //    result1=false;
        //    break;
        //   }
        //  }



        // }
        // candidatesThatConflict.Clear();
        // stateZ.Add(pos1,result1);
        // return result1;
        //}



     readonly Dictionary<int,Dictionary<Vector3Int,SpawnCandidateData>>hasData=new();
      readonly Dictionary<int,HashSet<Vector3Int>>hasNoData=new();
        protected bool GetCandidateData(int layer,Vector3 margin,Vector3Int pos1,int cnkIdx1,Vector3Int noiseInput1,
         out SpawnCandidateData spawnCandidateData1
        ){
         OpenSurfaceData(cnkIdx1);
         VoxelSystem.Concurrent.surfaceSpawnData_rwl.EnterReadLock();
         try{
          if(hasNoData[cnkIdx1].Contains(pos1)){
           spawnCandidateData1=default;
           return false;
          }
          if(hasData[cnkIdx1].TryGetValue(pos1,out spawnCandidateData1)){
           return true;
          }
         }catch{
          throw;
         }finally{
          VoxelSystem.Concurrent.surfaceSpawnData_rwl.ExitReadLock();
         }
         (Type simObject,SimObjectSettings simObjectSettings)?simObjectPicked1=VoxelSystem.biome.biomeSpawnSettings.TryGetSettingsToSpawnSimObject(layer,noiseInput1,out double selectionValue1,out SpawnPickingLayer pickingLayer1);
         if(simObjectPicked1==null){
          spawnCandidateData1=default;
          VoxelSystem.Concurrent.surfaceSpawnData_rwl.EnterWriteLock();
          try{
           hasNoData[cnkIdx1].Add(pos1);
          }catch{
           throw;
          }finally{
           VoxelSystem.Concurrent.surfaceSpawnData_rwl.ExitWriteLock();
          }
          return false;
         }
         SimObjectSettings simObjectSettings1=simObjectPicked1.Value.simObjectSettings;
         SimObjectSpawnModifiers modifiers1=VoxelSystem.biome.biomeSpawnSettings.GetSimObjectSpawnModifiers(noiseInput1,simObjectSettings1);
         Vector3 size1=simObjectSettings1.size;
         Bounds bounds1=new Bounds(Vector3.zero,size1);
         int priority1=simObjectSettings1.priority;
         //  TO DO: aplicar rotação do chão na rotação final
         Quaternion rotation1=Quaternion.AngleAxis(modifiers1.rotation,Vector3.up);
         Bounds maxScaledBounds1=new Bounds(Vector3.zero,Vector3.Scale(size1,simObjectSettings1.maxScale));
         Vector3 maxScaledExtents1=maxScaledBounds1.extents;
         //  seqResult tem que levar o tamanho maior, porque tem possibilidade de rotação
         int halfSeqSize1=Mathf.CeilToInt(Mathf.Max(size1.x,size1.z))+Mathf.CeilToInt(Mathf.Max(margin.x,margin.z));
         int seqResultX1=MathUtil.AlternatingSequenceWithSeparator(pos1.x,halfSeqSize1*2,0);
         int seqResultZ1=MathUtil.AlternatingSequenceWithSeparator(pos1.z,halfSeqSize1*2,0);//  seqStart:pickingLayer1.maxDimensions.z
         spawnCandidateData1=new(){
          simObjectPicked=simObjectPicked1.Value,
          simObjectSettings=simObjectSettings1,
          modifiers=modifiers1,
          size=size1,
          bounds=bounds1,
          priority=priority1,
          rotation=rotation1,
          priorityOverWest =(seqResultX1==0),priorityOverEast =(seqResultX1==1),priorityOverBothX=(seqResultX1==2),
          priorityOverSouth=(seqResultZ1==0),priorityOverNorth=(seqResultZ1==1),priorityOverBothZ=(seqResultZ1==2),
          selectionValue=selectionValue1,
          pickingLayer=pickingLayer1,
          halfSeqSize=halfSeqSize1,
         };
         VoxelSystem.Concurrent.surfaceSpawnData_rwl.EnterWriteLock();
         try{
          hasData[cnkIdx1][pos1]=spawnCandidateData1;
         }catch{
          throw;
         }finally{
          VoxelSystem.Concurrent.surfaceSpawnData_rwl.ExitWriteLock();
         }
         return true;
        }
     readonly CandidateDescendingComparer candidateDescendingComparer=new();
        class CandidateDescendingComparer:IComparer<Vector2>{
            public int Compare(Vector2 a,Vector2 b){
             int cmp=a.sqrMagnitude.CompareTo(b.sqrMagnitude);if(cmp!=0)return-cmp;
                 cmp=a.x           .CompareTo(b.x           );if(cmp!=0)return-cmp;
                 cmp=a.y           .CompareTo(b.y           );if(cmp!=0)return-cmp;
             return-a.GetHashCode().CompareTo(b.GetHashCode());
            }
        }
     readonly Dictionary<int,Dictionary<Vector3Int,bool>>state=new();
        protected bool RecursivelyTryReserveBoundsAt(int layer,Vector3 margin,Vector3Int pos1,Vector3Int noiseInput1,
         out SpawnCandidateData spawnCandidateData1,ref int recursionDepth,ref bool recursionLimitReached,ref int recursionCalls
        ){
         Vector2Int cCoord1=vecPosTocCoord(pos1);
         int        cnkIdx1=GetcnkIdx(cCoord1.x,cCoord1.y);
         OpenSurfaceData(cnkIdx1);
         int recursionLevel=recursionDepth;
         recursionCalls++;
         recursionDepth++;
         if(recursionDepth>=16){
          recursionLimitReached=true;
         }
         if(recursionCalls>=256){
          recursionLimitReached=true;
         }
         bool result1;
         if(recursionLimitReached){
          spawnCandidateData1=default;
          result1=false;
          goto _RecursionLimitReached;
         }else{
          bool cached;
          VoxelSystem.Concurrent.surfaceSpawnData_rwl.EnterReadLock();
          try{
           cached=state[cnkIdx1].TryGetValue(pos1,out result1);
          }catch{
           throw;
          }finally{
           VoxelSystem.Concurrent.surfaceSpawnData_rwl.ExitReadLock();
          }
          if(cached){
           if(result1){
            GetCandidateData(layer,margin,pos1,cnkIdx1,noiseInput1,out spawnCandidateData1);
           }else{
            spawnCandidateData1=default;
           }
           return result1;
          }
         }
         result1=true;
         SpawnPickingLayer pickingLayer1=null;
         int max=0;
         Vector2 size1=Vector2.zero;
         if(recursionLimitReached){
          spawnCandidateData1=default;
          result1=false;
          goto _RecursionLimitReached;
         }else if(!GetCandidateData(layer,margin,pos1,cnkIdx1,noiseInput1,out spawnCandidateData1)){
          result1=false;
         }else{
          pickingLayer1=spawnCandidateData1.pickingLayer;
          //  TO DO: usar o tamanho pensando na possibilidade de rotação
          //max=Mathf.CeilToInt(Mathf.Max(pickingLayer1.maxDimensions.x,pickingLayer1.maxDimensions.z))+Mathf.CeilToInt(Mathf.Max(margin.x,margin.z));
          max=Mathf.CeilToInt(Mathf.Sqrt(Mathf.Pow(pickingLayer1.maxDimensions.x,2)+Mathf.Pow(pickingLayer1.maxDimensions.z,2)))+Mathf.CeilToInt(Mathf.Max(margin.x,margin.z));
          size1=new(
           spawnCandidateData1.size.x,
           spawnCandidateData1.size.z
          );
         }
         var candidatesThatConflictBySize=new SortedDictionary<Vector2,Dictionary<Vector3Int,SpawnCandidateData>>(
          candidateDescendingComparer//  ordem decrescente
         );
         if(result1){
          Vector3Int coord2=new Vector3Int(0,Height/2-1,0);
          for(coord2.x=-max;coord2.x<=max;coord2.x++){
          for(coord2.z=-max;coord2.z<=max;coord2.z++){if(coord2.x==0&&coord2.z==0){continue;}
           Vector3Int pos2=pos1;
           pos2.x+=coord2.x;
           pos2.z+=coord2.z;
           Vector3Int vCoord2=vecPosTovCoord(pos2,out Vector2Int cnkRgn2);
           Vector3Int noiseInput2=vCoord2;noiseInput2.x+=cnkRgn2.x;
                                          noiseInput2.z+=cnkRgn2.y;
           Vector2Int cCoord2=vecPosTocCoord(pos2);
           int        cnkIdx2=GetcnkIdx(cCoord2.x,cCoord2.y);
           if(GetCandidateData(layer,margin,pos2,cnkIdx2,noiseInput2,out SpawnCandidateData spawnCandidateData2)){
            Vector2 size2=new(
             spawnCandidateData2.size.x,
             spawnCandidateData2.size.z
            );
            if(candidateDescendingComparer.Compare(size2,size1)>0){
             continue;
            }
            Bounds bounds1=spawnCandidateData1.bounds;
            Bounds bounds2=spawnCandidateData2.bounds;
            Bounds worldBounds1=bounds1;worldBounds1.center=pos1;
            Bounds worldBounds2=bounds2;worldBounds2.center=pos2;
            //if(!worldBounds2.Intersects(worldBounds1)){
            if(!PhysUtil.BoundsIntersectsRotatedAndScaled(
              worldBounds2,spawnCandidateData2.rotation,spawnCandidateData2.modifiers.scale,
              worldBounds1,spawnCandidateData1.rotation,spawnCandidateData1.modifiers.scale
             )
            ){
             continue;
            }
            if(
             !candidatesThatConflictBySize.TryGetValue(size2,out var candidatesThatConflict)
            ){
             candidatesThatConflictBySize.Add(size2,candidatesThatConflict=new());
            }
            candidatesThatConflict.Add(pos2,spawnCandidateData2);
           }
          }}
         }
         Dictionary<Vector3Int,SpawnCandidateData>candidatesThatConflictWithSameSize=null;
         if(result1){
          //Log.DebugMessage("candidatesThatConflictBySize:"+candidatesThatConflictBySize.Count,container.cnkIdx==0);
          foreach(var kvp in candidatesThatConflictBySize){
           Vector2 size2=kvp.Key;//  from bigger to smaller
           //Log.DebugMessage("size2:"+size2,container.cnkIdx==0);
           if(candidateDescendingComparer.Compare(size2,size1)<0){
            var candidatesThatConflict=kvp.Value;
            if(result1){
             foreach(var posSpawnCandidateDataPair2 in candidatesThatConflict){
              Vector3Int pos2=posSpawnCandidateDataPair2.Key;
              Vector3Int vCoord2=vecPosTovCoord(pos2,out Vector2Int cnkRgn2);
              Vector3Int noiseInput2=vCoord2;noiseInput2.x+=cnkRgn2.x;
                                             noiseInput2.z+=cnkRgn2.y;
              SpawnCandidateData spawnCandidateData2=posSpawnCandidateDataPair2.Value;
              if(RecursivelyTryReserveBoundsAt(layer,margin,pos2,noiseInput2,out spawnCandidateData2,ref recursionDepth,ref recursionLimitReached,ref recursionCalls)){
               result1=false;
               break;
              }
             }
            }
                candidatesThatConflict.Clear();
           }else if(candidateDescendingComparer.Compare(size2,size1)==0){
            if(candidatesThatConflictWithSameSize!=null){
               candidatesThatConflictWithSameSize.Clear();
               //  TO DO: merge values and then clear ^
            }
               candidatesThatConflictWithSameSize=kvp.Value;
           }else{
            kvp.Value.Clear();
           }
          }
         }
         Dictionary<Vector3Int,SpawnCandidateData>candidatesThatUltimatelyConflict=new();
         if(candidatesThatConflictWithSameSize!=null){
          if(result1){
           foreach(var posSpawnCandidateDataPair2 in candidatesThatConflictWithSameSize){
            Vector3Int pos2=posSpawnCandidateDataPair2.Key;
            Vector3Int vCoord2=vecPosTovCoord(pos2,out Vector2Int cnkRgn2);
            Vector3Int noiseInput2=vCoord2;noiseInput2.x+=cnkRgn2.x;
                                           noiseInput2.z+=cnkRgn2.y;
            SpawnCandidateData spawnCandidateData2=posSpawnCandidateDataPair2.Value;
            if(pos2.x!=pos1.x){
             if(
              spawnCandidateData2.priorityOverBothX&&
              spawnCandidateData1.priorityOverBothX
             ){
              result1=false;
              break;
             }
             if(
              spawnCandidateData2.priorityOverEast&&
              spawnCandidateData1.priorityOverWest
             ){
              result1=false;
              break;
             }
             if(
              spawnCandidateData2.priorityOverWest&&
              spawnCandidateData1.priorityOverEast
             ){
              result1=false;
              break;
             }
             if(spawnCandidateData2.priorityOverBothX){
              candidatesThatUltimatelyConflict.Add(pos2,spawnCandidateData2);
              continue;
             }
             if(spawnCandidateData1.priorityOverBothX){
              continue;
             }
             if(spawnCandidateData2.priorityOverWest&&pos2.x>pos1.x){
              candidatesThatUltimatelyConflict.Add(pos2,spawnCandidateData2);
              continue;
             }
             if(spawnCandidateData2.priorityOverEast&&pos2.x<pos1.x){
              candidatesThatUltimatelyConflict.Add(pos2,spawnCandidateData2);
              continue;
             }
            }else{//  mesmo X, desempata com Z
             if(
              spawnCandidateData2.priorityOverBothZ&&
              spawnCandidateData1.priorityOverBothZ
             ){
              result1=false;
              break;
             }
             if(
              spawnCandidateData2.priorityOverNorth&&
              spawnCandidateData1.priorityOverSouth
             ){
              result1=false;
              break;
             }
             if(
              spawnCandidateData2.priorityOverSouth&&
              spawnCandidateData1.priorityOverNorth
             ){
              result1=false;
              break;
             }
             if(spawnCandidateData2.priorityOverBothZ){
              candidatesThatUltimatelyConflict.Add(pos2,spawnCandidateData2);
              continue;
             }
             if(spawnCandidateData1.priorityOverBothZ){
              continue;
             }
             if(spawnCandidateData2.priorityOverSouth&&pos2.z>pos1.z){
              candidatesThatUltimatelyConflict.Add(pos2,spawnCandidateData2);
              continue;
             }
             if(spawnCandidateData2.priorityOverNorth&&pos2.z<pos1.z){
              candidatesThatUltimatelyConflict.Add(pos2,spawnCandidateData2);
              continue;
             }
            }
           }
          }
            candidatesThatConflictWithSameSize.Clear();
         }
         if(result1){
          foreach(var posSpawnCandidateDataPair2 in candidatesThatUltimatelyConflict){
           Vector3Int pos2=posSpawnCandidateDataPair2.Key;
           Vector3Int vCoord2=vecPosTovCoord(pos2,out Vector2Int cnkRgn2);
           Vector3Int noiseInput2=vCoord2;noiseInput2.x+=cnkRgn2.x;
                                          noiseInput2.z+=cnkRgn2.y;
           SpawnCandidateData spawnCandidateData2=posSpawnCandidateDataPair2.Value;
           if(RecursivelyTryReserveBoundsAt(layer,margin,pos2,noiseInput2,out spawnCandidateData2,ref recursionDepth,ref recursionLimitReached,ref recursionCalls)){
            result1=false;
            break;
           }
          }
         }
         candidatesThatUltimatelyConflict.Clear();
         _RecursionLimitReached:{
          if(recursionLimitReached){
           spawnCandidateData1=default;
           result1=false;
          }
         }
         if(!recursionLimitReached||recursionLevel==0){
          VoxelSystem.Concurrent.surfaceSpawnData_rwl.EnterWriteLock();
          try{
           state[cnkIdx1][pos1]=result1;
          }catch{
           throw;
          }finally{
           VoxelSystem.Concurrent.surfaceSpawnData_rwl.ExitWriteLock();
          }
         }
         recursionDepth--;
         return result1;
        }
        void OpenSurfaceData(int cnkIdx){
         VoxelSystem.Concurrent.surfaceSpawnData_rwl.EnterUpgradeableReadLock();
         try{
          if(hasData.ContainsKey(cnkIdx)){
           return;
          }
          VoxelSystem.Concurrent.surfaceSpawnData_rwl.EnterWriteLock();
          try{
           if(VoxelSystem.Concurrent.surfaceHasData.TryGetValue(cnkIdx,out Dictionary<Vector3Int,SpawnCandidateData>surfaceHasData)){
            container.hasData  [cnkIdx]=hasData  [cnkIdx]=surfaceHasData;
            container.hasNoData[cnkIdx]=hasNoData[cnkIdx]=VoxelSystem.Concurrent.surfaceHasNoData[cnkIdx];
            container.state    [cnkIdx]=state    [cnkIdx]=VoxelSystem.Concurrent.surfaceState    [cnkIdx];
                                                          VoxelSystem.Concurrent.surfaceDataOpen [cnkIdx]++;
           }else{
            container.hasData  [cnkIdx]=hasData  [cnkIdx]=VoxelSystem.Concurrent.surfaceHasData  [cnkIdx]=surfaceHasData=new();
            container.hasNoData[cnkIdx]=hasNoData[cnkIdx]=VoxelSystem.Concurrent.surfaceHasNoData[cnkIdx]               =new();
            container.state    [cnkIdx]=state    [cnkIdx]=VoxelSystem.Concurrent.surfaceState    [cnkIdx]               =new();
                                                          VoxelSystem.Concurrent.surfaceDataOpen [cnkIdx]=1;
           }
          }catch{
           throw;
          }finally{
           VoxelSystem.Concurrent.surfaceSpawnData_rwl.ExitWriteLock();
          }
         }catch{
          throw;
         }finally{
          VoxelSystem.Concurrent.surfaceSpawnData_rwl.ExitUpgradeableReadLock();
         }
        }
     internal readonly HashSet<int>toClose=new();
        void CloseSurfaceData(int cnkIdx){
         VoxelSystem.Concurrent.surfaceSpawnData_rwl.EnterWriteLock();
         try{
          if(VoxelSystem.Concurrent.surfaceHasData.TryGetValue(cnkIdx,out Dictionary<Vector3Int,SpawnCandidateData>surfaceHasData)){
           int surfaceDataOpen=--VoxelSystem.Concurrent.surfaceDataOpen[cnkIdx];
           if(surfaceDataOpen<=0){
            //
            //container.hasData  [cnkIdx]=hasData  [cnkIdx]=VoxelSystem.Concurrent.surfaceHasData  [cnkIdx]=surfaceHasData=new();
            //container.hasNoData[cnkIdx]=hasNoData[cnkIdx]=VoxelSystem.Concurrent.surfaceHasNoData[cnkIdx]               =new();
            //container.state    [cnkIdx]=state    [cnkIdx]=VoxelSystem.Concurrent.surfaceState    [cnkIdx]               =new();
            //                                              VoxelSystem.Concurrent.surfaceDataOpen [cnkIdx]=1;
           }
          }
         }catch{
          throw;
         }finally{
          VoxelSystem.Concurrent.surfaceSpawnData_rwl.ExitWriteLock();
         }
        }
     readonly System.Diagnostics.Stopwatch sw=new System.Diagnostics.Stopwatch();
        protected override void Execute(){
         switch(container.execution){
          case Execution.GetGround:{
           Log.DebugMessage("Execution.GetGround");
           foreach(var spawnedTypeBlockedArrayPair in container.blocked){
            Array.Clear(spawnedTypeBlockedArrayPair.Value,0,spawnedTypeBlockedArrayPair.Value.Length);
           }
           lock(VoxelSystem.chunkStateFileSync){
            chunkStateFileStream.Position=0L;
            chunkStateFileStreamReader.DiscardBufferedData();
            string line;
            while((line=chunkStateFileStreamReader.ReadLine())!=null){
             if(string.IsNullOrEmpty(line)){continue;}
             int cnkIdxStringStart=line.IndexOf("cnkIdx=")+7;
             int cnkIdxStringEnd  =line.IndexOf(" , ",cnkIdxStringStart);
             int cnkIdxStringLength=cnkIdxStringEnd-cnkIdxStringStart;
             int cnkIdx=int.Parse(line.Substring(cnkIdxStringStart,cnkIdxStringLength),NumberStyles.Any,CultureInfoUtil.en_US);
             int surfaceSimObjectsAddedStringStart=cnkIdxStringEnd+2;
             if(cnkIdx==container.cnkIdx){
              surfaceSimObjectsAddedStringStart=line.IndexOf("surfaceSimObjectsAdded=",surfaceSimObjectsAddedStringStart);
              if(surfaceSimObjectsAddedStringStart>=0){
               int surfaceSimObjectsAddedStringEnd=line.IndexOf(" , ",surfaceSimObjectsAddedStringStart)+3;
               string surfaceSimObjectsAddedString=line.Substring(surfaceSimObjectsAddedStringStart,surfaceSimObjectsAddedStringEnd-surfaceSimObjectsAddedStringStart);
               int surfaceSimObjectsAddedFlagStringStart=surfaceSimObjectsAddedString.IndexOf("=")+1;
               int surfaceSimObjectsAddedFlagStringEnd  =surfaceSimObjectsAddedString.IndexOf(" , ",surfaceSimObjectsAddedFlagStringStart);
               bool surfaceSimObjectsAddedFlag=bool.Parse(surfaceSimObjectsAddedString.Substring(surfaceSimObjectsAddedFlagStringStart,surfaceSimObjectsAddedFlagStringEnd-surfaceSimObjectsAddedFlagStringStart));
               if(surfaceSimObjectsAddedFlag){
                container.surfaceSimObjectsHadBeenAdded=true;
               }
              }
             }
            }
           }
           if(container.surfaceSimObjectsHadBeenAdded){
            break;
           }
           QueryParameters queryParameters=new QueryParameters(VoxelSystem.voxelTerrainLayer);
           Vector3Int vCoord1=new Vector3Int(0,Height+1,0);
           for(vCoord1.x=0             ;vCoord1.x<Width;vCoord1.x++){
           for(vCoord1.z=0             ;vCoord1.z<Depth;vCoord1.z++){
            Vector3 from=vCoord1;
                    from.x+=container.cnkRgn.x+.5f;
                    from.z+=container.cnkRgn.y+.5f;
            container.GetGroundRays.AddNoResize(new RaycastCommand(from,Vector3.down,queryParameters,Height+1));
            container.GetGroundHits.AddNoResize(new RaycastHit    ()                                        );
           }}
           break;
          }
          case Execution.ReserveBounds:{
           Log.DebugMessage("ReserveBounds");
           sw.Restart();
           //  TO DO: colocar em cleanup ao invés de execute e antes adicionar ao cache global como é feito com a água (em arquivos com binary writer e reader)



           //hasData.Clear();
           //hasNoData.Clear();
           //state.Clear();
           Vector3 margin=Vector3.one*5;
           int layer=0;
           Vector2Int cnkRgn1=container.cnkRgn;
           Vector3Int vCoord1=new Vector3Int(0,Height/2-1,0);
           for(vCoord1.x=0;vCoord1.x<Width;vCoord1.x++){
           for(vCoord1.z=0;vCoord1.z<Depth;vCoord1.z++){
            int index1=vCoord1.z+vCoord1.x*Depth;
            Vector3Int pos1=vCoord1;
            pos1.x+=cnkRgn1.x;
            pos1.z+=cnkRgn1.y;
            Vector3Int noiseInput1=vCoord1;noiseInput1.x+=cnkRgn1.x;
                                           noiseInput1.z+=cnkRgn1.y;
            container.testArray[index1]=(new Color(0,0,0,0),new Bounds(Vector3.zero,Vector3.one),Vector3.one,Quaternion.identity);
            int recursionCalls=0;
            int recursionDepth=0;
            bool recursionLimitReached=false;
            if(RecursivelyTryReserveBoundsAt(layer,margin,pos1,noiseInput1,out SpawnCandidateData spawnCandidateData1,ref recursionDepth,ref recursionLimitReached,ref recursionCalls)){
             Quaternion rotation=spawnCandidateData1.rotation;
             container.testArray[index1]=(Color.green,new Bounds(Vector3.zero,spawnCandidateData1.size),spawnCandidateData1.modifiers.scale,rotation);
             if(spawnCandidateData1.simObjectPicked.simObject==typeof(Sims.Rocks.RockBig_Desert_HighTower)){
              container.testArray[index1]=(Color.cyan,new Bounds(Vector3.zero,spawnCandidateData1.size),spawnCandidateData1.modifiers.scale,rotation);
             }
            }else{
             Vector2Int cCoord1=vecPosTocCoord(pos1);
             int        cnkIdx1=GetcnkIdx(cCoord1.x,cCoord1.y);
             bool gotData;
             VoxelSystem.Concurrent.surfaceSpawnData_rwl.EnterReadLock();
             try{
              gotData=hasData[cnkIdx1].ContainsKey(pos1);
             }catch{
              throw;
             }finally{
              VoxelSystem.Concurrent.surfaceSpawnData_rwl.ExitReadLock();
             }
             if(gotData){
              container.testArray[index1]=(Color.gray,new Bounds(Vector3.zero,Vector3.one),Vector3.one,Quaternion.identity);
             }
            }
            Log.DebugMessage("recursionCalls:"+recursionCalls,container.cnkIdx==0||sw.ElapsedMilliseconds>=60000L);
           }}
           VoxelSystem.Concurrent.surfaceSpawnData_rwl.EnterReadLock();
           try{
            toClose.UnionWith(hasData.Keys);
           }catch{
            throw;
           }finally{
            VoxelSystem.Concurrent.surfaceSpawnData_rwl.ExitReadLock();
           }
           foreach(int cnkIdx in toClose){
            CloseSurfaceData(cnkIdx);
           }
           toClose.Clear();
            //container.testArray[index1]=(Color.gray,new Bounds(Vector3.zero,Vector3.one),Vector3.one);
            //if(cnkRgn1.x!=0||cnkRgn1.y!=0){
            // continue;
            //}
            //if(vCoord1.x!=1){
            // continue;
            //}
            //if(vCoord1.z!=1&&vCoord1.z!=2){
            // continue;
            //}
            //if(
            // GetSimObjectSettings(margin,layer,pos1,noiseInput1,
            //  out(Type simObject,SimObjectSettings simObjectSettings)?simObjectPicked1,
            //  out SimObjectSettings simObjectSettings1,out SimObjectSpawnModifiers modifiers1,
            //  out Vector3 size1,out Bounds bounds1,out int priority1,
            //  out Quaternion rotation1,
            //  out bool priorityOverWest1 ,out bool priorityOverEast1 ,out bool priorityOverBothX1,
            //  out bool priorityOverSouth1,out bool priorityOverNorth1,out bool priorityOverBothZ1,
            //  out double selectionValue1,out SpawnPickingLayer pickingLayer1
            // )
            //){
            // Color debugColor=Color.gray;
            // bool canSpawn=true;
            // for(int x=1;x<pickingLayer1.maxDimensions.x+Mathf.CeilToInt(margin.x);++x){
            //  Vector3 pos2=pos1;
            //  pos2.x+=x;
            //  Vector3Int vCoord2=vecPosTovCoord(pos2,out Vector2Int cnkRgn2);
            //  Vector3Int noiseInput2=vCoord2;noiseInput2.x+=cnkRgn2.x;
            //                                 noiseInput2.z+=cnkRgn2.y;
            //  if(
            //   GetSimObjectSettings(margin,layer,pos2,noiseInput2,
            //    out(Type simObject,SimObjectSettings simObjectSettings)?simObjectPicked2,
            //    out SimObjectSettings simObjectSettings2,out SimObjectSpawnModifiers modifiers2,
            //    out Vector3 size2,out Bounds bounds2,out int priority2,
            //    out Quaternion rotation2,
            //    out bool priorityOverWest2 ,out bool priorityOverEast2 ,out bool priorityOverBothX2,
            //    out bool priorityOverSouth2,out bool priorityOverNorth2,out bool priorityOverBothZ2,
            //    out double selectionValue2,out SpawnPickingLayer pickingLayer2
            //   )
            //  ){
            //   Bounds bounds1a=bounds1;bounds1a.center=pos1;
            //   Bounds bounds2a=bounds2;bounds2a.center=pos2;
            //   if(bounds1a.Intersects(bounds2a)){
            //    if(priorityOverBothX2){
            //     if(priorityOverBothX1){
            //      debugColor=Color.red;
            //      canSpawn=false;
            //      break;
            //     }
            //    }
            //   //  canSpawn=false;
            //   //  break;
            //    if(!priorityOverBothX1){
            //     canSpawn=false;
            //     break;
            //    }
            //   }
            //  }
            // }
            // if(canSpawn){
            //  if(debugColor!=Color.red){
            //   debugColor=Color.green;
            //  }
            //  if(priorityOverBothX1){
            //   //debugColor=Color.cyan;
            //  }else if(priorityOverWest1){
            //   //debugColor=Color.blue;
            //  }else{
            //   //debugColor=Color.green;
            //  }
            // }
            // container.testArray[index1]=(debugColor,bounds1,modifiers1.scale);
            //}
            //bool recursion=true;
           // if(TryReserveBoundsAtRecursively(margin,layer,pos1,noiseInput1,ref recursion)){
           //  container.testArray[index1]=(Color.green,new Bounds(Vector3.zero,Vector3.one),Vector3.one);
           //  if(
           //   GetSimObjectSettings(margin,layer,pos1,noiseInput1,
           //    out(Type simObject,SimObjectSettings simObjectSettings)?simObjectPicked1,
           //    out SimObjectSettings simObjectSettings1,out SimObjectSpawnModifiers modifiers1,
           //    out Vector3 size1,out Bounds bounds1,out int priority1,
           //    out Quaternion rotation1,
           //    out bool priorityOverWest1 ,out bool priorityOverEast1 ,out bool priorityOverBothX1,
           //    out bool priorityOverSouth1,out bool priorityOverNorth1,out bool priorityOverBothZ1,
           //    out double selectionValue1,out SpawnPickingLayer pickingLayer1
           //   )
           //  ){
           //if(simObjectPicked1.Value.simObject==typeof(Sims.Rocks.RockBig_Desert_HighTower)){
           //  container.testArray[index1]=(Color.cyan,new Bounds(Vector3.zero,Vector3.one),Vector3.one);
           //}
           //  }
           // }
           //bool Recursion(){
           // Vector3Int vCoord2=new Vector3Int(0,Height/2-1,0);
           // for(vCoord2.x=0;vCoord2.x<Width;vCoord2.x++){
           // for(vCoord2.z=0;vCoord2.z<Depth;vCoord2.z++){
           //  if(Recursion()){
           //   Recursion();
           //  }
           // }}
           // return true;
           //}
           //foreach(var layer in BaseBiomeSimObjectsSpawnSettings.s){
           //}
           //Vector3 margin=container.margin;
           //Vector2Int cnkRgn1=container.cnkRgn;
           //Vector3Int vCoord1=new Vector3Int(0,Height/2-1,0);
           //for(vCoord1.x=0;vCoord1.x<Width;vCoord1.x++){
           //for(vCoord1.z=0;vCoord1.z<Depth;vCoord1.z++){
           // int index1=vCoord1.z+vCoord1.x*Depth;
           // container.testArray[index1]=(new Color(0,0,0,0),new Bounds(Vector3.zero,Vector3.one),Vector3.one);
           // Vector3Int pos1=vCoord1;
           // pos1.x+=cnkRgn1.x;
           // pos1.z+=cnkRgn1.y;
           // Vector3Int noiseInput1=vCoord1;noiseInput1.x+=cnkRgn1.x;
           //                                noiseInput1.z+=cnkRgn1.y;
           // if(TryReserveBoundsAt(pos1,noiseInput1,out Bounds bounds1,out SimObjectSpawnModifiers modifiers1,out Color debugColor)){
           //  container.testArray[index1]=(debugColor,bounds1,modifiers1.scale);
           // }
           //}}
            //Vector3Int pos1=vCoord1;
            //pos1.x+=cnkRgn1.x;
            //pos1.z+=cnkRgn1.y;
            //Vector3Int noiseInput1=vCoord1;noiseInput1.x+=cnkRgn1.x;
            //                               noiseInput1.z+=cnkRgn1.y;
            //if(
            // GetSimObjectSettings(pos1,noiseInput1,
            //  out(Type simObject,SimObjectSettings simObjectSettings)?simObjectPicked1,
            //  out SimObjectSettings simObjectSettings1,out SimObjectSpawnModifiers modifiers1,
            //  out Vector3 size1,out Bounds bounds1,out int priority1,
            //  out Quaternion rotation1,
            //  out bool priorityOverWest1 ,out bool priorityOverEast1 ,out bool priorityOverBothX1,
            //  out bool priorityOverSouth1,out bool priorityOverNorth1,out bool priorityOverBothZ1,
            //  out double selectionValue1
            // )
            //){
            // container.testArray[index1]=(Color.gray,new Bounds(Vector3.zero,Vector3.one),Vector3.one);
            // bool canSpawnInXTest2=true;
            // bool canSpawnInZTest2=true;
            // bool unknownInXTest2=false;
            // bool unknownInZTest2=false;
            // GetCoords(
            //  modifiers1,bounds1,rotation1,
            //  ref getCoordsOutputArray2,out int length2
            // );
            // var parallelForResult2=Parallel.For(0,length2,(i2,parallelForState2)=>{
            //  Vector3Int coord2=getCoordsOutputArray2[i2];
            //  if(
            //  coord2.x==0&&
            //  coord2.z==0
            //  ){
            //   return;
            //  }
            //  Vector3 pos2=pos1;
            //  pos2.x+=coord2.x;
            //  pos2.z+=coord2.z;
            //  Vector3Int vCoord2=vecPosTovCoord(pos2,out Vector2Int cnkRgn2);
            //  Vector3Int noiseInput2=vCoord2;noiseInput2.x+=cnkRgn2.x;
            //                                 noiseInput2.z+=cnkRgn2.y;
            //  if(
            //   GetSimObjectSettings(pos2,noiseInput2,
            //    out(Type simObject,SimObjectSettings simObjectSettings)?simObjectPicked2,
            //    out SimObjectSettings simObjectSettings2,out SimObjectSpawnModifiers modifiers2,
            //    out Vector3 size2,out Bounds bounds2,out int priority2,
            //    out Quaternion rotation2,
            //    out bool priorityOverWest2 ,out bool priorityOverEast2 ,out bool priorityOverBothX2,
            //    out bool priorityOverSouth2,out bool priorityOverNorth2,out bool priorityOverBothZ2,
            //    out double selectionValue2
            //   )
            //  ){//  can we still spawn simObjectPicked1 if there's a simObjectPicked2 here?
            //   bool local_canSpawnInXTest2=true;
            //   bool local_canSpawnInZTest2=true;
            //   //ResolveSpawnConflict(
            //   // size1,priority1,
            //   // size2,priority2,
            //   // coord2,
            //   // selectionValue1,
            //   // selectionValue2,
            //   // priorityOverWest1 ,priorityOverEast1 ,priorityOverBothX1,priorityOverSouth1,priorityOverNorth1,priorityOverBothZ1,
            //   // priorityOverWest2 ,priorityOverEast2 ,priorityOverBothX2,priorityOverSouth2,priorityOverNorth2,priorityOverBothZ2,
            //   // ref local_canSpawnInXTest2,
            //   // ref local_canSpawnInZTest2
            //   //);
            //   ////if(!local_canSpawnInXTest2||!local_canSpawnInZTest2){
            //   //// canSpawnInXTest2=false;
            //   //// canSpawnInZTest2=false;
            //   //// parallelForState2.Stop();
            //   //// return;
            //   ////}else{
            //   //// //unknownInXTest2=true;
            //   //// //unknownInZTest2=true;
            //   ////}
            //   //  Bounds bounds1a=bounds1;bounds1a.center=pos1;
            //   //  Bounds bounds2a=bounds2;bounds2a.center=pos2;
            //   //  if(
            //   //   //PhysUtil.BoundsIntersectsRotatedAndScaled(
            //   //   // bounds1a,rotation1,modifiers1.scale,
            //   //   // bounds2a,rotation2,modifiers2.scale
            //   //   //)
            //   //   bounds1a.Intersects(bounds2a)
            //   //  ){
            //   //   if(
            //   //    size1.x<=size2.x||
            //   //    size1.z<=size2.z
            //   //   ){
            //   //    if(!priorityOverBothX1||!priorityOverBothZ1){
            //   //     canSpawnInXTest2=false;
            //   //     canSpawnInZTest2=false;
            //   //     parallelForState2.Stop();
            //   //     return;
            //   //    }else{
            //   //     //unknownInXTest2=true;
            //   //     //unknownInZTest2=true;
            //   //    }
            //   //   }else{
            //   //    //unknownInXTest2=true;
            //   //    //unknownInZTest2=true;
            //   //   }
            //   // //  canSpawnInXTest3=false;
            //   // //  canSpawnInZTest3=false;
            //   // //  parallelForState3.Stop();
            //   // //  return;
            //   //  }
            //   //}else{//  empate
            //   // //Bounds bounds1a=bounds1;bounds1a.center=pos1;
            //   // //Bounds bounds2a=bounds2;bounds2a.center=pos2;
            //   // //if(
            //   // // PhysUtil.BoundsIntersectsScaledRotated(
            //   // //  bounds1a,rotation1,modifiers1.scale,
            //   // //  bounds2a,rotation2,modifiers2.scale
            //   // // )
            //   // //){
            //   // // if(priorityOverEast1){
            //   // //  if(priorityOverEast2){
            //   // //  }
            //   // // }
            //   // // //if(priority2>priority1){
            //   // // //canSpawnInXTest2=false;
            //   // // //canSpawnInZTest2=false;
            //   // // //parallelForState2.Stop();
            //   // // //return;
            //   // // //}else{
            //   // //  //if(
            //   // //  // size2.z>size1.z||
            //   // //  // size2.x>size1.x
            //   // //  //){
            //   // //  // canSpawnInXTest2=false;
            //   // //  // canSpawnInZTest2=false;
            //   // //  // parallelForState2.Stop();
            //   // //  // return;
            //   // //  //}
            //   // // //}
            //   // //}
            //   //}
            //  }
            // });
            // if(canSpawnInXTest2&&canSpawnInZTest2){
            //  SimObjectSpawnModifiers maxSpawnTestModifiers=new();
            //  maxSpawnTestModifiers.scale=Vector3.one;
            //  Vector3 maxSpawnSize=container.maxSpawnSize;
            //  Bounds maxSpawnTestBounds=new Bounds(Vector3.zero,maxSpawnSize);
            //  bool canSpawnInXTest3=true;
            //  bool canSpawnInZTest3=true;
            //  bool unknownInXTest3=false;
            //  bool unknownInZTest3=false;
            //  getCoordsOutputHashSet2.Clear();
            //  getCoordsOutputHashSet2.UnionWith(getCoordsOutputArray2);
            //  GetCoords(maxSpawnTestModifiers,maxSpawnTestBounds,Quaternion.identity,
            //   ref getCoordsOutputArray3,out int length3,getCoordsOutputHashSet2
            //  );
            //  var parallelForResult3=Parallel.For(0,length3,(i3,parallelForState3)=>{
            //   Vector3Int coord3=getCoordsOutputArray3[i3];
            //   if(
            //    coord3.x==0&&
            //    coord3.z==0
            //   ){
            //    return;
            //   }
            //   Vector3 pos3=pos1;
            //   pos3.x+=coord3.x;
            //   pos3.z+=coord3.z;
            //   Vector3Int vCoord3=vecPosTovCoord(pos3,out Vector2Int cnkRgn3);
            //   Vector3Int noiseInput3=vCoord3;noiseInput3.x+=cnkRgn3.x;
            //                                  noiseInput3.z+=cnkRgn3.y;
            //   if(
            //    GetSimObjectSettings(pos3,noiseInput3,
            //     out(Type simObject,SimObjectSettings simObjectSettings)?simObjectPicked3,
            //     out SimObjectSettings simObjectSettings3,out SimObjectSpawnModifiers modifiers3,
            //     out Vector3 size3,out Bounds bounds3,out int priority3,
            //     out Quaternion rotation3,
            //     out bool priorityOverWest3 ,out bool priorityOverEast3 ,out bool priorityOverBothX3,
            //     out bool priorityOverSouth3,out bool priorityOverNorth3,out bool priorityOverBothZ3,
            //     out double selectionValue3
            //    )
            //   ){//  can we still spawn simObjectPicked1 if there's a simObjectPicked3 here?
            //    bool local_canSpawnInXTest3=true;
            //    bool local_canSpawnInZTest3=true;
            //    //ResolveSpawnConflict(
            //    // size1,priority1,
            //    // size3,priority3,
            //    // coord3,
            //    // selectionValue1,
            //    // selectionValue3,
            //    // priorityOverWest1 ,priorityOverEast1 ,priorityOverBothX1,priorityOverSouth1,priorityOverNorth1,priorityOverBothZ1,
            //    // priorityOverWest3 ,priorityOverEast3 ,priorityOverBothX3,priorityOverSouth3,priorityOverNorth3,priorityOverBothZ3,
            //    // ref local_canSpawnInXTest3,
            //    // ref local_canSpawnInZTest3
            //    //);
            //    //if(!local_canSpawnInXTest3||!local_canSpawnInZTest3){
            //    // Bounds bounds1a=bounds1;bounds1a.center=pos1;
            //    // Bounds bounds3a=bounds3;bounds3a.center=pos3;
            //    // if(
            //    //  //PhysUtil.BoundsIntersectsRotatedAndScaled(
            //    //  // bounds1a,rotation1,modifiers1.scale,
            //    //  // bounds3a,rotation3,modifiers3.scale
            //    //  //)
            //    //  bounds1a.Intersects(bounds3a)
            //    // ){
            //    //  if(
            //    //   size1.x<=size3.x||
            //    //   size1.z<=size3.z
            //    //  ){
            //    //   if(!priorityOverBothX1||!priorityOverBothZ1){
            //    //    canSpawnInXTest3=false;
            //    //    canSpawnInZTest3=false;
            //    //    parallelForState3.Stop();
            //    //    return;
            //    //   }else{
            //    //    //if(
            //    //    // simObjectPicked1.Value.simObject==typeof(Sims.Trees.Pinus_elliottii_1)&&
            //    //    // simObjectPicked3.Value.simObject==typeof(Sims.Rocks.RockBig_Desert_HighTower)
            //    //    //){
            //    //     unknownInXTest3=true;
            //    //     unknownInZTest3=true;
            //    //   //  if(
            //    //   //   size1.x<size3.x||
            //    //   //   size1.z<size3.z
            //    //   //  ){
            //    //   //if(!priorityOverBothX3||!priorityOverBothZ3){
            //    //   // canSpawnInXTest3=false;
            //    //   // canSpawnInZTest3=false;
            //    //   // parallelForState3.Stop();
            //    //   // return;
            //    //   //}
            //    //   //  }
            //    //    //}
            //    //   }
            //    //  }else{
            //    //   //unknownInXTest3=true;
            //    //   //unknownInZTest3=true;
            //    //  }
            //    ////  canSpawnInXTest3=false;
            //    ////  canSpawnInZTest3=false;
            //    ////  parallelForState3.Stop();
            //    ////  return;
            //    // }
            //    //}else{
            //    // Bounds bounds1a=bounds1;bounds1a.center=pos1;
            //    // Bounds bounds3a=bounds3;bounds3a.center=pos3;
            //    // if(
            //    //  PhysUtil.BoundsIntersectsScaledRotated(
            //    //   bounds1a,rotation1,modifiers1.scale,
            //    //   bounds3a,rotation3,modifiers3.scale
            //    //  )
            //    // ){
            //    //  //if(
            //    //  // simObjectPicked1.Value.simObject==typeof(Sims.Trees.Pinus_elliottii_1)&&
            //    //  // simObjectPicked3.Value.simObject==typeof(Sims.Rocks.RockBig_Desert_HighTower)
            //    //  //){
            //    //   //if(priorityOverBothX1){
            //    //    //if(priorityOverBothX3){
            //    //     //Log.Warning("conflict unresolved:simObjectPicked1:"+simObjectPicked1+":simObjectPicked3:"+simObjectPicked3);
            //    //     unknownInXTest3=true;
            //    //     unknownInZTest3=true;
            //    //    //}
            //    //   //}
            //    //  //}
            //    //  //if(
            //    //  // simObjectPicked1.Value.simObject==typeof(Sims.Rocks.RockBig_Desert_HighTower)&&
            //    //  // simObjectPicked3.Value.simObject==typeof(Sims.Trees.Pinus_elliottii_1)
            //    //  //){
            //    //  // if(priorityOverBothX1){
            //    //  //  if(priorityOverBothX3){
            //    //  //   Log.Warning("conflict unresolved 2");
            //    //  //  }
            //    //  // }
            //    //  //}
            //    // }
            //    //}
            //   }
            //  });
            //  if(canSpawnInXTest3&&canSpawnInZTest3){
            //   if(unknownInXTest2||unknownInZTest2){
            //    container.testArray[index1]=(Color.red,bounds1,modifiers1.scale);
            //   }else if(unknownInXTest3||unknownInZTest3){
            //    container.testArray[index1]=(Color.yellow,bounds1,modifiers1.scale);
            //   }else{
            //    if(priorityOverBothX1){
            //     container.testArray[index1]=(Color.cyan,bounds1,modifiers1.scale);
            //    }else if(priorityOverWest1){
            //     container.testArray[index1]=(Color.blue,bounds1,modifiers1.scale);
            //    }else{
            //     container.testArray[index1]=(Color.green,bounds1,modifiers1.scale);
            //    }
            //   }
            //  }
            // }
            //}



             //container.testArray[index1]=(Color.gray,bounds1);
           //  SimObjectSpawnModifiers modifiersSpawnMax=new();
           //  modifiersSpawnMax.scale=Vector3.one;
           //  Vector3 sizeSpawnMax=container.maxSpawnSize;
           //  Bounds boundsSpawnMax=new Bounds(Vector3.zero,sizeSpawnMax);
           //  bool canSpawnInXTest3=true;
           //  bool canSpawnInZTest3=true;
           //  GetCoords(modifiersSpawnMax,boundsSpawnMax,Quaternion.identity,
           //   ref getCoordsOutputArray3,out int length3,getCoordsOutputHashSet2
           //  );
           //  var parallelForResult3=Parallel.For(0,length3,(i3,parallelForState3)=>{
           //   Vector3Int coord3=getCoordsOutputArray3[i3];
           //   if(
           //    coord3.x==0&&
           //    coord3.z==0
           //   ){
           //    return;
           //   }
           //   //Log.DebugMessage("coord3:"+coord3,container.cnkRgn==Vector2Int.zero&&vCoord1.x==0&&vCoord1.z==0);
           //   Vector3 pos3=pos1;
           //   pos3.x+=coord3.x;
           //   pos3.z+=coord3.z;
           //   Vector3Int vCoord3=vecPosTovCoord(pos3,out Vector2Int cnkRgn3);
           //   Vector3Int noiseInput3=vCoord3;noiseInput3.x+=cnkRgn3.x;
           //                                  noiseInput3.z+=cnkRgn3.y;
           //   if(
           //    GetSimObjectSettings(pos3,noiseInput3,
           //     out(Type simObject,SimObjectSettings simObjectSettings)?simObjectPicked3,
           //     out SimObjectSettings simObjectSettings3,out SimObjectSpawnModifiers modifiers3,
           //     out Vector3 size3,out Bounds bounds3,out int priority3,
           //     out Quaternion rotation3,
           //     out bool priorityOverWest3 ,out bool priorityOverEast3 ,out bool priorityOverBothX3,
           //     out bool priorityOverSouth3,out bool priorityOverNorth3,out bool priorityOverBothZ3,
           //     out double selectionValue3
           //     )
           //   ){//  can we still spawn simObjectPicked1 if there's a simObjectPicked3 here?               
           //    bool local_canSpawnInXTest3=true;
           //    bool local_canSpawnInZTest3=true;
           //    ResolveSpawnConflict(
           //     size1,priority1,
           //     size3,priority3,
           //     coord3,
           //     selectionValue1,
           //     selectionValue3,
           //     priorityOverWest1 ,priorityOverEast1 ,priorityOverBothX1,priorityOverSouth1,priorityOverNorth1,priorityOverBothZ1,
           //     priorityOverWest3 ,priorityOverEast3 ,priorityOverBothX3,priorityOverSouth3,priorityOverNorth3,priorityOverBothZ3,
           //     ref local_canSpawnInXTest3,
           //     ref local_canSpawnInZTest3
           //    );
           //    if(!local_canSpawnInXTest3||!local_canSpawnInZTest3){
           //     Bounds bounds1a=bounds1;bounds1a.center=pos1;
           //     Bounds bounds3a=bounds3;bounds3a.center=pos3;
           //     if(
           //      PhysUtil.BoundsIntersectsScaledRotated(
           //       bounds1a,rotation1,modifiers1.scale,
           //       bounds3a,rotation3,modifiers3.scale
           //      )
           //     ){
           //      canSpawnInXTest3=false;
           //      canSpawnInZTest3=false;
           //      parallelForState3.Stop();
           //      return;
           //     }
           //    }
           //   }
           //  });
           //  if(canSpawnInXTest3&&canSpawnInZTest3){
           //   container.testArray[index1]=(Color.green,bounds1,modifiers1.scale);
           //  }
           // }
           //}}
           //  Log.DebugMessage("trying to spawn:"+simObjectPicked1.Value.simObject);
           //  //container.testArray[index1]=(Color.gray,new Bounds(Vector3.zero,Vector3.one));
           //  bool canSpawnInXTest2=true;
           //  bool canSpawnInZTest2=true;
           //  GetCoords(
           //   modifiers1,bounds1,rotation1,
           //   ref getCoordsOutputArray2,out int length2
           //  );
           //  var parallelForResult2=Parallel.For(0,length2,(i2,parallelForState2)=>{
           //   Vector3Int coord2=getCoordsOutputArray2[i2];
           //   if(
           //   coord2.x==0&&
           //   coord2.z==0
           //   ){
           //    return;
           //   }
           //   Vector3 pos2=pos1;
           //   pos2.x+=coord2.x;
           //   pos2.z+=coord2.z;
           //   Vector3Int vCoord2=vecPosTovCoord(pos2,out Vector2Int cnkRgn2);
           //   Vector3Int noiseInput2=vCoord2;noiseInput2.x+=cnkRgn2.x;
           //                                  noiseInput2.z+=cnkRgn2.y;
           //   if(
           //    GetSimObjectSettings(pos2,noiseInput2,
           //     out(Type simObject,SimObjectSettings simObjectSettings)?simObjectPicked2,
           //     out SimObjectSettings simObjectSettings2,out SimObjectSpawnModifiers modifiers2,
           //     out Vector3 size2,out Bounds bounds2,out int priority2,
           //     out Quaternion rotation2,
           //     out bool priorityOverWest2 ,out bool priorityOverEast2 ,out bool priorityOverBothX2,
           //     out bool priorityOverSouth2,out bool priorityOverNorth2,out bool priorityOverBothZ2,
           //     out double selectionValue2
           //    )
           //   ){//  can we still spawn simObjectPicked1 if there's a simObjectPicked2 here?
           //    ResolveSpawnConflict(
           //     size1,priority1,
           //     size2,priority2,
           //     coord2,
           //     selectionValue1,
           //     selectionValue2,
           //     priorityOverWest1 ,priorityOverEast1 ,priorityOverBothX1,priorityOverSouth1,priorityOverNorth1,priorityOverBothZ1,
           //     priorityOverWest2 ,priorityOverEast2 ,priorityOverBothX2,priorityOverSouth2,priorityOverNorth2,priorityOverBothZ2,
           //     ref canSpawnInXTest2,
           //     ref canSpawnInZTest2
           //    );
           //    if(!canSpawnInXTest2||!canSpawnInZTest2){
           //     parallelForState2.Stop();
           //     return;
           //    }
           //   }
           //  });
           //  if(canSpawnInXTest2&&canSpawnInZTest2){
           //   SimObjectSpawnModifiers modifiersSpawnMax=new();
           //   modifiersSpawnMax.scale=Vector3.one;
           //   Vector3 sizeSpawnMax=container.maxSpawnSize;
           //   Bounds boundsSpawnMax=new Bounds(Vector3.zero,sizeSpawnMax);
           //   bool canSpawnInXTest3=true;
           //   bool canSpawnInZTest3=true;
           //   getCoordsOutputHashSet2.Clear();
           //   getCoordsOutputHashSet2.UnionWith(getCoordsOutputArray2);
           //   GetCoords(modifiersSpawnMax,boundsSpawnMax,Quaternion.identity,
           //    ref getCoordsOutputArray3,out int length3,getCoordsOutputHashSet2
           //   );
           //   var parallelForResult3=Parallel.For(0,length3,(i3,parallelForState3)=>{
           //    Vector3Int coord3=getCoordsOutputArray3[i3];
           //    if(
           //     coord3.x==0&&
           //     coord3.z==0
           //    ){
           //     return;
           //    }
           //    Vector3 pos3=pos1;
           //    pos3.x+=coord3.x;
           //    pos3.z+=coord3.z;
           //    Vector3Int vCoord3=vecPosTovCoord(pos3,out Vector2Int cnkRgn3);
           //    Vector3Int noiseInput3=vCoord3;noiseInput3.x+=cnkRgn3.x;
           //                                   noiseInput3.z+=cnkRgn3.y;
           //    if(
           //     GetSimObjectSettings(pos3,noiseInput3,
           //      out(Type simObject,SimObjectSettings simObjectSettings)?simObjectPicked3,
           //      out SimObjectSettings simObjectSettings3,out SimObjectSpawnModifiers modifiers3,
           //      out Vector3 size3,out Bounds bounds3,out int priority3,
           //      out Quaternion rotation3,
           //      out bool priorityOverWest3 ,out bool priorityOverEast3 ,out bool priorityOverBothX3,
           //      out bool priorityOverSouth3,out bool priorityOverNorth3,out bool priorityOverBothZ3,
           //      out double selectionValue3
           //     )
           //    ){//  can we still spawn simObjectPicked1 if there's a simObjectPicked3 here?
           //     bool local_canSpawnInXTest3=true;
           //     bool local_canSpawnInZTest3=true;
           //     ResolveSpawnConflict(
           //      size1,priority1,
           //      size3,priority3,
           //      coord3,
           //      selectionValue1,
           //      selectionValue3,
           //      priorityOverWest1 ,priorityOverEast1 ,priorityOverBothX1,priorityOverSouth1,priorityOverNorth1,priorityOverBothZ1,
           //      priorityOverWest3 ,priorityOverEast3 ,priorityOverBothX3,priorityOverSouth3,priorityOverNorth3,priorityOverBothZ3,
           //      ref local_canSpawnInXTest3,
           //      ref local_canSpawnInZTest3
           //     );
           //     if(!local_canSpawnInXTest3||!local_canSpawnInZTest3){
           //      Bounds bounds1a=bounds1;bounds1a.center=pos1;
           //      Bounds bounds3a=bounds3;bounds3a.center=pos3;
           //      if(
           //       PhysUtil.BoundsIntersectsScaledRotated(
           //        bounds1a,rotation1,modifiers1.scale,
           //        bounds3a,rotation3,modifiers3.scale
           //       )
           //      ){
           //       canSpawnInXTest3=false;
           //       canSpawnInZTest3=false;
           //       parallelForState3.Stop();
           //       return;
           //      }
           //     }
           //    }
           //   });
           //   if(canSpawnInXTest3&&canSpawnInZTest3){
           //    container.testArray[index1]=(Color.green,bounds1);
           //    //if(!priorityOverBothX1){
           //     //if(!priorityOverBothZ1){
           //    if(simObjectPicked1.Value.simObject==typeof(Sims.Rocks.RockBig_Desert_HighTower)){
           //     container.testArray[index1]=(Color.cyan,bounds1);
           //    }
           //      //Vector3 pos3=pos1;
           //      //pos3.z+=1;
           //      //Vector3Int vCoord3=vecPosTovCoord(pos3,out Vector2Int cnkRgn3);
           //      //Vector3Int noiseInput3=vCoord3;noiseInput3.x+=cnkRgn3.x;
           //      //                               noiseInput3.z+=cnkRgn3.y;
           //      ////(Type simObject,SimObjectSettings simObjectSettings)?simObjectPicked3=VoxelSystem.biome.biomeSpawnSettings.TryGetSettingsToSpawnSimObject(noiseInput3,out double selectionValue3);
           //      ////if(simObjectPicked3!=null){
           //      //// Log.Error("got a normal spawn...WHAT?!");
           //      ////}
           //      //Log.DebugMessage("non priority spawn point:pos1:"+pos1+":vCoord1:"+vCoord1+":cnkRgn1:"+cnkRgn1+":selectionValue1:"+selectionValue1+":"+simObjectPicked1.Value.simObject);
           //     //}
           //    //}
           //    Log.DebugMessage("got a spawn point:pos1:"+pos1+":vCoord1:"+vCoord1+":cnkRgn1:"+cnkRgn1+":selectionValue1:"+selectionValue1+":"+simObjectPicked1.Value.simObject);
           //   }
           //  }
           //  //if(priorityOverSouth1){
           //  // //container.testArray[index1]=Color.red;
           //  //}else{
           //  // if(priorityOverNorth1){
           //  //  //container.testArray[index1]=Color.yellow;
           //  // }
           //  //}
           // }
           //}}
           //for(vCoord1.x=0;vCoord1.x<Width;vCoord1.x++){
           //for(vCoord1.z=0;vCoord1.z<Depth;vCoord1.z++){
           // int index1=vCoord1.z+vCoord1.x*Depth;
           // container.testArray[index1]=new Color(0,0,0,0);
           // Vector3 pos1=vCoord1;
           // pos1.x+=cnkRgn1.x;
           // pos1.z+=cnkRgn1.y;
           // Vector3Int noiseInput1=vCoord1;noiseInput1.x+=cnkRgn1.x;
           //                                noiseInput1.z+=cnkRgn1.y;
           // (Type simObject,SimObjectSettings simObjectSettings)?simObjectPicked1=VoxelSystem.biome.biomeSpawnSettings.TryGetSettingsToSpawnSimObject(noiseInput1,out double selectionValue1);
           // if(simObjectPicked1!=null){//  try to spawn simObjectPicked1
           //  container.testArray[index1]=Color.gray;
           //  SimObjectSettings simObjectSettings1=simObjectPicked1.Value.simObjectSettings;
           //  Vector3 size1=simObjectSettings1.size;
           //  Bounds maxScaledBounds1=new Bounds(Vector3.zero,Vector3.Scale(size1,simObjectSettings1.maxScale));
           //  Vector3 maxScaledExtents1=maxScaledBounds1.extents;
           //  Vector3 margin1=Vector3.one;
           //  int seqResult1a=MathUtil.AlternatingSequenceWithSeparator((int)pos1.x,(Mathf.CeilToInt(maxScaledExtents1.x)+Mathf.CeilToInt(margin1.x))*2,0);
           //  bool priorityOverWest1 =seqResult1a==0;
           //  bool priorityOverEast1 =seqResult1a==1;
           //  bool priorityOverBothX1=seqResult1a==2;
           //  int priority1=simObjectSettings1.priority;
           //  Bounds bounds1=new Bounds(Vector3.zero,size1);
           //  SimObjectSpawnModifiers modifiers1=VoxelSystem.biome.biomeSpawnSettings.GetSimObjectSpawnModifiers(noiseInput1,simObjectSettings1);
           //  bool canSpawnInX2=true;
           //  bool canSpawnInZ2=true;
           //  int getCoordsOutputArraySize2=PhysUtil.GetCoordsInsideBoundsMinArraySize(bounds1,modifiers1.scale,margin1);
           //  if(getCoordsOutputArraySize2>getCoordsOutputArray2.Length){
           //   Array.Resize(ref getCoordsOutputArray2,getCoordsOutputArraySize2);
           //  }
           //  Quaternion rotation1=Quaternion.AngleAxis(modifiers1.rotation,Vector3.up);
           //  int length2=PhysUtil.GetCoordsInsideBoundsUsingParallelFor(bounds1,modifiers1.scale,rotation1,margin1,
           //   false,getCoordsOutputArray2
           //  );
           //  var parallelForResult2=Parallel.For(0,length2,(i2,parallelForState2)=>{
           //   Vector3Int coord2=getCoordsOutputArray2[i2];
           //   if(
           //   coord2.x==0&&
           //   coord2.z==0
           //   ){
           //    return;
           //   }
           //   Vector3 pos2=pos1;
           //   pos2.x+=coord2.x;
           //   pos2.z+=coord2.z;
           //   Vector3Int vCoord2=vecPosTovCoord(pos2,out Vector2Int cnkRgn2);
           //   Vector3Int noiseInput2=vCoord2;noiseInput2.x+=cnkRgn2.x;
           //                                  noiseInput2.z+=cnkRgn2.y;
           //   (Type simObject,SimObjectSettings simObjectSettings)?simObjectPicked2=VoxelSystem.biome.biomeSpawnSettings.TryGetSettingsToSpawnSimObject(noiseInput2,out double selectionValue2);
           //   if(simObjectPicked2!=null){//  can we still spawn simObjectPicked1 if there's simObjectPicked2?
           //    SimObjectSettings simObjectSettings2=simObjectPicked2.Value.simObjectSettings;
           //    Vector3 size2=simObjectSettings2.size;
           //    Bounds maxScaledBounds2=new Bounds(Vector3.zero,Vector3.Scale(size2,simObjectSettings2.maxScale));
           //    Vector3 maxScaledExtents2=maxScaledBounds2.extents;
           //    Vector3 margin2=Vector3.one;
           //    int seqResult2a=MathUtil.AlternatingSequenceWithSeparator((int)pos2.x,(Mathf.CeilToInt(maxScaledExtents2.x)+Mathf.CeilToInt(margin2.x))*2,0);
           //    bool priorityOverWest2 =seqResult2a==0;
           //    bool priorityOverEast2 =seqResult2a==1;
           //    bool priorityOverBothX2=seqResult2a==2;
           //    int priority2=simObjectSettings2.priority;
           //    if(coord2.x!=0){
           //     if(priorityOverBothX2){
           //      if(priorityOverBothX1){//  empate
           //       if(priority2>priority1){
           //        canSpawnInX2=false;
           //        parallelForState2.Stop();
           //       }else{
           //        if(size2.x>=size1.x){
           //         container.testArray[index1]=Color.yellow;
           //         canSpawnInX2=false;
           //         parallelForState2.Stop();
           //        }
           //       }
           //      }else{
           //       container.testArray[index1]=Color.yellow;
           //       canSpawnInX2=false;
           //       parallelForState2.Stop();
           //      }
           //     }else{
           //      if(coord2.x>0){
           //       if(priorityOverWest2){
           //        if(priorityOverWest1){
           //         container.testArray[index1]=new Color(1f,.5f,0.0156862754f,1f);
           //         canSpawnInX2=false;
           //         parallelForState2.Stop();
           //        }
           //       }
           //      }else{
           //       if(priorityOverEast2){
           //        if(priorityOverEast1){
           //         container.testArray[index1]=Color.red;
           //         canSpawnInX2=false;
           //         parallelForState2.Stop();
           //        }
           //       }
           //      }
           //     }
           //    }
           //   }
           //  });
           //  if(canSpawnInX2&&canSpawnInZ2){
           //   Vector3Int maxSpawnSize=container.maxSpawnSize;
           //   Bounds boundsMax=new Bounds(Vector3.zero,maxSpawnSize);
           //   Vector3 marginMax=Vector3.one;
           //   bool canSpawnInX3=true;
           //   bool canSpawnInZ3=true;
           //   var parallelForResult3=Parallel.For(0,length3,(i3,parallelForState3)=>{
           //    Vector3Int coord3=getCoordsOutputArray3[i3];
           //    if(
           //    coord3.x==0&&
           //    coord3.z==0
           //    ){
           //     return;
           //    }
           //    Vector3 pos3=pos1;
           //    pos3.x+=coord3.x;
           //    pos3.z+=coord3.z;
           //    Vector3Int vCoord3=vecPosTovCoord(pos3,out Vector2Int cnkRgn3);
           //    Vector3Int noiseInput3=vCoord3;noiseInput3.x+=cnkRgn3.x;
           //                                   noiseInput3.z+=cnkRgn3.y;
           //    (Type simObject,SimObjectSettings simObjectSettings)?simObjectPicked3=VoxelSystem.biome.biomeSpawnSettings.TryGetSettingsToSpawnSimObject(noiseInput3,out double selectionValue3);
           //    if(simObjectPicked3!=null){//  can we still spawn simObjectPicked1 if there's simObjectPicked3?
           //     container.testArray[index1]=Color.white;
           //     SimObjectSettings simObjectSettings3=simObjectPicked3.Value.simObjectSettings;
           //     Vector3 size3=simObjectSettings3.size;
           //     Bounds maxScaledBounds3=new Bounds(Vector3.zero,Vector3.Scale(size3,simObjectSettings3.maxScale));
           //     Vector3 maxScaledExtents3=maxScaledBounds3.extents;
           //     Vector3 margin3=Vector3.one;
           //     int seqResult3a=MathUtil.AlternatingSequenceWithSeparator((int)pos3.x,(Mathf.CeilToInt(maxScaledExtents3.x)+Mathf.CeilToInt(margin3.x))*2,0);
           //     bool priorityOverWest3 =seqResult3a==0;
           //     bool priorityOverEast3 =seqResult3a==1;
           //     bool priorityOverBothX3=seqResult3a==2;
           //     int priority3=simObjectSettings3.priority;
           //     Bounds bounds3=new Bounds(Vector3.zero,size3);
           //     SimObjectSpawnModifiers?modifiers3=null;
           //     if(coord3.x!=0){
           //      if(priorityOverBothX3){
           //       if(priorityOverBothX1){//  empate
           //        if(priority3>priority1){
           //         if(Conflicts3a()){
           //          canSpawnInX3=false;
           //          parallelForState3.Stop();
           //         }
           //        }else{
           //         if(size3.x>=size1.x){
           //          if(Conflicts3a()){
           //           canSpawnInX3=false;
           //           parallelForState3.Stop();
           //          }
           //         }
           //        }
           //       }else{
           //        if(Conflicts3a()){
           //         canSpawnInX3=false;
           //         parallelForState3.Stop();
           //        }
           //       }
           //      }else{
           //       if(coord3.x>0){
           //        if(priorityOverWest3){
           //         if(priorityOverWest1){
           //          if(Conflicts3a()){
           //           canSpawnInX3=false;
           //           parallelForState3.Stop();
           //          }
           //         }
           //        }
           //       }else{
           //        if(priorityOverEast3){
           //         if(priorityOverEast1){
           //          if(Conflicts3a()){
           //           canSpawnInX3=false;
           //           parallelForState3.Stop();
           //          }
           //         }
           //        }
           //       }
           //      }
           //     }
           //     bool Conflicts3a(){
           //      Bounds bounds1a=bounds1;bounds1a.center=pos1;
           //      Bounds bounds3a=bounds3;bounds3a.center=pos3;
           //      if(modifiers3==null){modifiers3=VoxelSystem.biome.biomeSpawnSettings.GetSimObjectSpawnModifiers(noiseInput3,simObjectPicked3.Value.simObjectSettings);}
           //      Quaternion rotation3=Quaternion.AngleAxis(modifiers3.Value.rotation,Vector3.up);
           //      return PhysUtil.BoundsIntersectsScaledRotated(
           //       bounds1a,rotation1,modifiers1      .scale,
           //       bounds3a,rotation3,modifiers3.Value.scale
           //      );
           //     }
           //    }
           //   });
           //   if(canSpawnInX3&&canSpawnInZ3){
           //    container.testArray[index1]=Color.green;
           //   }
           //  }
           // }
           //}}
           sw.Stop();
           Log.DebugMessage("VoxelTerrainSurfaceSimObjectsPlacerMultithreaded Execute ReserveBounds:cnkRgn:"+container.cnkRgn+":time:"+sw.ElapsedMilliseconds+" ms");
           //Vector2Int cnkRgn1=container.cnkRgn;
           //Vector3Int vCoord1=new Vector3Int(0,Height/2-1,0);
           //for(vCoord1.x=0;vCoord1.x<Width;vCoord1.x++){
           //for(vCoord1.z=0;vCoord1.z<Depth;vCoord1.z++){
           // int surfaceSpawnMapArrayIdx=vCoord1.z+vCoord1.x*Depth;
           // Vector3 pos1=vCoord1;
           // pos1.x+=cnkRgn1.x;
           // pos1.z+=cnkRgn1.y;
           // Vector3Int noiseInput1=vCoord1;noiseInput1.x+=cnkRgn1.x;
           //                                noiseInput1.z+=cnkRgn1.y;
           // (Type simObject,SimObjectSettings simObjectSettings)?simObjectPicked1=VoxelSystem.biome.biomeSpawnSettings.TryGetSettingsToSpawnSimObject(noiseInput1,out double selectionValue1);
           // if(simObjectPicked1!=null){
           //  //  try to spawn simObjectPicked1
           //  Log.DebugMessage("at pos1:"+pos1+":cnkRgn1:"+cnkRgn1+":picked simObjectPicked1");
           //  Vector3 margin1=Vector3.one;
           //  SimObjectSettings simObjectSettings1=simObjectPicked1.Value.simObjectSettings;
           //  Vector3 size1=simObjectSettings1.size;
           //  Bounds bounds1=new Bounds(Vector3.zero,size1);
           //  Bounds maxScaledBounds1=new Bounds(Vector3.zero,Vector3.Scale(size1,simObjectSettings1.maxScale));
           //  int seqResult1a=MathUtil.AlternatingSequenceWithSeparator((int)pos1.x,container.maxSpawnSize.x,0);
           //  int seqResult1b=MathUtil.AlternatingSequenceWithSeparator((int)pos1.z,container.maxSpawnSize.z,0);
           //  bool priorityOverEast1 =seqResult1a==0;
           //  bool priorityOverWest1 =seqResult1a==1;
           //  bool priorityOverBothX1=seqResult1a==2;
           //  bool priorityOverNorth1=seqResult1b==0;
           //  bool priorityOverSouth1=seqResult1b==1;
           //  bool priorityOverBothZ1=seqResult1b==2;
           //  SimObjectSpawnModifiers modifiers1=VoxelSystem.biome.biomeSpawnSettings.GetSimObjectSpawnModifiers(noiseInput1,simObjectPicked1.Value.simObjectSettings);
           //  //double highestSelectionValue2=double.MinValue;
           //  bool canSpawnInX2=false;
           //  bool canSpawnInZ2=false;
           //  int getCoordsOutputArraySize2=PhysUtil.GetCoordsInsideBoundsMinArraySize(bounds1,modifiers1.scale,margin1);
           //  if(getCoordsOutputArraySize2>getCoordsOutputArray2.Length){
           //   Array.Resize(ref getCoordsOutputArray2,getCoordsOutputArraySize2);
           //  }
           //  int length2=PhysUtil.GetCoordsInsideBoundsUsingParallelFor(bounds1,modifiers1.scale,Quaternion.AngleAxis(modifiers1.rotation,Vector3.up),margin1,
           //   true,getCoordsOutputArray2
           //  );
           //  for(int i2=0;i2<length2;++i2){
           //   Vector3Int coord2=getCoordsOutputArray2[i2];
           //   if(
           //    coord2.x==0&&
           //    coord2.z==0
           //   ){
           //    continue;
           //   }
           //   //Log.DebugMessage("coord2:"+coord2,container.cnkRgn.x==0&&container.cnkRgn.y==0);
           //   Vector3 pos2=pos1;
           //   pos2.x+=coord2.x;
           //   pos2.z+=coord2.z;
           //   Vector3Int vCoord2=vecPosTovCoord(pos2,out Vector2Int cnkRgn2);
           //   Vector3Int noiseInput2=vCoord2;noiseInput2.x+=cnkRgn2.x;
           //                                  noiseInput2.z+=cnkRgn2.y;
           //   (Type simObject,SimObjectSettings simObjectSettings)?simObjectPicked2=VoxelSystem.biome.biomeSpawnSettings.TryGetSettingsToSpawnSimObject(noiseInput2,out double selectionValue2);
           //   if(simObjectPicked2!=null){
           //    //  can we still spawn simObjectPicked1 if there's simObjectPicked2?
           //    Vector3 margin2=Vector3.one;
           //    SimObjectSettings simObjectSettings2=simObjectPicked2.Value.simObjectSettings;
           //    Vector3 size2=simObjectSettings2.size;
           //    Bounds bounds2=new Bounds(Vector3.zero,size2);
           //    Bounds maxScaledBounds2=new Bounds(Vector3.zero,Vector3.Scale(size2,simObjectSettings2.maxScale));
           //    int seqResult2a=MathUtil.AlternatingSequenceWithSeparator((int)pos2.x,container.maxSpawnSize.x,0);
           //    int seqResult2b=MathUtil.AlternatingSequenceWithSeparator((int)pos2.z,container.maxSpawnSize.z,0);
           //    bool priorityOverEast2 =seqResult2a==0;
           //    bool priorityOverWest2 =seqResult2a==1;
           //    bool priorityOverBothX2=seqResult2a==2;
           //    bool priorityOverNorth2=seqResult2b==0;
           //    bool priorityOverSouth2=seqResult2b==1;
           //    bool priorityOverBothZ2=seqResult2b==2;
           //    if(!priorityOverBothX2){
           //     if(coord2.x<0){
           //      if(priorityOverWest1){
           //       if(priorityOverWest2){
           //        canSpawnInX2=true;
           //       }
           //      }
           //     }
           //     if(coord2.x>0){
           //      if(priorityOverWest2){
           //       canSpawnInX2=false;
           //       goto _End2;
           //      }
           //     }
           //    }
           //    if(!priorityOverBothZ2){
           //     if(coord2.z<0){
           //      if(priorityOverSouth1){
           //       if(priorityOverSouth2){
           //        if(canSpawnInX2){
           //         canSpawnInZ2=true;
           //        }
           //       }
           //      }
           //     }
           //     if(coord2.z>0){
           //      if(priorityOverSouth2){
           //       canSpawnInZ2=false;
           //       goto _End2;
           //      }
           //     }
           //    }
           //   }
           //   //if(simObjectPicked2!=null){
           //   // if(selectionValue2>highestSelectionValue2){
           //   //  highestSelectionValue2=selectionValue2;
           //   // }
           //   //}
           //   //(Type simObject,SimObjectSettings simObjectSettings)?simObjectPicked2=null;
           //   //double selectionValue2=0;
           //   //if(!priorityOverBothX2){
           //   // if(coord2.x<0){
           //   //  if(priorityOverWest1){
           //   //   if(priorityOverWest2){
           //   //    if(Conflicts2b()){
           //   //     canSpawnInX2=false;
           //   //     goto _End2;
           //   //    }
           //   //    canSpawnInX2=true;
           //   //   }
           //   //  }
           //   // }
           //   // if(coord2.x>0){
           //   //  if(priorityOverWest2){
           //   //   if(Conflicts2a()){
           //   //    canSpawnInX2=false;
           //   //    goto _End2;
           //   //   }
           //   //  }
           //   // }
           //   //}
           //   //if(!priorityOverBothZ2){
           //   // if(coord2.z<0){
           //   //  if(priorityOverSouth1){
           //   //   if(priorityOverSouth2){
           //   //    if(canSpawnInX2){
           //   //     if(Conflicts2b()){
           //   //      canSpawnInZ2=false;
           //   //      goto _End2;
           //   //     }
           //   //     canSpawnInZ2=true;
           //   //    }
           //   //   }
           //   //  }
           //   // }
           //   // if(coord2.z>0){
           //   //  if(priorityOverSouth2){
           //   //   if(Conflicts2a()){
           //   //    canSpawnInZ2=false;
           //   //    goto _End2;
           //   //   }
           //   //  }
           //   // }
           //   //}
           //   //bool Conflicts2a(){
           //   // PickSimObject2();
           //   // if(simObjectPicked2!=null){
           //   //  return true;
           //   // }
           //   // return false;
           //   //}
           //   //bool Conflicts2b(){
           //   // //PickSimObject2();
           //   // if(simObjectPicked2!=null){
           //   //  if(selectionValue2>=selectionValue1){
           //   //   //return true;
           //   //  }
           //   // }
           //   // return false;
           //   //}
           //   //void PickSimObject2(){
           //   // if(simObjectPicked2==null){
           //   //  simObjectPicked2=VoxelSystem.biome.biomeSpawnSettings.TryGetSettingsToSpawnSimObject(noiseInput2,out selectionValue2);
           //   // }
           //   //}
           //  }
           //  _End2:{}
           //  //if(selectionValue1>highestSelectionValue2){
           //  // Log.DebugMessage("at pos1:"+pos1+":cnkRgn1:"+cnkRgn1+":spawn object:"+simObjectPicked1.Value.simObject);
           //  //}
           //  if(canSpawnInX2&&canSpawnInZ2){
           //   bool canSpawnInX3=false;
           //   bool canSpawnInZ3=false;
           //   Vector3 maxSpawnMargin=Vector3.one;
           //   Vector3 maxSpawnSize=container.maxSpawnSize;
           //   Bounds maxSpawnBounds=new Bounds(Vector3.zero,maxSpawnSize);
           //   int getCoordsOutputArraySize3=PhysUtil.GetCoordsInsideBoundsMinArraySize(maxSpawnBounds,Vector3.one,maxSpawnMargin);
           //   if(getCoordsOutputArraySize3>getCoordsOutputArray3.Length){
           //    Array.Resize(ref getCoordsOutputArray3,getCoordsOutputArraySize3);
           //   }
           //   getCoordsOutputHashSet2.Clear();
           //   getCoordsOutputHashSet2.UnionWith(getCoordsOutputArray2);
           //   int length3=PhysUtil.GetCoordsInsideBoundsUsingParallelFor(maxSpawnBounds,Vector3.one,Quaternion.identity,maxSpawnMargin,
           //    false,getCoordsOutputArray3,getCoordsOutputHashSet2
           //   );
           //   for(int i3=0;i3<length3;++i3){
           //    Vector3Int coord3=getCoordsOutputArray3[i3];
           //    Vector3 pos3=pos1;
           //    pos3.x+=coord3.x;
           //    pos3.z+=coord3.z;
           //    Vector3Int vCoord3=vecPosTovCoord(pos3,out Vector2Int cnkRgn3);
           //    Vector3Int noiseInput3=vCoord3;noiseInput3.x+=cnkRgn3.x;
           //                                   noiseInput3.z+=cnkRgn3.y;
           //    (Type simObject,SimObjectSettings simObjectSettings)?simObjectPicked3=VoxelSystem.biome.biomeSpawnSettings.TryGetSettingsToSpawnSimObject(noiseInput3,out double selectionValue3);
           //    SimObjectSpawnModifiers?modifiers3=null;
           //    if(simObjectPicked3!=null){
           //     //  can we still spawn simObjectPicked1 if there's simObjectPicked3?
           //     Vector3 margin3=Vector3.one;
           //     SimObjectSettings simObjectSettings3=simObjectPicked3.Value.simObjectSettings;
           //     Vector3 size3=simObjectSettings3.size;
           //     Bounds bounds3=new Bounds(Vector3.zero,size3);
           //     Bounds maxScaledBounds3=new Bounds(Vector3.zero,Vector3.Scale(size3,simObjectSettings3.maxScale));
           //     int seqResult3a=MathUtil.AlternatingSequenceWithSeparator((int)pos3.x,container.maxSpawnSize.x,0);
           //     int seqResult3b=MathUtil.AlternatingSequenceWithSeparator((int)pos3.z,container.maxSpawnSize.z,0);
           //     bool priorityOverEast3 =seqResult3a==0;
           //     bool priorityOverWest3 =seqResult3a==1;
           //     bool priorityOverBothX3=seqResult3a==2;
           //     bool priorityOverNorth3=seqResult3b==0;
           //     bool priorityOverSouth3=seqResult3b==1;
           //     bool priorityOverBothZ3=seqResult3b==2;
           //     if(!priorityOverBothX3){
           //      if(coord3.x<0){
           //       if(priorityOverWest1){
           //        if(priorityOverWest3){
           //         canSpawnInX3=true;
           //        }
           //       }
           //      }
           //      if(coord3.x>0){
           //       if(priorityOverWest3){
           //        if(Conflicts3a()){
           //         canSpawnInX3=false;
           //         goto _End3;
           //        }
           //       }
           //      }
           //     }
           //     if(!priorityOverBothZ3){
           //      if(coord3.z<0){
           //       if(priorityOverSouth1){
           //        if(priorityOverSouth3){
           //         if(canSpawnInX3){
           //          canSpawnInZ3=true;
           //         }
           //        }
           //       }
           //      }
           //      if(coord3.z>0){
           //       if(priorityOverSouth3){
           //        if(Conflicts3a()){
           //         canSpawnInZ3=false;
           //         goto _End3;
           //        }
           //       }
           //      }
           //     }
           //     bool Conflicts3a(){
           //      Bounds bounds1a=bounds1;bounds1a.center=pos1;
           //      Bounds bounds3a=bounds3;bounds3a.center=pos3;
           //      if(modifiers3==null){modifiers3=VoxelSystem.biome.biomeSpawnSettings.GetSimObjectSpawnModifiers(noiseInput3,simObjectPicked3.Value.simObjectSettings);}
           //      return false;
           //      return PhysUtil.BoundsIntersectsScaledRotated(
           //       bounds1a,Quaternion.AngleAxis(modifiers1      .rotation,Vector3.up),modifiers1      .scale,
           //       bounds3a,Quaternion.AngleAxis(modifiers3.Value.rotation,Vector3.up),modifiers3.Value.scale
           //      );
           //     }
           //    }
           //   }
           //   _End3:{}
           //   if(canSpawnInX3&&canSpawnInZ3){
           //    Log.DebugMessage("at pos1:"+pos1+":cnkRgn1:"+cnkRgn1+":spawn object:"+simObjectPicked1.Value.simObject);
           //    Vector3 scaledSize1=Vector3.Scale(size1,modifiers1.scale);
           //    if(
           //     scaledSize1.x+margin1.x>=Width||
           //     scaledSize1.z+margin1.z>=Depth
           //    ){
           //     goto _End1;
           //    }
           //   }
           //  }
           // }
           //}}
           //_End1:{}
           ////Vector3Int vCoord1=new Vector3Int(0,Height/2-1,0);
           ////for(vCoord1.x=0             ;vCoord1.x<Width;vCoord1.x++){
           ////for(vCoord1.z=0             ;vCoord1.z<Depth;vCoord1.z++){
           //// bool canSpawn=TryReserveBoundsAt(vCoord1,container.cnkRgn,out(Type simObject,SimObjectSettings simObjectSettings)?simObjectPicked1,out double selectionValue1);
           //// if(canSpawn){
           ////  //Log.DebugMessage("at vCoord1:"+vCoord1+":container.cnkRgn:"+container.cnkRgn+":spawn object:"+simObjectPicked1.Value.simObject);
           //// }
           ////}}
           ////Vector2Int chunkCenter=;
           ////Vector3Int reserveMaxSize=container.maxSpawnSize;
           ////Vector3Int reserveMaxRadius=reserveMaxSize/2;
           ////Vector3Int vCoord1=new Vector3Int(0,Height/2-1,0);
           ////for(vCoord1.x=0             ;vCoord1.x<Width;vCoord1.x++){
           ////for(vCoord1.z=0             ;vCoord1.z<Depth;vCoord1.z++){
           //// Vector3Int noiseInput1=vCoord1;noiseInput1.x+=container.cnkRgn.x;
           ////                                noiseInput1.z+=container.cnkRgn.y;
           //// (Type simObject,SimObjectSettings simObjectSettings)?simObjectPicked1=VoxelSystem.biome.biomeSpawnSettings.TryGetSettingsToSpawnSimObject(noiseInput1,out double selectionValue1);
           //// if(simObjectPicked1!=null){
           ////  Log.DebugMessage("noiseInput1:"+noiseInput1+":tentar dar spawn de:"+simObjectPicked1.Value.simObject,container.cnkRgn.x==0&&container.cnkRgn.y==0);
           ////  bool canSpawn=true;
           ////  Vector3 pos1=noiseInput1;
           ////  //  criar bounds para teste
           ////  SimObjectSettings simObjectSettings1=simObjectPicked1.Value.simObjectSettings;
           ////  Vector3 size1=simObjectSettings1.size;
           ////  //  TO DO: aplicar escala
           ////  Bounds bounds1=new Bounds(pos1,size1);
           ////  Log.DebugMessage("bounds1:"+bounds1,container.cnkRgn.x==0&&container.cnkRgn.y==0);
           ////  bool priorityOverWest1 =MathUtil.AlternatingSequence(Mathf.FloorToInt(pos1.x),container.maxSpawnSize.x,0)==0;
           ////  bool priorityOverSouth1=MathUtil.AlternatingSequence(Mathf.FloorToInt(pos1.z),container.maxSpawnSize.z,0)==0;
           ////  Vector3Int coord2=new Vector3Int(0,Height/2-1,0);
           ////  for(coord2.x=-Mathf.FloorToInt(size1.x);coord2.x<=Mathf.CeilToInt(size1.x);++coord2.x){
           ////  for(coord2.z=-Mathf.FloorToInt(size1.z);coord2.z<=Mathf.CeilToInt(size1.z);++coord2.z){
           ////   if(
           ////    coord2.x==0&&
           ////    coord2.z==0
           ////   ){
           ////    continue;
           ////   }
           ////   if(priorityOverWest1){
           ////    if(coord2.x<0){
           ////     if(priorityOverSouth1){
           ////      if(coord2.z<0){
           ////       continue;
           ////      }
           ////     }else{
           ////      if(coord2.z>0){
           ////       continue;
           ////      }
           ////     }
           ////    }
           ////   }else{
           ////    if(coord2.x>0){
           ////     if(priorityOverSouth1){
           ////      if(coord2.z<0){
           ////       continue;
           ////      }
           ////     }else{
           ////      if(coord2.z>0){
           ////       continue;
           ////      }
           ////     }
           ////    }
           ////   }
           ////   Vector3 pos2=pos1+coord2;
           ////   Vector3Int vCoord2=vecPosTovCoord(pos2,out Vector2Int cnkRgn2);
           ////   Vector3Int noiseInput2=vCoord2;noiseInput2.x+=cnkRgn2.x;
           ////                                  noiseInput2.z+=cnkRgn2.y;
           ////   (Type simObject,SimObjectSettings simObjectSettings)?simObjectPicked2=VoxelSystem.biome.biomeSpawnSettings.TryGetSettingsToSpawnSimObject(noiseInput2,out double selectionValue2);
           ////   if(simObjectPicked2!=null){
           ////    bool priorityOverWest2 =MathUtil.AlternatingSequence(Mathf.FloorToInt(pos2.x),container.maxSpawnSize.x,0)==0;
           ////    bool priorityOverSouth2=MathUtil.AlternatingSequence(Mathf.FloorToInt(pos2.z),container.maxSpawnSize.z,0)==0;
           ////    if(priorityOverWest2){
           ////     if(coord2.x>0){
           ////      canSpawn=false;
           ////      goto _SpawnResult2;
           ////     }
           ////    }else{
           ////     if(coord2.x<0){
           ////      canSpawn=false;
           ////      goto _SpawnResult2;
           ////     }
           ////    }
           ////    if(priorityOverSouth2){
           ////     if(coord2.z>0){
           ////      canSpawn=false;
           ////      goto _SpawnResult2;
           ////     }
           ////    }else{
           ////     if(coord2.z<0){
           ////      canSpawn=false;
           ////      goto _SpawnResult2;
           ////     }
           ////    }
           ////   }
           ////  }}
           ////  _SpawnResult2:{}
           ////  //Log.DebugMessage("canSpawn:"+canSpawn,container.cnkRgn.x==0&&container.cnkRgn.y==0);
           ////  //Log.DebugMessage("canSpawn:"+canSpawn,canSpawn);
           ////  if(canSpawn){
           ////   Vector3Int coord3=new Vector3Int(0,Height/2-1,0);
           ////   foreach(var coord in 
           ////    MathUtil.GetCoords(
           ////     minX:-Mathf.FloorToInt(size1.x)-reserveMaxRadius.x  ,
           ////     maxX: Mathf.FloorToInt(size1.x)+reserveMaxRadius.x-1,
           ////     minZ:-Mathf.FloorToInt(size1.z)-reserveMaxRadius.z  ,
           ////     maxZ: Mathf.FloorToInt(size1.z)+reserveMaxRadius.z-1,
           ////     innerMinX:-Mathf.FloorToInt(size1.x),
           ////     innerMaxX: Mathf.FloorToInt(size1.x),
           ////     innerMinZ:-Mathf.FloorToInt(size1.z),
           ////     innerMaxZ: Mathf.FloorToInt(size1.z)
           ////    )
           ////   ){
           ////    coord3.x=coord.x;
           ////    coord3.z=coord.z;
           ////    if(
           ////     coord3.x==0&&
           ////     coord3.z==0
           ////    ){
           ////     continue;
           ////    }
           ////    //Log.DebugMessage("coord3:"+coord3,canSpawn);
           ////    Vector3 pos3=pos1+coord3;
           ////    Vector3Int vCoord3=vecPosTovCoord(pos3,out Vector2Int cnkRgn3);
           ////    Vector3Int noiseInput3=vCoord3;noiseInput3.x+=cnkRgn3.x;
           ////                                   noiseInput3.z+=cnkRgn3.y;
           ////    bool priorityOverWest3 =MathUtil.AlternatingSequence(Mathf.FloorToInt(pos3.x),container.maxSpawnSize.x,0)==0;
           ////    bool priorityOverSouth3=MathUtil.AlternatingSequence(Mathf.FloorToInt(pos3.z),container.maxSpawnSize.z,0)==0;
           ////    if(priorityOverWest3){
           ////     if(coord3.x<0){
           ////      continue;
           ////     }
           ////    }else{
           ////     if(coord3.x>0){
           ////      continue;
           ////     }
           ////    }
           ////    if(priorityOverSouth3){
           ////     if(coord3.z<0){
           ////      continue;
           ////     }
           ////    }else{
           ////     if(coord3.z>0){
           ////      continue;
           ////     }
           ////    }
           ////    (Type simObject,SimObjectSettings simObjectSettings)?simObjectPicked3=VoxelSystem.biome.biomeSpawnSettings.TryGetSettingsToSpawnSimObject(noiseInput3,out double selectionValue3);
           ////    if(simObjectPicked3!=null){
           ////     SimObjectSettings simObjectSettings3=simObjectPicked3.Value.simObjectSettings;
           ////     Vector3 size3=simObjectSettings3.size;
           ////     //  TO DO: aplicar escala
           ////     Bounds bounds3=new Bounds(pos3,size3);
           ////     if(bounds3.Intersects(bounds1)){
           ////      canSpawn=false;
           ////      goto _SpawnResult3;
           ////     }
           ////    }
           ////   }
           ////  }
           ////  _SpawnResult3:{}
           ////  //Log.DebugMessage("canSpawn:"+canSpawn,canSpawn);
           ////  Log.DebugMessage("at pos:"+pos1+":cnkRgn:"+container.cnkRgn+":spawn object:"+simObjectPicked1.Value.simObject,canSpawn);
           //// }
           ////}}
           ////  primeiro checar proximidades dentro de bounds
           ////Vector3Int reservationCheckRadius=reservationMaxRadius+(reservationMaxSize/2);
           ////Log.DebugMessage("centerOfThisChunk:"+centerOfThisChunk+";reservationCheckRadius:"+reservationCheckRadius);
           //////  first check if anything would be spawned by this chunks:
           ////Vector3Int vCoord1=new Vector3Int(0,Height/2-1,0);
           ////for(vCoord1.x=0             ;vCoord1.x<Width;vCoord1.x++){
           ////for(vCoord1.z=0             ;vCoord1.z<Depth;vCoord1.z++){
           //// Vector3Int noiseInput=vCoord1;noiseInput.x+=centerOfThisChunk.x;
           ////                               noiseInput.z+=centerOfThisChunk.y;
           //// Log.DebugMessage("vCoord1:"+vCoord1,centerOfThisChunk.x==0&&centerOfThisChunk.y==0);
           //// (Type simObject,SimObjectSettings simObjectSettings)?simObjectPicked=VoxelSystem.biome.biomeSpawnSettings.TryGetSettingsToSpawnSimObject(noiseInput,out double selectionValue);
           //// if(simObjectPicked!=null){
           ////  //  algo terá seu spawn aqui, então iniciar checagem de bloqueios:
           ////  Log.DebugMessage("selectionValue:"+selectionValue+";try to reserve bounds for:simObjectPicked.Value.simObject:"+simObjectPicked.Value.simObject,centerOfThisChunk.x==0&&centerOfThisChunk.y==0);
           ////  Vector3Int simObjectOrigin=vCoord1;
           ////  simObjectOrigin.x+=centerOfThisChunk.x;
           ////  simObjectOrigin.z+=centerOfThisChunk.y;
           ////  CheckForBlockage(simObjectOrigin);
           //// }
           ////}}
           ////void CheckForBlockage(Vector3Int simObjectOrigin){
           //// Log.DebugMessage("CheckForBlockage:simObjectOrigin:"+simObjectOrigin,centerOfThisChunk.x==0&&centerOfThisChunk.y==0);
           //// for(int x=-reservationCheckRadius.x;x<=0;++x){
           //// for(int z=-reservationCheckRadius.z;z<=0;++z){
           ////  Vector3 pos=new Vector3Int(
           ////   x+simObjectOrigin.x,
           ////     simObjectOrigin.y,
           ////   z+simObjectOrigin.z
           ////  );
           ////  Vector3Int vCoord=vecPosTovCoord(pos,out Vector2Int cnkRgn);
           ////  Vector3Int noiseInput=vCoord;noiseInput.x+=cnkRgn.x;
           ////                               noiseInput.z+=cnkRgn.y;
           ////  Vector2Int cCoord=cnkRgnTocCoord(cnkRgn);
           ////  int cnkIdx=GetcnkIdx(cCoord.x,cCoord.y);
           ////  //  criar ou carregar mapa de spawn.
           ////  if(!spawnMaps.TryGetValue(cnkIdx,out SpawnMapInfo[]spawnMap)){
           ////   VoxelSystem.Concurrent.spawnMaps_rwl.EnterUpgradeableReadLock();
           ////   try{
           ////    if(!VoxelSystem.Concurrent.spawnMaps.TryGetValue(cnkIdx,out spawnMap)){
           ////     VoxelSystem.Concurrent.spawnMaps_rwl.EnterWriteLock();
           ////     try{
           ////      if(VoxelSystem.Concurrent.spawnMaps.TryAdd(cnkIdx,spawnMap)){
           ////       spawnMap=VoxelSystem.Concurrent.spawnMaps[cnkIdx]=new SpawnMapInfo[VoxelsPerChunk];
           ////      }
           ////     }catch{
           ////      throw;
           ////     }finally{
           ////      VoxelSystem.Concurrent.spawnMaps_rwl.ExitWriteLock();
           ////     }
           ////    }
           ////   }catch{
           ////    throw;
           ////   }finally{
           ////    VoxelSystem.Concurrent.spawnMaps_rwl.ExitUpgradeableReadLock();
           ////   }
           ////   spawnMaps.Add(cnkIdx,spawnMap);
           ////  }
           ////  //  primeiro, veja se a área já possui um bloqueio no centro do objeto.
           ////  int vxlIdx=GetvxlIdx(vCoord.x,vCoord.y,vCoord.z);
           ////  SpawnMapInfo spawnMapInfo=spawnMap[vxlIdx];
           ////  //Log.DebugMessage("spawnMapInfo.isBlocked:"+spawnMapInfo.isBlocked,centerOfThisChunk.x==0&&centerOfThisChunk.y==0);
           ////  //VoxelSystem.Concurrent.spawnMaps_rwl.EnterReadLock();
           ////  //try{
           ////  //}catch{
           ////  // throw;
           ////  //}finally{
           ////  // VoxelSystem.Concurrent.spawnMaps_rwl.ExitReadLock();
           ////  //}
           ////  //Log.DebugMessage("vCoord:"+vCoord,centerOfThisChunk.x==0&&centerOfThisChunk.y==0);
           ////  (Type simObject,SimObjectSettings simObjectSettings)?simObjectPicked=VoxelSystem.biome.biomeSpawnSettings.TryGetSettingsToSpawnSimObject(noiseInput,out double selectionValue);
           ////  if(simObjectPicked!=null){
           ////   Log.DebugMessage("selectionValue:"+selectionValue+";try to reserve bounds for:simObjectPicked.Value.simObject:"+simObjectPicked.Value.simObject,centerOfThisChunk.x==0&&centerOfThisChunk.y==0);
           ////  }
           //// for(int y=-reservationMaxRadius.y;y<=reservationMaxRadius.y;++y){
           ////  pos.y=y;
           ////  //vCoord.y=y;
           ////  //vCoord.y+=Mathf.FloorToInt(Height/2.0f);vCoord.y=Mathf.Clamp(vCoord.y,0,Height-1);
           //// // if(VoxelTerrainSurfaceSimObjectsPlacerMultithreaded.Stopped){
           //// //  goto _Break;
           //// // }
           //// // Vector3Int vCoord=vecPosTovCoord(pos);
           //// // //if(){
           ////  //Log.DebugMessage("vCoord:"+vCoord,centerOfThisChunk.x==0&&centerOfThisChunk.y==0);
           //// // //}
           //// //}}}
           //// }}}
           ////}
           break;
          }
          case Execution.FillSpawnData:{
           //Log.DebugMessage("Execution.FillSpawnData");
           //Vector3Int vCoord1=new Vector3Int(0,Height/2-1,0);
           //for(vCoord1.x=0             ;vCoord1.x<Width;vCoord1.x++){
           //for(vCoord1.z=0             ;vCoord1.z<Depth;vCoord1.z++){
           // int index=vCoord1.z+vCoord1.x*Depth;
           // if(container.gotGroundHits[index]==null){
           //  continue;
           // }
           // Vector3Int noiseInput=vCoord1;noiseInput.x+=container.cnkRgn.x;
           //                               noiseInput.z+=container.cnkRgn.y;
           // (Type simObject,SimObjectSettings simObjectSettings)?simObjectPicked=VoxelSystem.biome.biomeSpawnSettings.TryGetSettingsToSpawnSimObject(noiseInput);
           // if(simObjectPicked!=null){
           //  SimObjectSpawnModifiers modifiers=VoxelSystem.biome.biomeSpawnSettings.GetSimObjectSpawnModifiers(noiseInput,simObjectPicked.Value.simObjectSettings);
           //  RaycastHit floor=container.gotGroundHits[index].Value;
           //  Quaternion rotation=Quaternion.SlerpUnclamped(
           //   Quaternion.identity,
           //   Quaternion.FromToRotation(
           //    Vector3.up,
           //    floor.normal
           //   ),
           //   simObjectPicked.Value.simObjectSettings.inclination
           //  )*Quaternion.Euler(
           //   new Vector3(0f,modifiers.rotation,0f)
           //  );
           //  Vector3 position=new Vector3(
           //   floor.point.x,
           //   floor.point.y-modifiers.scale.y*simObjectPicked.Value.simObjectSettings.depth,
           //   floor.point.z
           //  )+rotation*(Vector3.down*modifiers.scale.y);
           //  foreach(var spawnedTypeBlockedArrayPair in container.blocked){
           //   SpawnedTypes spawnType=spawnedTypeBlockedArrayPair.Key;
           //   bool[]blocked=spawnedTypeBlockedArrayPair.Value;
           //   if(!simObjectPicked.Value.simObjectSettings.minSpacing.ContainsKey(spawnType)){
           //    continue;
           //   }
           //   if(!simObjectPicked.Value.simObjectSettings.maxSpacing.ContainsKey(spawnType)){
           //    continue;
           //   }
           //   Vector3 minSpacing=simObjectPicked.Value.simObjectSettings.minSpacing[spawnType];
           //   minSpacing=Vector3.Scale(minSpacing,modifiers.scale);
           //   minSpacing.x=Mathf.Max(minSpacing.x,1f);
           //   minSpacing.y=Mathf.Max(minSpacing.y,1f);
           //   minSpacing.z=Mathf.Max(minSpacing.z,1f);
           //   Vector3 maxSpacing=simObjectPicked.Value.simObjectSettings.maxSpacing[spawnType];
           //   maxSpacing=Vector3.Scale(maxSpacing,modifiers.scale);
           //   maxSpacing.x=Mathf.Max(maxSpacing.x,1f);
           //   maxSpacing.y=Mathf.Max(maxSpacing.y,1f);
           //   maxSpacing.z=Mathf.Max(maxSpacing.z,1f);
           //   if(Width-1-vCoord1.x<=Mathf.CeilToInt(minSpacing.x)){
           //    goto _Continue;
           //   }
           //   if(vCoord1.x<=Mathf.CeilToInt(minSpacing.x)){
           //    goto _Continue;
           //   }
           //   if(Depth-1-vCoord1.z<=Mathf.CeilToInt(minSpacing.z)){
           //    goto _Continue;
           //   }
           //   if(vCoord1.z<=Mathf.CeilToInt(minSpacing.z)){
           //    goto _Continue;
           //   }
           //   for(int x2=-Mathf.CeilToInt(minSpacing.x);x2<=Mathf.CeilToInt(minSpacing.x);x2++){
           //   for(int z2=-Mathf.CeilToInt(minSpacing.z);z2<=Mathf.CeilToInt(minSpacing.z);z2++){
           //    Vector3Int vCoord2=vCoord1;
           //    vCoord2.x+=x2;
           //    vCoord2.z+=z2;
           //    int index2=vCoord2.z+vCoord2.x*Depth;
           //    if(blocked[index2]){
           //     goto _Continue;
           //    }
           //   }}
           //   for(int x2=-Mathf.CeilToInt(minSpacing.x);x2<Mathf.CeilToInt(minSpacing.x);x2++){
           //   for(int z2=-Mathf.CeilToInt(minSpacing.z);z2<Mathf.CeilToInt(minSpacing.z);z2++){
           //    Vector3Int vCoord2=vCoord1;
           //    vCoord2.x+=x2;
           //    vCoord2.z+=z2;
           //    if(vCoord2.x<0){
           //     continue;
           //    }
           //    if(vCoord2.x>=Width){
           //     continue;
           //    }
           //    if(vCoord2.z<0){
           //     continue;
           //    }
           //    if(vCoord2.z>=Depth){
           //     continue;
           //    }
           //    int index2=vCoord2.z+vCoord2.x*Depth;
           //    blocked[index2]=true;
           //   }}
           //  }
           //  Log.DebugMessage("set to be spawned:simObjectPicked.Value.simObject:"+simObjectPicked.Value.simObject);
           //  container.spawnData.at.Add((position,rotation.eulerAngles,modifiers.scale,simObjectPicked.Value.simObject,null,new SimObject.PersistentData()));
           // }
           // _Continue:{
           //  Log.Warning("TO DO: grandes objetos (maiores que um chunk) devem ser salvos aqui para serem aplicados em outros chunks, com cooldown também em conta");
           //  continue;
           // }
           //}}
           break;
          }
          case Execution.SaveStateToFile:{
           //Log.DebugMessage("Execution.SaveStateToFile");
           Log.Warning("TO DO: salvar um blocked para todos os chunks, salvando também o cooldown");
           lock(VoxelSystem.chunkStateFileSync){
            //bool stateSavedFlag=false;
            //stringBuilder.Clear();
            //chunkStateFileStream.Position=0L;
            //chunkStateFileStreamReader.DiscardBufferedData();
            //string line;
            //while((line=chunkStateFileStreamReader.ReadLine())!=null){
            // if(string.IsNullOrEmpty(line)){continue;}
            // int totalCharactersRemoved=0;
            // lineStringBuilder.Clear();
            // lineStringBuilder.Append(line);
            // int cnkIdxStringStart=line.IndexOf("cnkIdx=")+7;
            // int cnkIdxStringEnd  =line.IndexOf(" , ",cnkIdxStringStart);
            // int cnkIdxStringLength=cnkIdxStringEnd-cnkIdxStringStart;
            // int cnkIdx=int.Parse(line.Substring(cnkIdxStringStart,cnkIdxStringLength),NumberStyles.Any,CultureInfoUtil.en_US);
            // int surfaceSimObjectsAddedStringStart=cnkIdxStringEnd+2;
            // int endOfLineStart=surfaceSimObjectsAddedStringStart;
            // if(cnkIdx==container.cnkIdx){
            //  surfaceSimObjectsAddedStringStart=line.IndexOf("surfaceSimObjectsAdded=",surfaceSimObjectsAddedStringStart);
            //  //Log.DebugMessage("surfaceSimObjectsAddedStringStart:"+surfaceSimObjectsAddedStringStart);
            //  if(surfaceSimObjectsAddedStringStart>=0){
            //   //Log.DebugMessage("surfaceSimObjectsAdded flag is present");
            //   int surfaceSimObjectsAddedStringEnd=line.IndexOf(" , ",surfaceSimObjectsAddedStringStart)+3;
            //   string surfaceSimObjectsAddedString=line.Substring(surfaceSimObjectsAddedStringStart,surfaceSimObjectsAddedStringEnd-surfaceSimObjectsAddedStringStart);
            //   //Log.DebugMessage("surfaceSimObjectsAddedString:"+surfaceSimObjectsAddedString);
            //   int surfaceSimObjectsAddedFlagStringStart=surfaceSimObjectsAddedString.IndexOf("=")+1;
            //   int surfaceSimObjectsAddedFlagStringEnd  =surfaceSimObjectsAddedString.IndexOf(" , ",surfaceSimObjectsAddedFlagStringStart);
            //   bool surfaceSimObjectsAddedFlag=bool.Parse(surfaceSimObjectsAddedString.Substring(surfaceSimObjectsAddedFlagStringStart,surfaceSimObjectsAddedFlagStringEnd-surfaceSimObjectsAddedFlagStringStart));
            //   //Log.DebugMessage("surfaceSimObjectsAddedFlag:"+surfaceSimObjectsAddedFlag);
            //   if(!surfaceSimObjectsAddedFlag){
            //    int toRemoveLength=surfaceSimObjectsAddedStringEnd-totalCharactersRemoved-(surfaceSimObjectsAddedStringStart-totalCharactersRemoved);
            //    lineStringBuilder.Remove(surfaceSimObjectsAddedStringStart-totalCharactersRemoved,toRemoveLength);
            //    totalCharactersRemoved+=toRemoveLength;
            //   }else{
            //    stateSavedFlag=true;
            //   }
            //  }
            // }
            // endOfLineStart  =line.IndexOf("} } , endOfLine",endOfLineStart);
            // int endOfLineEnd=line.IndexOf(" , endOfLine",endOfLineStart)+12;
            // lineStringBuilder.Remove(endOfLineStart-totalCharactersRemoved,endOfLineEnd-totalCharactersRemoved-(endOfLineStart-totalCharactersRemoved));
            // line=lineStringBuilder.ToString();
            // stringBuilder.Append(line);
            // if(cnkIdx==container.cnkIdx){
            //  if(!stateSavedFlag){
            //   //Log.DebugMessage("add surfaceSimObjectsAdded flag");
            //   stringBuilder.AppendFormat(CultureInfoUtil.en_US,"surfaceSimObjectsAdded={0} , ",true);
            //   stateSavedFlag=true;
            //  }
            // }
            // stringBuilder.AppendFormat(CultureInfoUtil.en_US,"}} }} , endOfLine{0}",Environment.NewLine);
            //}
            //if(!stateSavedFlag){
            // stringBuilder.AppendFormat(CultureInfoUtil.en_US,"{{ cnkIdx={0} , {{ ",container.cnkIdx);
            // stringBuilder.AppendFormat(CultureInfoUtil.en_US,"surfaceSimObjectsAdded={0} , ",true);
            // stringBuilder.AppendFormat(CultureInfoUtil.en_US,"}} }} , endOfLine{0}",Environment.NewLine);
            // stateSavedFlag=true;
            //}
            //chunkStateFileStream.SetLength(0L);
            //chunkStateFileStreamWriter.Write(stringBuilder.ToString());
            //chunkStateFileStreamWriter.Flush();
           }
           break;
          }
         }
        }
    }
}