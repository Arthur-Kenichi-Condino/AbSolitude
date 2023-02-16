using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace AKCondinoO{
    /// <summary>
    ///  TO DO: testar
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public struct DictionaryWrapper<TKey,TValue>:IEnumerator where TKey:struct where TValue:struct{
     private readonly(TKey,TValue)[]elements;
     private int position;
        public DictionaryWrapper(Dictionary<TKey,TValue>dictionary){
         elements=dictionary.Select(kvp=>(kvp.Key,kvp.Value)).ToArray();
         position=-1;
        }
        public bool MoveNext(){
         position++;
         return(elements!=null&&position<elements.Length);
        }
        public void Reset(){
         position=-1;
        }
        object IEnumerator.Current{
         get{
          return Current;
         }
        }
        public(TKey,TValue)Current{
         get{
          try{
           return elements[position];
          }catch(IndexOutOfRangeException){
           throw new InvalidOperationException();
          }
         }
        }
    }
}