using AKCondinoO.Bootstrap;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
namespace AKCondinoO.UIObjects{
    internal class WindowDockManager:MonoBehaviour{
     internal readonly UISystem uiSystem;
        internal WindowDockManager(UISystem uiSystem){
         this.uiSystem=uiSystem;
        }
        internal void OnRegistration(UIWindowRoot windowRoot){
         var minimized=windowRoot.minimized;
         var window=windowRoot.window;
         minimized.RegisterWindow(window);
         window.RegisterMinimizedBtn(minimized);
         minimized.gameObject.SetActive(false);
        }
     private readonly Dictionary<Window,Minimized>docked=new();
        internal void Minimize(Minimized minimizedBtn,Window window,bool closeButton=false,Vector2 rawPosition=default){
         if(docked.ContainsKey(window)){
          return;
         }
         minimizedBtn.previousWindowPos=((RectTransform)window.transform).anchoredPosition;
         window.gameObject.SetActive(false);
         var btnGO=minimizedBtn.gameObject;
         btnGO.SetActive(true);
         Vector2 minimizePos;
         if(closeButton){
          minimizePos=ScreenToCanvasPosition(rawPosition,minimizedBtn.root.canvas);
          minimizedBtn.minimizedFromCloseButton=true;
         }else{
          minimizePos=GetMinimizePosFromMouse(rawPosition,minimizedBtn,btnGO);
          minimizedBtn.minimizedFromCloseButton=false;
         }
         ((RectTransform)btnGO.transform).anchoredPosition=minimizePos;
         minimizedBtn.BringToFront();
         docked.Add(window,minimizedBtn);
        }
        internal Vector2 GetMinimizePosFromMouse(Vector2 mousePosition,Minimized minimizedBtn,GameObject minimizedBtnGO){
         Vector2 minimizedPos=ScreenToCanvasPosition(mousePosition,minimizedBtn.root.canvas);
         minimizedPos=ClampInsideCanvas(minimizedPos,minimizedBtnGO,minimizedBtn.root.canvas);
         return minimizedPos;
        }
        internal void Restore(Minimized minimizedBtn,Window window){
         if(!docked.TryGetValue(window,out var button)){
          return;
         }
         window.gameObject.SetActive(true);
         minimizedBtn.gameObject.SetActive(false);
         RectTransform windowRect=(RectTransform)window.transform;
         if(minimizedBtn.minimizedFromCloseButton){
          windowRect.anchoredPosition=GetWindowPosFromMinimizedPos(window,minimizedBtn);
         }
         window.BringToFront();
         docked.Remove(window);
        }
        internal Vector2 GetWindowPosFromMinimizedPos(Window window,Minimized minimizedBtn){
         var canvas=window.root.canvas;
         RectTransform canvasRect=(RectTransform)canvas.transform;
         RectTransform minimizedRect=(RectTransform)minimizedBtn.transform;
         RectTransform windowRect=(RectTransform)window.transform;
         float windowWidth =windowRect.sizeDelta.x;
         float windowHeight=windowRect.sizeDelta.y;
         Vector2 btnPos=minimizedRect.anchoredPosition;
         float btnWidth =minimizedRect.sizeDelta.x;
         float btnHeight=minimizedRect.sizeDelta.y;
         float halfW=(canvasRect.rect.width *canvas.scaleFactor)*0.5f;
         float halfH=(canvasRect.rect.height*canvas.scaleFactor)*0.5f;
         bool left  =btnPos.x< 0;
         bool right =btnPos.x>=0;
         bool bottom=btnPos.y< 0;
         bool top   =btnPos.y>=0;
         Logs.Debug(()=>"left:"+left+";right:"+right+";bottom:"+bottom+";top:"+top);
         Vector2 windowPos=new();
         if     (left  ){windowPos.x=btnPos.x+windowWidth *0.5f;}
         else if(right ){windowPos.x=btnPos.x-windowWidth *0.5f;}
         if     (bottom){windowPos.y=btnPos.y+windowHeight*0.5f;}
         else if(top   ){windowPos.y=btnPos.y-windowHeight*0.5f;}
         return windowPos;
        }
        internal void OnManualUpdate(){
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
         Vector2 pos,
         GameObject element,
         Canvas canvas,
         float margin=8f
        ){
         RectTransform elementRectTransform=(RectTransform)element.transform;
         Vector2 size=elementRectTransform.rect.size;
         Vector2 canvasSize=((RectTransform)canvas.transform).rect.size;
         float minX=-canvasSize.x*0.5f+size.x*elementRectTransform.pivot.x     +margin;
         float maxX= canvasSize.x*0.5f-size.x*(1f-elementRectTransform.pivot.x)-margin;
         float minY=-canvasSize.y*0.5f+size.y*elementRectTransform.pivot.y     +margin;
         float maxY= canvasSize.y*0.5f-size.y*(1f-elementRectTransform.pivot.y)-margin;
         pos.x=Mathf.Clamp(pos.x,minX,maxX);
         pos.y=Mathf.Clamp(pos.y,minY,maxY);
         return pos;
        }
        internal static bool IsNearScreenEdge(Vector2 pos,Canvas canvas){
         float edge=32f*canvas.scaleFactor;
         return
          pos.x<=edge||
          pos.x>=Screen.width -edge||
          pos.y<=edge||
          pos.y>=Screen.height-edge;
        }
    }
}