using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO{
    internal static class RotationHelper{
        /// <summary>
        ///  [https://stackoverflow.com/questions/43606135/split-quaternion-into-axis-rotations]
        /// </summary>
        /// <param name="q"></param>
        /// <returns></returns>
        internal static Quaternion IsolateRotationYComponent(Quaternion q){
         float theta=Mathf.Atan2(q.y,q.w);
         //  Quaternion representing rotation about the y axis
         return new Quaternion(0,Mathf.Sin(theta),0,Mathf.Cos(theta));
        }
        internal static Quaternion IsolateRotationXComponent(Quaternion q){
         float theta=Mathf.Atan2(q.x,q.w);
         //  Quaternion representing rotation about the x axis
         return new Quaternion(Mathf.Sin(theta),0,0,Mathf.Cos(theta));
        }
    }
}