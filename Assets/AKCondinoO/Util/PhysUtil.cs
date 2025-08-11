using System.Collections;
using System.Collections.Generic;
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
        /// <summary>
        ///  [https://answers.unity.com/questions/50279/check-if-layer-is-in-layermask.html]
        /// </summary>
        internal static bool LayerMaskContains(int layerMask,int layer){
         return layerMask==(layerMask|(1<<layer));
        }
        /// <summary>
        ///  Testa interseção entre dois Bounds (Unity) aplicando rotação e escala em torno de cada Bounds.center.
        ///  Usa Separating Axis Theorem (15 eixos) para OBB vs OBB.
        /// - touching (apenas tocar) é considerado intersect.
        /// - scale negativo é tratado com Mathf.Abs (mirroring não afeta magnitudes).
        /// </summary>
        /// <param name="boundsA">Bounds A (center em world space)</param>
        /// <param name="rotA">Rotação a aplicar a A (em torno de boundsA.center)</param>
        /// <param name="scaleA">Escala a aplicar a A (component-wise, >= 0 preferencial)</param>
        /// <param name="boundsB">Bounds B (center em world space)</param>
        /// <param name="rotB">Rotação a aplicar a B (em torno de boundsB.center)</param>
        /// <param name="scaleB">Escala a aplicar a B (component-wise, >= 0 preferencial)</param>
        /// <param name="epsilon">Pequena tolerância numérica (padrão 1e-6)</param>
        /// <returns>true se intersectam (inclui touching), false se separados</returns>
        public static bool BoundsIntersectsScaledRotated(
            Bounds boundsA, Quaternion rotA, Vector3 scaleA,
            Bounds boundsB, Quaternion rotB, Vector3 scaleB,
            float epsilon = 1e-6f)
        {
            // --- Semi-extents (meio do size) multiplicado pela escala (usando abs para segurança) ---
            Vector3 halfA = Vector3.Scale(boundsA.size * 0.5f, new Vector3(Mathf.Abs(scaleA.x), Mathf.Abs(scaleA.y), Mathf.Abs(scaleA.z)));
            Vector3 halfB = Vector3.Scale(boundsB.size * 0.5f, new Vector3(Mathf.Abs(scaleB.x), Mathf.Abs(scaleB.y), Mathf.Abs(scaleB.z)));

            // --- Centros em world space ---
            Vector3 CA = boundsA.center;
            Vector3 CB = boundsB.center;

            // --- Eixos locais (unitários) em world space ---
            Vector3 A0 = rotA * Vector3.right;   // local X
            Vector3 A1 = rotA * Vector3.up;      // local Y
            Vector3 A2 = rotA * Vector3.forward; // local Z

            Vector3 B0 = rotB * Vector3.right;
            Vector3 B1 = rotB * Vector3.up;
            Vector3 B2 = rotB * Vector3.forward;

            // --- Half-lengths along each local axis ---
            float a0 = halfA.x;
            float a1 = halfA.y;
            float a2 = halfA.z;

            float b0 = halfB.x;
            float b1 = halfB.y;
            float b2 = halfB.z;

            // --- Compute rotation matrix R = [Ai dot Bj] and absR with epsilon ---
            // R_ij = dot(A_i, B_j)
            float r00 = Vector3.Dot(A0, B0);
            float r01 = Vector3.Dot(A0, B1);
            float r02 = Vector3.Dot(A0, B2);

            float r10 = Vector3.Dot(A1, B0);
            float r11 = Vector3.Dot(A1, B1);
            float r12 = Vector3.Dot(A1, B2);

            float r20 = Vector3.Dot(A2, B0);
            float r21 = Vector3.Dot(A2, B1);
            float r22 = Vector3.Dot(A2, B2);

            // absR with epsilon to counteract arithmetic errors (and to treat near-zero as zero)
            float absR00 = Mathf.Abs(r00) + epsilon;
            float absR01 = Mathf.Abs(r01) + epsilon;
            float absR02 = Mathf.Abs(r02) + epsilon;

            float absR10 = Mathf.Abs(r10) + epsilon;
            float absR11 = Mathf.Abs(r11) + epsilon;
            float absR12 = Mathf.Abs(r12) + epsilon;

            float absR20 = Mathf.Abs(r20) + epsilon;
            float absR21 = Mathf.Abs(r21) + epsilon;
            float absR22 = Mathf.Abs(r22) + epsilon;

            // --- Vector t (center difference) expressed in A's frame: tA[i] = dot(t, A_i) ---
            Vector3 t = CB - CA;
            float tA0 = Vector3.Dot(t, A0);
            float tA1 = Vector3.Dot(t, A1);
            float tA2 = Vector3.Dot(t, A2);

            // --- Test axes L = A0, A1, A2 ---
            {
                float ra = a0;
                float rb = b0 * absR00 + b1 * absR01 + b2 * absR02;
                if (Mathf.Abs(tA0) > ra + rb) return false;
            }
            {
                float ra = a1;
                float rb = b0 * absR10 + b1 * absR11 + b2 * absR12;
                if (Mathf.Abs(tA1) > ra + rb) return false;
            }
            {
                float ra = a2;
                float rb = b0 * absR20 + b1 * absR21 + b2 * absR22;
                if (Mathf.Abs(tA2) > ra + rb) return false;
            }

            // --- Test axes L = B0, B1, B2 ---
            // t dot B_j = sum_i tA_i * R_ij
            {
                float tDotB0 = tA0 * r00 + tA1 * r10 + tA2 * r20;
                float ra = a0 * absR00 + a1 * absR10 + a2 * absR20;
                float rb = b0;
                if (Mathf.Abs(tDotB0) > ra + rb) return false;
            }
            {
                float tDotB1 = tA0 * r01 + tA1 * r11 + tA2 * r21;
                float ra = a0 * absR01 + a1 * absR11 + a2 * absR21;
                float rb = b1;
                if (Mathf.Abs(tDotB1) > ra + rb) return false;
            }
            {
                float tDotB2 = tA0 * r02 + tA1 * r12 + tA2 * r22;
                float ra = a0 * absR02 + a1 * absR12 + a2 * absR22;
                float rb = b2;
                if (Mathf.Abs(tDotB2) > ra + rb) return false;
            }

            // --- Test axes L = A0 x B0 ---
            {
                float ra = a1 * absR20 + a2 * absR10;
                float rb = b1 * absR02 + b2 * absR01;
                float tval = tA2 * r10 - tA1 * r20;
                if (Mathf.Abs(tval) > ra + rb) return false;
            }

            // L = A0 x B1
            {
                float ra = a1 * absR21 + a2 * absR11;
                float rb = b0 * absR02 + b2 * absR00;
                float tval = tA2 * r11 - tA1 * r21;
                if (Mathf.Abs(tval) > ra + rb) return false;
            }

            // L = A0 x B2
            {
                float ra = a1 * absR22 + a2 * absR12;
                float rb = b0 * absR01 + b1 * absR00;
                float tval = tA2 * r12 - tA1 * r22;
                if (Mathf.Abs(tval) > ra + rb) return false;
            }

            // L = A1 x B0
            {
                float ra = a0 * absR20 + a2 * absR00;
                float rb = b1 * absR12 + b2 * absR11;
                float tval = tA0 * r20 - tA2 * r00;
                if (Mathf.Abs(tval) > ra + rb) return false;
            }

            // L = A1 x B1
            {
                float ra = a0 * absR21 + a2 * absR01;
                float rb = b0 * absR12 + b2 * absR10;
                float tval = tA0 * r21 - tA2 * r01;
                if (Mathf.Abs(tval) > ra + rb) return false;
            }

            // L = A1 x B2
            {
                float ra = a0 * absR22 + a2 * absR02;
                float rb = b0 * absR11 + b1 * absR10;
                float tval = tA0 * r22 - tA2 * r02;
                if (Mathf.Abs(tval) > ra + rb) return false;
            }

            // L = A2 x B0
            {
                float ra = a0 * absR10 + a1 * absR00;
                float rb = b1 * absR22 + b2 * absR21;
                float tval = tA1 * r00 - tA0 * r10;
                if (Mathf.Abs(tval) > ra + rb) return false;
            }

            // L = A2 x B1
            {
                float ra = a0 * absR11 + a1 * absR01;
                float rb = b0 * absR22 + b2 * absR20;
                float tval = tA1 * r01 - tA0 * r11;
                if (Mathf.Abs(tval) > ra + rb) return false;
            }

            // L = A2 x B2
            {
                float ra = a0 * absR12 + a1 * absR02;
                float rb = b0 * absR21 + b1 * absR20;
                float tval = tA1 * r02 - tA0 * r12;
                if (Mathf.Abs(tval) > ra + rb) return false;
            }

            // No separating axis found -> intersect
            return true;
        }
    }
}