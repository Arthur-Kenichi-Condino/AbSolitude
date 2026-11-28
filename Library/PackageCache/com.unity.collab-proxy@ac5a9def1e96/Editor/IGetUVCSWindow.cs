using UnityEditor;

namespace Unity.PlasticSCM.Editor
{
    internal interface IGetUVCSWindow
    {
        EditorWindow Get();
        void Repaint();
    }
}
