using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace AKCondinoO{
    internal static class ArrayHelper{
        /// <summary>
        ///  Performance-oriented algorithm selection (https://stackoverflow.com/questions/1407715/how-to-quickly-zero-out-an-array)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sourceArray"></param>
        internal static void SetToDefaults<T>(this T[]sourceArray){
         if(sourceArray.Length<=25){
          for(int i=0;i<sourceArray.Length;i++){
           sourceArray[i]=default(T);
          }
         }else{
          Array.Clear(array:sourceArray,index:0,length:sourceArray.Length);
         }
        }
        /// <summary>
        ///  [https://stackoverflow.com/questions/10443461/c-sharp-array-findallindexof-which-findall-indexof]
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        internal static int[]IndicesOf<T>(this IEnumerable<T>collection,T value){
         return collection.Select((collectionValue,i)=>object.Equals(collectionValue,value)?i:-1).Where(i=>i!=-1).ToArray();
        }
    }
}