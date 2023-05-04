using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors.Skills.SkillBuffs{
    internal class SkillBuff{
     internal static readonly Queue<SkillBuff>pool=new Queue<SkillBuff>();
     internal static readonly List<SkillBuff>allActiveBuffs=new List<SkillBuff>();
    }
}