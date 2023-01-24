#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace AKCondinoO.SimObjectToSpriteTool{
    internal class ScreenshotBackground:MonoBehaviour{
     MeshFilter backgroundQuad;
     SimObjectScreenshotHelper screenshotHelper;
        void Awake(){
         backgroundQuad=GetComponentInChildren<MeshFilter>();
         screenshotHelper=transform.GetComponentInParent<SimObjectScreenshotHelper>();
        }
     [SerializeField]GameObject prefabToExtractThumbnail;
     Bounds bounds;
     Transform gameObjectToExtractThumbnailTransform;
        void Update(){
         if(prefabToExtractThumbnail!=null){
          gameObjectToExtractThumbnailTransform=null;
          GameObject gameObjectToExtractThumbnail=Instantiate(prefabToExtractThumbnail);
          MeshFilter[]meshFilters=gameObjectToExtractThumbnail.GetComponentsInChildren<MeshFilter>();
          if(meshFilters.Length>0){
           gameObjectToExtractThumbnailTransform=gameObjectToExtractThumbnail.transform;
               bounds=new Bounds();
           foreach(MeshFilter meshFilter in meshFilters){
            if(bounds.extents==Vector3.zero){
               bounds=meshFilter.mesh.bounds;
            }else{
               bounds.Encapsulate(meshFilter.mesh.bounds);
            }
           }
           if(bounds.extents!=Vector3.zero){
#if UNITY_EDITOR
#endif
            Camera camera=Camera.main;
            if(camera!=null){
             Util.PositionCameraToCoverFullObject(camera,gameObjectToExtractThumbnailTransform,bounds.size.y,bounds.size.z,bounds.size.x,bounds.center,gameObjectToExtractThumbnailTransform.lossyScale);
             backgroundQuad.transform.eulerAngles=new Vector3(
              backgroundQuad.transform.eulerAngles.x,
              backgroundQuad.transform.eulerAngles.y,
              gameObjectToExtractThumbnailTransform.eulerAngles.x
             );
             backgroundQuad.transform.position=gameObjectToExtractThumbnailTransform.position+gameObjectToExtractThumbnailTransform.rotation*bounds.center+camera.transform.forward*(bounds.extents.x+.01f);
             float camToBackgroundDis=Vector3.Distance(backgroundQuad.transform.position,camera.transform.position);
             Log.DebugMessage("camToBackgroundDis:"+camToBackgroundDis);
             float fovYRad=camera.fieldOfView*Mathf.Deg2Rad;
             //  calculate field of view in x (horizontal) axis
             float fovXRad=Mathf.Atan(camera.aspect*Mathf.Tan(fovYRad/2f))*2f;
             float fovY=fovYRad*Mathf.Rad2Deg;
             float fovX=fovXRad*Mathf.Rad2Deg;
             Log.DebugMessage("fovX:"+fovX);
             float backgroundQuadWidth=Mathf.Tan(fovXRad/2f)*camToBackgroundDis*2f;
             Log.DebugMessage("backgroundQuadWidth:"+backgroundQuadWidth);
             float backgroundQuadHeight=Mathf.Tan(fovYRad/2f)*camToBackgroundDis*2f;
             Log.DebugMessage("backgroundQuadHeight:"+backgroundQuadHeight);
             backgroundQuad.transform.localScale=new Vector3(backgroundQuadHeight,backgroundQuadWidth,1f);
             if(screenshotHelper!=null){
              screenshotHelper.TakeScreenshot(camera);
             }
            }
           }
          }
          prefabToExtractThumbnail=null;
         }
        }
        private void OnDrawGizmos(){
         DrawBounds();
        }
        void DrawBounds(){
         if(bounds.extents==Vector3.zero){
          return;
         }
         if(gameObjectToExtractThumbnailTransform==null){
          return;
         }
         Color color=Color.white;
         Bounds b=bounds;
         var p1=new Vector3(b.min.x,b.min.y,b.min.z);// bottom
         var p2=new Vector3(b.max.x,b.min.y,b.min.z);
         var p3=new Vector3(b.max.x,b.min.y,b.max.z);
         var p4=new Vector3(b.min.x,b.min.y,b.max.z);
         var p5=new Vector3(b.min.x,b.max.y,b.min.z);// top
         var p6=new Vector3(b.max.x,b.max.y,b.min.z);
         var p7=new Vector3(b.max.x,b.max.y,b.max.z);
         var p8=new Vector3(b.min.x,b.max.y,b.max.z);
         p1=gameObjectToExtractThumbnailTransform.TransformPoint(p1);
         p2=gameObjectToExtractThumbnailTransform.TransformPoint(p2);
         p3=gameObjectToExtractThumbnailTransform.TransformPoint(p3);
         p4=gameObjectToExtractThumbnailTransform.TransformPoint(p4);
         p5=gameObjectToExtractThumbnailTransform.TransformPoint(p5);
         p6=gameObjectToExtractThumbnailTransform.TransformPoint(p6);
         p7=gameObjectToExtractThumbnailTransform.TransformPoint(p7);
         p8=gameObjectToExtractThumbnailTransform.TransformPoint(p8);
         UnityEngine.Debug.DrawLine(p1,p2,color);
         UnityEngine.Debug.DrawLine(p2,p3,color);
         UnityEngine.Debug.DrawLine(p3,p4,color);
         UnityEngine.Debug.DrawLine(p4,p1,color);
         UnityEngine.Debug.DrawLine(p5,p6,color);
         UnityEngine.Debug.DrawLine(p6,p7,color);
         UnityEngine.Debug.DrawLine(p7,p8,color);
         UnityEngine.Debug.DrawLine(p8,p5,color);
         UnityEngine.Debug.DrawLine(p1,p5,color);// sides
         UnityEngine.Debug.DrawLine(p2,p6,color);
         UnityEngine.Debug.DrawLine(p3,p7,color);
         UnityEngine.Debug.DrawLine(p4,p8,color);
        }
    }
}