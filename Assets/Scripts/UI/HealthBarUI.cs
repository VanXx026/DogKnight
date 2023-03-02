using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    public GameObject healthUIPrefab;
    public Transform healthBarPoint; //敌人血条的位置
    public bool alwaysVisible; //是否角色加载后即可见或只有被攻击后才可见
    public float visibleTime; //攻击后才可见的可见时间
    private float timeLeft; //可见时间剩余时间
    Image healthSlider; //血量滑动条
    public Transform UIbar; //血条
    Transform cam;
    CharacterStates characterStates;

    private void Awake()
    {
        characterStates = GetComponent<CharacterStates>();

        characterStates.UpdateHealthBarOnAttack += UpdateHealthBar;
    }

    //当物体被加载启动时
    private void OnEnable()
    {
        //获取主相机
        cam = Camera.main.transform;

        foreach (Canvas canvas in FindObjectsOfType<Canvas>())
        {
            //如果canvas的渲染模式是世界坐标下
            if (canvas.renderMode == RenderMode.WorldSpace)
            {
                //你创建的血条也是属于canvas，所以这里的canvas.transform就是指你这个血条应该生成的位置
                UIbar = Instantiate(healthUIPrefab, canvas.transform).transform;
                healthSlider = UIbar.GetChild(0).GetComponent<Image>();
                UIbar.gameObject.SetActive(alwaysVisible);
            }
        }
    }

    private void UpdateHealthBar(int currentHealth, int maxHealth)
    {
        if (currentHealth <= 0)
        {
            Destroy(UIbar.gameObject);
        }
        //FIXME:好像有时会存在一个报错，说我东西没了还在尝试调用
        UIbar.gameObject.SetActive(true);
        timeLeft = visibleTime; //重置剩余可见时间

        float sliderPercent = (float)currentHealth / maxHealth;
        healthSlider.fillAmount = sliderPercent;
    }

    //迟一帧更新
    private void LateUpdate()
    {
        if(UIbar != null)
        {
            UIbar.position = healthBarPoint.position;
            UIbar.forward = -cam.forward; //血条的方向始终对着主摄像机

            //如果当前的可见剩余时间<=0 且 使用的是攻击显示血条模式
            if(timeLeft <= 0 && !alwaysVisible)
            {  
                UIbar.gameObject.SetActive(false);
            }
            else{
                timeLeft -= Time.deltaTime;
            }
        }
    }
}
