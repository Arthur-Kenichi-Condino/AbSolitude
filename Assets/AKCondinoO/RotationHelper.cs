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
        /// <summary>
        ///  [https://answers.unity.com/questions/26783/how-to-get-the-signed-angle-between-two-quaternion.html]
        /// </summary>
        /// <param name="rotationA"></param>
        /// <param name="rotationB"></param>
        /// <returns></returns>
        internal static float SignedAngleFromRotationYComponentFromAToB(Quaternion rotationA,Quaternion rotationB){
         //  get a "forward vector" for each rotation
         Vector3 forwardA=rotationA*Vector3.forward;
         Vector3 forwardB=rotationB*Vector3.forward;
         //  get a numeric angle for each vector, on the X-Z plane (relative to world forward)
         float angleA=Mathf.Atan2(forwardA.x,forwardA.z)*Mathf.Rad2Deg;
         float angleB=Mathf.Atan2(forwardB.x,forwardB.z)*Mathf.Rad2Deg;
         //  get the signed difference in these angles
         float angleDiff=Mathf.DeltaAngle(angleA,angleB);
         return angleDiff;
        }
    }
}