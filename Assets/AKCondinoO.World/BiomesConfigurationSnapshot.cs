using AKCondinoO.Bootstrap;
using AKCondinoO.SimObjects;
using AKCondinoO.Utilities;
using AKCondinoO.World.Biomes;
using AKCondinoO.World.MarchingCubes;
using AKCondinoO.World.Spawning;
using LibNoise;
using LibNoise.Operator;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Unity.Mathematics;
using UnityEngine;
using static AKCondinoO.World.BiomesConfigurationSnapshot;
using static AKCondinoO.World.Spawning.ByChanceObjectSpawnEntry<AKCondinoO.SimObjects.SimObject>;
using static AKCondinoO.World.WorldChunkManagerConst;
namespace AKCondinoO.World{
    internal enum NoiseChannel{
     None=-1,
     TerrainHeight=0,
     TerrainSurfaceMaterial=1,
     TerrainSurfaceSpawn=2,
     TerrainSurfaceRotation,
     TerrainSurfaceScale,
     Temperature,
    }
    internal class BiomesConfigurationContext{
     internal static readonly Utilities.ObjectPool<BiomesConfigurationContext>pool=
      Pool.GetPool<BiomesConfigurationContext>(
       "",
       ()=>new(),
       (BiomesConfigurationContext item)=>{
        Pool.ReturnArray<bool  >(item.hasTerrainHeightNoiseCache,true);item.hasTerrainHeightNoiseCache=null;
        Pool.ReturnArray<double>(item.   terrainHeightNoiseCache,true);item.   terrainHeightNoiseCache=null;
       }
      );
     internal int width;
     internal int height;
     internal int depth;
     internal Vector3Int coord;
     internal Vector3Int terrainHeightNoiseCachePadding;
     internal bool[]hasTerrainHeightNoiseCache;
     internal double[]terrainHeightNoiseCache;
        internal int TerrainHeightNoiseCacheIndex(){
         return(coord.z+terrainHeightNoiseCachePadding.z)+(coord.x+terrainHeightNoiseCachePadding.x)*depth;
        }
        internal void UpdateTerrainHeightNoiseCache(double noiseValue){
         int index=TerrainHeightNoiseCacheIndex();
         terrainHeightNoiseCache[index]=noiseValue;
         hasTerrainHeightNoiseCache[index]=true;
        }
        internal bool TryUseTerrainHeightNoiseCache(out double noiseValue){
         noiseValue=-1d;
         int index=TerrainHeightNoiseCacheIndex();
         if(hasTerrainHeightNoiseCache[index]){
          noiseValue=terrainHeightNoiseCache[index];
          return true;
         }
         return false;
        }
    }
    internal static class BiomesConfigurationSnapshot{
     private static readonly ReaderWriterLockSlim rwl=new(LockRecursionPolicy.SupportsRecursion);
        [ThreadStatic]
        private static int readDepth;
        internal struct ReadScope:IDisposable{
            public static ReadScope Enter(){
             rwl.EnterReadLock();
             readDepth++;
             return new();
            }
            public void Dispose(){
             readDepth--;
             rwl.ExitReadLock();
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void EnsureReading(){
         if(readDepth<=0)
          throw new InvalidOperationException(
           "você precisa avisar que está lendo dados do bioma com o uso de ReadScope! x.x"
          );
        }
     private static Snapshot snapshot;
        internal class Snapshot{
         internal static readonly Utilities.ObjectPool<Snapshot>pool=
          Pool.GetPool<Snapshot>(
           "",
           ()=>new(),
           (Snapshot item)=>{item.Dispose();}
          );
         internal float terrainSmoothingHeight;
         internal GraphNodeSnapshot graph;
            internal void DoSnapshot(){
             terrainSmoothingHeight=BiomesSystem.singleton.terrainSmoothingHeight;
             graph=BiomesSystem.singleton.biomeGraph.DoSnapshot(BiomesSystem.singleton.seed,null);
             graph.BuildSnapshotResolution();
            }
            internal void Dispose(){
             if(graph!=null){
              GraphNodeSnapshot.Return(graph.GetType(),graph);graph=null;
             }
            }
        }
        internal static void Build(){
         Snapshot newSnapshot=Snapshot.pool.Rent();
         newSnapshot.DoSnapshot();
         var oldSnapshot=snapshot;
         rwl.EnterWriteLock();
         try{
         snapshot=newSnapshot;
         }catch(Exception e){
          Logs.Error(e?.Message+"\n"+e?.StackTrace+"\n"+e?.Source);
         }finally{
          rwl.ExitWriteLock();
         }
         Snapshot.pool.Return(oldSnapshot);
        }
        internal static void DisposeAll(){
         Snapshot.pool.Return(snapshot);snapshot=null;
        }
        internal static void SetTerrainvxl(ref Voxel vxl,Vector3Int vCoord,Vector2Int cCoord,BiomesConfigurationContext context){
         EnsureReading();
         /*  fora do mundo, baixo:  */
         if(vCoord.y<=0){
          vxl=Voxel.bedrock;
          return;
         }
         /*  fora do mundo, cima:  */
         if(vCoord.y>=Height){
          vxl=Voxel.air;
          return;
         }
         var sampleInput=new SampleContext(vCoord,cCoord);
         var sampleDensityContext=new SampleDensityContext(sampleInput);
         sampleDensityContext.hasHeight=context.TryUseTerrainHeightNoiseCache(out sampleDensityContext.heightValue);
         float density=SampleDensity(ref sampleDensityContext,out bool updateTerrainHeightNoiseCache);
         if(updateTerrainHeightNoiseCache){
          context.UpdateTerrainHeightNoiseCache(sampleDensityContext.heightValue);
         }
         if(density<=Voxel.air.density){
          vxl=Voxel.air;
          return;
         }
         var sampleMaterialContext=new SampleMaterialContext(sampleInput);
         var material=SampleMaterial(ref sampleMaterialContext);
         vxl=new(
          density,
          material,
          Vector3.zero
         );
        }
        internal struct SampleContext{
         public Vector3Int vCoord;
         public Vector2Int cCoord;
         public Vector2Int cnkRgn;
         public Vector3Int noiseInputRounded;
         public Vector3    noiseInput       ;
            internal SampleContext(Vector3Int vCoord,Vector2Int cCoord){
             ValidatevCoord(ref cCoord,ref vCoord);
             this.vCoord=vCoord;
             this.cCoord=cCoord;
             cnkRgn=cCoordTocnkRgn(cCoord);
             noiseInputRounded=vCoord+new Vector3Int(cnkRgn.x,0,cnkRgn.y);
             noiseInput       =noiseInputRounded+new Vector3(.5f,.5f,.5f);
            }
        }
        internal struct SampleDensityContext{
         public Vector3Int vCoord;
         public Vector2Int cCoord;
         public Vector2Int cnkRgn;
         public Vector3Int noiseInputRounded;
         public Vector3    noiseInput       ;
         public bool hasHeight;
         public double heightValue;
            internal SampleDensityContext(SampleContext input){
             vCoord=input.vCoord;
             cCoord=input.cCoord;
             cnkRgn=input.cnkRgn;
             noiseInputRounded=input.noiseInputRounded;
             noiseInput       =input.noiseInput       ;
             hasHeight=false;
             heightValue=-1f;
            }
        }
        internal static float SampleDensity(ref SampleDensityContext context,out bool computed){
         EnsureReading();
         var snapshot=BiomesConfigurationSnapshot.snapshot;
         computed=false;
         Vector3Int vCoord=context.vCoord;
         Vector3Int noiseInputRounded=context.noiseInputRounded;
         Vector3    noiseInput       =context.noiseInput       ;
         if(vCoord.y<=0)
          return Voxel.bedrock.density;
         if(vCoord.y>=Height)
          return Voxel.air.density;
         if(!context.hasHeight){
          context.heightValue=snapshot.graph.GetValue(
           NoiseChannel.TerrainHeight,
           new(noiseInput.z,noiseInput.x,0)
          );
          computed=true;
          context.hasHeight=true;
         }
         double heightValue=context.heightValue;
         if(heightValue>=0d){
          if(noiseInputRounded.y<=heightValue+snapshot.terrainSmoothingHeight){
           float density=100.0f;
           float delta=(float)heightValue-noiseInputRounded.y;
           float smoothingValue=(snapshot.terrainSmoothingHeight-delta)/snapshot.terrainSmoothingHeight;
           density*=1f-smoothingValue;
           density=Mathf.Clamp(density,0f,100.0f);
           return density;
          }
         }
         return Voxel.air.density;
        }
        internal struct SampleMaterialContext{
         public Vector3Int vCoord;
         public Vector2Int cCoord;
         public Vector2Int cnkRgn;
         public Vector3Int noiseInputRounded;
         public Vector3    noiseInput       ;
            internal SampleMaterialContext(SampleContext input){
             vCoord=input.vCoord;
             cCoord=input.cCoord;
             cnkRgn=input.cnkRgn;
             noiseInputRounded=input.noiseInputRounded;
             noiseInput       =input.noiseInput       ;
            }
        }
        internal static MaterialId SampleMaterial(
         ref SampleMaterialContext context
        ){
         EnsureReading();
         var snapshot=BiomesConfigurationSnapshot.snapshot;
         Vector3Int vCoord=context.vCoord;
         Vector3Int noiseInputRounded=context.noiseInputRounded;
         Vector3    noiseInput       =context.noiseInput       ;
         var table=snapshot.graph.GetMaterialTable(
          NoiseChannel.TerrainSurfaceMaterial,
          new(noiseInput.z,noiseInput.x,0)
         );
         return table!=null?table.baseMaterial:MaterialId.MuddyDirt;
        }
        internal static SnapshotSpawnSettings GetSpawnSettings(NoiseChannel channel){
         var snapshotSpawnSettings=snapshot.graph.GetSpawnSettings(channel);
         return snapshotSpawnSettings;
        }
        internal static ByChanceObjectSpawnEntry<SimObject>GetSpawnEntry(NoiseChannel channel,Vector3Int vCoord,Vector2Int cCoord,int layer,out SpawnVariation variation){
         var snapshot=BiomesConfigurationSnapshot.snapshot;
         variation=default;
         Vector2Int cnkRgn=cCoordTocnkRgn(cCoord);
         Vector3Int noiseInputRounded=vCoord+new Vector3Int(cnkRgn.x,0,cnkRgn.y);
         Vector3    noiseInput       =noiseInputRounded+new Vector3(.5f,.5f,.5f);
         SnapshotBiomeSpawnTable table=snapshot.graph.GetBiomeSpawnTable(channel,new(noiseInput.z,noiseInput.x,0));
         if(table!=null){
          double spawnValue=Normalize01Clamped(snapshot.graph.GetValue(channel,new(noiseInput.z,noiseInput.x,0)));
          var picker=table.pickerByLayer[layer];
          if(picker.Get((float)spawnValue,out var result)){
           float rotationNoise=(float)Normalize01Clamped(snapshot.graph.GetValue(NoiseChannel.TerrainSurfaceRotation,new(noiseInput.z,noiseInput.x,0)));
           float    scaleNoise=(float)Normalize01Clamped(snapshot.graph.GetValue(NoiseChannel.TerrainSurfaceScale   ,new(noiseInput.z,noiseInput.x,0)));
           variation=result.variations.Get(
            new(){
             rotationNoise=rotationNoise,
                scaleNoise=   scaleNoise,
            },
            noiseInput
           );
           return result;
          }
         }
         return null;
        }
        internal static float NormalizeHeight(double height,float maxHeight){
         return(float)(height/maxHeight);
        }
        internal static float Normalize01Clamped(double value){
         value=Math.Max(-1.0,Math.Min(1.0,value));
         return(float)((value+1.0)*0.5);
        }
    }
}