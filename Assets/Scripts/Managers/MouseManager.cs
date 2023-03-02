using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
// using UnityEngine.Events; 单例模式舍弃

/*写于使用单例模式后*/
// 由于使用了单例模式，已经不需要拖拽脚本实现功能，所以这段代码注释掉了
// //系统序列化，可以让下面这个event事件在检查器inspector中显示出来
// [System.Serializable]
// public class EventVector3 : UnityEvent<Vector3> { }

/*写于使用单例模式泛型后*/
// public class MouseManager : MonoBehaviour //写好单例模式泛型后就不需要继承自MonoBehaviour了
public class MouseManager : Singleton<MouseManager>
{
    //单例模式
    //用于解决需要拖动挂载脚本的麻烦情况
    // public static MouseManager Instance; //写好单例模式泛型后就不需要了

    //指针2D图标
    public Texture2D point, doorway, attack, target, arrow;
    
    //获得射线撞击后的各种信息
    RaycastHit hitInfo;
    
    //event关键字修饰delegate类型（Action<T>）
    //Action是C#自带的泛型委托，T是Vector3类型
    public event Action<Vector3> OnMouseClicked;
    // public EventVector3 OnMouseClicked; 单例模式舍弃
    
    //当点击到敌人的时候，订阅者方法启动
    public event Action<GameObject> OnEnemyClicked;

    //如果Awake()内容和泛型中定义的一致，那么就不需要再写了
    // private void Awake()
    // {
    //     //因为在切换场景的时候，这个场景的MouseManager会被销毁；
    //     //而从另一个场景切换回来的时候，unity并不是恢复到这个场景，而是新建了一个场景，所以会新建一个MouseManager
    //     //而单例模式的目的是为了使整个游戏进程中该类只存在一个实例，所以检测到Instance非空，就要销毁。
    //     if (Instance != null)
    //     {
    //         Destroy(gameObject);
    //     }
    //     Instance = this;
    // }

    //如果需要重写Awake()的话，就可以这么写：
    protected override void Awake()
    {
        //在父类定义的Awake方法的基础上运行
        base.Awake();
        //附加内容
        DontDestroyOnLoad(this);
    }

    private void Update()
    {
        SetCursorTexture();
        MouseControl();
    }

    void SetCursorTexture()
    {
        //根据鼠标在屏幕坐标位置射出一条射线
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        //当射线与碰撞体有碰撞时返回true
        //out 关键字，一个方法返回多个返回值时用（大概
        if (Physics.Raycast(ray, out hitInfo))
        {
            //切换鼠标贴图
            switch (hitInfo.collider.gameObject.tag)
            {
                case "Ground":
                    Cursor.SetCursor(target, new Vector2(16, 16), CursorMode.Auto);
                    break;
                case "Enemy":
                    Cursor.SetCursor(attack, new Vector2(16, 16), CursorMode.Auto);
                    break;
                case "Attackable":
                    Cursor.SetCursor(attack, new Vector2(16, 16), CursorMode.Auto);
                    break;
                case "Portal":
                    Cursor.SetCursor(doorway, new Vector2(16, 16), CursorMode.Auto);
                    break;
                default:
                    Cursor.SetCursor(arrow, new Vector2(16, 16), CursorMode.Auto);
                    break;
            }
        }
    }

    void MouseControl()
    {
        //判断鼠标左键是否点击 且 射线是否射到存在碰撞体，如果射到地图外即为null
        if (Input.GetMouseButtonDown(0) && hitInfo.collider != null)
        {
            if (hitInfo.collider.gameObject.CompareTag("Ground"))
            {
                //鼠标事件非空(存在订阅者)则执行其方法
                OnMouseClicked?.Invoke(hitInfo.point);
            }
            if (hitInfo.collider.gameObject.CompareTag("Enemy"))
            {
                //鼠标事件非空(存在订阅者)则执行其方法
                OnEnemyClicked?.Invoke(hitInfo.collider.gameObject);
            }
            if (hitInfo.collider.gameObject.CompareTag("Attackable"))
            {
                //鼠标事件非空(存在订阅者)则执行其方法
                OnEnemyClicked?.Invoke(hitInfo.collider.gameObject);
            }
            if (hitInfo.collider.gameObject.CompareTag("Portal"))
            {
                //鼠标事件非空(存在订阅者)则执行其方法
                OnMouseClicked?.Invoke(hitInfo.point);
            }
        }
    }
}
