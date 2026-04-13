using System;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Bootstrap{
    [CreateAssetMenu(menuName="AKCondinoO/Input/Bindings")]
    internal class InputBindingsAsset:ScriptableObject{
     public GameMode gameMode;
     public List<InputBindingEntry>bindings;
    }
    [Serializable]
    internal class InputBindingEntry{
     public InputAction action;
     public InputCombinationType combinationType;
     public DeviceInput[]inputCombination;
    }
}