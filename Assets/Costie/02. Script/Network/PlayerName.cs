using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using UnityEngine.UI;
public class PlayerName : MonoBehaviour {
    public bool HaveName;
    public string MyName;
    public static string path;
    public static string Filepath;
    [SerializeField] private GameObject InputField;
    [SerializeField] private InputField.SubmitEvent submitEvent;
    private static PlayerName _instance = null;
    public static PlayerName instance
    {
        get
        {
            if (_instance == null)
                Debug.LogError("PlayerName is NULL");
            return _instance;
        }
    }
    private void Awake()
    {
        path = Application.persistentDataPath + "/Player";
        Filepath = "Name.txt";
        _instance = this;
    }
    // Use this for initialization
    void Start () {
        HaveName = IsHaveName();
        if (!HaveName) {
            InputField.SetActive(true);
            submitEvent = new InputField.SubmitEvent();
            submitEvent.AddListener(OnSendName);
            InputField.GetComponent<InputField>().onEndEdit = submitEvent;
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}
    public bool IsHaveName() {
        try
        {
            // Determine whether the directory exists.
            if (Directory.Exists(path))
            {
                try
                {
                    if (File.Exists(path + "/" + Filepath))
                    {
                        StreamReader sr = new StreamReader(path + "/" + Filepath);
                        string fileContext = sr.ReadToEnd();
                        sr.Close();
                        MyName = fileContext;
                        return true;
                    }
                }
                catch (Exception e)
                {
                    Debug.Log("The process failed : " + e.ToString());
                }
            }

        }
        catch (Exception e)
        {
            Debug.Log("The process failed : " + e.ToString());
        }
        return false;
    }

    public void OnSendName(string arg)
    {
        if (arg != "")
        {
            try
            {
                // Determine whether the directory exists.
                if (Directory.Exists(path))
                {
                    Debug.Log("That path exists already.");
                }
                Directory.CreateDirectory(path);
                try
                {
                    if (File.Exists(path + "/" + Filepath))
                    {
                        Debug.Log("File path exist already.");
                    }
                    File.Create(path + "/" + Filepath).Dispose();

                    StreamWriter sw1 = new StreamWriter(path + "/" + Filepath);
                    sw1.Write(arg);
                    sw1.Close();
                    InputField.SetActive(false);
                    MyName = arg;
                    NetworkManager.instance.TryJoinRandomRoom();
                }
                catch (Exception e)
                {
                    Debug.Log("The process failed : " + e.ToString());
                }

            }
            catch (Exception e)
            {
                Debug.Log("The process failed : " + e.ToString());
            }
        }
    }
}