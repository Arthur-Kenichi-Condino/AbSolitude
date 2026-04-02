using System;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.SimObjects{
    internal class SimObjectInstancedRendering{
     private readonly Dictionary<(Type type,string variant),SimObjectInstancedRenderer>renderers=new();
        internal void RegisterType((Type type,string variant)key,Mesh mesh,Material[]materials,int layer,int preallocate=0){
         renderers[key]=new(mesh,materials,layer,preallocate);
        }
        internal int AddInstance((Type type,string variant)key,SimObject simObject){
         if(renderers.TryGetValue(key,out var renderer)){
          return renderer.AddInstance(simObject);
         }
         return -1;
        }
        internal void RemoveInstance((Type type,string variant)key,int index){
         if(index<0){return;}
         renderers[key].RemoveAtSwapBack(index);
        }
        internal void Clear(bool destroy=false){
         foreach(var renderer in renderers.Values)renderer.Clear(destroy);
         if(destroy){
          renderers.Clear();
         }
        }
        internal void DrawAll(){
         foreach(var renderer in renderers.Values)renderer.DrawAll();
        }
    }
}