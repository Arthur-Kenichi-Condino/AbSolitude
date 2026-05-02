using Unity.VisualScripting;
using UnityEngine;
namespace AKCondinoO{
    internal static class PhysicsUtil{
        internal struct OrientedBounds{
         public Vector3 center;
         public Vector3 axisX;
         public Vector3 axisY;
         public Vector3 axisZ;
         public Vector3 extents;
        }
        //  Feito com ajuda do(a) ChatGPT e do(a) Gemini
        internal static bool Intersects(this in OrientedBounds A, in OrientedBounds B) {
         Vector3 t=B.center-A.center;
         //  Converte t para o espaço de A
         Vector3 T=new Vector3(Vector3.Dot(t,A.axisX),Vector3.Dot(t,A.axisY),Vector3.Dot(t,A.axisZ));
         //  Matriz de Rotação Relativa
         float R00=Vector3.Dot(A.axisX,B.axisX),R01=Vector3.Dot(A.axisX,B.axisY),R02=Vector3.Dot(A.axisX,B.axisZ);
         float R10=Vector3.Dot(A.axisY,B.axisX),R11=Vector3.Dot(A.axisY,B.axisY),R12=Vector3.Dot(A.axisY,B.axisZ);
         float R20=Vector3.Dot(A.axisZ,B.axisX),R21=Vector3.Dot(A.axisZ,B.axisY),R22=Vector3.Dot(A.axisZ,B.axisZ);
         float absR00=Mathf.Abs(R00)+1e-7f;float absR01=Mathf.Abs(R01)+1e-7f;float absR02=Mathf.Abs(R02)+1e-7f;
         float absR10=Mathf.Abs(R10)+1e-7f;float absR11=Mathf.Abs(R11)+1e-7f;float absR12=Mathf.Abs(R12)+1e-7f;
         float absR20=Mathf.Abs(R20)+1e-7f;float absR21=Mathf.Abs(R21)+1e-7f;float absR22=Mathf.Abs(R22)+1e-7f;
         float ra,rb;
         //  Eixos de A
         if(Mathf.Abs(T.x)>A.extents.x+(B.extents.x*absR00+B.extents.y*absR01+B.extents.z*absR02))return false;
         if(Mathf.Abs(T.y)>A.extents.y+(B.extents.x*absR10+B.extents.y*absR11+B.extents.z*absR12))return false;
         if(Mathf.Abs(T.z)>A.extents.z+(B.extents.x*absR20+B.extents.y*absR21+B.extents.z*absR22))return false;
         //  Eixos de B
         if(Mathf.Abs(T.x*R00+T.y*R10+T.z*R20)>B.extents.x+(A.extents.x*absR00+A.extents.y*absR10+A.extents.z*absR20))return false;
         if(Mathf.Abs(T.x*R01+T.y*R11+T.z*R21)>B.extents.y+(A.extents.x*absR01+A.extents.y*absR11+A.extents.z*absR21))return false;
         if(Mathf.Abs(T.x*R02+T.y*R12+T.z*R22)>B.extents.z+(A.extents.x*absR02+A.extents.y*absR12+A.extents.z*absR22))return false;
         //  9 Eixos Cruzados
         //  A.x X B.x
         ra=A.extents.y*absR20+A.extents.z*absR10;
         rb=B.extents.y*absR02+B.extents.z*absR01;
         if(Mathf.Abs(T.z*R10-T.y*R20)>ra+rb)return false;
         //  A.x X B.y
         ra=A.extents.y*absR21+A.extents.z*absR11;
         rb=B.extents.x*absR02+B.extents.z*absR00;
         if(Mathf.Abs(T.z*R11-T.y*R21)>ra+rb)return false;
         //  A.x X B.z
         ra=A.extents.y*absR22+A.extents.z*absR12;
         rb=B.extents.x*absR01+B.extents.y*absR00;
         if(Mathf.Abs(T.z*R12-T.y*R22)>ra+rb)return false;
         //  A.y X B.x
         ra=A.extents.x*absR20+A.extents.z*absR00;
         rb=B.extents.y*absR12+B.extents.z*absR11;
         if(Mathf.Abs(T.x*R20-T.z*R00)>ra+rb)return false;
         //  A.y X B.y
         ra=A.extents.x*absR21+A.extents.z*absR01;
         rb=B.extents.x*absR12+B.extents.z*absR10;
         if(Mathf.Abs(T.x*R21-T.z*R01)>ra+rb)return false;
         //  A.y X B.z
         ra=A.extents.x*absR22+A.extents.z*absR02;
         rb=B.extents.x*absR11+B.extents.y*absR10;
         if(Mathf.Abs(T.x*R22-T.z*R02)>ra+rb)return false;
         //  A.z X B.x
         ra=A.extents.x*absR10+A.extents.y*absR00;
         rb=B.extents.y*absR22+B.extents.z*absR21;
         if(Mathf.Abs(T.y*R00-T.x*R10)>ra+rb)return false;
         //  A.z X B.y
         ra=A.extents.x*absR11+A.extents.y*absR01;
         rb=B.extents.x*absR22+B.extents.z*absR20;
         if(Mathf.Abs(T.y*R01-T.x*R11)>ra+rb)return false;
         //  A.z X B.z
         ra=A.extents.x*absR12+A.extents.y*absR02;
         rb=B.extents.x*absR21+B.extents.y*absR20;
         if(Mathf.Abs(T.y*R02-T.x*R12)>ra+rb)return false;
         return true;
        }
    }
}