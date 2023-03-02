using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; //场景管理命名空间
using UnityEngine.AI;

//本来是要命名为SceneManager的，但是因为unity自带了这个类型，所以为了区别就用了Controller
public class SceneController : Singleton<SceneController>, IEndGameObserver
{
    GameObject player;
    public GameObject playerPrefab;
    NavMeshAgent playerAgent;
    public SceneFader fadeCanvasPrefab; //渐入渐出画布
    bool fadeFinished; //渐入渐出是否完成

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this); //在切换场景的时候不把SceneController删除掉
    }

    private void Start()
    {
        GameManager.Instance.AddObserver(this); //IEndGameObserver,实现人物死亡-结束游戏后返回主菜单
        fadeFinished = true;
    }

    //传送到目的地，参数为传送点
    // public void TransitionToDestination(TransitionPoint transitionPoint)
    // {
    //     switch (transitionPoint.transitionType)
    //     {
    //         case TransitionPoint.TransitionType.SAMESCENE:
    //             StartCoroutine(Transition(SceneManager.GetActiveScene().name, transitionPoint.destinationTag));
    //             break;
    //         case TransitionPoint.TransitionType.DIFFERENTSCENE:
    //             StartCoroutine(Transition(transitionPoint.sceneName, transitionPoint.destinationTag));
    //             break;
    //     }
    // }

    //参数为 场景名, 当前传送点要传送到的目的地的tag
    IEnumerator Transition(string sceneName, TransitionDestination.DestinationTag destinationTag)
    {
        //TODO:切换场景前保存数据
        SaveManager.Instance.SavePlayerData();

        SceneFader fadeCanvas = Instantiate(fadeCanvasPrefab);

        if (sceneName != SceneManager.GetActiveScene().name) //不同场景传送
        {
            //FIXME:添加淡入淡出效果
            yield return fadeCanvas.FadeIn(fadeCanvas.fadeInDuration); //淡入

            yield return SceneManager.LoadSceneAsync(sceneName); //异步加载等待场景加载完毕
            yield return Instantiate(playerPrefab, GetDestination(destinationTag).transform.position, GetDestination(destinationTag).transform.rotation); //等待Player生成完毕

            //玩家生成完毕后再读取数据
            SaveManager.Instance.LoadPlayerData();

            yield return fadeCanvas.FadeOut(fadeCanvas.fadeOutDuration); //淡出
            yield break;
        }
        else //同场景传送
        {
            yield return fadeCanvas.FadeIn(fadeCanvas.fadeInDuration); //淡入

            player = GameManager.Instance.playerStates.gameObject;
            playerAgent = player.GetComponent<NavMeshAgent>();
            playerAgent.enabled = false; //解决传送前点击地面，导航路线未完成导致传送后跟着导航路线走的问题

            player.transform.SetPositionAndRotation(GetDestination(destinationTag).transform.position, GetDestination(destinationTag).transform.rotation);

            playerAgent.enabled = true;
            yield return fadeCanvas.FadeOut(fadeCanvas.fadeOutDuration); //淡出
            yield break;
        }

    }

    //新建游戏
    public void TransitionToFirstLevel()
    {
        StartCoroutine(LoadLevel("Forest"));
    }

    //继续游戏
    public void TransitionToLoadGame()
    {
        StartCoroutine(LoadLevel(SaveManager.Instance.SceneName));
    }


    //返回到主菜单
    public void TransitionToMainMenu()
    {
        StartCoroutine(LoadMainMenu());
    }


    IEnumerator LoadLevel(string scene)
    {
        SceneFader fadeCanvas = Instantiate(fadeCanvasPrefab);
        if (scene != "")
        {
            yield return fadeCanvas.FadeIn(fadeCanvas.fadeInDuration); //淡入

            yield return SceneManager.LoadSceneAsync(scene);

            yield return player = Instantiate(playerPrefab, GameManager.Instance.GetEntrance().position, GameManager.Instance.GetEntrance().rotation);
            
            SaveManager.Instance.LoadPlayerData();
            
            SaveManager.Instance.SavePlayerData();

            yield return fadeCanvas.FadeOut(fadeCanvas.fadeOutDuration); //淡出
            yield break;
        }
    }

    //加载主菜单
    IEnumerator LoadMainMenu()
    {
        SceneFader fadeCanvas = Instantiate(fadeCanvasPrefab);
        yield return fadeCanvas.FadeIn(fadeCanvas.fadeInDuration); //淡入

        yield return SceneManager.LoadSceneAsync("MainMenu");

        yield return fadeCanvas.FadeOut(fadeCanvas.fadeOutDuration); //淡出

        yield break;
    }

    //根据 当前传送点要传送到的目的地 的 tag 获取目的地
    private TransitionDestination GetDestination(TransitionDestination.DestinationTag destinationTag)
    {
        var destinationPoint = FindObjectsOfType<TransitionDestination>(); //目的地点位，即每个传送门的子oj

        for (int i = 0; i < destinationPoint.Length; i++)
        {
            if (destinationPoint[i].destinationTag == destinationTag) //当某个目的地点位 的 目的地tag == 当前传送的要传送到的目的地 的 tag
            {
                return destinationPoint[i];
            }
        }

        return null;
    }

    public void EndNotify()
    {
        if (fadeFinished)
        {
            fadeFinished = false;
            StartCoroutine(LoadMainMenu());
        }
    }
}
