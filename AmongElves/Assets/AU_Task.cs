using Assets.Scripts.quiz;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AU_Task : MonoBehaviour
{
    public bool active = false;
    public bool solved = false;
    public QuizQuestion question;
    private SpriteRenderer m_spriteRenderer;

    private AU_Quiz quizmanager;

    // Start is called before the first frame update
    void Start()
    {
        m_spriteRenderer = gameObject.transform.GetComponentInChildren<SpriteRenderer>();
        quizmanager = GameObject.Find("GameOverlay").transform.GetComponentInChildren<AU_Quiz>(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Open()
    {
        quizmanager.setQuestion(question);
    }

    public void Done()
    {
        m_spriteRenderer.color = Color.green;
        solved = true;
    }

    public void setActive(bool active)
    {
        if (!solved)
        {
            if (active)
            {
                m_spriteRenderer.color = Color.yellow;
            }
            else
            {
                m_spriteRenderer.color = Color.white;
            }
        }
    }
}
