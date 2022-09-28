using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO{
    internal interface ISingletonInitialization{
     static MonoBehaviour singleton{get;set;}
        void Init();
        void OnDestroyingCoreEvent(object sender,EventArgs e);
    }
}