using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuInput : MonoBehaviour
{
    [SerializeField] private GameObject m_LetterPrefab;

    [Tooltip("Word to input")]
    [SerializeField] private string m_Word;

    [Tooltip("Where the word will spawn (extends to the right")]
    [SerializeField] private Transform m_WordStart;

    [Tooltip("Padding between letters")]
    [SerializeField] private float m_LetterPadding;
    
    [Tooltip("Scale of the letters")]
    [SerializeField] private float m_LetterScale;

    [Header("Scene")]
    [Tooltip("Next Scene name")]
    [SerializeField] private string m_NextSceneName;
    
    [Tooltip("Fade out animation prefab")]
    [SerializeField] private GameObject m_FadeOutPrefab;
    
    private List<Letter> m_Letters;
    private List<MovingObject> m_Objects;

    private ShaderTime m_FadeOutAnim;

    private int m_CurrentLetter;

    private bool m_Ended;

    private void Start()
    {
        m_Word = m_Word.ToLower();
        m_CurrentLetter = 0;
        
        m_Letters = new(m_Word.Length);
        m_Objects = new(m_Word.Length);

        for (int i = 0; i < m_Word.Length; i++)
        {
            Letter l = new(true, (KeyCode)m_Word[i]);

            Vector3 position = m_WordStart.position;
            position.x += m_LetterPadding * i;

            MovingObject obj = Instantiate(m_LetterPrefab, position, quaternion.identity).GetComponent<MovingObject>();

            obj.SetSpeed(0f);
            obj.SetDirection(new(0f, 0f));
            obj.transform.localScale = new(m_LetterScale, m_LetterScale, m_LetterScale);
            obj.SetLetter(l);

            obj.transform.parent = transform;

            m_Letters.Add(l);
            m_Objects.Add(obj);
        }
    }
    
    private void Update()
    {
        if (m_FadeOutAnim)
        {
            if (m_FadeOutAnim.HasAnimationEnded())
                SceneManager.LoadScene(m_NextSceneName);

            return;
        }
        
        if (m_Ended)
        {
            m_FadeOutAnim = Instantiate(m_FadeOutPrefab).GetComponent<ShaderTime>();
            return;
        }
        
        Letter l = m_Letters[m_CurrentLetter];
        l.CheckPress();

        if (l.WasPressed() != LetterPressStatus.Pressed)
            return;

        m_CurrentLetter++;
        l.SetUnpressable();

        if (m_CurrentLetter == m_Word.Length)
            m_Ended = true;
    }
}
