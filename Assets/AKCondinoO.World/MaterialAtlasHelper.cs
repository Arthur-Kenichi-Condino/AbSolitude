using AKCondinoO.Bootstrap;
using AKCondinoO.World.MarchingCubes;
using System;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
namespace AKCondinoO.World{
    internal static class MaterialAtlasHelper{
     static readonly Dictionary<uint,Vector2>atlasCoord=new(Enum.GetValues(typeof(MaterialId)).Length);

        internal static void SetAtlasData(){
         atlasCoord.Clear();
         Set(MaterialId.AirNone    ,0,0);
         Set(MaterialId.EdgeBedrock,0,2);
         Set(MaterialId.MuddyDirt  ,1,0);
         Set(MaterialId.SmallRocks ,0,1);
         Logs.Message(Logs.LogType.Debug,"UnsafeUtility.SizeOf<Vertex>():"+UnsafeUtility.SizeOf<Vertex>());
        }
        static void Set(MaterialId id,int x,int y){
         atlasCoord[(uint)id]=new(x,y);
        }
        internal static Vector2 GetCoord(MaterialId id){
         return GetCoord((uint)id);
        }
        internal static Vector2 GetCoord(uint id){
         return atlasCoord[(uint)id];
        }
    }
}