using AKCondinoO.Bootstrap;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.UIObjects{
    internal abstract class UIObjectModule:MonoBehaviour,IUIObjectModule{
     internal UIObject root;
     internal RectTransform rectTransform;
     private Bounds bounds;
        public virtual void OnAwake(UIObject root){
         this.root=root;
         rectTransform=(RectTransform)transform;
        }
     private Vector2 lastCanvasSize;
     private bool pendingCanvasClamp;
     protected virtual bool shouldAutoKeepSafe=>true;
        public virtual void OnManualUpdate(){
         if(transform.hasChanged){
         }
         if(shouldAutoKeepSafe){
          Vector2 canvasSize=GetCanvasSize();
          bool canvasResized=lastCanvasSize!=canvasSize;
          if(canvasResized){
           //Logs.Debug(()=>"canvasSize:"+canvasSize);
           OnCanvasResized();
           lastCanvasSize=canvasSize;
          }
          if(pendingCanvasClamp){
           //Logs.Debug(()=>"before:"+rectTransform.anchoredPosition);
           EnsureInsideCanvas();
           //Logs.Debug(()=>"after:"+rectTransform.anchoredPosition);
           pendingCanvasClamp=false;
          }
          if(transform.hasChanged){
           EnsureInsideCanvas();
          }
         }
         if(transform.hasChanged){
          transform.hasChanged=false;
         }
         //Logs.Debug(()=>"current:"+rectTransform.anchoredPosition);
        }
        public virtual void OnCanvasResized(){
         pendingCanvasClamp=true;
         //Logs.Debug(()=>"pendingCanvasClamp:"+pendingCanvasClamp);
        }
        protected void EnsureInsideCanvas(){
         //Logs.Debug(()=>"'reached EnsureInsideCanvas'");
         SetSafePos(rectTransform.anchoredPosition);
        }
        public virtual void SetSafePos(Vector2 anchoredPos){
         rectTransform.anchoredPosition=UISystem.ClampInsideCanvas(anchoredPos,this,root.canvas);
         transform.hasChanged=false;
        }
     internal readonly List<RectTransform>ignoredForBounds=new();
        public virtual void UpdateBounds(){
         bounds=CalculateBounds(rectTransform,ignoredForBounds);
         Logs.Debug(()=>"'bounds updated':center:"+bounds.center+";size:"+bounds.size);
        }
        static Bounds CalculateBounds(RectTransform root,List<RectTransform>ignoredForBounds){
         Bounds bounds=default;
         bool set=false;
         Traverse(root);
            void Traverse(RectTransform current){
             Logs.Debug(()=>"'traversing':"+current.name);
             if(ignoredForBounds.Contains(current)){
              Logs.Debug(()=>"'traversing':ignored");
              return;
             }
             Vector3[]corners=new Vector3[4];
             current.GetWorldCorners(corners);
             for(int i=0;i<4;++i){
              Vector3 point=root.InverseTransformPoint(corners[i]);
              if(!set){
               bounds=new Bounds(point,Vector3.zero);
               set=true;
              }else{
               bounds.Encapsulate(point);
              }
             }
             for(int i=0;i<current.childCount;++i){
              if(current.GetChild(i)is RectTransform child)
               Traverse(child);
             }
            }
         return bounds;
        }
        public virtual Bounds GetBounds(){
         UpdateBounds();
         return bounds;
        }
        public virtual Vector2 GetSize(bool raw=false){
         if(raw){
          return rectTransform.sizeDelta;
         }
         UpdateBounds();
         return new(bounds.size.x,bounds.size.y);
        }
        protected Vector2 GetCanvasSize(){
         return((RectTransform)root.canvas.transform).rect.size;
        }
        public virtual void BringToFront(){
         root.transform.SetAsLastSibling();
        }
        public virtual void Resize(Vector2 size){
         rectTransform.sizeDelta=size;
        }
    }
}