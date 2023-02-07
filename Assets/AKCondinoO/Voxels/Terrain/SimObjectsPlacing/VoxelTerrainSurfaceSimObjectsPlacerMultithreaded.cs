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
    internal class VoxelTerrainSurfaceSimObjectsPlacerContainer:BackgroundContainer{
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
        protected override void Execute(){
         switch(container.execution){
          case Execution.GetGround:{
           //Log.DebugMessage("Execution.GetGround");
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
           Vector3Int vCoord1=new Vector3Int(0,Height/2-1,0);
           for(vCoord1.x=0             ;vCoord1.x<Width;vCoord1.x++){
           for(vCoord1.z=0             ;vCoord1.z<Depth;vCoord1.z++){
            Vector3 from=vCoord1;
                    from.x+=container.cnkRgn.x-Width/2f+.5f;
                    from.z+=container.cnkRgn.y-Depth/2f+.5f;
            container.GetGroundRays.AddNoResize(new RaycastCommand(from,Vector3.down,Height,VoxelSystem.voxelTerrainLayer,1));
            container.GetGroundHits.AddNoResize(new RaycastHit    ()                                                        );
           }}
           break;
          }
          case Execution.FillSpawnData:{
           //Log.DebugMessage("Execution.FillSpawnData");
           Vector3Int vCoord1=new Vector3Int(0,Height/2-1,0);
           for(vCoord1.x=0             ;vCoord1.x<Width;vCoord1.x++){
           for(vCoord1.z=0             ;vCoord1.z<Depth;vCoord1.z++){
            int index=vCoord1.z+vCoord1.x*Depth;
            if(container.gotGroundHits[index]==null){
             continue;
            }
            Vector3Int noiseInput=vCoord1;noiseInput.x+=container.cnkRgn.x;
                                          noiseInput.z+=container.cnkRgn.y;
            (Type simObject,SimObjectSettings simObjectSettings)?simObjectPicked=VoxelSystem.biome.biomeSpawnSettings.TryGetSettingsToSpawnSimObject(noiseInput);
            if(simObjectPicked!=null){
             SimObjectSpawnModifiers modifiers=VoxelSystem.biome.biomeSpawnSettings.GetSimObjectSpawnModifiers(noiseInput,simObjectPicked.Value.simObjectSettings);
             RaycastHit floor=container.gotGroundHits[index].Value;
             Quaternion rotation=Quaternion.SlerpUnclamped(
              Quaternion.identity,
              Quaternion.FromToRotation(
               Vector3.up,
               floor.normal
              ),
              simObjectPicked.Value.simObjectSettings.inclination
             )*Quaternion.Euler(
              new Vector3(0f,modifiers.rotation,0f)
             );
             Vector3 position=new Vector3(
              floor.point.x,
              floor.point.y-modifiers.scale.y*simObjectPicked.Value.simObjectSettings.depth,
              floor.point.z
             )+rotation*(Vector3.down*modifiers.scale.y);
             foreach(var spawnedTypeBlockedArrayPair in container.blocked){
              SpawnedTypes spawnType=spawnedTypeBlockedArrayPair.Key;
              bool[]blocked=spawnedTypeBlockedArrayPair.Value;
              if(!simObjectPicked.Value.simObjectSettings.minSpacing.ContainsKey(spawnType)){
               continue;
              }
              if(!simObjectPicked.Value.simObjectSettings.maxSpacing.ContainsKey(spawnType)){
               continue;
              }
              Vector3 minSpacing=simObjectPicked.Value.simObjectSettings.minSpacing[spawnType];
              minSpacing=Vector3.Scale(minSpacing,modifiers.scale);
              minSpacing.x=Mathf.Max(minSpacing.x,1f);
              minSpacing.y=Mathf.Max(minSpacing.y,1f);
              minSpacing.z=Mathf.Max(minSpacing.z,1f);
              Vector3 maxSpacing=simObjectPicked.Value.simObjectSettings.maxSpacing[spawnType];
              maxSpacing=Vector3.Scale(maxSpacing,modifiers.scale);
              maxSpacing.x=Mathf.Max(maxSpacing.x,1f);
              maxSpacing.y=Mathf.Max(maxSpacing.y,1f);
              maxSpacing.z=Mathf.Max(maxSpacing.z,1f);
              if(Width-1-vCoord1.x<=Mathf.CeilToInt(minSpacing.x)){
               goto _Continue;
              }
              if(vCoord1.x<=Mathf.CeilToInt(minSpacing.x)){
               goto _Continue;
              }
              if(Depth-1-vCoord1.z<=Mathf.CeilToInt(minSpacing.z)){
               goto _Continue;
              }
              if(vCoord1.z<=Mathf.CeilToInt(minSpacing.z)){
               goto _Continue;
              }
              for(int x2=-Mathf.CeilToInt(minSpacing.x);x2<=Mathf.CeilToInt(minSpacing.x);x2++){
              for(int z2=-Mathf.CeilToInt(minSpacing.z);z2<=Mathf.CeilToInt(minSpacing.z);z2++){
               Vector3Int vCoord2=vCoord1;
               vCoord2.x+=x2;
               vCoord2.z+=z2;
               int index2=vCoord2.z+vCoord2.x*Depth;
               if(blocked[index2]){
                goto _Continue;
               }
              }}
              for(int x2=-Mathf.CeilToInt(minSpacing.x);x2<Mathf.CeilToInt(minSpacing.x);x2++){
              for(int z2=-Mathf.CeilToInt(minSpacing.z);z2<Mathf.CeilToInt(minSpacing.z);z2++){
               Vector3Int vCoord2=vCoord1;
               vCoord2.x+=x2;
               vCoord2.z+=z2;
               if(vCoord2.x<0){
                continue;
               }
               if(vCoord2.x>=Width){
                continue;
               }
               if(vCoord2.z<0){
                continue;
               }
               if(vCoord2.z>=Depth){
                continue;
               }
               int index2=vCoord2.z+vCoord2.x*Depth;
               blocked[index2]=true;
              }}
             }
             container.spawnData.at.Add((position,rotation.eulerAngles,modifiers.scale,simObjectPicked.Value.simObject,null,new SimObject.PersistentData()));
            }
            _Continue:{
             continue;
            }
           }}
           break;
          }
          case Execution.SaveStateToFile:{
           //Log.DebugMessage("Execution.SaveStateToFile");
           lock(VoxelSystem.chunkStateFileSync){
            bool stateSavedFlag=false;
            stringBuilder.Clear();
            chunkStateFileStream.Position=0L;
            chunkStateFileStreamReader.DiscardBufferedData();
            string line;
            while((line=chunkStateFileStreamReader.ReadLine())!=null){
             if(string.IsNullOrEmpty(line)){continue;}
             int totalCharactersRemoved=0;
             lineStringBuilder.Clear();
             lineStringBuilder.Append(line);
             int cnkIdxStringStart=line.IndexOf("cnkIdx=")+7;
             int cnkIdxStringEnd  =line.IndexOf(" , ",cnkIdxStringStart);
             int cnkIdxStringLength=cnkIdxStringEnd-cnkIdxStringStart;
             int cnkIdx=int.Parse(line.Substring(cnkIdxStringStart,cnkIdxStringLength),NumberStyles.Any,CultureInfoUtil.en_US);
             int surfaceSimObjectsAddedStringStart=cnkIdxStringEnd+2;
             int endOfLineStart=surfaceSimObjectsAddedStringStart;
             if(cnkIdx==container.cnkIdx){
              surfaceSimObjectsAddedStringStart=line.IndexOf("surfaceSimObjectsAdded=",surfaceSimObjectsAddedStringStart);
              //Log.DebugMessage("surfaceSimObjectsAddedStringStart:"+surfaceSimObjectsAddedStringStart);
              if(surfaceSimObjectsAddedStringStart>=0){
               //Log.DebugMessage("surfaceSimObjectsAdded flag is present");
               int surfaceSimObjectsAddedStringEnd=line.IndexOf(" , ",surfaceSimObjectsAddedStringStart)+3;
               string surfaceSimObjectsAddedString=line.Substring(surfaceSimObjectsAddedStringStart,surfaceSimObjectsAddedStringEnd-surfaceSimObjectsAddedStringStart);
               //Log.DebugMessage("surfaceSimObjectsAddedString:"+surfaceSimObjectsAddedString);
               int surfaceSimObjectsAddedFlagStringStart=surfaceSimObjectsAddedString.IndexOf("=")+1;
               int surfaceSimObjectsAddedFlagStringEnd  =surfaceSimObjectsAddedString.IndexOf(" , ",surfaceSimObjectsAddedFlagStringStart);
               bool surfaceSimObjectsAddedFlag=bool.Parse(surfaceSimObjectsAddedString.Substring(surfaceSimObjectsAddedFlagStringStart,surfaceSimObjectsAddedFlagStringEnd-surfaceSimObjectsAddedFlagStringStart));
               //Log.DebugMessage("surfaceSimObjectsAddedFlag:"+surfaceSimObjectsAddedFlag);
               if(!surfaceSimObjectsAddedFlag){
                int toRemoveLength=surfaceSimObjectsAddedStringEnd-totalCharactersRemoved-(surfaceSimObjectsAddedStringStart-totalCharactersRemoved);
                lineStringBuilder.Remove(surfaceSimObjectsAddedStringStart-totalCharactersRemoved,toRemoveLength);
                totalCharactersRemoved+=toRemoveLength;
               }else{
                stateSavedFlag=true;
               }
              }
             }
             endOfLineStart  =line.IndexOf("} } , endOfLine",endOfLineStart);
             int endOfLineEnd=line.IndexOf(" , endOfLine",endOfLineStart)+12;
             lineStringBuilder.Remove(endOfLineStart-totalCharactersRemoved,endOfLineEnd-totalCharactersRemoved-(endOfLineStart-totalCharactersRemoved));
             line=lineStringBuilder.ToString();
             stringBuilder.Append(line);
             if(cnkIdx==container.cnkIdx){
              if(!stateSavedFlag){
               //Log.DebugMessage("add surfaceSimObjectsAdded flag");
               stringBuilder.AppendFormat(CultureInfoUtil.en_US,"surfaceSimObjectsAdded={0} , ",true);
               stateSavedFlag=true;
              }
             }
             stringBuilder.AppendFormat(CultureInfoUtil.en_US,"}} }} , endOfLine{0}",Environment.NewLine);
            }
            if(!stateSavedFlag){
             stringBuilder.AppendFormat(CultureInfoUtil.en_US,"{{ cnkIdx={0} , {{ ",container.cnkIdx);
             stringBuilder.AppendFormat(CultureInfoUtil.en_US,"surfaceSimObjectsAdded={0} , ",true);
             stringBuilder.AppendFormat(CultureInfoUtil.en_US,"}} }} , endOfLine{0}",Environment.NewLine);
             stateSavedFlag=true;
            }
            chunkStateFileStream.SetLength(0L);
            chunkStateFileStreamWriter.Write(stringBuilder.ToString());
            chunkStateFileStreamWriter.Flush();
           }
           break;
          }
         }
        }
    }
}