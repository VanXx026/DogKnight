using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

//观察者模式的具体主题subject类，在这里聚集订阅者（通过List），并实现了方法用于增加观察者和删除观察者
public class GameManager : Singleton<GameManager>
{
    private CinemachineFreeLook followCamera;
    public CharacterStates playerStates;
    //收集实现了接口的游戏物体
    List<IEndGameObserver> endGameObservers = new List<IEndGameObserver>();

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this); //在切换场景的时候不把GameManager删除掉
    }

    //FIXME:第三人称需修改
    public void RegisterPlayer(CharacterStates player)
    {
        playerStates = player;

        //在获得player信息后将相机初始化
        followCamera = FindObjectOfType<CinemachineFreeLook>();

        if (followCamera != null)
        {
            followCamera.Follow = playerStates.transform.GetChild(2);
            followCamera.LookAt = playerStates.transform.GetChild(2);
        }
    }

    //添加观察者
    public void AddObserver(IEndGameObserver observer)
    {
        endGameObservers.Add(observer); //因为逻辑是敌人生成之后才会添加到列表中，所以不用担心有重复的问题
    }

    //移除观察者
    public void RemoveObserver(IEndGameObserver observer)
    {
        endGameObservers.Remove(observer);
    }

    //主题的状态变化提示方法，该方法告知所有的观察者状态变化并执行自己的EndNotify方法
    public void NotifyObserver()
    {
        foreach (var observer in endGameObservers)
        {
            observer.EndNotify();
        }
    }

    //FIXME:这个方法是否可以写在SceneController中？
    //FIXME:暂时不打算实现传送门功能
    public Transform GetEntrance()
    {
        foreach (var item in FindObjectsOfType<TransitionDestination>())
        {
            if (item.destinationTag == TransitionDestination.DestinationTag.ENTER)
            {
                return item.transform;
            }
        }
        return null;
    }
}
