using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; //别忘记了头文件

public class PlayerStatesUI : MonoBehaviour
{
    private Text levelText;
    private Image healthSlider;
    private Image expSlider;

    private void Awake()
    {
        //通过获取子物体的方式获取
        levelText = transform.GetChild(2).GetComponent<Text>();
        healthSlider = transform.GetChild(0).GetChild(0).GetComponent<Image>();
        expSlider = transform.GetChild(1).GetChild(0).GetComponent<Image>();
    }

    private void Update() 
    {
        levelText.text = "LEVEL "+GameManager.Instance.playerStates.characterData.currentLevel.ToString("00"); //字符串添加格式：01
        UpdateHealthSlider();
        UpdateExpSlider();
    }

    //更新血条显示
    private void UpdateHealthSlider()
    {
        float sliderPercent = (float)GameManager.Instance.playerStates.currentHealth / GameManager.Instance.playerStates.maxHealth;
        healthSlider.fillAmount = sliderPercent; 
    }

    //更新经验值条显示
    private void UpdateExpSlider()
    {
        float sliderPercent = (float)GameManager.Instance.playerStates.characterData.currentExp / GameManager.Instance.playerStates.characterData.upgradeExp;
        expSlider.fillAmount = sliderPercent; 
    }
}
