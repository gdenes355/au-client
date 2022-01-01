using Assets.Scripts.quiz;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AU_TaskManager : MonoBehaviour
{
    public TextAsset questionsFile;

    public List<QuizQuestion> questions;

    // Start is called before the first frame update
    void Start()
    {
        var questionString = INetworkManager.QuizQuestionString;
        if (questionString == null)
        {
            questionString = questionsFile.ToString();
        }
        var mcs = JsonUtility.FromJson <QuestionSet<MultipleChoice>>(questionString);
        var bfs = JsonUtility.FromJson<QuestionSet<BlankFilling>>(questionString);
        foreach (var q in bfs.questions)
        {
            if (q.type == "bf")
            {
                questions.Add(q);
            }
        }
        foreach (var q in mcs.questions)
        {
            if (q.type == "mc")
            {
                questions.Add(q);
            }
        }
        questions = questions.OrderBy(x => UnityEngine.Random.Range(0.0f, 1.0f)).ToList();

        var taskPoints = gameObject.transform.GetComponentsInChildren<AU_Task>();
        for (int i = 0; i < taskPoints.Count(); i++)
        {
            taskPoints[i].question = questions[i];
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [Serializable]
    public class QuestionSet<T>
    {
        public List<T> questions;
    }
}
