using System;
using System.IO;

namespace DiscordQuiplash.Games.Quiplash
{
    class Prompt
    {
        /*MEMBERS*/
        string question = "";
        string answerA = "";
        string answerB = "";
        int playerA = -1;
        int playerB = -1;

        /*CONSTRUCTOR*/
        public Prompt()
        {
            var sr = new StreamReader(new FileStream("data/prompts.txt", FileMode.Open));
            var random = new Random();
            int lines = 0;

            //get number of lines
            while (!sr.EndOfStream)
            {
                lines++;
                sr.ReadLine();
            }

            //lines is now the number of lines to skip to find a prompt
            lines = random.Next(0, lines);
            sr = new StreamReader(new FileStream("data/prompts.txt", FileMode.Open));

            while (lines != 0)
            {
                sr.ReadLine();
                lines--;
            }

            question = sr.ReadLine();
        }

        /*METHODS*/
        public override string ToString()
        {
            return
                Question
                + "\n\n"
                + "A) " + AnswerA
                + "\n"
                + "B) " + AnswerB;
        }

        public void answerSwap()
        {
            var dummyAnswer = answerA;
            answerA = answerB;
            answerB = dummyAnswer;

            var dummyPlayer = playerA;
            playerA = playerB;
            playerB = dummyPlayer;
        }

        /*PROPERTIES*/
        public string Question
        {
            get { return question; }
        }

        public string AnswerA
        {
            get { return answerA; }
            set { answerA = value; }
        }

        public string AnswerB
        {
            get { return answerB; }
            set { answerB = value; }
        }

        public int PlayerA
        {
            get { return playerA; }
            set { playerA = value; }
        }

        public int PlayerB
        {
            get { return playerB; }
            set { playerB = value; }
        }
    }
}