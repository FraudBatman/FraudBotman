using Discord;
using System;
using System.Collections.Generic;
using System.IO;

namespace DiscordQuiplash.Games.PlayerFibbage
{
    public class PlayerFibbagePrompt
    {
        /*MEMBERS*/
        string question;
        string truth;
        string[] lies;

        /*CONSTRUCTOR*/
        public PlayerFibbagePrompt()
        {
            var sr = new StreamReader(new FileStream("data/playerFibbagePrompts.txt", FileMode.Open));
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
            sr = new StreamReader(new FileStream("data/playerFibbagePrompts.txt", FileMode.Open));

            while (lines != 0)
            {
                sr.ReadLine();
                lines--;
            }

            question = sr.ReadLine();
        }

        /*METHODS*/
        public string GetLiarQuestion(string answererName)
        {
            return question.Replace("your", answererName + "'s");
        }

        public EmbedBuilder PresentPrompt(string answererName)
        {
            var rnd = new Random();
            var embed = new EmbedBuilder();
            embed.Title = this.GetLiarQuestion(answererName);
            embed.Color = new Color(255, 255, 0);
            embed.Description = "";

            int answerCount = 1 + lies.Length;
            var selected = new bool[answerCount];

            foreach (bool dummy in selected)
            {
                while (true)
                {
                    var select = rnd.Next(answerCount);
                    if (selected[select])
                    {
                        continue;
                    }
                    else
                    {
                        if (select == answerCount - 1)
                        {
                            embed.Description += truth + "\n";
                        }
                        else
                        {
                            embed.Description += lies[select] + "\n";
                        }
                        selected[select] = true;
                        break;
                    }
                }
            }

            embed.Description += "Answer via the DMs!";

            return embed;
        }

        /*PROPERTIES*/
        public string AnswererQuestion
        {
            get { return question; }
            set { question = value; }
        }
        public string Truth
        {
            get { return truth; }
            set { truth = value; }
        }
        public string[] Lies
        {
            get { return lies; }
            set { lies = value; }
        }
    }
}