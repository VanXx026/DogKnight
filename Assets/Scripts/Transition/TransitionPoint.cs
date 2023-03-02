using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//传送点
public class TransitionPoint : MonoBehaviour
{
    public enum TransitionType //传送的类型，是相同场景还是不同场景
    {
        SAMESCENE, DIFFERENTSCENE
    }

    [Header("Transition Info")]
    public string sceneName; //传送目的地的场景名
    public TransitionType transitionType; //创建枚举类型的实例
    public TransitionDestination.DestinationTag destinationTag; //要传送到的目的地的tag

    private bool canTrans; //能否被传送

    private void Update()
    {
        // if(Input.GetKeyDown(KeyCode.F) && canTrans)
        // {
        //     //TODO:SceneController 传送
        //     SceneController.Instance.TransitionToDestination(this); 
        // }
    }

    //当进入可以进行传送的区域时
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
            canTrans = true;
    }

    //当离开可以进行传送的区域时
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            canTrans = false;
    }
}
