using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO{
    public struct ListWrapper<T>:IEnumerator where T:struct{
     private readonly T[]elements;
     private int position;
        public ListWrapper(List<T>list){
         elements=list.ToArray();
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
        public T Current{
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