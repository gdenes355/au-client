using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.quiz
{
    [Serializable]
    public class MultipleChoice : QuizQuestion
    {
        public string Question;
        public List<string> Options;
        public int CorrectOption;
    }
}
