using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//帮助我们在create菜单中创建一个子集菜单
//就是在Assets目录下右键点击出来的菜单的create子选项下创捷一个新的选项Character States -> Data
//Assets -> Create -> Character States -> Data 创建出来的文件默认叫New Data
[CreateAssetMenu(fileName = "New Data", menuName = "Character States/Data")]
//继承ScriptableObject类
public class CharacterData_SO : ScriptableObject
{
    [Header("States Info")]
    public int maxHealth;
    public int currentHealth;
    public int maxDefence;
    public int currentDefence;

    [Header("Kill")]
    public int killExp; //怪物被杀掉后得到的经验值

    [Header("BladeMode")]
    public int maxDefenceBroken; //最大破防值，敌人达到最大破防值会晕眩，同时玩家可以使用刀剑模式进行切割
    public int currentDefenceBroken; //当前破防值
    public int attackBroken; //玩家攻击可造成的破防值数值

    [Header("Level")]
    public int currentLevel;
    public int maxLevel;
    public int upgradeExp; //升级所需的经验值
    public int currentExp; //当前等级的经验值
    public float levelBuff; //升级提升属性的变量
    public float LevelMutiplier //使升级的各种属性成线性变化
    {
        get {return 1+(currentLevel - 1) * levelBuff;}
    }

    // [Header("Transform")]
    // public Vector3 playerPosition;
    // public Quaternion playerRotation;

    public void UpdateExp(int killExp)
    {
        //TODO:多出来的经验分配给下一级
        currentExp += killExp;

        if(currentExp >= upgradeExp)
            LevelUp();
    }

    public void LevelUp()
    {
        //所有你想提升的数据
        currentLevel = Mathf.Clamp(currentLevel+1, 0, maxLevel); //Clamp将数值限定在最小值和最大值之间
        upgradeExp += (int)(upgradeExp * LevelMutiplier);

        maxHealth = (int)(maxHealth * LevelMutiplier);
        currentHealth = maxHealth;

        Debug.Log("LEVEL UP!" + currentLevel + "MaxHealth" + maxHealth);
    }
}
