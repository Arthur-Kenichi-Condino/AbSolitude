using System;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.SimObjects{
    internal class SimObjectInstancedRendering{
     private readonly Dictionary<Type,SimObjectInstancedRenderer>renderers=new();
        internal void RegisterType(Type type,Mesh mesh,Material[]materials,int layer,int preallocate=0){
         renderers[type]=new(mesh,materials,layer,preallocate);
        }
        internal int AddInstance(Type type,Matrix4x4 matrix){
         if(renderers.TryGetValue(type,out var renderer)){
          return renderer.AddInstance(matrix);
         }
         return -1;
        }
        internal void RemoveInstance(Type type,int index){
         renderers[type].RemoveAtSwapBack(index);
        }
        internal void Clear(){
         foreach(var renderer in renderers.Values)renderer.Clear();
        }
        internal void DrawAll(){
         foreach(var renderer in renderers.Values)renderer.DrawAll();
        }
    }
}