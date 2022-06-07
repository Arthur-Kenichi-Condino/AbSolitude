using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
namespace AKCondinoO{
    internal class Util{
        internal static void SetUtil(){
        }
        internal static void DrawRotatedBounds(Vector3[]boundsVertices,Color color,float duration=0){
         #if UNITY_EDITOR
         Debug.DrawLine(boundsVertices[0],boundsVertices[1],color,duration);
         Debug.DrawLine(boundsVertices[1],boundsVertices[2],color,duration);
         Debug.DrawLine(boundsVertices[2],boundsVertices[3],color,duration);
         Debug.DrawLine(boundsVertices[3],boundsVertices[0],color,duration);
         Debug.DrawLine(boundsVertices[4],boundsVertices[5],color,duration);
         Debug.DrawLine(boundsVertices[5],boundsVertices[6],color,duration);
         Debug.DrawLine(boundsVertices[6],boundsVertices[7],color,duration);
         Debug.DrawLine(boundsVertices[7],boundsVertices[4],color,duration);
         Debug.DrawLine(boundsVertices[0],boundsVertices[4],color,duration);// sides
         Debug.DrawLine(boundsVertices[1],boundsVertices[5],color,duration);
         Debug.DrawLine(boundsVertices[2],boundsVertices[6],color,duration);
         Debug.DrawLine(boundsVertices[3],boundsVertices[7],color,duration);
         #endif
        }
        internal static void DrawWireCapsule(Vector3 p1,Vector3 p2,float radius){
         #if UNITY_EDITOR
             //  Special case when both points are in the same position
             if(p1==p2){
              //  DrawWireSphere works only in gizmos methods
              Gizmos.DrawWireSphere(p1,radius);
              return;
             }
             Gizmos.DrawWireSphere(p1,radius);
             Gizmos.DrawLine(p1+Vector3.left   *radius,p2+Vector3.left   *radius);
             Gizmos.DrawLine(p1+Vector3.right  *radius,p2+Vector3.right  *radius);
             Gizmos.DrawLine(p1+Vector3.back   *radius,p2+Vector3.back   *radius);
             Gizmos.DrawLine(p1+Vector3.forward*radius,p2+Vector3.forward*radius);
             Gizmos.DrawWireSphere(p2,radius);
         #endif
        }
    }
}