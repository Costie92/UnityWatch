using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace hcp
{
    [System.Serializable]
    public enum E_ControlParam
    {
        NormalAttack,
        Reload,

        FirstSkill,
        // SecondSkill,  스킬 하나만 두기로 변경
        Ultimate,

        MAX
    }
    [System.Serializable]
    public enum E_BadState  //상태이상
    {
        None,

        Stun,

        MAX
    }
    [System.Serializable]
    public enum E_HeroType
    {
        Soldier,
        Hook,
        MAX
    }




    public class DataST
    {

    }
}