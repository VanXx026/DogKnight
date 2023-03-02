using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController_Test : MonoBehaviour
{
    Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }
    public void GetHit()
    {
        anim.SetTrigger("Hit");
    }
}
