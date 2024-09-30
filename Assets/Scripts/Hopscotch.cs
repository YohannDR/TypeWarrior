using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Hopscotch : MonoBehaviour
{
    [Tooltip("Fade out animation prefab")]
    [SerializeField] private GameObject m_FadeOutPrefab;
    private ShaderTime m_FadeOutAnim;

    [Tooltip("Prefab for the letter")]
    [SerializeField] private GameObject m_LetterPrefab;

    [Tooltip("Camera of the Scene")]
    [SerializeField] private CamManager m_CamManager;

    [Tooltip("Player Base")]
    [SerializeField] private GameObject m_PlayerBase;

    [SerializeField] private Animator m_WallAnimator;

    [SerializeField] private ParticleSystem m_ParticleSystem;

    [Header("Game")]

    [Tooltip("How much time the player is stun when he goes to a bad letter")]
    [SerializeField] private float m_StunTime;
    
    [Tooltip("Minimum Speed")]
    [SerializeField] private float m_MinSpeed;
    [Tooltip("Maximum Speed")]
    [SerializeField] private float m_MaxSpeed;
    [Tooltip("Slowed Speed")]
    [SerializeField] private float m_SlowedSpeed;

    [Tooltip("Time in the mini game you need to reach max speed")]
    [SerializeField] private float m_TimeToGoToMaxSpeed;
    [Tooltip("Time in the mini game where you go from max speed to slowed speed")]
    [SerializeField] private float m_TimeWhenToGoSlowedSpeed;
    [Tooltip("Time to reach slowed speed from max speed")]
    [SerializeField] private float m_TimeToGoToSlowedSpeed;


    [Header("Scene")]
    [Tooltip("Next Scene name")]
    [SerializeField] private string m_NextSceneName;

    private float m_TimerForHalfSpeed;

    private float m_TimeInHopscotch;

    private float m_PlatformSpeed;

    private int m_CurrentPlatformIndex;

    private float m_StunTimer;

    private readonly List<Letter> m_Letters = new();
    private readonly List<MovingObject> m_MovingObjects = new();

    private Transform m_CurrentPlatformTransform;
    private Transform m_LastLetterTransform;

    private bool m_InitMovement = true;

    private Player m_Player;
    private Boss m_Boss;

    // Start is called before the first frame update
    private void Start()
    {
        // Init the first letters
        for (float i = 0; i < 12; i += 1.7f)
        {
            CreateDuoLetter(i);
        }

        FetchData();

        // Set the current platform pos as the player pos at the init
        m_CurrentPlatformTransform = m_Player.transform;

        m_PlatformSpeed = m_MinSpeed;

        m_TimerForHalfSpeed = m_TimeToGoToSlowedSpeed;

        LifeBarUI.GetInstance.ChangeLifeBar(LifeBarUI.MiniGame.MinigameThree);

    }

    // Update is called once per frame
    private void Update()
    {
        // Kill the player if offscreen
        if (m_Player.transform.position.x < -10)
        {
            m_Player.Kill();
            return;
        }

        if (m_FadeOutAnim)
        {
            if (m_CamManager.Zoom(m_Boss.transform.position))
            {
                m_Boss.BossAnimator.Play("Robot_Transition");
                m_WallAnimator.Play("Broken");
                return;
            }
            
            if (m_FadeOutAnim.HasAnimationEnded())
            {
                SceneManager.LoadScene(m_NextSceneName);
                return;
            }
        }

        // Win if you reach the line
        if (m_Player.transform.position.x > 6)
        {
            m_FadeOutAnim = Instantiate(m_FadeOutPrefab).GetComponent<ShaderTime>();
            return;
        }

        // Check if there is more than 1.7 unit between the last platform and the spawn 
        if (12 - m_LastLetterTransform.position.x > 1.7f)
        {
            // If so create a new duo of platform
            CreateDuoLetter();
        }

        // Update player pos 
        m_Player.transform.position = m_CurrentPlatformTransform.position;
       
        // init time 
        if (!m_InitMovement)
            m_TimeInHopscotch += Time.deltaTime;

        // Manage the acceleration of the platform
        ManageSpeed();

        // Update Platform speed
        foreach (MovingObject m in m_MovingObjects)
            m.SetSpeed(m_PlatformSpeed);

        // While stun don't check the input
        if (m_StunTimer > 0)
        {
            m_StunTimer -= Time.deltaTime;
            return;
        }

        // Check the input
        if (!CheckValidateLetter(m_CurrentPlatformIndex + 0))
            CheckValidateLetter(m_CurrentPlatformIndex + 1);
    }

    private void FetchData()
    {
        m_Player = FindObjectOfType<Player>();
        m_Boss = FindObjectOfType<Boss>();
    }

    private bool CheckValidateLetter(int index)
    {
        // check if the letter was pressed
        m_Letters[index].CheckPress();

        LetterPressStatus status = m_Letters[index].WasPressed();

        // Do nothing if it is a good letter
        if (status != LetterPressStatus.Pressed && status != LetterPressStatus.Disabled)
            return false;

        // Stun if it is the a black letter 
        if (status == LetterPressStatus.Disabled)
            m_StunTimer = m_StunTime;

        // change player pos
        m_CurrentPlatformTransform = m_MovingObjects[index].transform;

        m_ParticleSystem.Play();

        // change plaform
        m_CurrentPlatformIndex += 2;

        // check if it is the init
        CheckStartMovement();

        return true;
    }

    private void CheckStartMovement()
    {
        if (!m_InitMovement)
            return;

        // launch all the platforms
        foreach (MovingObject t in m_MovingObjects)
        {
            t.SetDirection(new(-1, 0));
            t.SetSpeed(m_PlatformSpeed);
        }


        m_InitMovement = false;
        m_PlayerBase.SetActive(false);
    }

    private void CreateDuoLetter()
    {
        int goodLetter = Random.Range(0, 2);

        Letter letter1 = new(goodLetter == 0);
        Letter letter2 = new(goodLetter != 0);

        while (letter1.GetKey() == letter2.GetKey())
            letter2.RandomizeKey();

        // Add the two letters
        m_Letters.Add(letter1);
        m_Letters.Add(letter2);

        for (int i = 0; i < 2; i++)
        {
            Letter letter = i == 0 ? m_Letters[m_Letters.Count - 2] : m_Letters[m_Letters.Count - 1];

            MovingObject movingLetter = Instantiate(m_LetterPrefab, i == 0 ? new(12, 1, 0) : new Vector3(12, -1, 0), 
                Quaternion.identity).GetComponent<MovingObject>();

            movingLetter.SetDirection(new(- 1, 0));
            movingLetter.SetSpeed(m_PlatformSpeed);
            movingLetter.SetLetter(letter);

            m_MovingObjects.Add(movingLetter);

            if (i == 1)
                m_LastLetterTransform = movingLetter.transform;
        }
    }

    private void CreateDuoLetter(float baseX)
    {
        int goodLetter = Random.Range(0, 2);

        Letter letter1 = new(goodLetter == 1);
        Letter letter2 = new(goodLetter != 1);

        while (letter1.GetKey() == letter2.GetKey())
            letter2.RandomizeKey();

        m_Letters.Add(letter1);
        m_Letters.Add(letter2);

        for (int i = 0; i < 2; i++)
        {
            Letter letter = i == 0 ? m_Letters[m_Letters.Count - 2] : m_Letters[m_Letters.Count - 1];

            MovingObject movingLetter = Instantiate(m_LetterPrefab, i == 0 ? new(baseX, 1, 0) : new Vector3(baseX, -1, 0), 
                Quaternion.identity).GetComponent<MovingObject>();
            movingLetter.SetDirection(new(0, 0));
            movingLetter.SetSpeed(m_PlatformSpeed);
            movingLetter.SetLetter(letter);

            m_MovingObjects.Add(movingLetter);

            if (i == 1)
                m_LastLetterTransform = movingLetter.transform;
        }
    }

    private void ManageSpeed()
    {
        if (m_TimeInHopscotch <= m_TimeToGoToMaxSpeed)
            m_PlatformSpeed = Mathf.Lerp(m_MinSpeed, m_MaxSpeed, m_TimeInHopscotch / m_TimeToGoToMaxSpeed);
        else if (m_TimeInHopscotch >= m_TimeToGoToMaxSpeed && m_TimeInHopscotch >= m_TimeWhenToGoSlowedSpeed)
        {
            if (m_TimerForHalfSpeed > 0)
                m_TimerForHalfSpeed -= Time.deltaTime;

            m_PlatformSpeed = Mathf.Lerp(m_MaxSpeed, m_SlowedSpeed, (m_TimeToGoToSlowedSpeed - m_TimerForHalfSpeed) / 
                m_TimeToGoToSlowedSpeed);
        }
    }
}
