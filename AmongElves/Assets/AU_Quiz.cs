using Assets.Scripts.quiz;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AU_Quiz : MonoBehaviour
{
    float timer = 0.0f;
    public Text QuestionLabel;
    public Text ALabel;
    public Text BLabel;
    public Text CLabel;
    public Text DLabel;
    public InputField BlankFillingText;
    public Text TimerLabel;

    public GameObject MCRoot;
    public GameObject BlankRoot;

    private QuizQuestion quizQuestion;

    // Start is called before the first frame update
    void Start()
    {
        /*MultipleChoice q = new MultipleChoice();
        q.Question = "Can you pick As?";
        q.Options = new List<string> { "As", "Bs", "Cs", "Ds"};
        q.CorrectOption = 0;*/
        /*
        BlankFilling q = new BlankFilling();
        q.Question = "What is 128 in binary?";
        q.Answer = "10000000";
        */
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void FixedUpdate()
    {
        timer -= Time.fixedDeltaTime;
        timer = Mathf.Max(0.0f, timer);
        if (timer == 0.0f)
        {
            TimerLabel.text = "";
        }
        else
        {
            TimerLabel.text = "Wait " + timer.ToString("0.00") + "s";
        }
    }

    public void setQuestion(QuizQuestion quizQuestion)
    {
        this.quizQuestion = quizQuestion;

        if (quizQuestion is MultipleChoice)
        {
            var mc = (MultipleChoice)quizQuestion;
            QuestionLabel.text = mc.Question;
            ALabel.text = mc.Options[0];
            BLabel.text = mc.Options[1];
            CLabel.text = mc.Options[2];
            DLabel.text = mc.Options[3];
            BlankRoot.SetActive(false);
            MCRoot.SetActive(true);
            gameObject.SetActive(true);
        }
        else if (quizQuestion is BlankFilling)
        {
            var mc = (BlankFilling)quizQuestion;
            QuestionLabel.text = mc.Question;
            BlankFillingText.text = "";
            BlankRoot.SetActive(true);
            MCRoot.SetActive(false);
            gameObject.SetActive(true);
            BlankFillingText.text = "";
        }
        timer = 2;
    }

    public void answerMC(int index)
    {
        if (timer == 0)
        {
            var mc = (MultipleChoice)quizQuestion;
            if (index == mc.CorrectOption)
            {
                correctAnswer();
            }
            else
            {
                timer = 10.0f;
            }
        }
    }

    public void answerBlankFilling()
    {
        if (timer == 0)
        {
            string actual = BlankFillingText.text.ToLower().Trim();
            bool correct = false;
            foreach (string answer in ((BlankFilling)quizQuestion).Answers)
            {
                string expected = answer.ToLower().Trim();
                if (expected == actual)
                {
                    correct = true;
                    break;
                }
            }
            
            if (correct)
            {
                correctAnswer();
            }
            else
            {
                timer = 2.0f;
            }
        }
    }

    private void correctAnswer()
    {
        GameController.Instance.LocalPlayer.solvePuzzle();        
        gameObject.SetActive(false);
    }
}
