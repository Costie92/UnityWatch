﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroSoldierFPSCam : MonoBehaviour {
    [SerializeField]
    ParticleSystem fpsNormalMuzzleFlash;
    [SerializeField]
    HSHealDrone fpsHealDrone;

    public void HSNormalMuzzleFlash()
    {
        fpsNormalMuzzleFlash.Play();
    }
    public void HSHealDroneShow()
    {
        fpsHealDrone.gameObject.SetActive(true);
    }
}
