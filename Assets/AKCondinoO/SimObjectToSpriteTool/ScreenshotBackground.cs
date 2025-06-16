#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace AKCondinoO.SimObjectToSpriteTool{
    internal class ScreenshotBackground:MonoBehaviour{
     MeshFilter backgroundQuad;
     MeshFilter isolateLightCube;
     SimObjectScreenshotHelper screenshotHelper;
     SpriteRenderer previewSpriteRenderer;
        void Awake(){
         foreach(MeshFilter meshFilter in GetComponentsInChildren<MeshFilter>()){
          if(backgroundQuad==null&&meshFilter.name=="BackgroundQuad"){
           backgroundQuad=meshFilter;
           //Log.DebugMessage("backgroundQuad:"+backgroundQuad);
          }else if(isolateLightCube==null&&meshFilter.name=="IsolateLightCube"){
           isolateLightCube=meshFilter;
           //Log.DebugMessage("isolateLightCube:"+isolateLightCube);
          }
         }
         screenshotHelper=transform.GetComponentInParent<SimObjectScreenshotHelper>();
         previewSpriteRenderer=GetComponentInChildren<SpriteRenderer>();
         //Log.DebugMessage("previewSpriteRenderer:"+previewSpriteRenderer);
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
           gameObjectToExtractThumbnailTransform.position=transform.root.position;
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
            string path=AssetDatabase.GetAssetPath(prefabToExtractThumbnail).Replace("\\","/");
#else
            string path=Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).Replace("\\","/")+"/AbSolitude/Temp/Thumbnails/";
            Directory.CreateDirectory(path);
#endif
            if(!string.IsNullOrEmpty(path)){
             Log.DebugMessage("screenshot path:"+path);
            }
            Camera camera=GetComponentInChildren<Camera>();
            if(camera!=null){
             camera.enabled=true;
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
             isolateLightCube.transform.position=new Vector3(
              backgroundQuad.transform.position.x,
              backgroundQuad.transform.position.y,
              (camera.transform.position.z+backgroundQuad.transform.position.z)/2f
             );
             isolateLightCube.transform.rotation=backgroundQuad.transform.rotation;
             isolateLightCube.transform.localScale=new Vector3(backgroundQuadHeight,backgroundQuadWidth,camToBackgroundDis);
             Log.DebugMessage("screenshotHelper:"+screenshotHelper);
             if(screenshotHelper!=null){
              screenshotHelper.TakeScreenshot(camera,previewSpriteRenderer,path,gameObjectToExtractThumbnail);
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