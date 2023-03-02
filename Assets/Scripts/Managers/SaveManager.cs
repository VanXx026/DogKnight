using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : Singleton<SaveManager>
{
    string sceneName = "";
    public string SceneName { get { return PlayerPrefs.GetString(sceneName); } }
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);
    }

    //测试用
    private void Update()
    {
        // if(Input.GetKeyDown(KeyCode.Escape))
        // {
        //     SceneController.Instance.TransitionToMainMenu();
        //     Debug.Log("MainMenu");
        // }
        // if(Input.GetKeyDown(KeyCode.S))
        // {
        //     SavePlayerData();
        //     Debug.Log("Save");
        // }
        // if(Input.GetKeyDown(KeyCode.L))
        // {
        //     LoadPlayerData();
        //     Debug.Log("Load");
        // }
    }

    public void SavePlayerData()
    {
        Save(GameManager.Instance.playerStates.characterData, GameManager.Instance.playerStates.characterData.name);
    }

    public void LoadPlayerData()
    {
        Load(GameManager.Instance.playerStates.characterData, GameManager.Instance.playerStates.characterData.name);
    }

    //FIXME:当前保存的数据有：玩家的基本属性、玩家所在的场景
    public void Save(Object data, string key)
    {
        var jsonData = JsonUtility.ToJson(data); //将数据转化为Json字符串
        PlayerPrefs.SetString(key, jsonData); //将数据以键值形式建立起来
        PlayerPrefs.SetString(sceneName, SceneManager.GetActiveScene().name); //保存玩家当前所在的场景
        PlayerPrefs.Save(); //保存
    }

    public void Load(Object data, string key)
    {
        if (PlayerPrefs.HasKey(key))
        {
            JsonUtility.FromJsonOverwrite(PlayerPrefs.GetString(key), data); //将数据重写回Object中
        }
    }
}
