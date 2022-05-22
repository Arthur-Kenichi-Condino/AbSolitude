using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
namespace AKCondinoO{
    internal class CultureInfoUtil{
     internal static readonly CultureInfo en_US=new CultureInfo("en-US");
     internal static readonly CultureInfo pt_BR=new CultureInfo("pt-BR");
        internal static void SetUtil(){
         en_US.NumberFormat.CurrencyDecimalSeparator=".";
         en_US.NumberFormat.  NumberDecimalSeparator=".";
         en_US.NumberFormat. PercentDecimalSeparator=".";
          en_US.NumberFormat.NumberGroupSeparator="";
         pt_BR.NumberFormat.CurrencyDecimalSeparator=",";
         pt_BR.NumberFormat.  NumberDecimalSeparator=",";
         pt_BR.NumberFormat. PercentDecimalSeparator=",";
          pt_BR.NumberFormat.NumberGroupSeparator="";
        }
    }
}