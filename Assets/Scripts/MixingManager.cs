using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class MixingManager : MonoBehaviour
{
    [Tooltip("Fade out animation prefab")]
    [SerializeField] private GameObject m_FadeOutPrefab;
    private ShaderTime m_FadeOutAnim;

    [SerializeField] private GameObject m_LetterPrefab;

    [Tooltip("Camera of the Scene")]
    [SerializeField] private CamManager m_CamManager;

    [Header("Graphics")]

    [Tooltip("The radius in which letters can spawn")]
    [SerializeField] private float m_LetterSpawnRadius;
    
    [Tooltip("How far letters are from each other (in the guessed word)")]
    [SerializeField] private float m_LetterPadding;

    [Tooltip("Multiplicative padding using the spawn radius to determine how much below the guess word is")]
    [SerializeField] private float m_GuessPadding;

    [Header("Words")]

    [SerializeField] private List<string> m_Words;
    [SerializeField] private List<StringList> m_PossibleGuesses;

    [Tooltip("The amount of \"useless\" letters added")]
    [SerializeField] private int m_NumberOfAdditionalLetters;

    [Tooltip("Delay between each round, in seconds")]
    [SerializeField] private float m_InterRoundDelay;

    [Header("Damage")]
    
    [Tooltip("Damage the player receives upon inputting an incorrect word")]
    [SerializeField] private int m_PlayerDamage;
    
    [Tooltip("Damage the boss receives per letter of a correct word")]
    [SerializeField] private int m_BossDamage;

    [Tooltip("Whether the player receives damage when clicking an incorrect letter")]
    [SerializeField] private bool m_PunishMissClick;
    
    private string m_Guess;

    [Header("Scene")]
    [Tooltip("Next Scene name")]
    [SerializeField] private string m_NextSceneName;

    private readonly List<Letter> m_Letters = new();
    private List<MovingObject> m_Objects = new();
    private List<MovingObject> m_GuessObjects = new();

    private int m_CurrentWord;
    private float m_InterRoundTimer;

    private Player m_Player;
    private Boss m_Boss;

    private bool m_Ended;

    private readonly List<int> m_FetchedWords = new();
    private bool m_FirstWord = true;
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, m_LetterSpawnRadius);
    }

    private void Start()
    {
        m_CurrentWord = Random.Range(0, m_Words.Count);
        m_FetchedWords.Add(m_CurrentWord);
        SetupWord();
        FetchData();

        LifeBarUI.GetInstance.ChangeLifeBar(LifeBarUI.MiniGame.MinigameTwo);

    }

    private void FetchData()
    {
        m_Player = FindObjectOfType<Player>();
        m_Boss = FindObjectOfType<Boss>();
    }

    private void SetupWord()
    {
        m_Words[m_CurrentWord] = m_Words[m_CurrentWord].ToLower();
        CreateLetters();
        m_Guess = string.Empty;
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

        if (m_InterRoundTimer > 0)
        {
            m_InterRoundTimer -= Time.deltaTime;

            if (m_InterRoundTimer < 0)
                SetupWord();

            return;
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            TryValidateGuess();
            return;
        }

        int incorrectCount = 0;
        foreach (Letter l in m_Letters)
        {
            l.CheckPress();

            LetterPressStatus status = l.WasPressed();
            
            if (status == LetterPressStatus.IncorrectKey || status == LetterPressStatus.Disabled)
                incorrectCount++;

            if (status != LetterPressStatus.Pressed)
                continue;

            int index = m_Guess.Length;
            char letter = (char)l.GetKey();

            m_Guess += letter;
            m_GuessObjects[index].SetLetter(l);
            m_GuessObjects[index].gameObject.SetActive(true);

            l.SetUnpressable();
            l.Reset();
            break;
        }

        if (m_PunishMissClick && incorrectCount == m_Letters.Count)
        {
            m_Player.Damage(m_PlayerDamage);
            m_Boss.BossAnimator.Play("Robot_Attack");
        }
    }

    private void TryValidateGuess()
    {
        if (m_Guess.Length <= 2)
            return;

        if (m_PossibleGuesses[m_CurrentWord].Contains(m_Guess))
        {
            BossHealthStatus bossStatus = m_Boss.Damage(m_Guess.Length * m_BossDamage);

            m_FirstWord = false;

            if (bossStatus == BossHealthStatus.Dead || bossStatus == BossHealthStatus.ReachedThreshold)
            {
                m_Ended = true;
                return;
            }

            if (m_FetchedWords.Count == m_Words.Count)
                m_FetchedWords.Clear();

            int failsafe = 0;
            
            int rng = Random.Range(0, m_Words.Count);
            while (m_FetchedWords.Contains(rng))
            {
                rng = Random.Range(0, m_Words.Count);
                failsafe++;

                if (failsafe > 100)
                    break;
            }

            m_CurrentWord = rng;
            m_FetchedWords.Add(rng);
            
            m_InterRoundTimer = m_InterRoundDelay;
            DestroyLetters();
        }
        else
        {
            m_Guess = string.Empty;

            foreach (Letter l in m_Letters)
            {
                l.Reset();
                l.SetPressable();
            }
            
            foreach (MovingObject obj in m_GuessObjects)
                obj.gameObject.SetActive(false);

            m_Player.Damage(m_PlayerDamage);
        }
    }

    private void DestroyLetters()
    {
        m_Letters.Clear();

        foreach (MovingObject obj in m_Objects)
            Destroy(obj.gameObject);

        foreach (MovingObject obj in m_GuessObjects)
            Destroy(obj.gameObject);
    }

    private void CreateLetters()
    {
        foreach (char c in m_Words[m_CurrentWord])
            m_Letters.Add(new(true, (KeyCode)c));

        if (m_CurrentWord != 0)
        {
            for (int i = 0; i < m_NumberOfAdditionalLetters; i++)
                m_Letters.Add(new(true));
        }
    
        m_Objects = new(m_Letters.Count);

        for (int i = 0; i < m_Letters.Count; i++)
        {
            Vector3 position = Vector3.zero;
            if (!m_FirstWord)
            {
                position = GenerateLetterPosition(i);
            }
            else
            {
                float spacing = (m_LetterSpawnRadius * 2) / (m_Words[m_CurrentWord].Length - 1);
                position.x = -m_LetterSpawnRadius + spacing * i;
                position.y = Random.insideUnitCircle.y * m_LetterSpawnRadius;
            }
            Quaternion rotation = Quaternion.identity;

            MovingObject obj = Instantiate(m_LetterPrefab, position, rotation).GetComponent<MovingObject>();

            obj.SetSpeed(0f);
            obj.SetDirection(new(0f, 0f));
            obj.SetLetter(m_Letters[i]);

            obj.transform.parent = transform;

            m_Objects.Add(obj);
        }

        m_GuessObjects = new(m_Letters.Count);
        float offset = (m_Words[m_CurrentWord].Length - 1) * m_LetterPadding;
        offset = -(offset / 2f);
        
        for (int i = 0; i < m_Letters.Count; i++)
        {
            Vector3 position = new(offset + i * m_LetterPadding, -m_LetterSpawnRadius * 1.2f * m_GuessPadding, 0f);
            Quaternion rotation = Quaternion.identity;

            MovingObject obj = Instantiate(m_LetterPrefab, position, rotation).GetComponent<MovingObject>();

            obj.SetSpeed(0f);
            obj.SetDirection(new(0f, 0f));
            obj.SetLetter(m_Letters[i]);

            obj.transform.parent = transform;
            obj.gameObject.SetActive(false);

            m_GuessObjects.Add(obj);
        }
    }

    private Vector3 GenerateLetterPosition(int index)
    {
        int nbrTries = 0;
        Vector2 position;
        while (true)
        {
            position = Random.insideUnitCircle * m_LetterSpawnRadius;
            bool inLetter = false;

            for (int i = 0; i < index; i++)
            {
                Vector2 otherPos = m_Objects[i].transform.position;

                nbrTries++;
                if (otherPos.x - 1 < position.x && otherPos.x + 1 > position.x &&
                    otherPos.y - 1 < position.y && otherPos.y + 1 > position.y)
                {
                    inLetter = true;
                    break;
                }
            }

            if (!inLetter)
                break;

            if (nbrTries >= 500)
                break;
        }

        return new(position.x, position.y, 0);
    }
}
