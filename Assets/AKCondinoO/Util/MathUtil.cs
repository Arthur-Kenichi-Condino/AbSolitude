using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO{
    internal static class MathUtil{
        internal static double NextDouble(this System.Random random,double minimum,double maximum){
         return GenerateRandomDouble(random,minimum,maximum);
        }
        /// <summary>
        ///  [https://stackoverflow.com/questions/1064901/random-number-between-2-double-numbers]
        /// </summary>
        /// <param name="random"></param>
        /// <param name="minimum"></param>
        /// <param name="maximum"></param>
        /// <returns></returns>
        internal static double GenerateRandomDouble(System.Random random,double minimum,double maximum){
         return random.NextDouble()*(maximum-minimum)+minimum;
        }
        internal static bool CoinFlip(this System.Random random){
         return DoCoinFlip(random);
        }
        /// <summary>
        ///  [https://stackoverflow.com/questions/45597505/c-sharp-simple-coin-flip-algorithm-not-working]
        /// </summary>
        /// <param name="random"></param>
        /// <returns></returns>
        internal static bool DoCoinFlip(System.Random random){
         return random.Next(1,3)==1;
        }
        //  feito com ajuda do chatGPT
        /// <summary>
        ///  uma função que alterne entre os valores 0 e 1 com base em intervalos de um número x, e comece com o valor 0 no número y.
        /// </summary>
        /// <param name="num">o número em que você quer avaliar.</param>
        /// <param name="seqSize">o tamanho do intervalo de números entre as alternâncias (ex: 16).</param>
        /// <param name="seqStart">o número onde a sequência começa com 0 (ex: 0 ou qualquer outro número inicial).</param>
        /// <returns></returns>
        internal static int AlternatingSequence(int num,int seqSize,int seqStart){
         //  Ajusta o número para que y seja o primeiro valor de 0
         int adjustedNum=num-seqStart;
         //  Divide o número ajustado por x, arredonda para baixo e aplica mod 2
         return((adjustedNum/seqSize)%2==0)?0:1;
        }
        //  feito com ajuda do chatGPT
        internal static int AlternatingSequenceWithSeparator(int num,int seqSize,int seqStart){
         //  relative index from the chosen start
         int rel=num-seqStart;
         int cycleLen=seqSize+1;//  cada meia-ciclo: seqSize:'valores' + 1:'separador'
         int bigCycle=2*cycleLen;//  ciclo completo: zeros + separator + ones + separator
         //  normaliza index relativo para 0..bigCycle-1, funcionando com negativos
         int pos=((rel%bigCycle)+bigCycle)%bigCycle;
         //  mapa de posições:
         // pos in 0..blockSize-1          => bloco de 0
         // pos == blockSize               => separador (2)
         // pos in blockSize+1..bigCycle-2 => bloco de 1
         // pos == bigCycle-1              => separador (2)
         if(pos<seqSize)   return 0;
         if(pos==seqSize)  return 2;
         if(pos<bigCycle-1)return 1;
                           return 2;
        }
        //  feito com ajuda do chatGPT
        internal static IEnumerable<(int x,int z)>GetCoords(
         int minX,int maxX,
         int minZ,int maxZ,
          int innerMinX,int innerMaxX,
          int innerMinZ,int innerMaxZ
        ){
         for(int x=minX       ;x<=maxX       ;x++){for(int z=innerMaxZ+1;z<=maxZ       ;z++){yield return(x,z);}}//  Quadrante superior
         for(int x=minX       ;x<=maxX       ;x++){for(int z=minZ       ;z<=innerMinZ-1;z++){yield return(x,z);}}//  Quadrante inferior
         for(int x=minX       ;x<=innerMinX-1;x++){for(int z=innerMinZ  ;z<=innerMaxZ  ;z++){yield return(x,z);}}//  Quadrante esquerdo
         for(int x=innerMaxX+1;x<=maxX       ;x++){for(int z=innerMinZ  ;z<=innerMaxZ  ;z++){yield return(x,z);}}//  Quadrante direito
        }
        //  feito com ajuda do chatGPT
        internal static IEnumerable<(int x,int y,int z)>GetCoords3D(
         int minX,int maxX,
         int minY,int maxY,
         int minZ,int maxZ,
          int innerMinX,int innerMaxX,
          int innerMinY,int innerMaxY,
          int innerMinZ,int innerMaxZ
        ){
         for(int x=minX;x<=maxX;x++){for(int z=minZ;z<=maxZ;z++){for(int y=minY       ;y<=innerMinY-1;y++){yield return(x,y,z);}}}//  Borda em Y inferior
         for(int x=minX;x<=maxX;x++){for(int z=minZ;z<=maxZ;z++){for(int y=innerMaxY+1;y<=maxY       ;y++){yield return(x,y,z);}}}//  Borda em Y superior
         //  Região Y central, agora iteramos nas bordas de X e Z
         for(int y=innerMinY;y<=innerMaxY;y++){
          for(int x=minX;x<=maxX;x++){for(int z=minZ       ;z<=innerMinZ-1;z++){yield return(x,y,z);}}//  Borda em Z inferior
          for(int x=minX;x<=maxX;x++){for(int z=innerMaxZ+1;z<=maxZ       ;z++){yield return(x,y,z);}}//  Borda em Z superior
          //  Região Z central, bordas de X
          for(int z=innerMinZ;z<=innerMaxZ;z++){
           for(int x=minX       ;x<=innerMinX-1;x++){yield return(x,y,z);}//  Borda em X esquerdo
           for(int x=innerMaxX+1;x<=maxX       ;x++){yield return(x,y,z);}//  Borda em X direito
          }
         }
        }
    }
}