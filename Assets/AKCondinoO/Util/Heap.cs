#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AKCondinoO.Sims.Actors.Pathfinding.AStarPathfinding;
namespace AKCondinoO{
    internal class Heap<T>where T:IHeapItem<T>{
     readonly T[]items;
        internal Heap(int maxHeapSize){
         items=new T[maxHeapSize];
        }
     internal int count{get{return currentItemsCount;}}int currentItemsCount;//  Because the items array is not always full
        internal bool Contains(T item){
         return Equals(items[item.heapIndex],item);
        }
        internal void Add(T item){
         item.heapIndex=currentItemsCount;
         items[currentItemsCount]=item;
         SortUp(item);//  Teste os valores Parent deste item e mova o item na array até que ele tenha um Parent menor e (dois) Child maior(es) 
         currentItemsCount++;
        }
        internal void UpdateItem(T item){
         SortUp(item);
        }
        void Swap(T itemA,T itemB){
         items[itemA.heapIndex]=itemB;
         items[itemB.heapIndex]=itemA;
         int itemA_heapIndex=itemA.heapIndex;//  Não se esqueça de trocar o índice no próprio objeto/item! :c)
         itemA.heapIndex=itemB.heapIndex;
         itemB.heapIndex=itemA_heapIndex;
        }
        /// <summary>
        ///  Se o valor Child for 6, Parent sempre será (int)((6-1)/2)=2
        ///  Fique em Loop até que o valor do item tenha os seguintes atributos: seu Parent tem que ser menor; seus Childs, maiores
        /// </summary>
        void SortUp(T item){
         Log.DebugMessage("T item["+item.heapIndex+"];_compare_item_with_parents'_indices_and_sort_");
         _Loop:{
          var parentIdx=(item.heapIndex-1)/2;//  Se o valor Child for 6, Parent sempre será (int)((6-1)/2)=2
          var parentItm=items[parentIdx];
          if(item.CompareTo(parentItm)>0){//  Parent tem valor maior, trocar: colocar o Parent (menor prioridade) para baixo e o Child (maior prioridade) para cima 
           Log.DebugMessage("_parent[index:"+parentItm.heapIndex+((parentItm is Node)?",(Node.F:"+(parentItm as Node).heuristics+"),(Node.H:"+(parentItm as Node).disToTarget+")":(""))+ "]_>_item[index:"+item.heapIndex+((item is Node)?",(Node.F:"+(item as Node).heuristics+"),(Node.H:"+(item as Node).disToTarget+")":"")+"]_:_item_has_higher_priority_;_pull_item_up_;");
           Swap(item,parentItm);
          }else{
           Log.DebugMessage("_parent[index:"+parentItm.heapIndex+((parentItm is Node)?",(Node.F:"+(parentItm as Node).heuristics+"),(Node.H:"+(parentItm as Node).disToTarget+")":(""))+"]_<=_item[index:"+item.heapIndex+((item is Node)?",(Node.F:"+(item as Node).heuristics+"),(Node.H:"+(item as Node).disToTarget+")":"")+"]_:_item_has_equal_or_lower_priority_;_stop_moving_item_;");
           goto _End;
          }
          goto _Loop;
         }
         _End:{}
        }
        /// <summary>
        ///  Adquire o valor no topo da Array 'items' segundo as otimizações de Heap e reposiciona os valores para não deixar um espaço vazio
        /// </summary>
        /// <returns></returns>
        internal T RemoveFirst(){
         T firstItem=items[0];
         //  The last temporarily becomes the first (then sort down the first item)
         currentItemsCount--;//  [One item was removed]
         items[0]=items[currentItemsCount];
         items[0].heapIndex=0;
         //  Sort down até que o item tenha um Parent menor e Child(s) maior(es)
         SortDown(items[0]);
        return firstItem;}
        void SortDown(T item){
         Log.DebugMessage("T item["+item.heapIndex+"];_compare_item_with_children's_indices_and_sort_");
         _Loop:{
          var indexToSwap=0;
          var chldIdxLft=item.heapIndex*2+1;//  Se o valor de Parent for 2, o Child (left ) sempre será 2*2+1=5
          var chldIdxRgt=item.heapIndex*2+2;//  Se o valor de Parent for 2, o Child (right) sempre será 2*2+2=6
          if(chldIdxLft<currentItemsCount){ //  Se existe um Child à esquerda,
           indexToSwap=chldIdxLft;          // tentar dar Swap nele...
           if(chldIdxRgt<currentItemsCount){//  Mas se existe um Child à direita,
                                            // compare os dois.
            if(items[chldIdxLft].CompareTo(items[chldIdxRgt])<0){//  Se o Child da esquerda for maior (menor prioridade) que o da direita,
             indexToSwap=chldIdxRgt;                             // então o Swap deve ser com o da direita (maior prioridade)
            }
           }
           //  Tentar Swap
           if(item.CompareTo(items[indexToSwap])<0){//  Se o item para Sort Down tem menor prioridade (é maior) que seu Child para Swap,
            Log.DebugMessage("T item["+item.heapIndex+"];_child[index:"+items[indexToSwap].heapIndex+((items[indexToSwap] is Node)?",(Node.F:"+(items[indexToSwap] as Node).heuristics+"),(Node.H:"+(items[indexToSwap] as Node).disToTarget+")":(""))+"]_<=_item[index:"+item.heapIndex+((item is Node)?",(Node.F:"+(item as Node).heuristics+"),(Node.H:"+(item as Node).disToTarget+")":"")+"]_:_item_has_equal_or_lower_priority_;_push_item_down_");
            Swap(item,items[indexToSwap]);          // realizar o Swap para baixo
           }else{
            Log.DebugMessage("T item["+item.heapIndex+"];_child[index:"+items[indexToSwap].heapIndex+((items[indexToSwap] is Node)?",(Node.F:"+(items[indexToSwap] as Node).heuristics+"),(Node.H:"+(items[indexToSwap] as Node).disToTarget+")":(""))+ "]_>_item[index:"+item.heapIndex+((item is Node)?",(Node.F:"+(item as Node).heuristics+"),(Node.H:"+(item as Node).disToTarget+")":"")+"]_:_item_has_higher_priority_;_stop_moving_item_");
            goto _End;//  Child tem menor prioridade (é maior), então não realizar mais Swaps
           }
          }else{
           Log.DebugMessage("T item["+item.heapIndex+"];_no_(more)_children_to_try_to_swap_with_");
           goto _End;//  Não há mais Childs para dar Swap
          }
          goto _Loop;
         }
         _End:{}
        }
    }
    internal interface IHeapItem<T>:IComparable<T>{
     int heapIndex{get;set;}
    }
}