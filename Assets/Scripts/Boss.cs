using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum BossHealthStatus
{
    Alive,
    ReachedThreshold,
    Dead
}

public class Boss : MonoBehaviour
{
    [SerializeField] private int m_Health;
    [SerializeField] private int[] m_Thresholds;

    public Animator BossAnimator;

    private int m_CurrentThreshold;

    private void Start()
    {
        SetupAttack();
        BossAnimator = GetComponent<Animator>();
        BossAnimator.Play("Robot_IdleBase");
    }

    public BossHealthStatus Damage(int damage)
    {
        if (damage >= m_Health)
        {
            m_Health = 0;
        }
        else
        {
            m_Health -= damage;
            LifeBarUI.GetInstance.ChangeFoeLifeBar(m_Health);
            BossAnimator.Play("Robot_TakeDamage");
        }

        if (m_Health == 0)
            return BossHealthStatus.Dead;

        if (m_Health > m_Thresholds[m_CurrentThreshold])
            return BossHealthStatus.Alive;

        m_Health = m_Thresholds[m_CurrentThreshold];
        m_CurrentThreshold++;
        return BossHealthStatus.ReachedThreshold;
    }

    public void SetupAttack()
    {
        switch (SceneManager.GetActiveScene().name)
        {
            case "Wheel":
                BossAnimator.SetInteger("MiniGameIndex", 0);
                break;

            case "Mixing":
                BossAnimator.SetInteger("MiniGameIndex", 1);
                break;

            case "Hopscotch":
                BossAnimator.SetInteger("MiniGameIndex", 2);
                break;

            case "Memory":
                BossAnimator.SetInteger("MiniGameIndex", 3);
                break;
        }
    }
}
