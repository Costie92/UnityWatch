using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayButton : MonoBehaviour {
    [SerializeField] private Button button;
	// Use this for initialization
	void Start () {
        button = this.GetComponent<Button>();
        button.onClick.AddListener(onClickPlay);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
    public void onClickPlay() {
        SceneManager.LoadScene(1);
    }
}
