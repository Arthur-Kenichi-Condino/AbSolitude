using UnityEngine;
namespace AKCondinoO.Utilities{
    internal static class DrawGizmos{
        ///<summary>
        ///  [https://gist.github.com/unitycoder/58f4b5d80f423d29e35c814a9556f9d9]
        ///</summary>
        ///<param name="b"></param>
        ///<param name="color"></param>
        ///<param name="duration"></param>
        internal static void Bounds(Bounds b,Color color,float duration=0){
         Vector3 p1=new(b.min.x,b.min.y,b.min.z);// bottom
         Vector3 p2=new(b.max.x,b.min.y,b.min.z);
         Vector3 p3=new(b.max.x,b.min.y,b.max.z);
         Vector3 p4=new(b.min.x,b.min.y,b.max.z);
         Vector3 p5=new(b.min.x,b.max.y,b.min.z);// top
         Vector3 p6=new(b.max.x,b.max.y,b.min.z);
         Vector3 p7=new(b.max.x,b.max.y,b.max.z);
         Vector3 p8=new(b.min.x,b.max.y,b.max.z);
         UnityEngine.Debug.DrawLine(p1,p2,color,duration);
         UnityEngine.Debug.DrawLine(p2,p3,color,duration);
         UnityEngine.Debug.DrawLine(p3,p4,color,duration);
         UnityEngine.Debug.DrawLine(p4,p1,color,duration);
         UnityEngine.Debug.DrawLine(p5,p6,color,duration);
         UnityEngine.Debug.DrawLine(p6,p7,color,duration);
         UnityEngine.Debug.DrawLine(p7,p8,color,duration);
         UnityEngine.Debug.DrawLine(p8,p5,color,duration);
         UnityEngine.Debug.DrawLine(p1,p5,color,duration);// sides
         UnityEngine.Debug.DrawLine(p2,p6,color,duration);
         UnityEngine.Debug.DrawLine(p3,p7,color,duration);
         UnityEngine.Debug.DrawLine(p4,p8,color,duration);
        }
    }
}