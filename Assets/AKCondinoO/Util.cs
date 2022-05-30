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
             Gizmos.DrawLine(p1+Vector3.left*radius,p2+Vector3.left*radius);
         #endif
        }
    }
}