#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Voxels.Biomes;
using AKCondinoO.Sims;
using AKCondinoO.Voxels.Terrain;
using AKCondinoO.Voxels.Terrain.MarchingCubes;
using AKCondinoO.Voxels.Terrain.SimObjectsPlacing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
namespace AKCondinoO.Voxels{
    internal class VoxelSystem:MonoBehaviour,ISingletonInitialization{
     internal const int MaxcCoordx=312;
     internal const int MaxcCoordy=312;
     internal const ushort Height=(256);
     internal const ushort Width=(16);
     internal const ushort Depth=(16);
     internal const ushort FlattenOffset=(Width*Depth);
     internal const int VoxelsPerChunk=(FlattenOffset*Height);
        #region chunk
            internal static Vector2Int vecPosTocCoord(Vector3 pos){
                                                              pos.x/=(float)Width;
                                                              pos.z/=(float)Depth;
             return new Vector2Int((pos.x>0)?(pos.x-(int)pos.x==0.5f?Mathf.FloorToInt(pos.x):Mathf.RoundToInt(pos.x)):(int)Math.Round(pos.x,MidpointRounding.AwayFromZero),
                                   (pos.z>0)?(pos.z-(int)pos.z==0.5f?Mathf.FloorToInt(pos.z):Mathf.RoundToInt(pos.z)):(int)Math.Round(pos.z,MidpointRounding.AwayFromZero)
                                  );
            }
            internal static Vector2Int vecPosTocnkRgn(Vector3 pos){Vector2Int coord=vecPosTocCoord(pos);
             return new Vector2Int(coord.x*Width,
                                   coord.y*Depth);
            }
            internal static Vector2Int cnkRgnTocCoord(Vector2Int cnkRgn){return new Vector2Int(cnkRgn.x/Width,cnkRgn.y/Depth);}
            internal static Vector2Int cCoordTocnkRgn(Vector2Int cCoord){return new Vector2Int(cCoord.x*Width,cCoord.y*Depth);}
            internal static int GetcnkIdx(int cx,int cy){return cy+cx*(MaxcCoordy*2+1);}
        #endregion
        #region voxel
            internal static Vector3Int vecPosTovCoord(Vector3 pos){
             Vector2Int rgn=vecPosTocnkRgn(pos);
             pos.x=(pos.x>0)?(pos.x-(int)pos.x==0.5f?Mathf.FloorToInt(pos.x):Mathf.RoundToInt(pos.x)):(int)Math.Round(pos.x,MidpointRounding.AwayFromZero);
             pos.y=(pos.y>0)?(pos.y-(int)pos.y==0.5f?Mathf.FloorToInt(pos.y):Mathf.RoundToInt(pos.y)):(int)Math.Round(pos.y,MidpointRounding.AwayFromZero);
             pos.z=(pos.z>0)?(pos.z-(int)pos.z==0.5f?Mathf.FloorToInt(pos.z):Mathf.RoundToInt(pos.z)):(int)Math.Round(pos.z,MidpointRounding.AwayFromZero);
             Vector3Int coord=new Vector3Int((int)pos.x-rgn.x,(int)pos.y,(int)pos.z-rgn.y);
             coord.x+=Mathf.FloorToInt(Width /2.0f);coord.x=Mathf.Clamp(coord.x,0,Width -1);
             coord.y+=Mathf.FloorToInt(Height/2.0f);coord.y=Mathf.Clamp(coord.y,0,Height-1);
             coord.z+=Mathf.FloorToInt(Depth /2.0f);coord.z=Mathf.Clamp(coord.z,0,Depth -1);
             return coord;
            }
            internal static int GetvxlIdx(int vcx,int vcy,int vcz){return vcy*FlattenOffset+vcx*Depth+vcz;}
            internal static int GetoftIdx(Vector2Int offset){//  ..for neighbors
             if(offset.x== 0&&offset.y== 0)return 0;
             if(offset.x==-1&&offset.y== 0)return 1;
             if(offset.x== 1&&offset.y== 0)return 2;
             if(offset.x== 0&&offset.y==-1)return 3;
             if(offset.x==-1&&offset.y==-1)return 4;
             if(offset.x== 1&&offset.y==-1)return 5;
             if(offset.x== 0&&offset.y== 1)return 6;
             if(offset.x==-1&&offset.y== 1)return 7;
             if(offset.x== 1&&offset.y== 1)return 8;
             return -1;
            }
        #endregion
        #region validation
            internal static void ValidateCoord(ref Vector2Int region,ref Vector3Int vxlCoord){int a,c;
             a=region.x;c=vxlCoord.x;ValidateCoordAxis(ref a,ref c,Width);region.x=a;vxlCoord.x=c;
             a=region.y;c=vxlCoord.z;ValidateCoordAxis(ref a,ref c,Depth);region.y=a;vxlCoord.z=c;
            }
            internal static void ValidateCoordAxis(ref int axis,ref int coord,int axisLength){
             if      (coord<0){          axis-=axisLength*Mathf.CeilToInt (Math.Abs(coord)/(float)axisLength);coord=(coord%axisLength)+axisLength;
             }else if(coord>=axisLength){axis+=axisLength*Mathf.FloorToInt(Math.Abs(coord)/(float)axisLength);coord=(coord%axisLength);
             }
            }
        #endregion
     internal static int voxelTerrainLayer;
     internal static VoxelSystem singleton{get;set;}
     [SerializeField]internal int _MarchingCubesExecutionCountLimit=7;
     internal readonly MarchingCubesMultithreaded[]marchingCubesBGThreads=new MarchingCubesMultithreaded[Environment.ProcessorCount];
     internal readonly VoxelTerrainSurfaceSimObjectsPlacerMultithreaded[]surfaceSimObjectsPlacerBGThreads=new VoxelTerrainSurfaceSimObjectsPlacerMultithreaded[Environment.ProcessorCount];
     internal static Vector2Int expropriationDistance{get;}=new Vector2Int(12,12);
     internal static Vector2Int instantiationDistance{get;}=new Vector2Int(12,12);
     internal static readonly BaseBiome biome=new BaseBiome();
     [SerializeField]VoxelTerrainChunk _VoxelTerrainChunkPrefab;
     internal VoxelTerrainChunk[]terrain;
        void Awake(){
         if(singleton==null){singleton=this;}else{DestroyImmediate(this);return;}
         voxelTerrainLayer=1<<LayerMask.NameToLayer("VoxelTerrain");
         VoxelTerrainChunk.sMarchingCubesExecutionCount=0;
         MarchingCubesMultithreaded.Stop=false;
         for(int i=0;i<marchingCubesBGThreads.Length;++i){
                       marchingCubesBGThreads[i]=new MarchingCubesMultithreaded();
         }
         VoxelTerrainSurfaceSimObjectsPlacerMultithreaded.Stop=false;
         for(int i=0;i<surfaceSimObjectsPlacerBGThreads.Length;++i){
                       surfaceSimObjectsPlacerBGThreads[i]=new VoxelTerrainSurfaceSimObjectsPlacerMultithreaded();
         }
        }
        public void Init(){
         int maxConnections=1;
         int poolSize=maxConnections*(expropriationDistance.x*2+1)
                                    *(expropriationDistance.y*2+1);
         terrainSynchronization.Clear();
         terrain=new VoxelTerrainChunk[poolSize];
         for(int i=0;i<terrain.Length;++i){
          VoxelTerrainChunk cnk=terrain[i]=Instantiate(_VoxelTerrainChunkPrefab);
          cnk.expropriated=terrainPool.AddLast(cnk);
          cnk.OnInstantiated();
          terrainSynchronization.Add(cnk,cnk.marchingCubesBG.synchronizer);
         }
         AtlasHelper.SetAtlasData();
         biome.Seed=0;
         proceduralGenerationCoroutine=StartCoroutine(ProceduralGenerationCoroutine());
        }
        public void OnDestroyingCoreEvent(object sender,EventArgs e){
         Log.DebugMessage("VoxelSystem:OnDestroyingCoreEvent");
         if(terrain!=null){
          for(int i=0;i<terrain.Length;++i){
           terrain[i].OnDestroyingCore();
          }
         }
         MarchingCubesMultithreaded.Stop=true;
         for(int i=0;i<marchingCubesBGThreads.Length;++i){
                       marchingCubesBGThreads[i].Wait();
         }
         VoxelTerrainSurfaceSimObjectsPlacerMultithreaded.Stop=true;
         for(int i=0;i<surfaceSimObjectsPlacerBGThreads.Length;++i){
                       surfaceSimObjectsPlacerBGThreads[i].Wait();
         }
         biome.DisposeModules();
        }
        void OnDestroy(){
        }
        void Update(){
         foreach(var kvp in terrainActive){
          VoxelTerrainChunk cnk=kvp.Value;
          cnk.ManualUpdate();
         }
        }
     Coroutine proceduralGenerationCoroutine;
     internal readonly HashSet<Gameplayer>generationRequests=new HashSet<Gameplayer>();
      readonly Dictionary<Gameplayer,Vector2Int>  activatingCoordinates=new Dictionary<Gameplayer,Vector2Int>();
      readonly Dictionary<Gameplayer,Vector2Int>deactivatingCoordinates=new Dictionary<Gameplayer,Vector2Int>();
      readonly HashSet<Gameplayer>toGenerate=new HashSet<Gameplayer>();
     internal readonly        LinkedList<VoxelTerrainChunk>terrainPool           =new        LinkedList<VoxelTerrainChunk>();
     internal readonly    Dictionary<int,VoxelTerrainChunk>terrainActive         =new    Dictionary<int,VoxelTerrainChunk>();
     internal readonly Dictionary<VoxelTerrainChunk,object>terrainSynchronization=new Dictionary<VoxelTerrainChunk,object>();
        IEnumerator ProceduralGenerationCoroutine(){
            Loop:{
             yield return null;
             if(generationRequests.Count>0){
              foreach(var gameplayer in generationRequests){
               if(activatingCoordinates.TryGetValue(gameplayer,out Vector2Int cCoord_Previous)){
                deactivatingCoordinates[gameplayer]=cCoord_Previous;
               }
               activatingCoordinates[gameplayer]=gameplayer.cCoord;
               toGenerate.Add(gameplayer);
              }
                generationRequests.Clear();
              foreach(var gameplayer in toGenerate){
               if(deactivatingCoordinates.TryGetValue(gameplayer,out Vector2Int cCoord_Previous)){                
                //Log.DebugMessage("deactivate chunks around cCoord_Previous:"+cCoord_Previous);
                #region expropriation
                    for(Vector2Int eCoord=new Vector2Int(),cCoord1=new Vector2Int();eCoord.y<=expropriationDistance.y;eCoord.y++){for(cCoord1.y=-eCoord.y+cCoord_Previous.y;cCoord1.y<=eCoord.y+cCoord_Previous.y;cCoord1.y+=eCoord.y*2){
                    for(           eCoord.x=0                                      ;eCoord.x<=expropriationDistance.x;eCoord.x++){for(cCoord1.x=-eCoord.x+cCoord_Previous.x;cCoord1.x<=eCoord.x+cCoord_Previous.x;cCoord1.x+=eCoord.x*2){
                     if(Math.Abs(cCoord1.x)>=MaxcCoordx||
                        Math.Abs(cCoord1.y)>=MaxcCoordy){
                      goto _skip;
                     }
                     if(
                      activatingCoordinates.All(
                       kvp=>{
                        Vector2Int activatingCoordinate=kvp.Value;
                        return Mathf.Abs(cCoord1.x-activatingCoordinate.x)>instantiationDistance.x||
                               Mathf.Abs(cCoord1.y-activatingCoordinate.y)>instantiationDistance.y;
                       }
                      )
                     ){
                          int cnkIdx1=GetcnkIdx(cCoord1.x,cCoord1.y);
                          if(terrainActive.TryGetValue(cnkIdx1,out VoxelTerrainChunk cnk)){
                           if(cnk.expropriated==null){
                            cnk.expropriated=terrainPool.AddLast(cnk);
                           }
                          }
                     }
                     _skip:{}
                     if(eCoord.x==0){break;}
                    }}
                     if(eCoord.y==0){break;}
                    }}
                #endregion
               }
               Vector2Int cCoord=activatingCoordinates[gameplayer];
               //Log.DebugMessage("generate voxel chunks around cCoord:"+cCoord);
               #region instantiation
                   for(Vector2Int iCoord=new Vector2Int(),cCoord1=new Vector2Int();iCoord.y<=instantiationDistance.y;iCoord.y++){for(cCoord1.y=-iCoord.y+cCoord.y;cCoord1.y<=iCoord.y+cCoord.y;cCoord1.y+=iCoord.y*2){
                   for(           iCoord.x=0                                      ;iCoord.x<=instantiationDistance.x;iCoord.x++){for(cCoord1.x=-iCoord.x+cCoord.x;cCoord1.x<=iCoord.x+cCoord.x;cCoord1.x+=iCoord.x*2){
                    if(Math.Abs(cCoord1.x)>=MaxcCoordx||
                       Math.Abs(cCoord1.y)>=MaxcCoordy){
                     goto _skip;
                    }
                    int cnkIdx1=GetcnkIdx(cCoord1.x,cCoord1.y);
                    if(!terrainActive.TryGetValue(cnkIdx1,out VoxelTerrainChunk cnk)){
                         cnk=terrainPool.First.Value;
                         terrainPool.RemoveFirst();
                         cnk.expropriated=null;
                         bool firstCall=cnk.id==null;
                         if(!firstCall&&terrainActive.ContainsKey(cnk.id.Value.cnkIdx)){
                          terrainActive.Remove(cnk.id.Value.cnkIdx);
                         }
                         terrainActive.Add(cnkIdx1,cnk);
                         cnk.OncCoordChanged(cCoord1,cnkIdx1,firstCall);
                    }else{
                         if(cnk.expropriated!=null){
                          terrainPool.Remove(cnk.expropriated);
                          cnk.expropriated=null;
                         }
                    }
                    _skip:{}
                    if(iCoord.x==0){break;}
                   }}
                    if(iCoord.y==0){break;}
                   }}
               #endregion
              }
              toGenerate.Clear();
             }
            }
            goto Loop;
        }
     internal bool navMeshSourcesCollectionChanged;
     internal readonly SortedDictionary<int,NavMeshBuildSource>navMeshSources=new SortedDictionary<int,NavMeshBuildSource>();
     internal readonly SortedDictionary<int,NavMeshBuildMarkup>navMeshMarkups=new SortedDictionary<int,NavMeshBuildMarkup>();
      readonly List<NavMeshBuildSource>sources=new List<NavMeshBuildSource>();
      readonly List<NavMeshBuildMarkup>markups=new List<NavMeshBuildMarkup>();
        internal void CollectNavMeshSources(out List<NavMeshBuildSource>sourcesCollected){
         sourcesCollected=sources;
         if(navMeshSourcesCollectionChanged){
            navMeshSourcesCollectionChanged=false;
          Log.DebugMessage("CollectNavMeshSources");
          sources.Clear();
          markups.Clear();
          sources.AddRange(navMeshSources.Values);
          markups.AddRange(navMeshMarkups.Values);
          NavMeshBuilder.CollectSources(null,NavMeshHelper.navMeshLayer,NavMeshCollectGeometry.PhysicsColliders,0,markups,sources);
         }
        }
    }
}