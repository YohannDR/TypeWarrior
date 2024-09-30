using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    [SerializeField] private int m_Health;

    private Animator m_Animator;

    private float m_DeathTimer;

    private void Start()
    {
        m_Animator = GetComponent<Animator>();
        PlayIdle();
    }

    public bool Damage(int damage)
    {
        if (damage >= m_Health)
        {
            Kill();
        }
        else
        {
            m_Health -= damage;
            m_Animator.Play("Player_Degat");
            LifeBarUI.GetInstance.ChangePlayerLifeBar(m_Health);
        }
        
        return m_Health == 0;
    }

    public void Kill()
    {
        m_Health = 0;
        m_Animator.Play("Player_Lose");
        LifeBarUI.GetInstance.ChangePlayerLifeBar(m_Health);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        m_DeathTimer = 1f;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();

        if (m_Health == 0)
        {
            m_DeathTimer -= Time.deltaTime;
            
            if (m_DeathTimer < 0f)
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    public void PlayIdle()
    {
        switch (SceneManager.GetActiveScene().name)
        {
            case "Wheel":
                m_Animator.Play("Player_RDL");
                m_Animator.SetInteger("MiniGameIndex", 0);
                break;

            case "Mixing":
                m_Animator.Play("Player_MDL");
                m_Animator.SetInteger("MiniGameIndex", 1);
                break;

            case "Hopscotch":
                m_Animator.Play("Player_Marelle");
                m_Animator.SetInteger("MiniGameIndex", 2);
                break;

            case "Memory":
                m_Animator.Play("Player_Memory");
                m_Animator.SetInteger("MiniGameIndex", 3);
                break;
        }
    }
}
