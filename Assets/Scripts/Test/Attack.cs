using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : MonoBehaviour
{
    Animator anim;
    public float interval = 0.1f;
    Coroutine attackCoroutine;

    private void Awake()
    {
        anim = GetComponent<Animator>();

    }

    // private void Start()
    // {
    //     anim.SetTrigger("attack");
    // }

    private void Update()
    {
        // anim.SetTrigger("attack");
        if(Input.GetMouseButtonDown(0))
        {
            Debug.Log("in");
            anim.SetTrigger("attack");
            anim.ResetTrigger("back");

            if(attackCoroutine != null)
                StopCoroutine(attackCoroutine);

            attackCoroutine = StartCoroutine("DoAttack");
        }
    }

    //连击计时
    IEnumerator DoAttack() 
    {
        float time = interval;

        while(time >= 0)
        {
            time -= Time.deltaTime;

            yield return null;
        }

        anim.SetTrigger("back"); //连击时间已过，退出到Idle状态
        Debug.Log(time);

    }

}
