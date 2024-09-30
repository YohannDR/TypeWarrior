using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class MemoryManager : MonoBehaviour
{
    [Tooltip("Fade out animation prefab")]
    [SerializeField] private GameObject m_FadeOutPrefab;
    private ShaderTime m_FadeOutAnim;

    [SerializeField] private GameObject m_LetterPrefab;

    [Tooltip("How far letters are from each other")]
    [SerializeField] private float m_LetterPadding;

    [Tooltip("Camera of the Scene")]
    [SerializeField] private CamManager m_CamManager;

    [Tooltip("Whether the amount of letters at the start is random")]
    [SerializeField] private bool m_RandomStart;
    
    [FormerlySerializedAs("m_LetterAmount")]
    [Tooltip("Amount of letters at the start")]
    [SerializeField, HideIf("m_RandomStart")] private int m_LetterStartAmount;

    [Tooltip("Range for the random start letters.\nX is lower range, Y is upper range")]
    [SerializeField, ShowIf("m_RandomStart")] private Vector2Int m_RandomStartRange;

    [Tooltip("Delay before the letters become invisible")]
    [SerializeField] private float m_InvisibilityDelay;

    [Tooltip("The amount of new letters added after each round")]
    [SerializeField] private int m_LetterIncrement;

    [Tooltip("Whether to shuffle the letters of the word or keep it as is")]
    [SerializeField] private bool m_ShuffleWord;

    [Tooltip("Words")]
    [SerializeField] private List<StringList> m_Words;
    
    [Header("Damage")]
    
    [Tooltip("Damage the player receives upon missing a letter")]
    [SerializeField] private int m_PlayerDamage;
    
    [Tooltip("Damage the boss receives per letter of a correct word")]
    [SerializeField] private int m_BossDamage;

    [Tooltip("Next Scene name")]
    [SerializeField] private string m_NextSceneName;

    private float m_InvisibilityTimer;

    private readonly List<Letter> m_Letters = new();
    private List<MovingObject> m_Objects = new();
    private int m_CurrentLetter;
    private int m_AmountOfLetters;

    private Player m_Player;
    private Boss m_Boss;

    private string m_CurrentWord;
    private int m_CurrentRow;

    private bool m_Ended;
    
    private void OnValidate()
    {
        if (!m_RandomStart)
            return;
        
        if (m_RandomStartRange.x >= m_RandomStartRange.y)
        {
            Debug.LogError($"[{gameObject.name}] : The lower bound ({m_RandomStartRange.x}) of the random range" +
                $"can't be higher or equal to the upper bound ({m_RandomStartRange.y})");
        }
    }

    private void Start()
    {
        if (m_RandomStart)
            m_LetterStartAmount = Random.Range(m_RandomStartRange.x, m_RandomStartRange.y);

        m_CurrentRow = 0;

        SetupWord();
        
        FetchData();

        LifeBarUI.GetInstance.ChangeLifeBar(LifeBarUI.MiniGame.MinigameFour);
    }

    private void FetchData()
    {
        m_Player = FindObjectOfType<Player>();
        m_Boss = FindObjectOfType<Boss>();
    }

    private void SetupWord()
    {
        m_InvisibilityTimer = m_InvisibilityDelay;
        
        m_CurrentLetter = 0;
        m_AmountOfLetters = m_LetterStartAmount;

        PickWord();

        CreateObjects();
        RandomizeLetters();
    }

    private void DestroyObjects()
    {
        m_Letters.Clear();

        foreach (MovingObject obj in m_Objects)
            Destroy(obj.gameObject);
    }

    private void CreateObjects()
    {
        m_Objects = new(m_CurrentWord.Length);

        float offset = (m_CurrentWord.Length - 1) * m_LetterPadding;
        offset = -(offset / 2f);
        
        for (int i = 0; i < m_CurrentWord.Length; i++)
        {
            Vector3 position = new(offset + i * m_LetterPadding, 0f, 0f);
            Quaternion rotation = Quaternion.identity;

            MovingObject obj = Instantiate(m_LetterPrefab, position, rotation).GetComponent<MovingObject>();

            obj.SetSpeed(0f);
            obj.SetDirection(new(0f, 0f));
            obj.SetLetter(null);

            obj.transform.parent = transform;

            if (i >= m_AmountOfLetters)
            {
                obj.enabled = false;
                obj.gameObject.GetComponentInChildren<SpriteRenderer>().enabled = false;
            }

            m_Objects.Add(obj);
        }
    }

    private void Update()
    {
        if (m_FadeOutAnim)
        {
            if (m_CamManager.Zoom(m_Boss.transform.position))
            {
                m_Boss.BossAnimator.Play("Robot_Death");
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

        if (m_InvisibilityTimer > 0)
        {
            m_InvisibilityTimer -= Time.deltaTime;

            if (m_InvisibilityTimer <= 0)
            {
                TurnLettersInvisible();
            }
        }

        Letter letter = m_Letters[m_CurrentLetter];

        if (m_InvisibilityTimer > 0)
            return;

        // Handle inputs
        letter.CheckPress();

        LetterPressStatus status = letter.WasPressed();
        if (status == LetterPressStatus.NotPressed)
        {
            // Letter wasn't pressed, don't do anything
            return;
        }

        if (status == LetterPressStatus.IncorrectKey)
        {
            // Inputted an incorrect key, restart the minigame
            m_CurrentLetter = 0;

            m_InvisibilityTimer = m_InvisibilityDelay;

            RandomizeLetters();

            m_Player.Damage(m_PlayerDamage);
            m_Boss.BossAnimator.Play("Robot_Attack");
            return;
        }

        // Disable letter
        letter.SetUnpressable();

        m_Objects[m_CurrentLetter].transform.GetChild(0).gameObject.GetComponent<MeshRenderer>().enabled = true;

        // Goto the next letter
        m_CurrentLetter++;

        if (m_CurrentLetter != m_AmountOfLetters)
        {
            // There are still letters to input, so simply continue
            return;
        }

        if (m_AmountOfLetters >= m_CurrentWord.Length)
        {
            m_CurrentRow++;

            BossHealthStatus bossStatus = m_Boss.Damage(m_CurrentWord.Length * m_BossDamage);

            if (bossStatus == BossHealthStatus.Dead || bossStatus == BossHealthStatus.ReachedThreshold)
            {
                m_Ended = true;
            }
            else
            {
                DestroyObjects();
                SetupWord();
            }

            return;
        }

        m_InvisibilityTimer = m_InvisibilityDelay;

        // Go back to the first letter
        m_CurrentLetter = 0;
        // Increase the amount of letters
        m_AmountOfLetters += m_LetterIncrement;

        if (m_AmountOfLetters > m_CurrentWord.Length)
            m_AmountOfLetters = m_CurrentWord.Length;

        // Randomize everything
        RandomizeLetters();
    }

    private void TurnLettersInvisible()
    {
        foreach (MovingObject obj in m_Objects)
            obj.transform.GetChild(0).gameObject.GetComponent<MeshRenderer>().enabled = false;
    }

    private void PickWord()
    {
        m_CurrentWord = m_Words[m_CurrentRow % m_Words.Count].List[Random.Range(0, m_Words[m_CurrentRow].List.Count)];
    }

    private void RandomizeLetters()
    {
        m_Letters.Clear();
        foreach (char t in m_CurrentWord.ToLower())
            m_Letters.Add(new(true, (KeyCode)t));

        if (m_ShuffleWord && m_AmountOfLetters != m_CurrentWord.Length)
            m_Letters.Shuffle();

        for (int i = 0; i < m_AmountOfLetters; i++)
        {
            m_Objects[i].SetLetter(m_Letters[i]);
            m_Objects[i].enabled = true;
            m_Objects[i].transform.GetChild(0).gameObject.GetComponent<MeshRenderer>().enabled = true;
            m_Objects[i].gameObject.GetComponentInChildren<SpriteRenderer>().enabled = true;
        }
    }
}
