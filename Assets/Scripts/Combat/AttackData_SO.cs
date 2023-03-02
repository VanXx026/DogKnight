using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Attack", menuName = "Character States/Attack")]
public class AttackData_SO : ScriptableObject
{
    //近战攻击距离
    public float attackRange;
    //远程攻击距离
    public float skillRange;
    //CD
    public float coolDown;
    public int minDamage;
    public int maxDamage;
    //暴击增伤百分比
    public float criticalMultiplier;
    //暴击率
    public float criticalChance;
}
