using AKCondinoO.Bootstrap;
using UnityEngine;
namespace AKCondinoO.UIObjects{
    internal abstract class UIObjectModule:MonoBehaviour,IUIObjectModule{
     internal UIObject root;
     protected RectTransform rectTransform;
        public virtual void OnAwake(UIObject root){
         this.root=root;
         rectTransform=(RectTransform)transform;
        }
     private Vector2 lastCanvasSize;
     private bool pendingCanvasClamp;
     protected virtual bool shouldAutoKeepSafe=>true;
        public virtual void OnManualUpdate(){
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
           transform.hasChanged=false;
          }
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
         rectTransform.anchoredPosition=UISystem.ClampInsideCanvas(anchoredPos,gameObject,root.canvas);
         transform.hasChanged=false;
        }
        protected Vector2 GetCanvasSize(){
         return((RectTransform)root.canvas.transform).rect.size;
        }
        public virtual void BringToFront(){
         root.transform.SetAsLastSibling();
        }
    }
}