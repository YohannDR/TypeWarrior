using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

[Serializable]
public class WheelSpeedSettings
{
    [Header("Common settings")]

    [Tooltip("Base rotation speed")]
    public float m_RotationSpeed;
    
    [Tooltip("Whether to use an additive or multiplicative speed increment")]
    public bool m_UseMultiplicativeSpeedIncrement;

    [Header("Additive")]
    [Header("(Use Multiplicative Speed Increment must be false)")]
    [Tooltip("Speed added each time a letter is pressed")]
    public float m_AdditiveSpeedIncrement;

    [Header("Multiplicative")]
    [Header("(Use Multiplicative Speed Increment must be true)")]
    [Tooltip("Whether the base rotation speed or the current rotation speed is used when incrementing." +
             "\nCurrent *= Mult" +
             "\nCurrent += Base * Mult")]
    public bool m_UseBaseSpeedIncrement;
    [Tooltip("Multiplicative increment of the rotation speed after a letter is pressed")]
    public float m_RotationSpeedIncrement;

    [HideInInspector] public float m_BaseRotationSpeed;
}

public class WheelManager : MonoBehaviour
{
    [Tooltip("Fade out animation prefab")]
    [SerializeField] private GameObject m_FadeOutPrefab;
    private ShaderTime m_FadeOutAnim;

    [SerializeField] private GameObject m_LetterPrefab;

    [Tooltip("Camera of the Scene")]
    [SerializeField] private CamManager m_CamManager;

    [Header("Wheel")]
    [Tooltip("How far letters are from the center of the wheel")]
    [SerializeField] private float m_LetterPadding;

    [Tooltip("Base rotation speed for the wheel")]
    [SerializeField] private List<WheelSpeedSettings> m_SpeedSettings;

    [Header("Damage")]
    
    [Tooltip("Damage the player receives upon selecting an incorrect letter")]
    [SerializeField] private int m_PlayerDamage;
    
    [Tooltip("Damage the boss receives upon selecting a correct letter")]
    [SerializeField] private int m_BossDamage;

    [Tooltip("Whether the player receives damage when clicking an incorrect letter")]
    [SerializeField] private bool m_PunishMissClick;
    
    [Tooltip("Whether damage is inflicted per word or per letter")]
    [SerializeField] private bool m_IndiviudalLetterDamage;

    [Header("Words")]
    [SerializeField] private List<string> m_Words;

    [Header("Scene")]
    [Tooltip("Next Scene name")]
    [SerializeField] private string m_NextSceneName;

    private List<Letter> m_Letters = new();

    private List<MovingObject> m_Objects = new();

    private int m_CurrentWord;

    private float m_BaseRotationSpeed;

    private bool m_CounterClockwise;
    private float m_SliceAngle;

    private Player m_Player;

    private Boss m_Boss;

    private bool m_Ended;

    private int m_NumberOfPressedLetters;

    private void Awake()
    {
        LifeBarUI.GetInstance.ChangeLifeBar(LifeBarUI.MiniGame.MiniGameOne);
    }

    private void Start()
    {
        Vector3 position = transform.position;
        transform.position = Vector3.zero;

        m_CurrentWord = 0;

        SetupWord();
        
        transform.position = position;

        FetchData();

        m_Ended = false;
        m_NumberOfPressedLetters = 0;

        foreach (WheelSpeedSettings s in m_SpeedSettings)
            s.m_BaseRotationSpeed = s.m_RotationSpeed;

    }

    private void SetupWord()
    {
        DestroyObjects();
        CreateLetters();
        
        // Compute the angular size of each letter
        m_SliceAngle = 360f / m_Letters.Count;

        CreateObjects();
    }

    private void FetchData()
    {
        m_Player = FindObjectOfType<Player>();
        m_Boss = FindObjectOfType<Boss>();
    }

    private void CreateLetters()
    {
        m_Letters = new(m_Words[m_CurrentWord].Length);

        foreach (char c in m_Words[m_CurrentWord])
            m_Letters.Add(new(true, (KeyCode)c));
    }
    
    private void CreateObjects()
    {
        m_Objects = new(m_Letters.Count);

        for (int i = 0; i < m_Letters.Count; i++)
        {
            Vector3 position = new(0f, m_LetterPadding, 0f);
            Quaternion rotation = Quaternion.Euler(0f, 0f, i * m_SliceAngle);
            position = rotation * position;

            MovingObject obj = Instantiate(m_LetterPrefab, position, rotation).GetComponent<MovingObject>();

            obj.SetSpeed(0f);
            obj.SetDirection(new(0f, 0f));
            obj.SetLetter(m_Letters[i]);

            obj.transform.parent = transform;

            m_Objects.Add(obj);
        }
    }

    private void DestroyObjects()
    {
        m_Letters.Clear();

        foreach (MovingObject obj in m_Objects)
            Destroy(obj.gameObject);
    }

    private void Update()
    {
        if (m_FadeOutAnim)
        {
            if (m_CamManager.Zoom(m_Boss.transform.position))
            {
                m_Boss.BossAnimator.Play("Robot_Transition");
                return;
            }
            
            if (m_FadeOutAnim.HasAnimationEnded())
            {
                SceneManager.LoadScene(m_NextSceneName);
                return;
            }
        }
        
        if (m_Ended)
        {
            m_FadeOutAnim = Instantiate(m_FadeOutPrefab).GetComponent<ShaderTime>();
            return;
        }
        
        Rotate();
        CheckForLetter();
    }

    private void Rotate()
    {
        Vector3 angles = transform.eulerAngles;
        float rotationSpeed = m_SpeedSettings[m_CurrentWord].m_RotationSpeed * Time.deltaTime;

        // Apply rotation in the correct direction
        if (m_CounterClockwise)
            angles.z += rotationSpeed;
        else
            angles.z -= rotationSpeed;

        transform.localRotation = Quaternion.Euler(angles);
    }

    private void CheckForLetter()
    {
        float rotation = transform.eulerAngles.z - m_SliceAngle / 2f;

        // Flip rotation
        if (rotation > 0)
            rotation = 360f - rotation;
        else
            rotation = -rotation;

        // Compute on which slice the rotation currently is
        int index = (int)(rotation / m_SliceAngle);
        // Get the concerned letter
        Letter letter = m_Letters[index];

        if (!letter.CanBePressed())
        {
            // Letter can't be pressed, abort
            return;
        }

        // Handle inputs
        letter.CheckPress();

        LetterPressStatus status = letter.WasPressed();

        if (status == LetterPressStatus.Disabled || (m_PunishMissClick && status == LetterPressStatus.IncorrectKey))
        {
            m_Player.Damage(m_PlayerDamage);
            m_Boss.BossAnimator.Play("Robot_Attack");
            letter.Reset();
            return;
        }

        if (status != LetterPressStatus.Pressed)
        {
            // Letter wasn't pressed, don't do anything
            return;
        }

        m_NumberOfPressedLetters++;
        
        // Flip rotation direction
        m_CounterClockwise ^= true;

        // Black out letter
        letter.SetUnpressable();

        if (m_IndiviudalLetterDamage)
        {
            BossHealthStatus bossStatus = m_Boss.Damage(m_BossDamage);

            if (bossStatus == BossHealthStatus.ReachedThreshold || bossStatus == BossHealthStatus.Dead)
            {
                m_Ended = true;
                return;
            }
        }

        IncrementSpeed();

        if (m_NumberOfPressedLetters != m_Words[m_CurrentWord].Length)
            return;

        m_CurrentWord = (m_CurrentWord + 1) % m_Words.Count;
        m_NumberOfPressedLetters = 0;

        m_SpeedSettings[m_CurrentWord].m_RotationSpeed = m_SpeedSettings[m_CurrentWord].m_BaseRotationSpeed;
        m_CounterClockwise = false;

        transform.eulerAngles = Vector3.zero;

        SetupWord();

        if (!m_IndiviudalLetterDamage)
        {
            BossHealthStatus bossStatus = m_Boss.Damage(m_BossDamage);

            if (bossStatus == BossHealthStatus.ReachedThreshold || bossStatus == BossHealthStatus.Dead)
                m_Ended = true;
        }
    }

    private void IncrementSpeed()
    {
        if (!m_SpeedSettings[m_CurrentWord].m_UseMultiplicativeSpeedIncrement)
        {
            // Using additive increment, simply add
            m_SpeedSettings[m_CurrentWord].m_RotationSpeed += m_SpeedSettings[m_CurrentWord].m_AdditiveSpeedIncrement;
            return;
        }

        // Using multiplicative increment
        if (m_SpeedSettings[m_CurrentWord].m_UseBaseSpeedIncrement)
        {
            // Add a fraction of the base speed, creating a linear scaling
            m_SpeedSettings[m_CurrentWord].m_RotationSpeed += m_SpeedSettings[m_CurrentWord].m_BaseRotationSpeed * m_SpeedSettings[m_CurrentWord].m_RotationSpeedIncrement;
        }
        else
        {
            // Add a fraction of the current speed, creating an exponential scaling
            m_SpeedSettings[m_CurrentWord].m_RotationSpeed *= m_SpeedSettings[m_CurrentWord].m_RotationSpeedIncrement;
        }
    }
}
