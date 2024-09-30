using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

public class MovingObject : MonoBehaviour
{
    private Letter m_MovingLetter;

    [SerializeField] private Vector2 m_Direction;
    [SerializeField] private float m_Speed;
    [SerializeField] private TMP_Text m_Text;


    private SpriteRenderer m_SpriteRenderer;

    private float m_ColorTime = 3f;
    private float m_ColorTimer;

    private void Awake()
    {
        m_Text = GetComponentInChildren<TMP_Text>();
    }
    
    private void Update()
    {
        Vector3 tempDir = new(m_Direction.x, m_Direction.y, 0f);

        if (m_MovingLetter == null)
            return;

        transform.position += tempDir.normalized * (m_Speed * Time.deltaTime);

        m_ColorTimer += Time.deltaTime;

        m_Text.text = m_MovingLetter.GetText();
        m_Text.color = m_MovingLetter.CanBePressed() ? Color.Lerp(Color.Lerp(Color.blue, Color.red, Mathf.PingPong(Time.time, 1)), Color.magenta, Mathf.PingPong(Time.time, 1)) : Color.black;

        if (m_ColorTimer >= m_ColorTime)
            m_ColorTimer = 0;

        // Destroy the object if off-screen
        if (transform.position.x < -13)
            Destroy(gameObject);
    }

    public void SetDirection(Vector2 direction)
    {
        m_Direction = direction; 
    }

    public void SetSpeed(float speed)
    {
        m_Speed = speed;
    }

    public void SetLetter(Letter letter)
    {
        m_MovingLetter = letter;
    }
}
