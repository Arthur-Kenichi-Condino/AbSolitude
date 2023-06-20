#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO{
    internal static class RenderingUtil{
     internal static Shader StandardShader;
        internal static void SetUtil(){
         StandardShader=Shader.Find("Standard");
        }
        internal enum BlendMode:int{
         Opaque=0,
         Cutout=1,
         Fade=2,
         Transparent=3,
        }
        /// <summary>
        ///  [https://stackoverflow.com/questions/72309866/how-to-change-material-rendering-mode-to-fade-by-script]
        ///  [https://answers.unity.com/questions/1004666/change-material-rendering-mode-in-runtime.html]
        /// </summary>
        internal static void SetupStandardShaderMaterialBlendMode(Material standardShaderMaterial,BlendMode blendMode){
         //standardShaderMaterial.SetFloat("_Mode",(float)blendMode);//  causes error message
         switch(blendMode){
          case BlendMode.Opaque:{
           standardShaderMaterial.SetFloat("_Mode",0);
           standardShaderMaterial.SetFloat("_SrcBlend",(int)UnityEngine.Rendering.BlendMode.One);
           standardShaderMaterial.SetFloat("_DstBlend",(int)UnityEngine.Rendering.BlendMode.Zero);
           standardShaderMaterial.SetFloat("_ZWrite",1);
           standardShaderMaterial.DisableKeyword("_ALPHATEST_ON");
           standardShaderMaterial.DisableKeyword("_ALPHABLEND_ON");
           standardShaderMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
           standardShaderMaterial.renderQueue=-1;
           break;
          }
          case BlendMode.Cutout:{
           standardShaderMaterial.SetFloat("_Mode",1);
           standardShaderMaterial.SetFloat("_SrcBlend",(int)UnityEngine.Rendering.BlendMode.One);
           standardShaderMaterial.SetFloat("_DstBlend",(int)UnityEngine.Rendering.BlendMode.Zero);
           standardShaderMaterial.SetFloat("_ZWrite",1);
           standardShaderMaterial.EnableKeyword("_ALPHATEST_ON");
           standardShaderMaterial.DisableKeyword("_ALPHABLEND_ON");
           standardShaderMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
           standardShaderMaterial.renderQueue=2450;
           break;
          }
          case BlendMode.Fade:{
           standardShaderMaterial.SetFloat("_Mode",2);
           standardShaderMaterial.SetFloat("_SrcBlend",(int)UnityEngine.Rendering.BlendMode.SrcAlpha);
           standardShaderMaterial.SetFloat("_DstBlend",(int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
           standardShaderMaterial.SetFloat("_ZWrite",0);
           standardShaderMaterial.DisableKeyword("_ALPHATEST_ON");
           standardShaderMaterial.EnableKeyword("_ALPHABLEND_ON");
           standardShaderMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
           standardShaderMaterial.renderQueue=3000;
           break;
          }
          case BlendMode.Transparent:{
           standardShaderMaterial.SetFloat("_Mode",3);
           standardShaderMaterial.SetFloat("_SrcBlend",(int)UnityEngine.Rendering.BlendMode.One);
           standardShaderMaterial.SetFloat("_DstBlend",(int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
           standardShaderMaterial.SetFloat("_ZWrite",0);
           standardShaderMaterial.DisableKeyword("_ALPHATEST_ON");
           standardShaderMaterial.DisableKeyword("_ALPHABLEND_ON");
           standardShaderMaterial.EnableKeyword("_ALPHAPREMULTIPLY_ON");
           standardShaderMaterial.renderQueue=3000;
           break;
          }
         }
         //Log.DebugMessage("standardShaderMaterial.renderQueue:"+standardShaderMaterial.renderQueue);
        }
    }
}