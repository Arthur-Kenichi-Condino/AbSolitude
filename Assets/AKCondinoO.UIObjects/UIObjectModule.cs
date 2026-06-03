using AKCondinoO.Bootstrap;
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
        public virtual void UpdateBounds(){
         bounds=RectTransformUtility.CalculateRelativeRectTransformBounds(rectTransform);
         Logs.Debug(()=>"'bounds updated':center:"+bounds.center+";size:"+bounds.size);
        }
        public virtual Bounds GetBounds(){
         UpdateBounds();
         return bounds;
        }
        public virtual Vector2 GetSize(){
         UpdateBounds();
         return new(bounds.size.x,bounds.size.y);
        }
        protected Vector2 GetCanvasSize(){
         return((RectTransform)root.canvas.transform).rect.size;
        }
        public virtual void BringToFront(){
         root.transform.SetAsLastSibling();
        }
    }
}