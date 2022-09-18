#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace AKCondinoO{
    internal class GameEntry:MonoBehaviour{
        void Update(){
         SceneManager.LoadScene("MainMenuScene",LoadSceneMode.Single);
        }
    }
}