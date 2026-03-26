using System;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO{
    internal static class DictionaryAddRangeHelper{
        internal enum DictionaryAddRangeMethod:byte{
         Override        =0,
         NewOnly         =1,
         SimpleThrowError=2,
        }
        //  [https://stackoverflow.com/questions/6050633/why-doesnt-dictionary-have-addrange]
        internal static void AddRange<TKey,TValue>(this IDictionary<TKey,TValue>dic,IDictionary<TKey,TValue>dicToAdd,DictionaryAddRangeMethod method){
         if      (method==DictionaryAddRangeMethod.Override        ){
          dicToAdd.ForEachDo(x=>dic[x.Key]=x.Value);
         }else if(method==DictionaryAddRangeMethod.NewOnly         ){
          dicToAdd.ForEachDo(x=>{if(!dic.ContainsKey(x.Key))dic.Add(x.Key,x.Value);});
         }else if(method==DictionaryAddRangeMethod.SimpleThrowError){
          dicToAdd.ForEachDo(x=>dic.Add(x.Key,x.Value));
         }
        }
        static void ForEachDo<T>(this IEnumerable<T>source,Action<T>action){
         foreach(var item in source)action(item);
        }
        internal static bool ContainsKeys<TKey,TValue>(this IDictionary<TKey,TValue>dic,IEnumerable<TKey>keys){
         bool result=false;
         keys.ForEachIfThenBreak((x)=>{result=dic.ContainsKey(x);return result;});
         return result;
        }
        static void ForEachIfThenBreak<T>(this IEnumerable<T>source,Func<T,bool>func){
         foreach(var item in source){
          bool result=func(item);
          if(result)break;
         }
        }
    }
}