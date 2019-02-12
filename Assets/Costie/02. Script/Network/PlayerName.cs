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
        //이름이 담긴 파일이 존재하는지 확인 이후 인풋필드 Active
        HaveName = IsHaveName();
        if (!HaveName) {
            InputField.SetActive(true);
            submitEvent = new InputField.SubmitEvent();
            submitEvent.AddListener(OnSendName);
            InputField.GetComponent<InputField>().onEndEdit = submitEvent;
        }
	}
	
    //파일의 존재 여부를 확인하여 있을 경우 이름을 불러옴
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
    //폴더가 없을 경우 인풋 필드에 적힌 string 값을 파일형태로 저장 후 NetworkManager에 전송
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