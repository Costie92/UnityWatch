using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveCtrl //일반 공격이나 스킬 재장전등 모두 포함하는 개념 .
{
    protected E_ControlParam controlParam;
    public E_ControlParam ControlParam {
        get { return controlParam; }
    }
    protected float coolTime;
    public float CoolTime
    {
        get { return coolTime; }
    }
    protected float lastActivatedTime = 0f;

    public ActiveCtrl(E_ControlParam contParam, float coolTime)
    {
        this.controlParam = contParam;
        this.coolTime = coolTime;
    }

    public virtual void Activate()
    {
        lastActivatedTime = Time.time;
    }
    public virtual bool IsCoolTimeOver()  //쿨타임  끝났는지 여부 반환.
    {
        if (lastActivatedTime + coolTime > Time.time)   //쿨탐 다 차지 않음.
        {
            return false;
        }

        return true;
    }
}
