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

namespace AKCondinoO.Voxels.Terrain.SimObjectsPlacing{
    internal struct SpawnMapInfo{
     internal bool isBlocked;
     internal SpawnedTypes type;
     internal Vector3Int origin;
     internal Bounds bounds;
    }
    internal class VoxelTerrainSurfaceSimObjectsPlacerContainer:BackgroundContainer{
     internal Vector3Int maxSpawnSize;
     internal Vector2Int cCoord;
     internal Vector2Int cnkRgn;
     internal        int cnkIdx;
     internal bool surfaceSimObjectsHadBeenAdded;
     internal NativeList<RaycastCommand>GetGroundRays;
     internal NativeList<RaycastHit    >GetGroundHits;
     internal readonly Dictionary<int,RaycastHit?>gotGroundHits=new Dictionary<int,RaycastHit?>(Width*Depth);
     internal readonly Dictionary<SpawnedTypes,bool[]>blocked=new Dictionary<SpawnedTypes,bool[]>();
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
        bool TryReserveBoundsAt(Vector3Int vCoord1,Vector2Int cnkRgn1,out(Type simObject,SimObjectSettings simObjectSettings)?simObjectPicked1,out double selectionValue1){
         bool canSpawn=true;
         Vector3Int noiseInput1=vCoord1;noiseInput1.x+=cnkRgn1.x;
                                        noiseInput1.z+=cnkRgn1.y;
         simObjectPicked1=VoxelSystem.biome.biomeSpawnSettings.TryGetSettingsToSpawnSimObject(noiseInput1,out selectionValue1);
         if(simObjectPicked1==null){
          return false;
         }
         SimObjectSettings simObjectSettings1=simObjectPicked1.Value.simObjectSettings;
         Vector3 size1=simObjectSettings1.size;
         bool priorityOverWest1 =MathUtil.AlternatingSequence(vCoord1.x+cnkRgn1.x,container.maxSpawnSize.x,0)==0;
         bool priorityOverEast1 =!priorityOverWest1 ;
         bool priorityOverSouth1=MathUtil.AlternatingSequence(vCoord1.z+cnkRgn1.y,container.maxSpawnSize.z,0)==0;
         bool priorityOverNorth1=!priorityOverSouth1;
         Vector3Int coord2=new Vector3Int(0,Height/2-1,0);
         for(coord2.x=Mathf.FloorToInt(-size1.x);coord2.x<Mathf.CeilToInt(size1.x);coord2.x++){
         for(coord2.z=Mathf.FloorToInt(-size1.z);coord2.z<Mathf.CeilToInt(size1.z);coord2.z++){
          if(
           coord2.x==0&&
           coord2.z==0
          ){
           continue;
          }
          Vector3 pos2=noiseInput1+coord2;
          Vector3Int vCoord2=vecPosTovCoord(pos2,out Vector2Int cnkRgn2);
          bool priorityOverWest2 =MathUtil.AlternatingSequence(vCoord2.x+cnkRgn2.x,container.maxSpawnSize.x,0)==0;
          bool priorityOverEast2 =!priorityOverWest2 ;
          bool priorityOverSouth2=MathUtil.AlternatingSequence(vCoord2.z+cnkRgn2.y,container.maxSpawnSize.z,0)==0;
          bool priorityOverNorth2=!priorityOverSouth2;
          if(
           Conflicts(coord2,
            priorityOverWest1,priorityOverEast1,
            priorityOverWest2,priorityOverEast2
           )
          ){
           canSpawn=false;
           goto _Break;
          }
         }}
         _Break:{}
         return canSpawn;
        }
        bool Conflicts(Vector3Int coord2,
         bool priorityOverWest1,bool priorityOverEast1,
         bool priorityOverWest2,bool priorityOverEast2
        ){
         if(coord2.x<0){
          if(priorityOverWest1){
           return true;
          }
         }
         //if(coord2.x>0){
         // if(priorityOverEast1){
         //  if(priorityOverWest2){
         //   return false;
         //  }
         //  return true;
         // }
         //}
         return false;
         //if(priorityOverWest1&&coord2.x<0)return true;//  objeto vence / candidato perde
         //if(priorityOverEast1&&coord2.x>0)return true;//  objeto vence / candidato perde
         ////  Caso especial: prioridades opostas, permitir que um lado sempre vença
         //if(priorityOverEast1&&priorityOverWest2)return true;//  empate -> objeto vence / candidato perde
         //return false;//  objeto perde / candidato vence
        }
        protected override void Execute(){
         switch(container.execution){
          case Execution.ReserveBounds:{
           Log.DebugMessage("ReserveBounds");
           Vector3Int vCoord1=new Vector3Int(0,Height/2-1,0);
           for(vCoord1.x=0             ;vCoord1.x<Width;vCoord1.x++){
           for(vCoord1.z=0             ;vCoord1.z<Depth;vCoord1.z++){
            bool canSpawn=TryReserveBoundsAt(vCoord1,container.cnkRgn,out(Type simObject,SimObjectSettings simObjectSettings)?simObjectPicked1,out double selectionValue1);
            if(canSpawn){
             //Log.DebugMessage("at vCoord1:"+vCoord1+":container.cnkRgn:"+container.cnkRgn+":spawn object:"+simObjectPicked1.Value.simObject);
            }
           }}
           //Vector2Int chunkCenter=;
           //Vector3Int reserveMaxSize=container.maxSpawnSize;
           //Vector3Int reserveMaxRadius=reserveMaxSize/2;
           //Vector3Int vCoord1=new Vector3Int(0,Height/2-1,0);
           //for(vCoord1.x=0             ;vCoord1.x<Width;vCoord1.x++){
           //for(vCoord1.z=0             ;vCoord1.z<Depth;vCoord1.z++){
           // Vector3Int noiseInput1=vCoord1;noiseInput1.x+=container.cnkRgn.x;
           //                                noiseInput1.z+=container.cnkRgn.y;
           // (Type simObject,SimObjectSettings simObjectSettings)?simObjectPicked1=VoxelSystem.biome.biomeSpawnSettings.TryGetSettingsToSpawnSimObject(noiseInput1,out double selectionValue1);
           // if(simObjectPicked1!=null){
           //  Log.DebugMessage("noiseInput1:"+noiseInput1+":tentar dar spawn de:"+simObjectPicked1.Value.simObject,container.cnkRgn.x==0&&container.cnkRgn.y==0);
           //  bool canSpawn=true;
           //  Vector3 pos1=noiseInput1;
           //  //  criar bounds para teste
           //  SimObjectSettings simObjectSettings1=simObjectPicked1.Value.simObjectSettings;
           //  Vector3 size1=simObjectSettings1.size;
           //  //  TO DO: aplicar escala
           //  Bounds bounds1=new Bounds(pos1,size1);
           //  Log.DebugMessage("bounds1:"+bounds1,container.cnkRgn.x==0&&container.cnkRgn.y==0);
           //  bool priorityOverWest1 =MathUtil.AlternatingSequence(Mathf.FloorToInt(pos1.x),container.maxSpawnSize.x,0)==0;
           //  bool priorityOverSouth1=MathUtil.AlternatingSequence(Mathf.FloorToInt(pos1.z),container.maxSpawnSize.z,0)==0;
           //  Vector3Int coord2=new Vector3Int(0,Height/2-1,0);
           //  for(coord2.x=-Mathf.FloorToInt(size1.x);coord2.x<=Mathf.CeilToInt(size1.x);++coord2.x){
           //  for(coord2.z=-Mathf.FloorToInt(size1.z);coord2.z<=Mathf.CeilToInt(size1.z);++coord2.z){
           //   if(
           //    coord2.x==0&&
           //    coord2.z==0
           //   ){
           //    continue;
           //   }
           //   if(priorityOverWest1){
           //    if(coord2.x<0){
           //     if(priorityOverSouth1){
           //      if(coord2.z<0){
           //       continue;
           //      }
           //     }else{
           //      if(coord2.z>0){
           //       continue;
           //      }
           //     }
           //    }
           //   }else{
           //    if(coord2.x>0){
           //     if(priorityOverSouth1){
           //      if(coord2.z<0){
           //       continue;
           //      }
           //     }else{
           //      if(coord2.z>0){
           //       continue;
           //      }
           //     }
           //    }
           //   }
           //   Vector3 pos2=pos1+coord2;
           //   Vector3Int vCoord2=vecPosTovCoord(pos2,out Vector2Int cnkRgn2);
           //   Vector3Int noiseInput2=vCoord2;noiseInput2.x+=cnkRgn2.x;
           //                                  noiseInput2.z+=cnkRgn2.y;
           //   (Type simObject,SimObjectSettings simObjectSettings)?simObjectPicked2=VoxelSystem.biome.biomeSpawnSettings.TryGetSettingsToSpawnSimObject(noiseInput2,out double selectionValue2);
           //   if(simObjectPicked2!=null){
           //    bool priorityOverWest2 =MathUtil.AlternatingSequence(Mathf.FloorToInt(pos2.x),container.maxSpawnSize.x,0)==0;
           //    bool priorityOverSouth2=MathUtil.AlternatingSequence(Mathf.FloorToInt(pos2.z),container.maxSpawnSize.z,0)==0;
           //    if(priorityOverWest2){
           //     if(coord2.x>0){
           //      canSpawn=false;
           //      goto _SpawnResult2;
           //     }
           //    }else{
           //     if(coord2.x<0){
           //      canSpawn=false;
           //      goto _SpawnResult2;
           //     }
           //    }
           //    if(priorityOverSouth2){
           //     if(coord2.z>0){
           //      canSpawn=false;
           //      goto _SpawnResult2;
           //     }
           //    }else{
           //     if(coord2.z<0){
           //      canSpawn=false;
           //      goto _SpawnResult2;
           //     }
           //    }
           //   }
           //  }}
           //  _SpawnResult2:{}
           //  //Log.DebugMessage("canSpawn:"+canSpawn,container.cnkRgn.x==0&&container.cnkRgn.y==0);
           //  //Log.DebugMessage("canSpawn:"+canSpawn,canSpawn);
           //  if(canSpawn){
           //   Vector3Int coord3=new Vector3Int(0,Height/2-1,0);
           //   foreach(var coord in 
           //    MathUtil.GetCoords(
           //     minX:-Mathf.FloorToInt(size1.x)-reserveMaxRadius.x  ,
           //     maxX: Mathf.FloorToInt(size1.x)+reserveMaxRadius.x-1,
           //     minZ:-Mathf.FloorToInt(size1.z)-reserveMaxRadius.z  ,
           //     maxZ: Mathf.FloorToInt(size1.z)+reserveMaxRadius.z-1,
           //     innerMinX:-Mathf.FloorToInt(size1.x),
           //     innerMaxX: Mathf.FloorToInt(size1.x),
           //     innerMinZ:-Mathf.FloorToInt(size1.z),
           //     innerMaxZ: Mathf.FloorToInt(size1.z)
           //    )
           //   ){
           //    coord3.x=coord.x;
           //    coord3.z=coord.z;
           //    if(
           //     coord3.x==0&&
           //     coord3.z==0
           //    ){
           //     continue;
           //    }
           //    //Log.DebugMessage("coord3:"+coord3,canSpawn);
           //    Vector3 pos3=pos1+coord3;
           //    Vector3Int vCoord3=vecPosTovCoord(pos3,out Vector2Int cnkRgn3);
           //    Vector3Int noiseInput3=vCoord3;noiseInput3.x+=cnkRgn3.x;
           //                                   noiseInput3.z+=cnkRgn3.y;
           //    bool priorityOverWest3 =MathUtil.AlternatingSequence(Mathf.FloorToInt(pos3.x),container.maxSpawnSize.x,0)==0;
           //    bool priorityOverSouth3=MathUtil.AlternatingSequence(Mathf.FloorToInt(pos3.z),container.maxSpawnSize.z,0)==0;
           //    if(priorityOverWest3){
           //     if(coord3.x<0){
           //      continue;
           //     }
           //    }else{
           //     if(coord3.x>0){
           //      continue;
           //     }
           //    }
           //    if(priorityOverSouth3){
           //     if(coord3.z<0){
           //      continue;
           //     }
           //    }else{
           //     if(coord3.z>0){
           //      continue;
           //     }
           //    }
           //    (Type simObject,SimObjectSettings simObjectSettings)?simObjectPicked3=VoxelSystem.biome.biomeSpawnSettings.TryGetSettingsToSpawnSimObject(noiseInput3,out double selectionValue3);
           //    if(simObjectPicked3!=null){
           //     SimObjectSettings simObjectSettings3=simObjectPicked3.Value.simObjectSettings;
           //     Vector3 size3=simObjectSettings3.size;
           //     //  TO DO: aplicar escala
           //     Bounds bounds3=new Bounds(pos3,size3);
           //     if(bounds3.Intersects(bounds1)){
           //      canSpawn=false;
           //      goto _SpawnResult3;
           //     }
           //    }
           //   }
           //  }
           //  _SpawnResult3:{}
           //  //Log.DebugMessage("canSpawn:"+canSpawn,canSpawn);
           //  Log.DebugMessage("at pos:"+pos1+":cnkRgn:"+container.cnkRgn+":spawn object:"+simObjectPicked1.Value.simObject,canSpawn);
           // }
           //}}
           //  primeiro checar proximidades dentro de bounds
           //Vector3Int reservationCheckRadius=reservationMaxRadius+(reservationMaxSize/2);
           //Log.DebugMessage("centerOfThisChunk:"+centerOfThisChunk+";reservationCheckRadius:"+reservationCheckRadius);
           ////  first check if anything would be spawned by this chunks:
           //Vector3Int vCoord1=new Vector3Int(0,Height/2-1,0);
           //for(vCoord1.x=0             ;vCoord1.x<Width;vCoord1.x++){
           //for(vCoord1.z=0             ;vCoord1.z<Depth;vCoord1.z++){
           // Vector3Int noiseInput=vCoord1;noiseInput.x+=centerOfThisChunk.x;
           //                               noiseInput.z+=centerOfThisChunk.y;
           // Log.DebugMessage("vCoord1:"+vCoord1,centerOfThisChunk.x==0&&centerOfThisChunk.y==0);
           // (Type simObject,SimObjectSettings simObjectSettings)?simObjectPicked=VoxelSystem.biome.biomeSpawnSettings.TryGetSettingsToSpawnSimObject(noiseInput,out double selectionValue);
           // if(simObjectPicked!=null){
           //  //  algo terá seu spawn aqui, então iniciar checagem de bloqueios:
           //  Log.DebugMessage("selectionValue:"+selectionValue+";try to reserve bounds for:simObjectPicked.Value.simObject:"+simObjectPicked.Value.simObject,centerOfThisChunk.x==0&&centerOfThisChunk.y==0);
           //  Vector3Int simObjectOrigin=vCoord1;
           //  simObjectOrigin.x+=centerOfThisChunk.x;
           //  simObjectOrigin.z+=centerOfThisChunk.y;
           //  CheckForBlockage(simObjectOrigin);
           // }
           //}}
           //void CheckForBlockage(Vector3Int simObjectOrigin){
           // Log.DebugMessage("CheckForBlockage:simObjectOrigin:"+simObjectOrigin,centerOfThisChunk.x==0&&centerOfThisChunk.y==0);
           // for(int x=-reservationCheckRadius.x;x<=0;++x){
           // for(int z=-reservationCheckRadius.z;z<=0;++z){
           //  Vector3 pos=new Vector3Int(
           //   x+simObjectOrigin.x,
           //     simObjectOrigin.y,
           //   z+simObjectOrigin.z
           //  );
           //  Vector3Int vCoord=vecPosTovCoord(pos,out Vector2Int cnkRgn);
           //  Vector3Int noiseInput=vCoord;noiseInput.x+=cnkRgn.x;
           //                               noiseInput.z+=cnkRgn.y;
           //  Vector2Int cCoord=cnkRgnTocCoord(cnkRgn);
           //  int cnkIdx=GetcnkIdx(cCoord.x,cCoord.y);
           //  //  criar ou carregar mapa de spawn.
           //  if(!spawnMaps.TryGetValue(cnkIdx,out SpawnMapInfo[]spawnMap)){
           //   VoxelSystem.Concurrent.spawnMaps_rwl.EnterUpgradeableReadLock();
           //   try{
           //    if(!VoxelSystem.Concurrent.spawnMaps.TryGetValue(cnkIdx,out spawnMap)){
           //     VoxelSystem.Concurrent.spawnMaps_rwl.EnterWriteLock();
           //     try{
           //      if(VoxelSystem.Concurrent.spawnMaps.TryAdd(cnkIdx,spawnMap)){
           //       spawnMap=VoxelSystem.Concurrent.spawnMaps[cnkIdx]=new SpawnMapInfo[VoxelsPerChunk];
           //      }
           //     }catch{
           //      throw;
           //     }finally{
           //      VoxelSystem.Concurrent.spawnMaps_rwl.ExitWriteLock();
           //     }
           //    }
           //   }catch{
           //    throw;
           //   }finally{
           //    VoxelSystem.Concurrent.spawnMaps_rwl.ExitUpgradeableReadLock();
           //   }
           //   spawnMaps.Add(cnkIdx,spawnMap);
           //  }
           //  //  primeiro, veja se a área já possui um bloqueio no centro do objeto.
           //  int vxlIdx=GetvxlIdx(vCoord.x,vCoord.y,vCoord.z);
           //  SpawnMapInfo spawnMapInfo=spawnMap[vxlIdx];
           //  //Log.DebugMessage("spawnMapInfo.isBlocked:"+spawnMapInfo.isBlocked,centerOfThisChunk.x==0&&centerOfThisChunk.y==0);
           //  //VoxelSystem.Concurrent.spawnMaps_rwl.EnterReadLock();
           //  //try{
           //  //}catch{
           //  // throw;
           //  //}finally{
           //  // VoxelSystem.Concurrent.spawnMaps_rwl.ExitReadLock();
           //  //}
           //  //Log.DebugMessage("vCoord:"+vCoord,centerOfThisChunk.x==0&&centerOfThisChunk.y==0);
           //  (Type simObject,SimObjectSettings simObjectSettings)?simObjectPicked=VoxelSystem.biome.biomeSpawnSettings.TryGetSettingsToSpawnSimObject(noiseInput,out double selectionValue);
           //  if(simObjectPicked!=null){
           //   Log.DebugMessage("selectionValue:"+selectionValue+";try to reserve bounds for:simObjectPicked.Value.simObject:"+simObjectPicked.Value.simObject,centerOfThisChunk.x==0&&centerOfThisChunk.y==0);
           //  }
           // for(int y=-reservationMaxRadius.y;y<=reservationMaxRadius.y;++y){
           //  pos.y=y;
           //  //vCoord.y=y;
           //  //vCoord.y+=Mathf.FloorToInt(Height/2.0f);vCoord.y=Mathf.Clamp(vCoord.y,0,Height-1);
           // // if(VoxelTerrainSurfaceSimObjectsPlacerMultithreaded.Stopped){
           // //  goto _Break;
           // // }
           // // Vector3Int vCoord=vecPosTovCoord(pos);
           // // //if(){
           //  //Log.DebugMessage("vCoord:"+vCoord,centerOfThisChunk.x==0&&centerOfThisChunk.y==0);
           // // //}
           // //}}}
           // }}}
           //}
           break;
          }
          case Execution.GetGround:{
           //Log.DebugMessage("Execution.GetGround");
           //foreach(var spawnedTypeBlockedArrayPair in container.blocked){
           // Array.Clear(spawnedTypeBlockedArrayPair.Value,0,spawnedTypeBlockedArrayPair.Value.Length);
           //}
           //lock(VoxelSystem.chunkStateFileSync){
           // chunkStateFileStream.Position=0L;
           // chunkStateFileStreamReader.DiscardBufferedData();
           // string line;
           // while((line=chunkStateFileStreamReader.ReadLine())!=null){
           //  if(string.IsNullOrEmpty(line)){continue;}
           //  int cnkIdxStringStart=line.IndexOf("cnkIdx=")+7;
           //  int cnkIdxStringEnd  =line.IndexOf(" , ",cnkIdxStringStart);
           //  int cnkIdxStringLength=cnkIdxStringEnd-cnkIdxStringStart;
           //  int cnkIdx=int.Parse(line.Substring(cnkIdxStringStart,cnkIdxStringLength),NumberStyles.Any,CultureInfoUtil.en_US);
           //  int surfaceSimObjectsAddedStringStart=cnkIdxStringEnd+2;
           //  if(cnkIdx==container.cnkIdx){
           //   surfaceSimObjectsAddedStringStart=line.IndexOf("surfaceSimObjectsAdded=",surfaceSimObjectsAddedStringStart);
           //   if(surfaceSimObjectsAddedStringStart>=0){
           //    int surfaceSimObjectsAddedStringEnd=line.IndexOf(" , ",surfaceSimObjectsAddedStringStart)+3;
           //    string surfaceSimObjectsAddedString=line.Substring(surfaceSimObjectsAddedStringStart,surfaceSimObjectsAddedStringEnd-surfaceSimObjectsAddedStringStart);
           //    int surfaceSimObjectsAddedFlagStringStart=surfaceSimObjectsAddedString.IndexOf("=")+1;
           //    int surfaceSimObjectsAddedFlagStringEnd  =surfaceSimObjectsAddedString.IndexOf(" , ",surfaceSimObjectsAddedFlagStringStart);
           //    bool surfaceSimObjectsAddedFlag=bool.Parse(surfaceSimObjectsAddedString.Substring(surfaceSimObjectsAddedFlagStringStart,surfaceSimObjectsAddedFlagStringEnd-surfaceSimObjectsAddedFlagStringStart));
           //    if(surfaceSimObjectsAddedFlag){
           //     container.surfaceSimObjectsHadBeenAdded=true;
           //    }
           //   }
           //  }
           // }
           //}
           //if(container.surfaceSimObjectsHadBeenAdded){
           // break;
           //}
           //QueryParameters queryParameters=new QueryParameters(VoxelSystem.voxelTerrainLayer);
           //Vector3Int vCoord1=new Vector3Int(0,Height/2-1,0);
           //for(vCoord1.x=0             ;vCoord1.x<Width;vCoord1.x++){
           //for(vCoord1.z=0             ;vCoord1.z<Depth;vCoord1.z++){
           // Vector3 from=vCoord1;
           //         from.x+=container.cnkRgn.x-Width/2f+.5f;
           //         from.z+=container.cnkRgn.y-Depth/2f+.5f;
           // container.GetGroundRays.AddNoResize(new RaycastCommand(from,Vector3.down,queryParameters,Height));
           // container.GetGroundHits.AddNoResize(new RaycastHit    ()                                        );
           //}}
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