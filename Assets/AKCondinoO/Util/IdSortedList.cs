#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO{
    internal class IdSortedList<T>{
     readonly Dictionary<int,T>list=new();
      readonly Dictionary<T,int>itemIds=new();
     int nextId=0;
      readonly List<int>releasedIds=new();
        internal bool Add(T item){
         if(itemIds.ContainsKey(item)){
          return false;
         }
         int id;
         if(releasedIds.Count>0){
          id=releasedIds[0];
          releasedIds.RemoveAt(0);
         }else{
          id=nextId;
          nextId++;
         }
         list.Add(id,item);
         itemIds.Add(item,id);
         return true;
        }
        internal bool Remove(T item){
         if(!itemIds.TryGetValue(item,out int id)){
          return false;
         }
         list.Remove(id);
         itemIds.Remove(item);
         releasedIds.Add(id);
         return true;
        }
     int traversingNextId=0;
        internal bool TraverseNext(out T item){
         item=default(T);
         if(list.Count<=0){
          return false;
         }
         int traverseStart=traversingNextId;
         int traverseEnd=traversingNextId-1;
         bool reachedLastId=false;
         bool gotItem=false;
         T listItem;
         int t=traverseStart;
         for(;;){
          //Log.DebugMessage("TraverseNext:t:"+t);
          gotItem=list.TryGetValue(t,out listItem);
          t++;
          if(t>=nextId){
           t=0;
           if(reachedLastId){
            break;
           }
           reachedLastId=true;
          }
          if(reachedLastId){
           if(t>traverseEnd){
            break;
           }
          }
          if(gotItem){
           break;
          }
         }
         if(gotItem){
          //Log.DebugMessage("TraverseNext:listItem:"+listItem);
          item=listItem;
         }
         traversingNextId=t;
         return true;
        }
    }
}