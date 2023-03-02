using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Rock : MonoBehaviour
{
    public enum RockStates { HITPLAYER, HITENEMY, HITNOTHING };
    public RockStates rockStates;
    private Rigidbody rb;
    [Header("Basic Settings")]
    public float force;
    public int damage;
    public GameObject target;
    public GameObject rockBreakPartical; //获取石头破碎效果的预制体
    private Vector3 direction;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.velocity = Vector3.one; //Rock生成是速度为0，防止一开始就进入HITNOTHING状态
        rockStates = RockStates.HITPLAYER;
        FlyToTarget();
    }

    private void FixedUpdate()
    {
        //sqrMagnitude:用于计算距离，但是和distance的区别是没有开方
        if (rb.velocity.sqrMagnitude < 1f)
        {
            rockStates = RockStates.HITNOTHING;
        }
    }

    public void FlyToTarget()
    {
        if (target == null)
            target = FindObjectOfType<PlayerController>().gameObject;

        direction = target.transform.position - transform.position + Vector3.up; //加上v3.up是为了给石头一个斜向上的方向
        rb.AddForce(direction * force, ForceMode.Impulse); //ForceMode.Impulse 给该物体一个瞬间的力
    }

    //打中的物体碰撞判断
    private void OnCollisionEnter(Collision other)
    {
        switch (rockStates)
        {
            case RockStates.HITPLAYER:
                if (other.gameObject.CompareTag("Player"))
                {
                    other.gameObject.GetComponent<NavMeshAgent>().isStopped = true;
                    other.gameObject.GetComponent<NavMeshAgent>().velocity = direction * force; //受击击退
                    other.gameObject.GetComponent<CharacterStates>().TakeDamage(damage, other.gameObject.GetComponent<CharacterStates>());

                    other.gameObject.GetComponent<Animator>().SetTrigger("Dizzy");

                    rockStates = RockStates.HITNOTHING;
                }
                break;

            case RockStates.HITENEMY:
                //判断组件是否存在，GetComponent也会返回一个bool值
                if (other.gameObject.GetComponent<Golem>())
                {
                    var otherStates = other.gameObject.GetComponent<CharacterStates>();
                    otherStates.TakeDamage(damage, otherStates);

                    Instantiate(rockBreakPartical, transform.position, Quaternion.identity); //石头碰撞到敌人后生成破碎石头特效
                    Destroy(gameObject);
                }
                break;
        }
    }
}
