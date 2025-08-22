using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
namespace AKCondinoO{
    internal static class PhysUtil{
     internal static int considerGroundLayer;
      internal static readonly string[]considerGroundLayerNames=new string[]{
       "VoxelTerrain",
       "Construction",
      };
     internal static int physObstaclesLayer;
      internal static readonly string[]physObstaclesLayerNames=new string[]{
       "Default",
       "VoxelTerrain",
       "Construction",
      };
     internal static int shootingHitsLayer;
      internal static readonly string[]shootingHitsLayerNames=new string[]{
       "Default",
       "VoxelTerrain",
       "Construction",
       "Hurtbox",
      };
     internal static int simActorLayer;
      internal static readonly string[]simActorLayerNames=new string[]{
       "Default",
      };
        internal static void SetUtil(){
         SetLayer(ref considerGroundLayer,considerGroundLayerNames);
         SetLayer(ref  physObstaclesLayer, physObstaclesLayerNames);
         SetLayer(ref   shootingHitsLayer,  shootingHitsLayerNames);
         SetLayer(ref       simActorLayer,      simActorLayerNames);
        }
        static void SetLayer(ref int layer,string[]layerNames){
         for(int i=0;i<layerNames.Length;++i){
          if(i==0){
           layer= LayerMask.GetMask(layerNames[i]);
          }else{
           layer|=LayerMask.GetMask(layerNames[i]);
          }
         }
        }
        ///<summary>
        ///  [https://answers.unity.com/questions/50279/check-if-layer-is-in-layermask.html]
        ///</summary>
        internal static bool LayerMaskContains(int layerMask,int layer){
         return layerMask==(layerMask|(1<<layer));
        }
        //  feito com ajuda do chatGPT
    /// <summary>
    /// Verifica se dois Bounds rotacionados e escalados se intersectam.
    /// Funciona como Bounds.Intersects, mas leva em conta rotação e escala.
    /// </summary>
    public static bool BoundsIntersectsRotatedAndScaled(
        Bounds boundsA, Quaternion rotationA, Vector3 scaleA,
        Bounds boundsB, Quaternion rotationB, Vector3 scaleB,
        float epsilon = 1e-6f)
    {
        // 1) Centros dentro um do outro
        if (PointInsideScaledRotatedBounds(boundsA, rotationA, scaleA, boundsB.center, epsilon))
            return true;
        if (PointInsideScaledRotatedBounds(boundsB, rotationB, scaleB, boundsA.center, epsilon))
            return true;

        // 2) Algum vértice dentro do outro
        foreach (var v in GetBoundsVertices(boundsA, rotationA, scaleA))
            if (PointInsideScaledRotatedBounds(boundsB, rotationB, scaleB, v, epsilon))
                return true;

        foreach (var v in GetBoundsVertices(boundsB, rotationB, scaleB))
            if (PointInsideScaledRotatedBounds(boundsA, rotationA, scaleA, v, epsilon))
                return true;

        // 3) Método SAT completo
        return CheckIntersectionUsingSAT(boundsA, rotationA, scaleA, boundsB, rotationB, scaleB, epsilon);
    }

    /// <summary>
    /// Verifica se um ponto está dentro de um Bounds rotacionado e escalado
    /// </summary>
    public static bool PointInsideScaledRotatedBounds(
        Bounds bounds, Quaternion rotation, Vector3 scale, Vector3 point, float epsilon = 1e-6f)
    {
        // Leve o ponto para o frame local do bounds
        Vector3 localPoint = point - bounds.center;
        localPoint = Quaternion.Inverse(rotation) * localPoint;
        localPoint = new Vector3(
            localPoint.x / scale.x,
            localPoint.y / scale.y,
            localPoint.z / scale.z
        );

        // Compare com as meias-extensões locais
        Vector3 halfExtents = bounds.extents;
        return Mathf.Abs(localPoint.x) <= halfExtents.x + epsilon &&
               Mathf.Abs(localPoint.y) <= halfExtents.y + epsilon &&
               Mathf.Abs(localPoint.z) <= halfExtents.z + epsilon;
    }

    /// <summary>
    /// Retorna todos os vértices de um Bounds rotacionado e escalado
    /// </summary>
    public static IEnumerable<Vector3> GetBoundsVertices(Bounds bounds, Quaternion rotation, Vector3 scale)
    {
        Vector3 e = Vector3.Scale(bounds.extents, scale); // meias-extensões escaladas
        Vector3[] axes = { rotation * Vector3.right, rotation * Vector3.up, rotation * Vector3.forward };
        for (int x = -1; x <= 1; x += 2)
            for (int y = -1; y <= 1; y += 2)
                for (int z = -1; z <= 1; z += 2)
                    yield return bounds.center + x * e.x * axes[0] + y * e.y * axes[1] + z * e.z * axes[2];
    }

    /// <summary>
    /// SAT completo entre dois OBB (Bounds + rotação + escala)
    /// </summary>
    public static bool CheckIntersectionUsingSAT(
        Bounds boundsA, Quaternion rotA, Vector3 scaleA,
        Bounds boundsB, Quaternion rotB, Vector3 scaleB,
        float epsilon = 1e-6f)
    {
        Vector3[] axesA = { rotA * Vector3.right, rotA * Vector3.up, rotA * Vector3.forward };
        Vector3[] axesB = { rotB * Vector3.right, rotB * Vector3.up, rotB * Vector3.forward };

        Vector3 aExt = Vector3.Scale(boundsA.extents, scaleA);
        Vector3 bExt = Vector3.Scale(boundsB.extents, scaleB);

        Vector3 t = boundsB.center - boundsA.center; // centro diferença em world

        // Projete t no frame A
        float[] tA = new float[3] {
            Vector3.Dot(t, axesA[0]),
            Vector3.Dot(t, axesA[1]),
            Vector3.Dot(t, axesA[2])
        };

        // Matriz de rotação R = Ai · Bj
        float[,] R = new float[3, 3];
        float[,] absR = new float[3, 3];
        for (int i = 0; i < 3; i++)
            for (int j = 0; j < 3; j++)
            {
                R[i, j] = Vector3.Dot(axesA[i], axesB[j]);
                absR[i, j] = Mathf.Abs(R[i, j]) + epsilon;
            }

        // Teste 15 eixos (A0,A1,A2,B0,B1,B2, AxB)
        for (int i = 0; i < 3; i++)
        {
            float ra = aExt[i];
            float rb = bExt.x * absR[i, 0] + bExt.y * absR[i, 1] + bExt.z * absR[i, 2];
            if (Mathf.Abs(tA[i]) > ra + rb) return false;
        }
        for (int j = 0; j < 3; j++)
        {
            float ra = aExt.x * absR[0, j] + aExt.y * absR[1, j] + aExt.z * absR[2, j];
            float rb = bExt[j];
            float tDotB = tA[0] * R[0, j] + tA[1] * R[1, j] + tA[2] * R[2, j];
            if (Mathf.Abs(tDotB) > ra + rb) return false;
        }

        // Eixos cruzados AxB
        for (int i = 0; i < 3; i++)
            for (int j = 0; j < 3; j++)
            {
                int i1 = (i + 1) % 3, i2 = (i + 2) % 3;
                int j1 = (j + 1) % 3, j2 = (j + 2) % 3;
                float ra = aExt[i1] * absR[i2, j] + aExt[i2] * absR[i1, j];
                float rb = bExt[j1] * absR[i, j2] + bExt[j2] * absR[i, j1];
                float tval = tA[i2] * R[i1, j] - tA[i1] * R[i2, j];
                if (Mathf.Abs(tval) > ra + rb) return false;
            }

        // Nenhum eixo separador encontrado -> intersecta
        return true;
    }
//        ///<summary>
//        ///  Testa interseção entre dois Bounds (Unity) aplicando rotação e escala em torno de cada Bounds.center.
//        ///  Usa Separating Axis Theorem (15 eixos) para OBB vs OBB.
//        /// - touching (apenas tocar) é considerado intersect.
//        /// - scale negativo é tratado com Mathf.Abs (mirroring não afeta magnitudes).
//        ///</summary>
//        ///<param name="boundsA">Bounds A (center em world space)</param>
//        ///<param name="rotA">Rotação a aplicar a A (em torno de boundsA.center)</param>
//        ///<param name="scaleA">Escala a aplicar a A (component-wise, >= 0 preferencial)</param>
//        ///<param name="boundsB">Bounds B (center em world space)</param>
//        ///<param name="rotB">Rotação a aplicar a B (em torno de boundsB.center)</param>
//        ///<param name="scaleB">Escala a aplicar a B (component-wise, >= 0 preferencial)</param>
//        ///<param name="epsilon">Pequena tolerância numérica (padrão 1e-6)</param>
//        ///<returns>true se intersectam (inclui touching), false se separados</returns>
//        internal static bool BoundsIntersectsScaledRotated(
//         Bounds boundsA,Quaternion rotA,Vector3 scaleA,
//         Bounds boundsB,Quaternion rotB,Vector3 scaleB,
//         float epsilon=1e-6f
//        ){
//         if(PointInsideScaledRotatedBounds(boundsA,rotA,scaleA,boundsB.center,epsilon))return true;//  Teste extra: centro de B dentro de A
//         if(PointInsideScaledRotatedBounds(boundsB,rotB,scaleB,boundsA.center,epsilon))return true;//  Teste extra: centro de A dentro de B
//    // Teste extra: vértices de B dentro de A
//    foreach (var vertex in GetBoundsVertices(boundsB, rotB, scaleB)) {
//        if (PointInsideScaledRotatedBounds(boundsA, rotA, scaleA, vertex, epsilon)) return true;
//    }
//    // Teste extra: vértices de A dentro de B
//    foreach (var vertex in GetBoundsVertices(boundsA, rotA, scaleA)) {
//        if (PointInsideScaledRotatedBounds(boundsB, rotB, scaleB, vertex, epsilon)) return true;
//    }
//         //---  Semi-extents (meio do size) multiplicado pela escala (usando abs para segurança)  ---
//         Vector3 halfA=Vector3.Scale(boundsA.size*0.5f,new Vector3(Mathf.Abs(scaleA.x),Mathf.Abs(scaleA.y),Mathf.Abs(scaleA.z)));
//         Vector3 halfB=Vector3.Scale(boundsB.size*0.5f,new Vector3(Mathf.Abs(scaleB.x),Mathf.Abs(scaleB.y),Mathf.Abs(scaleB.z)));
//         //---  Centros em world space  ---
//         Vector3 CA=boundsA.center;
//         Vector3 CB=boundsB.center;
//         //---  Eixos locais (unitários) em world space  ---
//         Vector3 A0=rotA*Vector3.right  ;//  local X
//         Vector3 A1=rotA*Vector3.up     ;//  local Y
//         Vector3 A2=rotA*Vector3.forward; // local Z
//         Vector3 B0=rotB*Vector3.right  ;
//         Vector3 B1=rotB*Vector3.up     ;
//         Vector3 B2=rotB*Vector3.forward;
//         //---  Half-lengths along each local axis  ---
//         float a0=halfA.x;
//         float a1=halfA.y;
//         float a2=halfA.z;
//         float b0=halfB.x;
//         float b1=halfB.y;
//         float b2=halfB.z;
//         //---  Compute rotation matrix R = (Ai dot Bj) and absR with epsilon  ---
//         //  R_ij = dot(A_i,B_j)
//         float r00=Vector3.Dot(A0,B0);
//         float r01=Vector3.Dot(A0,B1);
//         float r02=Vector3.Dot(A0,B2);
//         float r10=Vector3.Dot(A1,B0);
//         float r11=Vector3.Dot(A1,B1);
//         float r12=Vector3.Dot(A1,B2);
//         float r20=Vector3.Dot(A2,B0);
//         float r21=Vector3.Dot(A2,B1);
//         float r22=Vector3.Dot(A2,B2);
//         //  absR with epsilon to counteract arithmetic errors (and to treat near-zero as zero)
//         float absR00=Mathf.Abs(r00)+epsilon;
//         float absR01=Mathf.Abs(r01)+epsilon;
//         float absR02=Mathf.Abs(r02)+epsilon;
//         float absR10=Mathf.Abs(r10)+epsilon;
//         float absR11=Mathf.Abs(r11)+epsilon;
//         float absR12=Mathf.Abs(r12)+epsilon;
//         float absR20=Mathf.Abs(r20)+epsilon;
//         float absR21=Mathf.Abs(r21)+epsilon;
//         float absR22=Mathf.Abs(r22)+epsilon;
//         //---  Vector t (center difference) expressed in A's frame: tA[i] = dot(t,A_i)  ---
//         Vector3 t=CB-CA;
//         float tA0=Vector3.Dot(t,A0);
//         float tA1=Vector3.Dot(t,A1);
//         float tA2=Vector3.Dot(t,A2);
//         //---  Test axes L = A0,A1,A2  ---
//         {
//          float ra=a0;
//          float rb=b0*absR00+b1*absR01+b2*absR02;
//          if(Mathf.Abs(tA0)>ra+rb)return false;
//         }
//         {
//          float ra=a1;
//          float rb=b0*absR10+b1*absR11+b2*absR12;
//          if(Mathf.Abs(tA1)>ra+rb)return false;
//         }
//         {
//          float ra=a2;
//          float rb=b0*absR20+b1*absR21+b2*absR22;
//          if(Mathf.Abs(tA2)>ra+rb)return false;
//         }
//         //---  Test axes L = B0,B1,B2  ---
//         //  t dot B_j = sum_i tA_i * R_ij
//         {
//          float tDotB0=tA0*r00+tA1*r10+tA2*r20;
//          float ra=a0*absR00+a1*absR10+a2*absR20;
//          float rb=b0;
//          if(Mathf.Abs(tDotB0)>ra+rb)return false;
//         }
//         {
//          float tDotB1=tA0*r01+tA1*r11+tA2*r21;
//          float ra=a0*absR01+a1*absR11+a2*absR21;
//          float rb=b1;
//          if(Mathf.Abs(tDotB1)>ra+rb)return false;
//         }
//         {
//          float tDotB2=tA0*r02+tA1*r12+tA2*r22;
//          float ra=a0*absR02+a1*absR12+a2*absR22;
//          float rb=b2;
//          if(Mathf.Abs(tDotB2)>ra+rb)return false;
//         }
//         //---  Test axes L = A0 x B0  ---
//         {
//          float ra=a1*absR20+a2*absR10;
//          float rb=b1*absR02+b2*absR01;
//          float tval=tA2*r10-tA1*r20;
//          if(Mathf.Abs(tval)>ra+rb)return false;
//         }
//         //  L = A0 x B1
//         {
//          float ra=a1*absR21+a2*absR11;
//          float rb=b0*absR02+b2*absR00;
//          float tval=tA2*r11-tA1*r21;
//          if(Mathf.Abs(tval)>ra+rb)return false;
//         }
//         //  L = A0 x B2
//         {
//          float ra=a1*absR22+a2*absR12;
//          float rb=b0*absR01+b1*absR00;
//          float tval=tA2*r12-tA1*r22;
//          if(Mathf.Abs(tval)>ra+rb)return false;
//         }
//         //  L = A1 x B0
//         {
//          float ra=a0*absR20+a2*absR00;
//          float rb=b1*absR12+b2*absR11;
//          float tval=tA0*r20-tA2*r00;
//          if(Mathf.Abs(tval)>ra+rb)return false;
//         }
//         //  L = A1 x B1
//         {
//          float ra=a0*absR21+a2*absR01;
//          float rb=b0*absR12+b2*absR10;
//          float tval=tA0*r21-tA2*r01;
//          if(Mathf.Abs(tval)>ra+rb)return false;
//         }
//         //  L = A1 x B2
//         {
//          float ra=a0*absR22+a2*absR02;
//          float rb=b0*absR11+b1*absR10;
//          float tval=tA0*r22-tA2*r02;
//          if(Mathf.Abs(tval)>ra+rb)return false;
//         }
//         //  L = A2 x B0
//         {
//          float ra=a0*absR10+a1*absR00;
//          float rb=b1*absR22+b2*absR21;
//          float tval=tA1*r00-tA0*r10;
//          if(Mathf.Abs(tval)>ra+rb)return false;
//         }
//         //  L = A2 x B1
//         {
//          float ra=a0*absR11+a1*absR01;
//          float rb=b0*absR22+b2*absR20;
//          float tval=tA1*r01-tA0*r11;
//          if(Mathf.Abs(tval)>ra+rb)return false;
//         }
//         //  L = A2 x B2
//         {
//          float ra=a0*absR12+a1*absR02;
//          float rb=b0*absR21+b1*absR20;
//          float tval=tA1*r02-tA0*r12;
//          if(Mathf.Abs(tval)>ra+rb)return false;
//         }
//         //  No separating axis found -> intersect
//         return true;
//        }
//static IEnumerable<Vector3> GetBoundsVertices(Bounds bounds, Quaternion rot, Vector3 scale) {
//    Vector3 center = bounds.center;
//    Vector3 ext = Vector3.Scale(bounds.extents, new Vector3(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z)));
//    for (int x = -1; x <= 1; x += 2)
//    for (int y = -1; y <= 1; y += 2)
//    for (int z = -1; z <= 1; z += 2) {
//        Vector3 local = new Vector3(x * ext.x, y * ext.y, z * ext.z);
//        yield return center + rot * local;
//    }
//}
//internal static bool PointInsideScaledRotatedBounds(
// Bounds bounds, Quaternion rot, Vector3 scale,
// Vector3 pos, float epsilon = 1e-6f
//){
// Vector3 scaledSize = Vector3.Scale(bounds.size, new Vector3(
//   Mathf.Abs(scale.x),
//   Mathf.Abs(scale.y),
//   Mathf.Abs(scale.z)
// ));
// Vector3 localPoint = pos - bounds.center;
// localPoint = Quaternion.Inverse(rot) * localPoint;
// localPoint = new Vector3(
//   localPoint.x / scale.x,
//   localPoint.y / scale.y,
//   localPoint.z / scale.z
// );
// Vector3 halfExtents = bounds.size * 0.5f; // use o bounds original, pois o ponto já foi ajustado pela escala
// return
//  localPoint.x >= -halfExtents.x - epsilon && localPoint.x <= halfExtents.x + epsilon &&
//  localPoint.y >= -halfExtents.y - epsilon && localPoint.y <= halfExtents.y + epsilon &&
//  localPoint.z >= -halfExtents.z - epsilon && localPoint.z <= halfExtents.z + epsilon;
//}
        //  feito com ajuda do chatGPT,
        // versão 3D
        //internal static IEnumerable<(int x,int y,int z)>GetCoords3DInsideBounds(
        // Bounds bounds,Quaternion rot,Vector3 scale,Vector3 margin
        //){
        // //  Aplicar escala nos extents
        // Vector3 scaledExtents=Vector3.Scale(bounds.extents,scale)+margin;
        // //  Calcular matriz de rotação
        // Matrix4x4 rotationMatrix=Matrix4x4.Rotate(rot);
        // //  Calcular AABB que engloba o OBB
        // Vector3 right  =rotationMatrix.MultiplyVector(Vector3.right  )*scaledExtents.x;
        // Vector3 up     =rotationMatrix.MultiplyVector(Vector3.up     )*scaledExtents.y;
        // Vector3 forward=rotationMatrix.MultiplyVector(Vector3.forward)*scaledExtents.z;
        // Vector3 worldExtents=new Vector3(
        //  Mathf.Abs(right.x)+Mathf.Abs(up.x)+Mathf.Abs(forward.x),
        //  Mathf.Abs(right.y)+Mathf.Abs(up.y)+Mathf.Abs(forward.y),
        //  Mathf.Abs(right.z)+Mathf.Abs(up.z)+Mathf.Abs(forward.z)
        // );
        // //  Determinar limites globais de iteração
        // Vector3 min=bounds.center-worldExtents;
        // Vector3 max=bounds.center+worldExtents;
        // //  Iterar por todas as coordenadas inteiras no AABB
        // for(int x=Mathf.FloorToInt(min.x);x<=Mathf.CeilToInt(max.x);x++)
        // for(int y=Mathf.FloorToInt(min.y);y<=Mathf.CeilToInt(max.y);y++)
        // for(int z=Mathf.FloorToInt(min.z);z<=Mathf.CeilToInt(max.z);z++){
        //  if(GetCoordsPointInsideScaledRotatedBounds(new Vector3(x,y,z),bounds,rot,scale,margin))
        //   yield return(x,y,z);
        // }
        //}
        ////  feito com ajuda do chatGPT,
        //// versão 2D (XZ)
        //internal static IEnumerable<(int x,int z)>GetCoordsInsideBounds(
        // Bounds bounds,Quaternion rot,Vector3 scale,Vector3 margin
        //){
        // Vector3 scaledExtents=Vector3.Scale(bounds.extents,scale)+margin;
        // Matrix4x4 rotationMatrix=Matrix4x4.Rotate(rot);
        // Vector3 right  =rotationMatrix.MultiplyVector(Vector3.right  )*scaledExtents.x;
        // Vector3 up     =rotationMatrix.MultiplyVector(Vector3.up     )*scaledExtents.y;
        // Vector3 forward=rotationMatrix.MultiplyVector(Vector3.forward)*scaledExtents.z;
        // Vector3 worldExtents=new Vector3(
        //  Mathf.Abs(right.x)+Mathf.Abs(up.x)+Mathf.Abs(forward.x),
        //  Mathf.Abs(right.y)+Mathf.Abs(up.y)+Mathf.Abs(forward.y),
        //  Mathf.Abs(right.z)+Mathf.Abs(up.z)+Mathf.Abs(forward.z)
        // );
        // Vector3 min=bounds.center-worldExtents;
        // Vector3 max=bounds.center+worldExtents;
        // //  apenas XZ
        // for(int x=Mathf.FloorToInt(min.x);x<=Mathf.CeilToInt(max.x);x++)
        // for(int z=Mathf.FloorToInt(min.z);z<=Mathf.CeilToInt(max.z);z++){
        //  Vector3 point=new Vector3(x,bounds.center.y,z);
        //  if(GetCoordsPointInsideScaledRotatedBounds(point,bounds,rot,scale,margin))
        //   yield return(x,z);
        // }
        //}
        ////  método de checagem otimizado para usar em GetCoords3D e GetCoords acima
        //private static bool GetCoordsPointInsideScaledRotatedBounds(
        // Vector3 point,Bounds bounds,Quaternion rot,Vector3 scale,Vector3 margin
        //){
        // Vector3 localPoint=Quaternion.Inverse(rot)*(point-bounds.center);
        // localPoint=new Vector3(
        //  localPoint.x/scale.x,
        //  localPoint.y/scale.y,
        //  localPoint.z/scale.z
        // );
        // return Mathf.Abs(localPoint.x)<=bounds.extents.x+(margin.x/scale.x)&&
        //        Mathf.Abs(localPoint.y)<=bounds.extents.y+(margin.y/scale.y)&&
        //        Mathf.Abs(localPoint.z)<=bounds.extents.z+(margin.z/scale.z);
        //}
internal static int GetCoords3DInsideBoundsUsingParallelFor(
    Bounds bounds, Vector3 scale, Quaternion rotation, Vector3 margin,
    bool sorted, Vector3Int[] outputArray
)
{
    Vector3 scaledExtents = Vector3.Scale(bounds.extents, scale);
    Vector3 finalExtents = scaledExtents + margin;
    Vector3 min = bounds.center - finalExtents;
    Vector3 max = bounds.center + finalExtents;

    int index = 0;

    // Lista local por thread para evitar lock constante
    System.Collections.Concurrent.ConcurrentBag<Vector3Int> bag = new();

    Parallel.For((int)Mathf.FloorToInt(min.x), (int)Mathf.CeilToInt(max.x) + 1, x =>
    {
        List<Vector3Int> localList = new List<Vector3Int>();
        for (int y = Mathf.FloorToInt(min.y); y <= Mathf.CeilToInt(max.y); y++)
        {
            for (int z = Mathf.FloorToInt(min.z); z <= Mathf.CeilToInt(max.z); z++)
            {
                Vector3 point = new Vector3(x, y, z);
                // Rotação correta: transformar ponto para frame local
                Vector3 localPoint = Quaternion.Inverse(rotation) * (point - bounds.center);

                if (Mathf.Abs(localPoint.x) <= finalExtents.x &&
                    Mathf.Abs(localPoint.y) <= finalExtents.y &&
                    Mathf.Abs(localPoint.z) <= finalExtents.z)
                {
                    localList.Add(new Vector3Int(x, y, z));
                }
            }
        }
        // Adiciona a lista local ao ConcurrentBag
        foreach (var v in localList) bag.Add(v);
    });

    // Copiar do bag para outputArray
    foreach (var v in bag) outputArray[index++] = v;

    // Ordenação opcional
    if (sorted && index > 1)
        Array.Sort(outputArray, 0, index, Comparer<Vector3Int>.Create((a, b) =>
        {
            int cmpX = a.x.CompareTo(b.x); if (cmpX != 0) return cmpX;
            int cmpY = a.y.CompareTo(b.y); if (cmpY != 0) return cmpY;
            return a.z.CompareTo(b.z);
        }));

    return index;
}
        //---  2D (X e Z somente)  ---
internal static int GetCoordsInsideBoundsUsingParallelFor(
    Bounds bounds, Vector3 scale, Quaternion rotation, Vector3 margin,
    bool sorted, Vector3Int[] outputArray, HashSet<Vector3Int> ignored = null
)
{
    Vector3 scaledExtents = Vector3.Scale(bounds.extents, scale);
    Vector3 finalExtents = scaledExtents + margin;
    Vector3 min = bounds.center - finalExtents;
    Vector3 max = bounds.center + finalExtents;

    int index = 0;

    // Lista local por thread para evitar lock constante
    System.Collections.Concurrent.ConcurrentBag<Vector3Int> bag = new();

    Parallel.For((int)Mathf.FloorToInt(min.x), (int)Mathf.CeilToInt(max.x) + 1, x =>
    {
        List<Vector3Int> localList = new List<Vector3Int>();

        for (int z = Mathf.FloorToInt(min.z); z <= Mathf.CeilToInt(max.z); z++)
        {
            Vector3 point = new Vector3(x, bounds.center.y, z);

            // Rotação correta: ponto para frame local
            Vector3 localPoint = Quaternion.Inverse(rotation) * (point - bounds.center);

            if (Mathf.Abs(localPoint.x) <= finalExtents.x &&
                Mathf.Abs(localPoint.z) <= finalExtents.z)
            {
                Vector3Int coord = new Vector3Int(x, Mathf.RoundToInt(bounds.center.y), z);

                if (ignored == null || !ignored.Contains(coord))
                {
                    localList.Add(coord);
                }
            }
        }

        // Adiciona a lista local ao ConcurrentBag
        foreach (var v in localList) bag.Add(v);
    });

    // Copiar do bag para outputArray
    foreach (var v in bag) outputArray[index++] = v;

    // Ordenação opcional
    if (sorted && index > 1)
        Array.Sort(outputArray, 0, index, Comparer<Vector3Int>.Create((a, b) =>
        {
            int cmpX = a.x.CompareTo(b.x); if (cmpX != 0) return cmpX;
            return a.z.CompareTo(b.z);
        }));

    return index;
}
        //internal static int GetCoordsInsideBoundsUsingParallelFor(
        // Bounds bounds,Vector3 scale,Quaternion rotation,Vector3 margin,
        // bool sorted,Vector3Int[]outputArray,HashSet<Vector3Int>ignored=null
        //){
        // Vector3 scaledExtents=Vector3.Scale(bounds.extents,scale);
        // Vector3 finalExtents=scaledExtents+margin;
        // Vector3 min=bounds.center-finalExtents;
        // Vector3 max=bounds.center+finalExtents;
        // int index=0;
        // Parallel.For((int)Math.Floor(min.x),   (int)Math.Ceiling(max.x)+1,x=>{
        //    for(int z=(int)Math.Floor(min.z);z<=(int)Math.Ceiling(max.z);  z++){
        //     Vector3 localPoint=new Vector3(x,bounds.center.y,z)-bounds.center;
        //             localPoint=rotation*localPoint;
        //     if(
        //      Mathf.Abs(localPoint.x)<=finalExtents.x&&
        //      Mathf.Abs(localPoint.z)<=finalExtents.z
        //     ){
        //      Vector3Int coord=new Vector3Int(x,Mathf.RoundToInt(bounds.center.y),z);
        //      if(ignored==null||!ignored.Contains(coord)){
        //       lock(outputArray){
        //        outputArray[index++]=coord;
        //       }
        //      }
        //     }
        //    }
        // });
        // if(sorted&&index>1)
        //  Array.Sort(outputArray,0,index,Comparer<Vector3Int>.Create((a,b)=>{
        //   int cmpX=a.x.CompareTo(b.x);if(cmpX!=0)return cmpX;
        //     return a.z.CompareTo(b.z);
        //  }));
        // return index;
        //}
        ///<summary>
        ///  Calcula o tamanho necessário do array para armazenar todos os pontos inteiros dentro de um Bounds escalado, com margem.
        ///</summary>
        ///<param name="bounds">Bounds original (sem escala aplicada)</param>
        ///<param name="scale">Escala a ser aplicada</param>
        ///<param name="margin">Margem adicional aplicada após escala</param>
        ///<returns>Tamanho mínimo seguro do array para armazenar todas as coordenadas</returns>
        internal static int GetCoordsInsideBoundsMinArraySize3D(Bounds bounds,Vector3 scale,Vector3 margin){
         //  Aplica escala e adiciona margem
         Vector3 scaledExtents=Vector3.Scale(bounds.extents,scale)+margin;
         //  Conta número de coordenadas inteiras por eixo
         int countX=Mathf.CeilToInt(scaledExtents.x*2)+1;
         int countY=Mathf.CeilToInt(scaledExtents.y*2)+1;
         int countZ=Mathf.CeilToInt(scaledExtents.z*2)+1;
         //  Retorna o produto total
         return countX*countY*countZ;
        }
        ///<summary>
        ///  Calcula o tamanho necessário do array para armazenar todas as coordenadas XZ dentro de um Bounds escalado, com margem.
        ///</summary>
        internal static int GetCoordsInsideBoundsMinArraySize(Bounds bounds,Vector3 scale,Vector3 margin){
         Vector3 scaledExtents=Vector3.Scale(bounds.extents,scale)+margin;
         int countX=Mathf.CeilToInt(scaledExtents.x*2)+1;
         int countZ=Mathf.CeilToInt(scaledExtents.z*2)+1;
         return countX*countZ;
        }
    }
}