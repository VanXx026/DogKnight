using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EzySlice;
using DG.Tweening;
using Cinemachine;

public class BladeModeController : MonoBehaviour
{
    private Animator anim;
    private bool isBladeMode;
    private float normalFOV; //普通模式下视野
    public float bladeModeFOV; //刀剑模式下视野
    private Vector3 normalOffset;
    public Vector3 bladeModeOffset;
    public GameObject cutPlane; //切割平板，在主摄像机上
    public Material crossMaterial;
    public LayerMask layerMask; //Cuttable
    public CinemachineFreeLook freeLook;
    private PlayerController playerController;
    private CinemachineComposer[] composers;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        playerController = GetComponent<PlayerController>();
        freeLook = FindObjectOfType<CinemachineFreeLook>();
        cutPlane = Camera.main.transform.GetChild(0).gameObject;

        normalFOV = freeLook.m_Lens.FieldOfView;

        composers = new CinemachineComposer[3];
        for (int i = 0; i < 3; i++)
            composers[i] = freeLook.GetRig(i).GetCinemachineComponent<CinemachineComposer>();
        normalOffset = composers[0].m_TrackedObjectOffset;

        isBladeMode = false;
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; //鼠标的模式，锁定在屏幕中间
        Cursor.visible = false; //鼠标不可见

        anim.SetBool("BladeMode", isBladeMode);
    }

    private void Update()
    {

        if (Input.GetMouseButtonDown(1)) //按下时开启刀剑模式
        {
            BladeMode(true);
        }
        if (Input.GetMouseButtonUp(1)) //松开时开启刀剑模式
        {
            BladeMode(false);
        }

        //如果在刀剑模式下
        if (isBladeMode)
        {
            //人物跟随镜头旋转
            transform.rotation = Quaternion.Lerp(transform.rotation, Camera.main.transform.rotation, 0.2f);
            RotatePlane();

            if (Input.GetMouseButtonDown(0))
            {
                //FIXME:加入切割动画

                Slice();
            }
        }
    }

    private void RotatePlane()
    {
        cutPlane.transform.eulerAngles += new Vector3(0, 0, -Input.GetAxis("Mouse X") * 5);
    }

    //在两个状态之前切换、true则刀剑模式，false则普通模式
    private void BladeMode(bool state)
    {
        isBladeMode = state;
        anim.SetBool("BladeMode", isBladeMode);

        cutPlane.transform.localEulerAngles = Vector3.right * -2; //初始下，切割平面为Euler(0,0,0)
        cutPlane.SetActive(state); //是否启用切割平面

        //如果为true，则使用键盘的"Horizontal"和"Vertical"控制镜头移动
        //如果为false，则使用鼠标的"Mouse X"(左右)和"Mouse Y"(上下)控制镜头移动
        string x = state ? "Horizontal" : "Mouse X";
        string y = state ? "Vertical" : "Mouse Y";
        freeLook.m_XAxis.m_InputAxisName = x;
        freeLook.m_YAxis.m_InputAxisName = y;

        //TODO:应该是切换两种状态吧
        float fov = state ? bladeModeFOV : normalFOV;
        Vector3 offset = state ? bladeModeOffset : normalOffset;
        //在刀剑模式下，慢动作(0.2f)
        float timeScale = state ? .2f : 1;

        //子弹时间
        Time.timeScale = timeScale;
        //TODO:平滑地设置补间动画
        DOVirtual.Float(freeLook.m_Lens.FieldOfView, fov, .1f, FieldOfView); //从正常视野到刀剑模式视野的补间
        DOVirtual.Float(composers[0].m_TrackedObjectOffset.x, offset.x, .2f, CameraOffsetX);//.SetUpdate(true); //从人物在视野中间到视野左侧的补间
        DOVirtual.Float(composers[0].m_TrackedObjectOffset.y, offset.y, .2f, CameraOffsetY);//.SetUpdate(true); //从人物在视野中间到视野左侧的补间

        //Stop character movement
        playerController.enabled = !state;
    }

    void FieldOfView(float fov)
    {
        freeLook.m_Lens.FieldOfView = fov;
    }

    void CameraOffsetX(float x)
    {
        foreach (CinemachineComposer c in composers)
        {
            c.m_TrackedObjectOffset.Set(x, c.m_TrackedObjectOffset.y, c.m_TrackedObjectOffset.z);
        }
    }

    void CameraOffsetY(float y)
    {
        foreach (CinemachineComposer c in composers)
        {
            c.m_TrackedObjectOffset.Set(c.m_TrackedObjectOffset.x, y, c.m_TrackedObjectOffset.z);
        }
    }

    private void Slice()
    {
        Collider[] hits = Physics.OverlapBox(cutPlane.transform.position, new Vector3(5, 0.1f, 5), cutPlane.transform.rotation, layerMask);

        if (hits.Length <= 0)
            return;

        for (int i = 0; i < hits.Length; i++)
        {
            //给砍到的go附加静态网格
            if (hits[i].transform.GetComponentInChildren<SkinnedMeshRenderer>())
            {
                Mesh staticMesh = new Mesh();
                SkinnedMeshRenderer skinnedMeshRenderer = hits[i].transform.GetComponentInChildren<SkinnedMeshRenderer>();
                skinnedMeshRenderer.BakeMesh(staticMesh); //通过蒙皮网格生成静态网格

                GameObject newGo = new GameObject();
                newGo.transform.position = skinnedMeshRenderer.transform.position;
                newGo.transform.eulerAngles = skinnedMeshRenderer.transform.eulerAngles;
                newGo.AddComponent<MeshFilter>().sharedMesh = staticMesh;
                newGo.AddComponent<MeshRenderer>().sharedMaterials = skinnedMeshRenderer.sharedMaterials;

                //敌人在这时候视为死亡
                hits[i].GetComponent<EnemyStatesUI>().EnemyStatesBar.gameObject.SetActive(false); //关闭血条栏
                GetComponent<CharacterStates>().characterData.UpdateExp(hits[i].GetComponent<CharacterStates>().characterData.killExp); //获得经验
                Destroy(hits[i].gameObject); //怪物本体摧毁

                SlicedHull hull = SliceObject(newGo, crossMaterial);
                if (hull != null)
                {
                    GameObject bottom = hull.CreateLowerHull(newGo, crossMaterial); //创建切割的上部
                    GameObject top = hull.CreateUpperHull(newGo, crossMaterial); //创建切割的下部
                    AddHullComponents(bottom);
                    AddHullComponents(top);
                    Destroy(newGo); //将原本的物体删除
                }
            }
            else
            {
                SlicedHull hull = SliceObject(hits[i].gameObject, crossMaterial);
                if (hull != null)
                {
                    GameObject bottom = hull.CreateLowerHull(hits[i].gameObject, crossMaterial); //创建切割的上部
                    GameObject top = hull.CreateUpperHull(hits[i].gameObject, crossMaterial); //创建切割的下部
                    AddHullComponents(bottom);
                    AddHullComponents(top);
                    Destroy(hits[i].gameObject); //将原本的物体删除
                }
            }
        }
    }

    private void AddHullComponents(GameObject go)
    {
        go.layer = 8; //Cuttable
        Rigidbody rb = go.AddComponent<Rigidbody>(); //添加刚体组件

        //如果这行注释掉的话，在切割模式下的切片下坠会感觉起来像掉帧一样
        rb.interpolation = RigidbodyInterpolation.Interpolate; //插值(interpolation)允许你在固定帧率下平滑运行物理效果。

        MeshCollider collider = go.AddComponent<MeshCollider>();
        collider.convex = true; //网格使用凸面碰撞体

        //给当前刚体应用一个模拟爆炸的力
        rb.AddExplosionForce(100, go.transform.position, 20);

        StartCoroutine(DestoryHullComponent(go)); //开启协程，三秒后消失
    }

    IEnumerator DestoryHullComponent(GameObject go)
    {
        float timer = 0;
        while (timer < 3.0f)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        Destroy(go);
    }

    private SlicedHull SliceObject(GameObject obj, Material crossSectionMaterial = null)
    {
        //TODO:为什么要有MeshFilter呢？
        if (obj.GetComponent<MeshFilter>() == null)
            return null;

        return obj.Slice(cutPlane.transform.position, cutPlane.transform.up, crossSectionMaterial);
    }
}
