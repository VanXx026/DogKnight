using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//不继承任何类
//static:随时随地都可以快速调用方法
public static class ExtensionMethod
{
    //攻击扇面的余弦值，也就是说扇面为(-60, 60)
    private const float dotThresheld = 0.5f; //需要使用const，因为是在static类中 
    //this关键字：后面跟着的类就是需要扩展的类，就是可以直接通过一个实例的transform.IsFacingTarget()调用这个方法
    public static bool IsFacingTarget(this Transform transform, Transform target)
    {
        //target的方向
        var vectorToTarget = target.position - transform.position;
        vectorToTarget.Normalize();

        float dot = Vector3.Dot(transform.forward, vectorToTarget);

        return dot >= dotThresheld;
    }
}
