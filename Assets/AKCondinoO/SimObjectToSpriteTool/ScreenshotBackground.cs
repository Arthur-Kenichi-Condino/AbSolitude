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
     [SerializeField]GameObject prefabToExtractThumbnail;
     Bounds bounds;
     Transform gameObjectToExtractThumbnailTransform;
        void Update(){
         if(prefabToExtractThumbnail!=null){
          GameObject gameObjectToExtractThumbnail=Instantiate(prefabToExtractThumbnail,Vector3.zero,Quaternion.identity);
          MeshFilter[]meshFilters=gameObjectToExtractThumbnail.GetComponentsInChildren<MeshFilter>();
          if(meshFilters.Length>0){
               bounds=new Bounds();
           foreach(MeshFilter meshFilter in meshFilters){
            if(bounds.extents==Vector3.zero){
             gameObjectToExtractThumbnailTransform=meshFilter.transform;
               bounds=meshFilter.mesh.bounds;
            }else{
               bounds.Encapsulate(meshFilter.mesh.bounds);
            }
           }
#if UNITY_EDITOR
#endif
          }
          prefabToExtractThumbnail=null;
         }
         DrawBounds();
        }
        private void OnDrawGizmos(){
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
         gameObjectToExtractThumbnailTransform.TransformPoint(p1);
         gameObjectToExtractThumbnailTransform.TransformPoint(p2);
         gameObjectToExtractThumbnailTransform.TransformPoint(p3);
         gameObjectToExtractThumbnailTransform.TransformPoint(p4);
         gameObjectToExtractThumbnailTransform.TransformPoint(p5);
         gameObjectToExtractThumbnailTransform.TransformPoint(p6);
         gameObjectToExtractThumbnailTransform.TransformPoint(p7);
         gameObjectToExtractThumbnailTransform.TransformPoint(p8);
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