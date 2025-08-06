#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Voxels.Terrain.MarchingCubes;
using LibNoise;
using LibNoise.Generator;
using LibNoise.Operator;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AKCondinoO.Voxels.VoxelSystem;
using static AKCondinoO.Voxels.Terrain.MarchingCubes.MarchingCubesTerrain;
namespace AKCondinoO.Voxels.Biomes{
    internal partial class BaseBiome{
     internal BiomeBehaviour biomeBehaviour{get{if(VoxelSystem.singleton!=null){return VoxelSystem.singleton.biomeBehaviour;}return null;}}
     internal readonly BaseBiomeSimObjectsSpawnSettings biomeSpawnSettings;
         internal BaseBiome(){
          biomeSpawnSettings=new BaseBiomeSimObjectsSpawnSettings(this);
         }
     internal Vector3 deround{get;}=new Vector3(.5f,.5f,.5f);
     protected readonly List<ModuleBase>modules=new List<ModuleBase>();//  dispose after usage
     protected System.Random[]random=new System.Random[2];
     int seed_v;
         internal virtual int Seed{
          get{return seed_v;}
          set{       seed_v=value;
           biomeSpawnSettings.Set();
           random[0]=new System.Random(seed_v          );
           random[1]=new System.Random(random[0].Next());
           SetModules();
           selectors[0]=(Select)modules[5];
          }
         }
         protected virtual void SetModules(){
          modules.Add(new Const(  0));
          modules.Add(new Const(  1));
          modules.Add(new Const( -1));
          modules.Add(new Const( .5));
          modules.Add(new Const(Height));
              ModuleBase module1=new Const(5);
                  ModuleBase module2a=new RidgedMultifractal(
                   frequency:.003,
                    lacunarity:2.0,
                     octaves:6,
                      seed:random[1].Next(),
                       quality:QualityMode.Low
                  );
                  ModuleBase module2b=new Turbulence(input:module2a);
                   ((Turbulence)module2b).Seed=random[1].Next();
                   ((Turbulence)module2b).Frequency=.25;
                   ((Turbulence)module2b).Power=1.0;
                  ModuleBase module2c=new ScaleBias(
                   scale:1.0,
                    bias:.11*Height,
                     input:module2b
                  ); 
                      ModuleBase module3a=new Billow(
                       frequency:.007,
                        lacunarity:2.0,
                         persistence:0.5,
                          octaves:6,
                           seed:random[1].Next(),
                            quality:QualityMode.Low
                      );
                      ModuleBase module3b=new Turbulence(input:module3a);
                       ((Turbulence)module3b).Seed=random[1].Next();
                       ((Turbulence)module3b).Frequency=.25;  
                       ((Turbulence)module3b).Power=1.0;
                      ModuleBase module3c=new ScaleBias(
                       scale:1.0,
                        bias:.11*Height,
                         input:module3b
                      );
                          ModuleBase module4a=new Perlin(
                           frequency:.015,
                            lacunarity:2.0,
                             persistence:0.5,
                              octaves:6,
                               seed:random[1].Next(),
                                quality:QualityMode.Low
                          );
                          ModuleBase module4b=new Select(
                           inputA:module2c,
                           inputB:module3c,
                            controller:module4a
                          );
                           ((Select)module4b).SetBounds(min:-.2,max:.2);
                           ((Select)module4b).FallOff=.25;
          modules.Add(module4b);//  5
                          ModuleBase module4c=new Multiply(lhs:module4b,rhs:module1);
          modules.Add(module4c);//  6
          biomeSpawnSettings.simObjectSpawnChancePerlin=new Perlin(
           frequency:.25,
            lacunarity:2.0,
             persistence:0.5,
              octaves:6,
               seed:seed_v,
                quality:QualityMode.Low
          );
          biomeSpawnSettings.scaleModifierPerlin=new Perlin(
           frequency:.70,
            lacunarity:3.5,
             persistence:0.8,
              octaves:6,
               seed:seed_v,
                quality:QualityMode.Low
          );
          biomeSpawnSettings.rotationModifierPerlin=new Perlin(
           frequency:.85,
            lacunarity:3.5,
             persistence:0.8,
              octaves:6,
               seed:seed_v,
                quality:QualityMode.Low
          );
         }
         internal void DisposeModules(){
          foreach(var module in modules){
                      module.Dispose();
          }
          modules.Clear();
          biomeSpawnSettings.simObjectSpawnChancePerlin.Dispose();
          biomeSpawnSettings.   scaleModifierPerlin.Dispose();
          biomeSpawnSettings.rotationModifierPerlin.Dispose();
         }
     protected Select[]selectors=new Select[1];
         internal virtual int Selection(Vector3 noiseInput){
          double min=selectors[0].Minimum;
          double max=selectors[0].Maximum;
          double fallOff=selectors[0].FallOff*.5;
          double selectionValue=selectors[0].Controller.GetValue(noiseInput.z,noiseInput.x,0);
          if(selectionValue<=min-fallOff||selectionValue>=max+fallOff){
           return 1;
          }else{
           return 0;
          }
         }
     internal virtual int cacheLength{get{return 1;}}
             internal void Setvxl(
              Vector3Int noiseInputRounded,
               double[][][]noiseCache1,//  ...terrain height cache
                MaterialId[][][]materialIdCache1,
                 int oftIdx,
                  int noiseIndex,
                   ref Voxel vxl
             ){
              Vector3 noiseInput=noiseInputRounded+deround;
              if(noiseCache1!=null&&noiseCache1[0][oftIdx]==null){
                 noiseCache1[0][oftIdx]=new double[FlattenOffset];
              }
              double noiseValue=(noiseCache1!=null&&noiseCache1[0][oftIdx][noiseIndex]!=0)?
               noiseCache1[0][oftIdx][noiseIndex]:
                (noiseCache1!=null?
                 (noiseCache1[0][oftIdx][noiseIndex]=Noise()):
                  Noise());
              double Noise(){return modules[6/* terrain height module index */].GetValue(noiseInput.z,noiseInput.x,0);}
              if(noiseInput.y<=noiseValue){
               double d;
               vxl=new Voxel(d=Density(100,noiseInput,noiseValue),Vector3.zero,Material(d,noiseInput,materialIdCache1,oftIdx,noiseIndex));
               return;
              }
              vxl=Voxel.air;
             }
             protected double Density(
              double density,
               Vector3 noiseInput,
                double noiseValue,
                 float smoothing=4f
             ){
              double value=density;
              double delta=noiseValue-noiseInput.y;//  noiseInput.y sempre será menor ou igual a noiseValue
              if(delta<=smoothing){
               double smoothingValue=(smoothing-delta)/smoothing;
               value*=1d-smoothingValue;
               if(value<1)
                  value=1;
               else if(value>100)
                       value=100;
              }
              return value;
             }
     protected MaterialId[]materialIdPicking=new MaterialId[2]{
      MaterialId.Rock,
      MaterialId.Dirt,
     };
             protected virtual MaterialId Material(
              double density,
               Vector3 noiseInput,
                MaterialId[][][]materialIdCache1,
                 int oftIdx,
                  int noiseIndex
             ){
              if(-density>=isoLevel){
               return MaterialId.Air;
              }
              if(materialIdCache1!=null){
               if(materialIdCache1[0][oftIdx]==null){
                  materialIdCache1[0][oftIdx]=new MaterialId[FlattenOffset];
               }
               if(materialIdCache1[0][oftIdx][noiseIndex]!=0){
                return materialIdCache1[0][oftIdx][noiseIndex];
               }
              }
              MaterialId m=materialIdPicking[Selection(noiseInput)];
              return materialIdCache1!=null?materialIdCache1[0][oftIdx][noiseIndex]=m:m;
             }
    }
}