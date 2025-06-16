using AKCondinoO.Sims.Actors;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace AKCondinoO{
    internal class ReflectionUtil{
     internal static Dictionary<(string name,Type derivedFrom),Type>typeByNameCache=new();
        //  [https://stackoverflow.com/questions/20008503/get-type-by-name#:~:text=To%20load%20a%20type%20by,GetType(typeof(System.]
        internal static Type GetTypeByName(string name,Type derivedFrom=null){
         Type result;
         if(typeByNameCache.TryGetValue((name,derivedFrom),out result)){
          return result;
         }
         result=
          AppDomain.CurrentDomain.GetAssemblies()
           .Reverse()
            .Select(assembly=>assembly.GetType(name))
             .FirstOrDefault(t=>{return t!=null&&(derivedFrom==null||IsTypeDerivedFrom(t,derivedFrom));})
          ??
           AppDomain.CurrentDomain.GetAssemblies()
            .Reverse()
             .SelectMany(assembly=>assembly.GetTypes())
              .FirstOrDefault(t=>{return t.Name.Contains(name)&&(derivedFrom==null||IsTypeDerivedFrom(t,derivedFrom));});
         typeByNameCache.Add((name,derivedFrom),result);
         return result;
        }
        internal static bool IsTypeDerivedFrom(Type t,Type baseType){
         return t==baseType||t.IsSubclassOf(baseType);
        }
    }
}