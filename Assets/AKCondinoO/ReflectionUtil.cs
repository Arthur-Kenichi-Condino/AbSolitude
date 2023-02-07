using AKCondinoO.Sims.Actors;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace AKCondinoO{
    internal class ReflectionUtil{
        //  [https://stackoverflow.com/questions/20008503/get-type-by-name#:~:text=To%20load%20a%20type%20by,GetType(typeof(System.]
        internal static Type GetTypeByName(string name){
         return
          AppDomain.CurrentDomain.GetAssemblies()
           .Reverse()
            .Select(assembly=>assembly.GetType(name))
             .FirstOrDefault(t=>t!=null)
          ??
           AppDomain.CurrentDomain.GetAssemblies()
            .Reverse()
             .SelectMany(assembly=>assembly.GetTypes())
              .FirstOrDefault(t=>t.Name.Contains(name));
        }
        internal static bool IsTypeDerivedFrom(Type t,Type baseType){
         return t==baseType||t.IsSubclassOf(baseType);
        }
    }
}