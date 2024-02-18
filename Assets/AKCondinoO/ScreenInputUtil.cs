#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static AKCondinoO.InputHandler;
namespace AKCondinoO{
    internal static class ScreenInputUtil{
        internal static Vector2 RectActualSize(RectTransform rect,Canvas canvas){
         Vector2 size=RectTransformUtility.PixelAdjustRect(rect,canvas).size*canvas.scaleFactor;
         return size;
        }
        internal static Vector2 ActualSize(this RectTransform rect,Canvas canvas){
         return RectActualSize(rect,canvas);
        }
        internal static Vector2 RectActualSize2(RectTransform rect,ref Vector3[]worldCorners){
         if(worldCorners==null||worldCorners.Length!=4){
          worldCorners=new Vector3[4];
         }
         var v=worldCorners;
         rect.GetWorldCorners(v);
         return new Vector2(v[3].x-v[0].x,v[1].y-v[0].y);
        }
        internal static Vector2 ActualSize2(this RectTransform rect,ref Vector3[]worldCorners){
         return RectActualSize2(rect,ref worldCorners);
        }
    }
}