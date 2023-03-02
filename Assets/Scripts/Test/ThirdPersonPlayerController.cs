using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ThirdPersonPlayerController : MonoBehaviour
{
    NavMeshAgent agent;
    CharacterController characterController;
    Animator anim;
    public float maxSpeed;
    float currentVelocity = 0f;
    float attackTime = 1f;
    int attackNum = 0;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();

    }

    private void Update()
    {
        // switchRunAndWalk();
        Attack();

        //动画切换
        anim.SetFloat("Speed", characterController.velocity.sqrMagnitude, 0.2f, Time.deltaTime);
    }

    private void FixedUpdate()
    {
        Movement();
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
            transform.localRotation = Quaternion.Euler(0, moveDegree, 0);
            Vector3 moveDir = Quaternion.Euler(0f, moveDegree, 0f) * Vector3.forward;
            characterController.Move(moveDir.normalized * maxSpeed * Time.deltaTime);
        }
        else
        {
            characterController.Move(Vector3.zero);
        }
    }

    // private void switchRunAndWalk()
    // {
    //     if (Input.GetKeyDown(KeyCode.LeftShift) && isRunning)
    //     {
    //         isRunning = false;
    //         maxSpeed = maxSpeed / 2;
    //     }
    //     else if (Input.GetKeyDown(KeyCode.LeftShift) && !isRunning)
    //     {
    //         isRunning = true;
    //         maxSpeed = maxSpeed * 2;
    //     }
    // }

    private void Attack()
    {
        // StartCoroutine("IEAttack");
        if (Input.GetMouseButtonDown(0))
        {
            anim.SetTrigger("Attack");
            anim.SetInteger("AttackNum", ++attackNum);
            StopCoroutine("IEAttack");
            StartCoroutine("IEAttack");
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
}
