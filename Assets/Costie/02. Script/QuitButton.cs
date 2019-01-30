using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuitButton : MonoBehaviour {

    [SerializeField] private Button button;
    // Use this for initialization
    void Start()
    {
        button = this.GetComponent<Button>();
        button.onClick.AddListener(onClickQuit);
    }

    // Update is called once per frame
    void Update()
    {
        if(Application.platform == RuntimePlatform.Android)
        {
            if (Input.GetKey(KeyCode.Escape))
            {
                Application.Quit();
            }
        }
    }
    public void onClickQuit()
    {
        if (Application.platform == RuntimePlatform.WindowsEditor) {
            UnityEditor.EditorApplication.isPlaying = false;
        }
        else
        {
            Application.Quit();
        }
    }
}
