using AKCondinoO.Bootstrap;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.UIObjects{
    internal class UISystem:MonoSingleton<UISystem>{
     internal int uiLayer{get;private set;}
     internal bool initialized;
     internal UIGameEventHandler gameEventHandler;
     internal WindowDockManager windowDockManager;
     internal WindowsRoot windowsRoot;
     private readonly HashSet<UIObject>windows=new();
        public override void Initialize(){
         base.Initialize();
         uiLayer=LayerMask.NameToLayer("UI");
         gameEventHandler=new(this);
         windowDockManager=new(this);
         windowsRoot=GetComponentInChildren<WindowsRoot>();
         initialized=true;
         UIObject[]existingWindows=GetComponentsInChildren<UIObject>();
         foreach(UIObject uiObject in existingWindows){
          AddWindow(uiObject);
         }
        }
        internal void AddWindow(UIObject uiObject){
         Logs.Debug(()=>"try add:"+uiObject.name,uiObject);
         if(!initialized){
          Logs.Debug(()=>"'!initialized':can't add:"+uiObject.name,uiObject);
          return;
         }
         if(windows.Add(uiObject)){
          Logs.Debug(()=>"added:"+uiObject.name,uiObject);
          uiObject.OnAddWindow();
          RegisterWindow(uiObject);
         }else{
          Logs.Debug(()=>"already added:"+uiObject.name,uiObject);
         }
        }
        internal void RegisterWindow(UIObject uiObject){
         windowDockManager.OnRegistration(uiObject);
        }
        public override void ManualUpdate(){
         base.ManualUpdate();
         windowDockManager.OnManualUpdate();
         foreach(var uiObject in windows){
          uiObject.ManualUpdate();
         }
        }
        internal static Vector2 ScreenToCanvasPosition(
         Vector2 screenPos,
         Canvas canvas
        ){
         RectTransform canvasRect=(RectTransform)canvas.transform;
         RectTransformUtility.ScreenPointToLocalPointInRectangle(
          canvasRect,
          screenPos,
          canvas.worldCamera,
          out Vector2 canvasPos
         );
         return canvasPos;
        }
        internal static Vector2 ClampInsideCanvas(
         Vector2 anchoredPos,
         UIObjectModule element,
         Canvas canvas,
         float margin=8f
        ){
         Bounds bounds=element.GetBounds();
         Vector2 canvasSize=((RectTransform)canvas.transform).rect.size;
         float minX=-canvasSize.x*0.5f-bounds.min.x+margin;
         float maxX= canvasSize.x*0.5f-bounds.max.x-margin;
         float minY=-canvasSize.y*0.5f-bounds.min.y+margin;
         float maxY= canvasSize.y*0.5f-bounds.max.y-margin;
         anchoredPos.x=Mathf.Clamp(anchoredPos.x,minX,maxX);
         anchoredPos.y=Mathf.Clamp(anchoredPos.y,minY,maxY);
         return anchoredPos;
        }
        internal static bool IsNearCanvasEdgeLocalSpace(Vector2 anchoredPos,Canvas canvas,out bool left,out bool right,out bool bottom,out bool top,Vector2 edge){
         RectTransform canvasRect=(RectTransform)canvas.transform;
         float halfW=canvasRect.rect.width *0.5f;
         float halfH=canvasRect.rect.height*0.5f;
         left  =anchoredPos.x <=-halfW+edge.x;
         right =anchoredPos.x >= halfW-edge.x;
         bottom=anchoredPos.y <=-halfH+edge.y;
         top   =anchoredPos.y >= halfH-edge.y;
         return
          left||
          right||
          bottom||
          top;
        }
        internal static bool IsNearScreenEdgeScreenSpace(Vector2 screenPos,Canvas canvas,Vector2 edge){
         edge*=canvas.scaleFactor;
         return
          screenPos.x<=              edge.x||
          screenPos.x>=Screen.width -edge.x||
          screenPos.y<=              edge.y||
          screenPos.y>=Screen.height-edge.y;
        }
    }
}