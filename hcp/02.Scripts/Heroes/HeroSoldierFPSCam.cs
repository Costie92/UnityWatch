using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroSoldierFPSCam : MonoBehaviour {
    [SerializeField]
    ParticleSystem fpsNormalMuzzleFlash;
    [SerializeField]
    GameObject rifleModel;

    public void HSNormalMuzzleFlash()
    {
        fpsNormalMuzzleFlash.Play();
    }
}
