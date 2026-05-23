using AKCondinoO.Bootstrap;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static AKCondinoO.UIObjects.UISystem;
namespace AKCondinoO.UIObjects{
    internal class WindowDockManager:MonoBehaviour{
     internal readonly UISystem uiSystem;
        internal WindowDockManager(UISystem uiSystem){
         this.uiSystem=uiSystem;
        }
        internal void OnRegistration(UIObject windowRoot){
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
         Vector2 previousWindowPos=((RectTransform)window.transform).anchoredPosition;
         window.gameObject.SetActive(false);
         var btnGO=minimizedBtn.gameObject;
         btnGO.SetActive(true);
         Vector2 minimizePos;
         bool minimizedFromCloseButton=false;
         if(closeButton){
          minimizePos=ScreenToCanvasPosition(rawPosition,minimizedBtn.root.canvas);
          minimizedFromCloseButton=true;
         }else{
          minimizePos=GetMinimizePosFromMouse(rawPosition,minimizedBtn,btnGO,false);
          minimizedFromCloseButton=false;
         }
         minimizedBtn.OnMinimizedButtonEnabled(previousWindowPos,minimizedFromCloseButton);
         minimizedBtn.SetSafePos(minimizePos);
         minimizedBtn.BringToFront();
         docked.Add(window,minimizedBtn);
        }
        internal Vector2 GetMinimizePosFromMouse(Vector2 mousePosition,Minimized minimizedBtn,GameObject minimizedBtnGO,bool clamp=true){
         Vector2 minimizedPos=ScreenToCanvasPosition(mousePosition,minimizedBtn.root.canvas);
         if(clamp){
          minimizedPos=ClampInsideCanvas(minimizedPos,minimizedBtnGO,minimizedBtn.root.canvas);
         }
         return minimizedPos;
        }
        internal void Restore(Minimized minimizedBtn,Window window){
         if(!docked.TryGetValue(window,out var button)){
          return;
         }
         window.gameObject.SetActive(true);
         minimizedBtn.gameObject.SetActive(false);
         RectTransform windowRect=(RectTransform)window.transform;
         Vector2 anchoredPosition=new();
         if(minimizedBtn.minimizedFromCloseButton&&!minimizedBtn.draggedAfterCloseButton){
          anchoredPosition=minimizedBtn.previousWindowPos;
         }else{
          anchoredPosition=GetWindowPosFromMinimizedPos(window,minimizedBtn,false);
         }
         window.SetSafePos(anchoredPosition);
         window.BringToFront();
         docked.Remove(window);
        }
        internal Vector2 GetWindowPosFromMinimizedPos(Window window,Minimized minimizedBtn,bool clamp=true){
         var canvas=window.root.canvas;
         RectTransform canvasRect=(RectTransform)canvas.transform;
         RectTransform minimizedRect=(RectTransform)minimizedBtn.transform;
         RectTransform windowRect=(RectTransform)window.transform;
         float windowWidth =windowRect.sizeDelta.x;
         float windowHeight=windowRect.sizeDelta.y;
         Vector2 btnPos=minimizedRect.anchoredPosition;
         float btnWidth =minimizedRect.sizeDelta.x;
         float btnHeight=minimizedRect.sizeDelta.y;
         float edge=Mathf.Max(btnWidth,btnHeight);
         bool nearEdge=IsNearCanvasEdgeLocalSpace(btnPos,canvas,out bool left,out bool right,out bool bottom,out bool top,edge);
         Vector2 windowPos=new(
          btnPos.x-windowWidth *0.5f,
          btnPos.y-windowHeight*0.5f
         );
         if(nearEdge){
          if(left  ){
           windowPos.x=btnPos.x+windowWidth *0.5f;
          }
          if(right ){
          }
          if(bottom){
           windowPos.y=btnPos.y+windowHeight*0.5f;
          }
          if(top   ){
          }
         }
         Logs.Debug(()=>"canvasRect:"+canvasRect.rect.size+";btnPos:"+btnPos+";nearEdge:"+nearEdge+";edge:"+edge+";left:"+left+";right:"+right+";bottom:"+bottom+";top:"+top);
         if(clamp){
          windowPos=ClampInsideCanvas(windowPos,window.gameObject,canvas);
         }
         return windowPos;
        }
        internal void OnManualUpdate(){
        }
    }
}