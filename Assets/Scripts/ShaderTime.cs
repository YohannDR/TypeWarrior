using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShaderTime : MonoBehaviour
{
    private Animator m_Animator;
    private Renderer m_Renderer;

    private float m_Time;

    [SerializeField] private bool m_KillAfterEnd;

    private static readonly int CustomTime = Shader.PropertyToID("_CustomTime");

    private void Start()
    {
        m_Animator = GetComponent<Animator>();
        m_Renderer = transform.GetChild(0).GetComponent<Renderer>();

        m_Renderer.material.SetFloat(CustomTime, 0f);

        m_Time = 0f;
    }

    private void Update()
    {
        m_Time += Time.deltaTime;
        m_Renderer.material.SetFloat(CustomTime, m_Time);

        if (m_KillAfterEnd && HasAnimationEnded())
            Destroy(gameObject);
    }

    public bool HasAnimationEnded()
    {
        return m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1;
    }
}
