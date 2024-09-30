using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using TMPro;

public enum LetterPressStatus
{
    // Nothing was pressed
    NotPressed,
    // The letter was correctly pressed
    Pressed,
    // The incorrect letter was pressed
    IncorrectKey,
    // The letter was pressed, but it's disabled
    Disabled
}

[System.Serializable, Inspectable]
public class Letter
{
    [SerializeField] private KeyCode m_BoundKey;
    [SerializeField] private bool m_ShouldPress;
    [SerializeField] private bool m_WasPressed;
    [SerializeField] private bool m_IncorrectPress;

    [SerializeField] static public float S_Color = 100;
    [SerializeField] static public float V_Color = 100;

    public Letter()
    {

    }

    public Letter(bool shouldPress)
    {
        Create(shouldPress);
    }

    public Letter(bool shouldPress, KeyCode keyCode)
    {
        Create(keyCode, shouldPress);
    }

    public KeyCode GetKey()
    {
        return m_BoundKey;
    }

    public LetterPressStatus WasPressed()
    {
        if (!m_ShouldPress && m_WasPressed)
            return LetterPressStatus.Disabled;

        if (m_WasPressed)
            return LetterPressStatus.Pressed;

        if (m_IncorrectPress)
            return LetterPressStatus.IncorrectKey;

        return LetterPressStatus.NotPressed;
    }

    public bool CanBePressed()
    {
        return m_ShouldPress;
    }

    public void SetPressable()
    {
        m_ShouldPress = true;
    }

    public void SetUnpressable()
    {
        m_ShouldPress = false;
    }

    public void Create(KeyCode keyCode, bool shouldPress)
    {
        m_BoundKey = keyCode;
        m_ShouldPress = shouldPress;
        m_WasPressed = false;
        m_IncorrectPress = false;
    }

    public void Create(bool shouldPress)
    {
        m_ShouldPress = shouldPress;
        m_WasPressed = false;
        m_IncorrectPress = false;
        RandomizeKey();
    }

    public void CheckPress()
    {
        if (m_WasPressed)
            return;

        if (!Input.anyKeyDown)
            return;

        bool pressed = Input.GetKeyDown(m_BoundKey);

        if (pressed)
            m_WasPressed = true;
        else
            m_IncorrectPress = true;
    }

    public void Reset()
    {
        m_WasPressed = false;
        m_IncorrectPress = false;
    }

    public void RandomizeKey()
    {
        m_BoundKey = (KeyCode)Random.Range((int)KeyCode.A, (int)KeyCode.Z + 1);
    }

    public string GetText()
    {
        return m_BoundKey.ToString().ToUpper();
    }
}
