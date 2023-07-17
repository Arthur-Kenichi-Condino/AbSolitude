using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using System;
public class CatlikeCodingVoxelTerrainLightingShaderGUI:CatlikeCodingBaseShaderGUI{
    enum RenderingMode{
     Opaque,
     Cutout,
     Fade,
     Transparent
    }
    struct RenderingSettings{
     public RenderQueue queue;
     public string renderType;
     public BlendMode srcBlend,dstBlend;
     public bool zWrite;
     public static RenderingSettings[]modes={
      new RenderingSettings(){
       queue=RenderQueue.Geometry,
       renderType="",
       srcBlend=BlendMode.One,
       dstBlend=BlendMode.Zero,
       zWrite=true
      },
      new RenderingSettings(){
       queue=RenderQueue.AlphaTest,
       renderType="TransparentCutout",
       srcBlend=BlendMode.One,
       dstBlend=BlendMode.Zero,
       zWrite=true
      },
      new RenderingSettings(){
       queue=RenderQueue.AlphaTest,
       renderType="Transparent",
       srcBlend=BlendMode.SrcAlpha,
       dstBlend=BlendMode.OneMinusSrcAlpha,
       zWrite=true
      },
      new RenderingSettings(){
       queue=RenderQueue.Transparent,
       renderType="Transparent",
       srcBlend=BlendMode.One,
       dstBlend=BlendMode.OneMinusSrcAlpha,
       zWrite=false
      }
     };
    }
    public override void OnGUI(MaterialEditor editor,MaterialProperty[]properties){
     base.OnGUI(editor,properties);
     DoRenderingMode();
    }
 [NonSerialized]RenderingMode mode=RenderingMode.Opaque;
    void DoRenderingMode(){
     if(IsKeywordEnabled("_RENDERING_CUTOUT")){
      mode=RenderingMode.Cutout;
     }else if(IsKeywordEnabled("_RENDERING_FADE")){
      mode=RenderingMode.Fade;
     }else if(IsKeywordEnabled("_RENDERING_TRANSPARENT")){
      mode=RenderingMode.Transparent;
     }
        EditorGUI.BeginChangeCheck();
         mode=(RenderingMode)EditorGUILayout.EnumPopup(MakeLabel("Rendering Mode"),mode);
         if(EditorGUI.EndChangeCheck()){
          RecordAction("Rendering Mode");
          SetKeyword("_RENDERING_CUTOUT"     ,mode==RenderingMode.Cutout     );
          SetKeyword("_RENDERING_FADE"       ,mode==RenderingMode.Fade       );
          SetKeyword("_RENDERING_TRANSPARENT",mode==RenderingMode.Transparent);
          RenderingSettings settings=RenderingSettings.modes[(int)mode];
          foreach(Material m in editor.targets){
           m.renderQueue=(int)settings.queue;
           m.SetOverrideTag("RenderType",settings.renderType);
           m.SetInt("_SrcBlend",(int)settings.srcBlend);
           m.SetInt("_DstBlend",(int)settings.dstBlend);
           m.SetInt("_ZWrite",settings.zWrite?1:0);
          }
         }
    }
}