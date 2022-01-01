using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AU_Body : MonoBehaviour
{
    [SerializeField] SpriteRenderer m_bodySprite;
    public AU_PlayerController Player { get; set; }

    public bool m_reported;

    public void setColor(Color newColor) 
    {
        m_bodySprite.color = newColor;
    }
}
