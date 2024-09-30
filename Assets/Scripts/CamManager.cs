using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamManager : MonoBehaviour
{

    private Camera m_Cam;

    private float m_BaseSize;

    private const float TransitionTime = 1.5f;
    private float m_TransitionTimer;

    private bool m_IsZooming;

    // Start is called before the first frame update
    private void Start()
    {
        m_Cam = GetComponent<Camera>();
        m_BaseSize = m_Cam.orthographicSize;
        m_TransitionTimer = TransitionTime;
    }

    // Update is called once per frame
    private void Update()
    {
        if (m_IsZooming)
            m_TransitionTimer -= Time.deltaTime;

    }

    public bool Zoom(Vector3 center)
    {
        m_IsZooming = true;

        if (m_TransitionTimer > 0)
        {
            m_Cam.orthographicSize = 2;
            m_Cam.transform.position = new(center.x, center.y, m_Cam.transform.position.z);
            return true;
        }

        m_TransitionTimer = TransitionTime;
        m_IsZooming = false;
        m_Cam.orthographicSize = m_BaseSize;
        m_Cam.transform.position = new(0, 0, m_Cam.transform.position.z);

        return false;
    }
}
