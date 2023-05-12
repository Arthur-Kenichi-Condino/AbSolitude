#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims{
    internal partial class SimObject{
     internal Stats?stats;
        internal struct Stats{
         bool pendingRefresh;
         //
         /// <summary>
         ///  Função get da integridade física atual para o Sim
         /// </summary>
         internal float IntegrityGet(SimObject sim){
          return integrity_value;
         }
         /// <summary>
         ///  Função set
         /// </summary>
         internal void IntegritySet(float value,SimObject sim){
          integrity_value=value;
         }
          float integrity_value;
          /// <summary>
          ///  
          /// </summary>
          public float maxIntegrity;
             /// <summary>
             ///  
             /// </summary>
             public float stamina;
              /// <summary>
              ///  
              /// </summary>
              public float maxStamina;
         /// <summary>
         ///  
         /// </summary>
         public float sanity;
          /// <summary>
          ///  
          /// </summary>
          public float maxSanity;
             /// <summary>
             ///  
             /// </summary>
             public float focus;
              /// <summary>
              ///  
              /// </summary>
              public float maxFocus;
         //
         /// <summary>
         ///  
         /// </summary>
         public float bodily_kinesthetic;
             /// <summary>
             ///  
             /// </summary>
             public float strength;
         /// <summary>
         ///  
         /// </summary>
         public float spatial;
             /// <summary>
             ///  
             /// </summary>
             public float agility;
         /// <summary>
         ///  
         /// </summary>
         public float naturalistic;
             /// <summary>
             ///  
             /// </summary>
             public float vitality;
         /// <summary>
         ///  
         /// </summary>
         public float interpersonal;
         /// <summary>
         ///  
         /// </summary>
         public float intrapersonal;
         /// <summary>
         ///  
         /// </summary>
         public float linguistic;
         /// <summary>
         ///  
         /// </summary>
         public float logical_mathematical;
             /// <summary>
             ///  
             /// </summary>
             public float intelligence;
         /// <summary>
         ///  
         /// </summary>
         public float musical;
             /// <summary>
             ///  
             /// </summary>
             public float dexterity;
         //
         /// <summary>
         ///  
         /// </summary>
         public float luck;
                 //
                 /// <summary>
                 ///  
                 /// </summary>
                 public float physicalPowerFlatValue;
                 /// <summary>
                 ///  
                 /// </summary>
                 public float physicalDefenseFlatValue;
                 /// <summary>
                 ///  
                 /// </summary>
                 public float magicalPowerFlatValue;
                 /// <summary>
                 ///  
                 /// </summary>
                 public float magicalDefenseFlatValue;
        }
    }
}