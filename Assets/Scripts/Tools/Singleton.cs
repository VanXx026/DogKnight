using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//单例模式泛型
public class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    private static T instance;

    //提供外部访问
    public static T Instance
    {
        get { return instance; }
    }

    //判断当前类型的单例是否已经初始化过了
    public static bool IsInitialized
    {
        get { return instance != null; }
    }

    //protected：只允许自己和继承者访问这个方法
    //virtual：虚函数，允许继承者重写这个函数
    protected virtual void Awake()
    {
        if (instance != null)
            Destroy(gameObject);
        else
            instance = (T)this;
    }

    //存在多个同类型的单例时销毁该单例
    protected virtual void Ondestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}
