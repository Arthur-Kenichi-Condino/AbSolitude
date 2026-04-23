using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO{
    internal static class MathUtil{
        /// <summary>
        ///  Feito com ajuda do chatGPT:
        /// uma função que alterne entre os valores 0 e 1 com base em intervalos de um número x, e comece com o valor 0 no número y.
        /// </summary>
        /// <param name="num">o número em que você quer avaliar.</param>
        /// <param name="seqSize">o tamanho do intervalo de números entre as alternâncias (ex: 16).</param>
        /// <param name="seqStart">o número onde a sequência começa com 0 (ex: 0 ou qualquer outro número inicial).</param>
        /// <returns></returns>
        internal static int AlternatingSequence(int num,int seqSize,int seqStart){
         int adjustedNum=num-seqStart;
         int block=Mathf.FloorToInt((float)adjustedNum/seqSize);
         return(Mathf.Abs(block)%2==0)?0:1;
        }
        //  feito com ajuda do chatGPT
        internal static int AlternatingSequenceWithSeparator(int num,int seqSize,int seqStart){
         int rel=num-seqStart;
         int cycleLen=seqSize+1;//  bloco + separador
         int bigCycle=2*cycleLen;//  ciclo completo: sep+0...sep+1...
         //  Normaliza para 0..bigCycle-1, funciona para negativos
         int pos=((rel%bigCycle)+bigCycle)%bigCycle;
         if(pos==0||pos==cycleLen)     return 2;//  separador
         if(pos>0&&pos<cycleLen)       return 0;//  bloco de 0
         if(pos>cycleLen&&pos<bigCycle)return 1;//  bloco de 1
         return 2;//  fallback (nunca deve acontecer)
        }
    }
}