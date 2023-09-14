using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO{
    internal static class DebugGizmos{
        //#if UNITY_EDITOR
            [System.Diagnostics.Conditional("ENABLE_DEBUG_GIZMOS")]
            internal static void DrawBounds(Bounds b,Color color,float duration=0){//[https://gist.github.com/unitycoder/58f4b5d80f423d29e35c814a9556f9d9]
             var p1=new Vector3(b.min.x,b.min.y,b.min.z);// bottom
             var p2=new Vector3(b.max.x,b.min.y,b.min.z);
             var p3=new Vector3(b.max.x,b.min.y,b.max.z);
             var p4=new Vector3(b.min.x,b.min.y,b.max.z);
             var p5=new Vector3(b.min.x,b.max.y,b.min.z);// top
             var p6=new Vector3(b.max.x,b.max.y,b.min.z);
             var p7=new Vector3(b.max.x,b.max.y,b.max.z);
             var p8=new Vector3(b.min.x,b.max.y,b.max.z);
             Debug.DrawLine(p1,p2,color,duration);
             Debug.DrawLine(p2,p3,color,duration);
             Debug.DrawLine(p3,p4,color,duration);
             Debug.DrawLine(p4,p1,color,duration);
             Debug.DrawLine(p5,p6,color,duration);
             Debug.DrawLine(p6,p7,color,duration);
             Debug.DrawLine(p7,p8,color,duration);
             Debug.DrawLine(p8,p5,color,duration);
             Debug.DrawLine(p1,p5,color,duration);// sides
             Debug.DrawLine(p2,p6,color,duration);
             Debug.DrawLine(p3,p7,color,duration);
             Debug.DrawLine(p4,p8,color,duration);
            }
        //#endif
    }
}