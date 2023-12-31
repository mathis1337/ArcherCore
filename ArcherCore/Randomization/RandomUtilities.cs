﻿using ArcherCore.Time;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArcherCore.Randomization
{
    public static class RandomUtilities 
    {
        /// <summary>
        /// This method gets a random string based on length of numOfChars
        /// </summary>
        /// <param name="numOfChars">input int</param>
        /// <param name="onlyLetters">input bool</param>
        /// <param name="addTimestamp">input bool</param>
        /// <returns>
        /// Returns a random string
        /// </returns>
        public static string GetRandomString(int numOfChars, bool onlyLetters = false, bool addTimestamp = false)
        {
            var chars = !onlyLetters ? "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789" : "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            var stringChars = new char[numOfChars];
            var random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            var finalString = addTimestamp ? (new string(stringChars)) + TimeUtilities.GetUnixTime().ToString() : new string(stringChars);

            return finalString;
        }

        /// <summary>
        /// This method gets a random number between min and max
        /// </summary>
        /// <param name="min">input int</param>
        /// <param name="max">input int</param>
        /// <returns>
        /// Returns a int between min and max
        /// </returns>
        public static int GetRandomNumber(int? min = 0, int? max = int.MaxValue)
        {
            int randomNumber = 0;
            Random rnd = new Random();
            randomNumber = rnd.Next(min.Value, max.Value);

            return randomNumber;
        }

    }
}
