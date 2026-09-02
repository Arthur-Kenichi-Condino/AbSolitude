using UnityEngine;
namespace AKCondinoO.UIObjects{
    internal interface IUIObjectModule{
        void OnAwake(UIObject root);
        void OnManualUpdate();
        void OnCanvasResized();
        void SetSafePos(Vector2 anchoredPos);
        void UpdateBounds();
        Bounds GetBounds();
        Vector2 GetSize(bool raw);
        void BringToFront();
    }
}