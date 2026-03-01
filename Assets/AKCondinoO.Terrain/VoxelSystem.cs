using AKCondinoO.Bootstrap;
using System;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Terrain{
    internal class VoxelSystem:MonoSingleton<VoxelSystem>{
     public override int initOrder{get{return 9;}}
     [SerializeField]internal Vector2Int expropriationDistance=new Vector2Int(9,9);//  ...pool size
     [SerializeField]internal Vector2Int instantiationDistance=new Vector2Int(6,6);
     private readonly HashSet<Vector2Int>chunkRef=new();
        internal void AddRef(Vector2Int coord){
         Logs.Message(Logs.LogType.Debug,"coord:"+coord);
        }
        internal void RemoveRef(Vector2Int coord){
         Logs.Message(Logs.LogType.Debug,"coord:"+coord);
        }
        internal void EnsureExists(Vector2Int coord){
         Logs.Message(Logs.LogType.Debug,"coord:"+coord);
        }
    }
}