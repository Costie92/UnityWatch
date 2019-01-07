using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class Projectile : MonoBehaviourPun {
    [Tooltip("it could be damage or heal amount")]
    [SerializeField]
    protected float amount;
    
    [SerializeField]
    protected float velocity;
    
}
