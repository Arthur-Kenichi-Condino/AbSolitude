using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
namespace AKCondinoO{
    internal class Util{
        internal static void SetUtil(){
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