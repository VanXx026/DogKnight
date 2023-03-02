using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStates : MonoBehaviour
{
    public event Action<int, int> UpdateHealthBarOnAttack; //订阅的方法必须含有两个int类型的参数，在这里是maxHealth和currentHealth
    public event Action<int, int> UpdateDefenceBrokenBarOnAttack;
    //通过把通用的SlimeData作为模板data，复制一份给characterData，那么许多个Slime之间只需维护一份模板数据，就可以实现每个Slime都有一份数据，而不是通用
    public CharacterData_SO templateData;
    public CharacterData_SO characterData;
    public AttackData_SO attackData;

    //想public调用却又不想在Inspector中出现
    [HideInInspector]
    public bool isCritical; //是否暴击了

    void Awake()
    {
        if(templateData != null)
            characterData = Instantiate(templateData); //复制一份模板数据给characterData
    }

    #region Read from Data_SO
    //c#property属性 public 类型 变量名 {get;set}
    public int maxHealth{
        get{
            if(characterData != null)
                return characterData.maxHealth;
            else 
                return 0;
        }
        set{
            characterData.maxHealth = value; //value就是默认传进setter的参数
        }
    }
    public int currentHealth{
        get{
            if(characterData != null)
                return characterData.currentHealth;
            else 
                return 0;
        }
        set{
            characterData.currentHealth = value;
        }
    }
    public int maxDefence{
        get{
            if(characterData != null)
                return characterData.maxDefence;
            else 
                return 0;
        }
        set{
            characterData.maxDefence = value;
        }
    }
    public int currentDefence{
        get{
            if(characterData != null)
                return characterData.currentDefence;
            else 
                return 0;
        }
        set{
            characterData.currentDefence = value;
        }
    }
    #endregion

    #region Character Combat

    //收到伤害判定
    public void TakeDamage(CharacterStates attacker, CharacterStates defender)
    {
        //没破防（伤害比防御力还低）就设定伤害为0
        int damage = Mathf.Max(attacker.CurrentDamage() - defender.currentDefence, 0);
        currentHealth = Mathf.Max(currentHealth - damage, 0);

        defender.GetComponent<Animator>().SetTrigger("Hit");

        // defender.GetComponent<Rigidbody>().AddForce(attacker.transform.up * 100, ForceMode.Impulse);

        //TODO:Update UI
        UpdateHealthBarOnAttack?.Invoke(currentHealth, maxHealth); //执行订阅者的方法
        
        //TODO:Update exp
        if(defender.currentHealth <= 0)
            attacker.characterData.UpdateExp(defender.characterData.killExp);
    }

    //收到破防值判定
    public void TakeDefenceBroken(CharacterStates attacker, CharacterStates defender)
    {
        defender.characterData.currentDefenceBroken = Mathf.Min(defender.characterData.currentDefenceBroken + attacker.characterData.attackBroken, defender.characterData.maxDefenceBroken);

        UpdateDefenceBrokenBarOnAttack?.Invoke(characterData.currentDefenceBroken, characterData.maxDefenceBroken); //执行订阅者的方法

        if(defender.characterData.currentDefenceBroken >= defender.characterData.maxDefenceBroken)
        {
            defender.GetComponent<Animator>().SetBool("Dizzy", true);
            defender.gameObject.layer = 8; //Cuttable
        }    
    }

    //override:飞行道具用
    public void TakeDamage(int damage, CharacterStates defender)
    {
        int currentDamage = Mathf.Max(damage - defender.currentDefence, 0);
        currentHealth = Mathf.Max(currentHealth - damage, 0);

        UpdateHealthBarOnAttack?.Invoke(currentHealth, maxHealth); //执行订阅者的方法

        //Update exp
        if(currentHealth <= 0)
            GameManager.Instance.playerStates.characterData.UpdateExp(defender.characterData.killExp);
    }

    public int CurrentDamage()
    {
        //FIXME:这里因为Range方法是左闭右开的，取不到最大值
        float coreDamage = UnityEngine.Random.Range(attackData.minDamage, attackData.maxDamage);

        if(isCritical)
        {
            coreDamage *= attackData.criticalMultiplier;
        }

        return (int)coreDamage;
    }
    
    #endregion
}
