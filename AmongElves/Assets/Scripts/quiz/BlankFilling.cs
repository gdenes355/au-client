using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.quiz
{
    [Serializable]
    public class BlankFilling : QuizQuestion
    {
        public string Question;
        public string Answer;
        public List<string> Answers;
    }
}
