#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.SimObjectToSpriteTool{
    //  [https://answers.unity.com/questions/733240/how-to-take-a-screenshot-and-apply-it-as-a-texture.html]
    //  [https://answers.unity.com/questions/1473282/show-screen-capture-as-ui-image.html]
    internal class SimObjectScreenshotHelper:MonoBehaviour{
        internal void TakeScreenshot(Camera camera,SpriteRenderer previewSpriteRenderer){
         StartCoroutine(TakeScreenshotCoroutine(camera,previewSpriteRenderer));
        }
     WaitForEndOfFrame waitForEndOfFrame=new WaitForEndOfFrame();
        IEnumerator TakeScreenshotCoroutine(Camera camera,SpriteRenderer previewSpriteRenderer){
         Log.DebugMessage("waiting for waitForEndOfFrame in game view to then take a screenshot");
         yield return waitForEndOfFrame;
         if(camera==null){
          camera=Camera.main;
         }
         if(camera!=null){
          FullScreenMode screenMode=Screen.fullScreenMode;
          int screenWidth=Screen.width;int screenHeight=Screen.height;
          Log.DebugMessage("screenWidth:"+screenWidth+";screenHeight:"+screenHeight+";screenMode:"+screenMode);
          int sWidth=Screen.width;int sHeight=Screen.height;
          Log.DebugMessage("sWidth:"+sWidth+";sHeight:"+sHeight+";screenMode:"+screenMode);
          RenderTexture rt=new RenderTexture(sWidth,sHeight,24);
          camera.targetTexture=rt;
          Texture2D screenshot=new Texture2D(sWidth,sHeight,TextureFormat.RGB24,false);
          camera.Render();
          RenderTexture.active=rt;
          screenshot.ReadPixels(new Rect(0,0,sWidth,sHeight),0,0);
          screenshot.Apply();
          camera.targetTexture=null;
          RenderTexture.active=null;
          Destroy(rt);
          Log.DebugMessage("previewSpriteRenderer:"+previewSpriteRenderer);
          if(previewSpriteRenderer!=null){
           previewSpriteRenderer.transform.position=camera.transform.position-camera.transform.forward;
           previewSpriteRenderer.transform.forward=camera.transform.forward;
           Sprite oldSprite=previewSpriteRenderer.sprite;
           previewSpriteRenderer.sprite=null;
           if(oldSprite!=null){
            Destroy(oldSprite);
           }
           Sprite tempSprite=Sprite.Create(screenshot,new Rect(0,0,sWidth,sHeight),new Vector2(0,0));
           previewSpriteRenderer.sprite=tempSprite;
          }
         }
        }
    }
}