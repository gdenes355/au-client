using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PausePlayer : MonoBehaviour
{

    public Color color;
    public bool isDead;
    public bool isImpostor;
    public bool hasCalledMeeting;
    public string playerName;
    public bool isEnabled;
    public int otherPlayerId;
    public List<Color> voters;


    private Button voteButton;
    RawImage iconImpostor;
    Image iconTree;
    Image iconMegaPhone;
    Text nameText;
    Image iconCol;

    private AU_PauseScene pauseScene;
    private List<Image> voteIcons;

    // Start is called before the first frame update
    void Start()
    {
        voteButton = gameObject.transform.Find("VoteButton").GetComponent<Button>();
        iconImpostor = voteButton.transform.Find("IconImpostor").GetComponent<RawImage>();
        iconTree = voteButton.transform.Find("IconTree").GetComponent<Image>();
        iconMegaPhone = voteButton.transform.Find("IconMegaphone").GetComponent<Image>();
        nameText = voteButton.transform.Find("Name").GetComponent<Text>();
        iconCol = voteButton.transform.Find("IconCol").GetComponent<Image>();
        pauseScene = FindObjectsOfType<AU_PauseScene>()[0];
        voteIcons = new List<Image>(voteButton.transform.Find("votes").gameObject.GetComponentsInChildren<Image>());
    }

    void Update()
    {
        iconImpostor.enabled = isImpostor;
        iconTree.enabled = isDead;
        iconMegaPhone.enabled = hasCalledMeeting;
        nameText.text = playerName;
        iconCol.color = color;
        voteButton.interactable = isEnabled;
    }

    private void FixedUpdate()
    {
        for (int i = 0; i < voteIcons.Count; i++)
        {
            if (i < voters.Count)
            {
                voteIcons[i].enabled = true;
                voteIcons[i].color = voters[i];
            }
            else
            {
                voteIcons[i].enabled = false;
            }
        }
    }

    public void castVote()
    {
        if (isEnabled)
        {
            pauseScene.castVote(otherPlayerId);
        }
    }
}
