using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using EzySlice;

public class PlayerController : MonoBehaviour
{
    private Animator anim;
    //存放着人物信息的脚本组件CharacterStates，所有的SO脚本都与其相关联
    private CharacterStates characterStates;
    private CharacterController characterController;
    private bool isDead;
    //攻击的目标
    private List<GameObject> attackTarget;
    public float maxSpeed;
    float currentVelocity = 0f;
    float attackTime = 1f;
    int attackNum = 0;
    private Coroutine coroutine = null;

    //可能不需要
    // //需要头文件：UnityEngine.AI
    // private NavMeshAgent agent;
    // //记录玩家的agent中的stoppingDistance
    // private float stoppingDistance;


    //所有对自身变量的获取都应该放在awake方法中，比如RigidBody，Animator等，养成好习惯
    private void Awake()
    {
        // agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        characterStates = GetComponent<CharacterStates>();
        characterController = GetComponent<CharacterController>();
        attackTarget = new List<GameObject>();
    }

    private void OnEnable()
    {
        GameManager.Instance.RegisterPlayer(characterStates);
    }

    // private void Start()
    // {
    //     SaveManager.Instance.LoadPlayerData();
    // }

    // private void OnDisable()
    // {
    //     //取消订阅
    //     if(!MouseManager.IsInitialized) return;
    //     MouseManager.Instance.OnMouseClicked -= MoveToTarget;
    //     MouseManager.Instance.OnEnemyClicked -= EventAttack;
    // }

    private void Update()
    {
        isDead = characterStates.characterData.currentHealth == 0;

        if (isDead)
            GameManager.Instance.NotifyObserver(); //昭告天下，皇上驾崩了

        Attack();

        switchAnimation();
    }

    private void FixedUpdate()
    {
        Movement();
    }

    //动画切换设置
    private void switchAnimation()
    {
        anim.SetFloat("Speed", characterController.velocity.sqrMagnitude, 0.2f, Time.deltaTime);
        anim.SetBool("Death", isDead);
    }

    private void Movement()
    {
        //获得键盘输入
        float horizontal = Input.GetAxisRaw("Horizontal"); //左右
        float vertical = Input.GetAxisRaw("Vertical"); //上下
        Vector3 direction = new Vector3(horizontal, 0, vertical).normalized; //人物移动的方向，不考虑相机

        //人物如果要开始移动
        if (direction.sqrMagnitude > 0.01f)
        {
            float moveDegree = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + Camera.main.transform.eulerAngles.y; //移动方向 = 玩家控制wsad的方向 + 摄像机正视方向
            moveDegree = Mathf.SmoothDampAngle(transform.eulerAngles.y, moveDegree, ref currentVelocity, 0.1f);
            transform.localRotation = Quaternion.Euler(0, moveDegree, 0); // 改变当前旋转角度
            Vector3 moveDir = Quaternion.Euler(0f, moveDegree, 0f) * Vector3.forward; 
            characterController.Move(moveDir.normalized * maxSpeed * Time.deltaTime);
        }
        else
        {
            characterController.Move(Vector3.zero);
        }
    }

    public void StopMove()
    {
        characterController.Move(Vector3.zero);
    }

    private void Attack()
    {
        if (Input.GetMouseButtonDown(0))
        {
            anim.SetTrigger("Attack");
            anim.SetInteger("AttackNum", ++attackNum);
            if (coroutine != null)
                StopCoroutine(coroutine);
            coroutine = StartCoroutine("IEAttack");
        }
    }

    IEnumerator IEAttack()
    {
        while (attackTime > 0)
        {
            attackTime -= Time.deltaTime;
            yield return null;
        }
        attackNum = 0;
        attackTime = 1f;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<EnemyController>())
        {
            attackTarget.Add(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<EnemyController>())
        {
            if (attackTarget.Contains(other.gameObject))
            {
                attackTarget.Remove(other.gameObject);
            }
        }
    }

    // Animation Event
    void Hit()
    {
        // //如果attackTarget是可攻击的物体
        // if (attackTarget.CompareTag("Attackable"))
        // {
        //     //反击时石头是否为HITNOTHING状态
        //     if (attackTarget.GetComponent<Rock>() && attackTarget.GetComponent<Rock>().rockStates == Rock.RockStates.HITNOTHING)
        //     {
        //         attackTarget.GetComponent<Rock>().rockStates = Rock.RockStates.HITENEMY;
        //         //同样，在玩家击飞石头前，也需要施加一个初始速度防止石头进入HITNOTHING状态
        //         attackTarget.GetComponent<Rigidbody>().velocity = Vector3.one;
        //         //FIXME:需要添加一个forceToRock来调整这个力的大小
        //         attackTarget.GetComponent<Rigidbody>().AddForce(transform.forward * 10, ForceMode.Impulse);
        //     }
        // }
        // //否则为敌人
        // else
        // {
        //     var targetStates = attackTarget.GetComponent<CharacterStates>();

        //     targetStates.TakeDamage(characterStates, targetStates);
        // }
        foreach (var target in attackTarget)
        {
            //FIXME:可能存在敌人死亡后还留在List里面的问题
            CharacterStates targetStates;
            if (target != null)
            {
                targetStates = target.GetComponent<CharacterStates>();
                targetStates.TakeDamage(characterStates, targetStates);
                targetStates.TakeDefenceBroken(characterStates, targetStates);
            }
        }
    }

    // //用于获得玩家的positoin和rotation
    // public void GetPosAndRotate()
    // {
    //     characterStates.characterData.playerPosition = transform.position;
    //     characterStates.characterData.playerRotation = transform.rotation;
    // }
}
