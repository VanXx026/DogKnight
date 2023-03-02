using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Playables; //TimeLine

public class MainMenu : MonoBehaviour
{
    Button newGameBtn;
    Button continueBtn;
    Button exitBtn;
    PlayableDirector playableDirector;

    private void Awake()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        newGameBtn = transform.GetChild(1).GetComponent<Button>();
        continueBtn = transform.GetChild(2).GetComponent<Button>();
        exitBtn = transform.GetChild(3).GetComponent<Button>();

        //添加onClick事件
        newGameBtn.onClick.AddListener(PlayTimeLine); //点击newGameBtn时先执行TimeLine动画
        continueBtn.onClick.AddListener(ContinueGame);
        exitBtn.onClick.AddListener(ExitGame);

        //获取TimeLine文件
        playableDirector = FindObjectOfType<PlayableDirector>(); 
        playableDirector.stopped += NewGame; //当TimeLine播放完后执行NewGame方法
    }

    //播放TimeLine
    private void PlayTimeLine()
    {
        playableDirector.Play();
    }

    //obj是不用的参数，只是上面playableDirector.stopped添加委托的时候说需要这个类型的参数
    private void NewGame(PlayableDirector obj)
    {
        PlayerPrefs.DeleteAll();
        //转换场景
        SceneController.Instance.TransitionToFirstLevel();
    }

    private void ContinueGame()
    {
        //转换场景
        SceneController.Instance.TransitionToLoadGame();
    }

    private void ExitGame()
    {
        Application.Quit(); //退出游戏
        Debug.Log("退出游戏");
    }
}
