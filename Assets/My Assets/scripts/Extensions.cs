using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.My_Assets.scripts
{
    public static class Extensions
    {
        public static void SetLambdaMessage(this Dictionary<string, Dictionary<string, double>> lambdaMessage, string childName, string parentVal, double val)
        {
            if (lambdaMessage.ContainsKey(childName))
            {
                if (lambdaMessage[childName].ContainsKey(parentVal))
                {
                    lambdaMessage[childName][parentVal] = val;
                }
                else
                {
                    lambdaMessage[childName].Add(parentVal, val);
                }
            }
            else
            {
                Dictionary<string, double> dir = new Dictionary<string, double>
                {
                    {parentVal, val}
                };

                lambdaMessage.Add(childName, dir);
            }
        }
        public static void SetPiMessages(this Dictionary<string, Dictionary<string, double>> piMessages, string parentName, string childVal, double val)
        {
            if (piMessages.ContainsKey(parentName))
            {
                if (piMessages[parentName].ContainsKey(childVal))
                {
                    piMessages[parentName][childVal] = val;
                }
                else
                {
                    piMessages[parentName].Add(childVal, val);
                }
            }
            else
            {
                Dictionary<string, double> dir = new Dictionary<string, double>
                {
                    {childVal, val}
                };

                piMessages.Add(parentName, dir);
            }
        }
    } 
}
