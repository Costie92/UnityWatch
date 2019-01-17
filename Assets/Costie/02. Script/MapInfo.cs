using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapInfo : MonoBehaviour {

    public Transform ASpawnPoint, BSpawnPoint, CSpawnPoint, DSpawnPoint;
    public GameObject BottomOutLine;

    private static MapInfo _instance = null;
    public static MapInfo instance
    {
        get
        {
            if (_instance == null)
                Debug.LogError("MapInfo is NULL");
            return _instance;
        }
    }
    // Use this for initialization
    private void Awake()
    {
        _instance = this;
    }
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
