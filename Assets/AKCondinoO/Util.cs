using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
namespace AKCondinoO{
    internal class Util{
     internal static int UILayer;
        internal static void SetUtil(){
         UILayer=LayerMask.NameToLayer("UI");
        }
        internal static Transform FindChildRecursively(Transform parent,string name){
         foreach(Transform child in parent){
          if(child.name==name){
           return child;
          }else{
           Transform found=FindChildRecursively(child,name);
           if(found!=null){
            return found;
           }
          }
         }
         return null;
        }
        /// <summary>
        ///  https://forum.unity.com/threads/make-sure-4-points-of-bounds-are-always-visible-no-matter-the-aspect-ratio.1165082/ quote: "The principal is fairly simple which is that tangent of the half of fov angle in horizontal axis is equal to the half the width of the plane divided by the distance of the plane (from the camera). ( tan( fovX / 2f ) = ( obj.width / 2f ) / ( distance ) )"
        /// </summary>
        internal static void PositionCameraToCoverFullObject(Camera cam,Transform targetTransform,Bounds bounds,Vector3 scale){
         float fovYRad=cam.fieldOfView*Mathf.Deg2Rad;
         //  calculate field of view in x (horizontal) axis
         float fovXRad=Mathf.Atan(cam.aspect*Mathf.Tan(fovYRad/2f))*2f;
         //  get the width of the target quad
         float width =bounds.size.x*scale.x;
         float height=bounds.size.y*scale.y;
         //  calculate distance of the camera so the width of the target quad match the camera width at that point in the world
         float disX=(width /2f)/Mathf.Tan(fovXRad/2f);
         float disY=(height/2f)/Mathf.Tan(fovYRad/2f);
         float targetDistance=disX>disY?disX:disY;
         cam.transform.position=targetTransform.position-Vector3.forward*targetDistance;
         cam.transform.LookAt(targetTransform);
        }
        internal static void DrawBounds(Bounds b,Color color,float duration=0){//[https://gist.github.com/unitycoder/58f4b5d80f423d29e35c814a9556f9d9]
         var p1=new Vector3(b.min.x,b.min.y,b.min.z);// bottom
         var p2=new Vector3(b.max.x,b.min.y,b.min.z);
         var p3=new Vector3(b.max.x,b.min.y,b.max.z);
         var p4=new Vector3(b.min.x,b.min.y,b.max.z);
         var p5=new Vector3(b.min.x,b.max.y,b.min.z);// top
         var p6=new Vector3(b.max.x,b.max.y,b.min.z);
         var p7=new Vector3(b.max.x,b.max.y,b.max.z);
         var p8=new Vector3(b.min.x,b.max.y,b.max.z);
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