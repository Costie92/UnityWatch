using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public abstract class Hero : MonoBehaviourPun {
    protected E_BadState badState = E_BadState.None;
    
    protected Animator anim;
    [SerializeField]
    protected float maxHP;
    [SerializeField]
    protected float currHP;
    [SerializeField]
    protected bool IsDie = false;
    protected Dictionary<E_ControlParam, ActiveCtrl> activeCtrlDic = new Dictionary<E_ControlParam, ActiveCtrl>();

    [Tooltip("needed amount for Ult Activate")]
    [SerializeField]
    protected float neededUltAmount;
    [Tooltip("now Amount for Ult Activate")]
    [SerializeField]
    protected float nowUltAmount;

    [Tooltip("Screen Center Point Vector")]
    [SerializeField]
    protected Vector3 screenCenterPoint;

    [Tooltip("Max Shot Length (For infinite Range)")]
    [SerializeField]
    protected float maxShotLength = 5000;

    protected float maxShotLengthDiv;

    [SerializeField]
    protected int mapLayer;

    protected virtual void Awake()
    {
        mapLayer = LayerMask.NameToLayer(Constants.mapLayerName);
        maxShotLengthDiv = 1/maxShotLength;
        screenCenterPoint = new Vector3(Camera.main.pixelWidth / 2, Camera.main.pixelHeight / 2, Camera.main.nearClipPlane);
        anim = this.gameObject.GetComponent<Animator>();
        SetActiveCtrls();
    }

    protected virtual void SetActiveCtrls()//어웨이크 시 불러온다든지... 스킬들 세팅해주는 함수임.
    {
        activeCtrlDic.Clear();
    }
    public virtual void MoveHero(Vector3 moveV)
    {
    }
    public virtual void RotateHero(Vector3 rotateV)
    {
    }
    public virtual void ControlHero(E_ControlParam param)
    {
    }
    public virtual void GetDamaged(float damage)
    {
        currHP -= damage;
        Debug.Log("겟 데미지드");
    }
    public virtual void GetHealed(float heal)
    {
        currHP += heal;
        Debug.Log("겟 힐");
    }
    public virtual void PlusUltAmount(float value)
    {
        if (nowUltAmount < neededUltAmount)
        {
            nowUltAmount += value;
        }
        if (nowUltAmount > neededUltAmount)
        {
            nowUltAmount = neededUltAmount;
        }
    }
    public float UltAmountPercent
    {
        get {
            return nowUltAmount / neededUltAmount;
        }
    }
}
