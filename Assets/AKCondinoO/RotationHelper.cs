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
        internal static float SignedAngleFromRotationYComponentFromAToB(Quaternion rotationA,Quaternion rotationB,bool isolate=true){
         if(isolate){
          rotationA=IsolateRotationYComponent(rotationA);
          rotationB=IsolateRotationYComponent(rotationB);
         }
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
        internal static float SignedAngleFromRotationXComponentFromAToB(Quaternion rotationA,Quaternion rotationB,bool isolate=true){
         if(isolate){
          rotationA=IsolateRotationXComponent(rotationA);
          rotationB=IsolateRotationXComponent(rotationB);
         }
         //  get a "forward vector" for each rotation
         Vector3 forwardA=rotationA*Vector3.forward;
         Vector3 forwardB=rotationB*Vector3.forward;
         //  get a numeric angle for each vector, on the Y-Z plane (relative to world forward)
         float angleA=Mathf.Atan2(forwardA.y,forwardA.z)*Mathf.Rad2Deg;
         float angleB=Mathf.Atan2(forwardB.y,forwardB.z)*Mathf.Rad2Deg;
         //  get the signed difference in these angles
         float angleDiff=Mathf.DeltaAngle(angleA,angleB);
         return angleDiff;
        }
        /// <summary>
        ///  [https://forum.unity.com/threads/quaternion-how-to-compute-delta-angle-for-each-axis.242208/]
        /// </summary>
        /// <param name="rotationA"></param>
        /// <param name="rotationB"></param>
        /// <returns></returns>
        internal static Quaternion RotDiffFromAToB(Quaternion rotationA,Quaternion rotationB){
         return rotationB*Quaternion.Inverse(rotationA);
        }
        //  [https://forum.unity.com/threads/how-do-i-clamp-a-quaternion.370041/#post-6531533]
        internal static Quaternion Clamp(Quaternion q,Vector3 minAxisAngle,Vector3 maxAxisAngle){
         q.x/=(q.w==0f?1f:q.w);
         q.y/=(q.w==0f?1f:q.w);
         q.z/=(q.w==0f?1f:q.w);
         q.w=1.0f;
         float angleX=2.0f*Mathf.Rad2Deg*Mathf.Atan(q.x);
         angleX=Mathf.Clamp(angleX,minAxisAngle.x,maxAxisAngle.x);
         q.x=Mathf.Tan(0.5f*Mathf.Deg2Rad*angleX);
         float angleY=2.0f*Mathf.Rad2Deg*Mathf.Atan(q.y);
         angleY=Mathf.Clamp(angleY,minAxisAngle.y,maxAxisAngle.y);
         q.y=Mathf.Tan(0.5f*Mathf.Deg2Rad*angleY);
         float angleZ=2.0f*Mathf.Rad2Deg*Mathf.Atan(q.z);
         angleZ=Mathf.Clamp(angleZ,minAxisAngle.z,maxAxisAngle.z);
         q.z=Mathf.Tan(0.5f*Mathf.Deg2Rad*angleZ);
         return q.normalized;
        }
    }
}