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

namespace AKCondinoO.Voxels.Terrain.SimObjectsPlacing{
    internal struct SpawnMapInfo{
     internal bool isBlocked;
     internal SpawnedTypes type;
     internal Vector3Int origin;
     internal Bounds bounds;
    }
    internal class VoxelTerrainSurfaceSimObjectsPlacerContainer:BackgroundContainer{
     internal readonly Color[]testArray=new Color[FlattenOffset];
     internal Vector3 maxSpawnSize;
     internal Vector3 margin;
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
        void ResolveSpawnConflict(
         Vector3 size1,int priority1,
         Vector3 size2,int priority2,
         Vector3Int coord,
         bool priorityOverWest1 ,bool priorityOverEast1 ,bool priorityOverBothX1,bool priorityOverSouth1,bool priorityOverNorth1,bool priorityOverBothZ1,
         bool priorityOverWest2 ,bool priorityOverEast2 ,bool priorityOverBothX2,bool priorityOverSouth2,bool priorityOverNorth2,bool priorityOverBothZ2,
         ref bool canSpawnInX,
         ref bool canSpawnInZ
        ){
         //  TO DO: adicionar prioridade por valor ou tamanho opcional ao encontrar priority over both
         //  TO DO: e prioridade para sul e oeste no empate priority over both
         if(!priorityOverBothX1){
          if(coord.x<0){
           if(priorityOverBothX2){
            canSpawnInX=false;
            return;
           }
           if(!priorityOverWest1){
            canSpawnInX=false;
            return;
           }
          }
          if(coord.x>0){
           if(priorityOverBothX2){
            canSpawnInX=false;
            return;
           }
           if(!priorityOverEast1){
            canSpawnInX=false;
            return;
           }
          }
          if(coord.x==0){
           if(!priorityOverBothZ1){
            if(coord.z<0){
             if(priorityOverBothZ2){
              canSpawnInZ=false;
              return;
             }
             if(!priorityOverSouth1){
              canSpawnInZ=false;
              return;
             }
            }
            if(coord.z>0){
             if(priorityOverBothZ2){
              canSpawnInZ=false;
              return;
             }
             if(!priorityOverNorth1){
              canSpawnInZ=false;
              return;
             }
            }
           }else{
            if(priorityOverBothZ2){//  empate
             if(priority2>priority1){
              canSpawnInZ=false;
              return;
             }else{
              if(size2.z>=size1.z){
               canSpawnInZ=false;
               return;
              }
             }
            }
           }
          }
         }else{
          if(coord.x!=0){
           if(priorityOverBothX2){//  empate
            if(priority2>priority1){
             canSpawnInX=false;
             return;
            }else{
             if(size2.x>=size1.x){
              canSpawnInX=false;
              return;
             }
            }
           }
          }
          if(coord.x==0){
           if(!priorityOverBothZ1){
            if(coord.z<0){
             if(priorityOverBothZ2){
              canSpawnInZ=false;
              return;
             }
             if(!priorityOverSouth1){
              canSpawnInZ=false;
              return;
             }
            }
            if(coord.z>0){
             if(priorityOverBothZ2){
              canSpawnInZ=false;
              return;
             }
             if(!priorityOverNorth1){
              canSpawnInZ=false;
              return;
             }
            }
           }else{
            if(priorityOverBothZ2){//  empate
             if(priority2>priority1){
              canSpawnInZ=false;
              return;
             }else{
              if(size2.z>=size1.z){
               canSpawnInZ=false;
               return;
              }
             }
            }
           }
          }
         }
        }
     Vector3Int[]getCoordsOutputArray2=new Vector3Int[0];
     readonly HashSet<Vector3Int>getCoordsOutputHashSet2=new();
     Vector3Int[]getCoordsOutputArray3=new Vector3Int[0];
        void GetCoords(
         SimObjectSpawnModifiers modifiers,Bounds bounds,Quaternion rotation,
         ref Vector3Int[]getCoordsOutputArray,out int length
        ){
         Vector3 margin=container.margin;
         int getCoordsOutputArraySize=PhysUtil.GetCoordsInsideBoundsMinArraySize(bounds,modifiers.scale,margin);
         if(getCoordsOutputArraySize>getCoordsOutputArray.Length){
          Array.Resize(ref getCoordsOutputArray,getCoordsOutputArraySize);
         }
         length=PhysUtil.GetCoordsInsideBoundsUsingParallelFor(bounds,modifiers.scale,rotation,margin,
          false,getCoordsOutputArray
         );
        }
        bool GetSimObjectSettings(Vector3 pos,Vector3Int noiseInput,
         out(Type simObject,SimObjectSettings simObjectSettings)?simObjectPicked,
         out SimObjectSettings simObjectSettings,out SimObjectSpawnModifiers modifiers,
         out Vector3 size,out Bounds bounds,out int priority,
         out Quaternion rotation,
         out bool priorityOverWest ,out bool priorityOverEast ,out bool priorityOverBothX,
         out bool priorityOverSouth,out bool priorityOverNorth,out bool priorityOverBothZ,
         out double selectionValue
        ){
         //  TO DO: rotação levando em conta o solo
         simObjectPicked=VoxelSystem.biome.biomeSpawnSettings.TryGetSettingsToSpawnSimObject(noiseInput,out selectionValue);
         if(simObjectPicked==null){
          simObjectSettings=default;modifiers=default;
          size=default;bounds=default;priority=default;
          rotation=default;
          priorityOverWest =default;priorityOverEast =default;priorityOverBothX=default;
          priorityOverSouth=default;priorityOverNorth=default;priorityOverBothZ=default;
          return false;
         }
         simObjectSettings=simObjectPicked.Value.simObjectSettings;
         modifiers=VoxelSystem.biome.biomeSpawnSettings.GetSimObjectSpawnModifiers(noiseInput,simObjectSettings);
         size=simObjectSettings.size;
         bounds=new Bounds(Vector3.zero,size);
         priority=simObjectSettings.priority;
         rotation=Quaternion.AngleAxis(modifiers.rotation,Vector3.up);
         Vector3 margin=container.margin;
         Bounds maxScaledBounds=new Bounds(Vector3.zero,Vector3.Scale(size,simObjectSettings.maxScale));
         Vector3 maxScaledExtents=maxScaledBounds.extents;
         int seqResultX=MathUtil.AlternatingSequenceWithSeparator((int)pos.x,(Mathf.CeilToInt(maxScaledExtents.x)+Mathf.CeilToInt(margin.x))*2,0);
         int seqResultZ=MathUtil.AlternatingSequenceWithSeparator((int)pos.z,(Mathf.CeilToInt(maxScaledExtents.z)+Mathf.CeilToInt(margin.z))*2,0);
         priorityOverWest =seqResultX==0;
         priorityOverEast =seqResultX==1;
         priorityOverBothX=seqResultX==2;
         priorityOverSouth=seqResultZ==0;
         priorityOverNorth=seqResultZ==1;
         priorityOverBothZ=seqResultZ==2;
         return true;
        }
     readonly System.Diagnostics.Stopwatch sw=new System.Diagnostics.Stopwatch();
        protected override void Execute(){
         switch(container.execution){
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
          case Execution.ReserveBounds:{
           Log.DebugMessage("ReserveBounds");
           sw.Restart();
           Vector3 margin=container.margin;
           Vector2Int cnkRgn1=container.cnkRgn;
           Vector3Int vCoord1=new Vector3Int(0,Height/2-1,0);
           for(vCoord1.x=0;vCoord1.x<Width;vCoord1.x++){
           for(vCoord1.z=0;vCoord1.z<Depth;vCoord1.z++){
            int index1=vCoord1.z+vCoord1.x*Depth;
            container.testArray[index1]=new Color(0,0,0,0);
            Vector3Int pos1=vCoord1;
            pos1.x+=cnkRgn1.x;
            pos1.z+=cnkRgn1.y;
            Vector3Int noiseInput1=vCoord1;noiseInput1.x+=cnkRgn1.x;
                                           noiseInput1.z+=cnkRgn1.y;
            if(
             GetSimObjectSettings(pos1,noiseInput1,
              out(Type simObject,SimObjectSettings simObjectSettings)?simObjectPicked1,
              out SimObjectSettings simObjectSettings1,out SimObjectSpawnModifiers modifiers1,
              out Vector3 size1,out Bounds bounds1,out int priority1,
              out Quaternion rotation1,
              out bool priorityOverWest1 ,out bool priorityOverEast1 ,out bool priorityOverBothX1,
              out bool priorityOverSouth1,out bool priorityOverNorth1,out bool priorityOverBothZ1,
              out double selectionValue1
             )
            ){
             container.testArray[index1]=Color.gray;
             bool canSpawnInX2=true;
             bool canSpawnInZ2=true;
             GetCoords(
              modifiers1,bounds1,rotation1,
              ref getCoordsOutputArray2,out int length2
             );
             var parallelForResult2=Parallel.For(0,length2,(i2,parallelForState2)=>{
              Vector3Int coord2=getCoordsOutputArray2[i2];
              if(
              coord2.x==0&&
              coord2.z==0
              ){
               return;
              }
              Vector3 pos2=pos1;
              pos2.x+=coord2.x;
              pos2.z+=coord2.z;
              Vector3Int vCoord2=vecPosTovCoord(pos2,out Vector2Int cnkRgn2);
              Vector3Int noiseInput2=vCoord2;noiseInput2.x+=cnkRgn2.x;
                                             noiseInput2.z+=cnkRgn2.y;
              if(
               GetSimObjectSettings(pos2,noiseInput2,
                out(Type simObject,SimObjectSettings simObjectSettings)?simObjectPicked2,
                out SimObjectSettings simObjectSettings2,out SimObjectSpawnModifiers modifiers2,
                out Vector3 size2,out Bounds bounds2,out int priority2,
                out Quaternion rotation2,
                out bool priorityOverWest2 ,out bool priorityOverEast2 ,out bool priorityOverBothX2,
                out bool priorityOverSouth2,out bool priorityOverNorth2,out bool priorityOverBothZ2,
                out double selectionValue2
               )
              ){//  can we still spawn simObjectPicked1 if there's a simObjectPicked2 here?
               ResolveSpawnConflict(
                size1,priority1,
                size2,priority2,
                coord2,
                priorityOverWest1 ,priorityOverEast1 ,priorityOverBothX1,priorityOverSouth1,priorityOverNorth1,priorityOverBothZ1,
                priorityOverWest2 ,priorityOverEast2 ,priorityOverBothX2,priorityOverSouth2,priorityOverNorth2,priorityOverBothZ2,
                ref canSpawnInX2,
                ref canSpawnInZ2
               );
               if(!canSpawnInX2||!canSpawnInZ2){
                parallelForState2.Stop();
                return;
               }
              }
             });
             if(canSpawnInX2&&canSpawnInZ2){
              container.testArray[index1]=Color.green;
              if(!priorityOverBothX1){
               if(!priorityOverBothZ1){
                container.testArray[index1]=Color.cyan;
                Vector3 pos3=pos1;
                pos3.z+=1;
                Vector3Int vCoord3=vecPosTovCoord(pos3,out Vector2Int cnkRgn3);
                Vector3Int noiseInput3=vCoord3;noiseInput3.x+=cnkRgn3.x;
                                               noiseInput3.z+=cnkRgn3.y;
                Log.DebugMessage("got a normal spawn:pos1:"+pos1+":pos3:"+pos3+":vCoord1:"+vCoord1+":vCoord3:"+vCoord3+":cnkRgn1:"+cnkRgn1+":cnkRgn3:"+cnkRgn3);
                //(Type simObject,SimObjectSettings simObjectSettings)?simObjectPicked3=VoxelSystem.biome.biomeSpawnSettings.TryGetSettingsToSpawnSimObject(noiseInput3,out double selectionValue3);
                //if(simObjectPicked3!=null){
                // Log.Error("got a normal spawn...WHAT?!");
                //}
               }
              }
             }else{
             }
             if(priorityOverSouth1){
              //container.testArray[index1]=Color.red;
             }else{
              if(priorityOverNorth1){
               //container.testArray[index1]=Color.yellow;
              }
             }
            }
           }}
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
           //   int getCoordsOutputArraySize3=PhysUtil.GetCoordsInsideBoundsMinArraySize(boundsMax,Vector3.one,marginMax);
           //   if(getCoordsOutputArraySize3>getCoordsOutputArray3.Length){
           //    Array.Resize(ref getCoordsOutputArray3,getCoordsOutputArraySize3);
           //   }
           //   getCoordsOutputHashSet2.Clear();
           //   getCoordsOutputHashSet2.UnionWith(getCoordsOutputArray2);
           //   int length3=PhysUtil.GetCoordsInsideBoundsUsingParallelFor(boundsMax,Vector3.one,Quaternion.identity,marginMax,
           //    false,getCoordsOutputArray3,getCoordsOutputHashSet2
           //   );
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