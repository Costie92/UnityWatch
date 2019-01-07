using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelegateCtrl : ActiveCtrl {
    System.Action action;
    public DelegateCtrl(E_ControlParam contParam, float coolTime , System.Action action):base(contParam,coolTime)
    {
        if (action == null)
        {
            Debug.LogError("DelegateCtrl : 델리게이트 전달 불가");
        }
        this.action = action;
    }
    public override void Activate()
    {
        base.Activate();
        action();
    }
}
