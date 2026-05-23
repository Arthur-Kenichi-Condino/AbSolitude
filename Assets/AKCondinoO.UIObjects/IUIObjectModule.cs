using UnityEngine;
namespace AKCondinoO.UIObjects{
    internal interface IUIObjectModule{
        void OnAwake(UIObject root);
        void OnManualUpdate();
        void OnCanvasResized();
        void SetSafePos(Vector2 anchoredPos);
        void BringToFront();
    }
}