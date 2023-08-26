using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace IWI.UI
{
    [AddComponentMenu("IWI/UI/Inverse Masked Image")]
    public class InverseMaskedImage : Image
    {
        private static readonly int StencilComp = Shader.PropertyToID("_StencilComp");
        public override Material materialForRendering
        {
            get
            {
                Material newMaterial = new Material(base.materialForRendering);
                newMaterial.SetInt(StencilComp, (int)CompareFunction.NotEqual);
                return newMaterial;
            }
        }
        
        #if UNITY_EDITOR
        [MenuItem("GameObject/IWI/UI/Inverse Masked Image", priority = 1)]
        private static void CreateInverseImage(MenuCommand menuCommand)
        {
            GameObject go = new GameObject("Inverse Masked Image");
            go.AddComponent<InverseMaskedImage>().raycastTarget = false;
            GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            Selection.activeObject = go;
        }
        #endif
    }
}
