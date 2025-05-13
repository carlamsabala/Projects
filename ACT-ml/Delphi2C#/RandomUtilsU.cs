using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;

namespace RandomUtilsU
{
    public static class RandomUtils
    {
        private static readonly Random _random = new Random();

        public static readonly string[] FirstNames = new string[]
        {
            "Daniele", "Debora", "Mattia", "Jack", "James", "William", "Joseph", "David", "Charles", "Thomas",
            "Ethan", "Liam", "Noah", "Logan", "Lucas", "Mason", "Benjamin", "Alexander", "Elijah", "Jordan",
            "Alexander", "Jamie", "Tyler", "Caleb", "Kieran", "Ryan", "Colton", "Jaxon", "Gavin", "Ryder"
        };

        public static readonly string[] LastNames = new string[]
        {
            "Smith", "Johnson", "Williams", "Brown", "Black", "Red", "Green", "Willis", "Jones", "Miller",
            "Davis", "Wilson", "Martinez", "Anderson"
        };

        public static readonly string[] Countries = new string[]
        {
            "italy", "new york", "illinois", "arizona", "nevada", "uk", "france", "georgia", "spain", "portugal",
            "germany", "norway", "california", "usa", "japan", "australia", "singapore", "hong kong", "taiwan",
            "south africa", "canada", "switzerland", "sweden", "netherlands", "belgium"
        };

        public const string LOREM_IPSUM =
            "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua." +
            "Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat." +
            "Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur." +
            "Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";

        public static readonly string[] WORDS = new string[]
        {
            "bite", "mate", "quill", "back", "church", "pear", "knit", "bent", "wrench", "crack", "heavenly",
            "deceive", "maddening", "plain", "writer", "rapid", "acidic", "decide", "hat", "paint", "cow",
            "dysfunctional", "pet", "giraffe", "connection", "sour", "voracious", "cloudy", "wry", "curve",
            "agree", "eggnog", "flaky", "painstaking", "warm", "silk", "icy", "hellish", "toy", "milky",
            "skirt", "test", "daffy", "questionable", "gamy", "aware", "berry", "throne", "oven", "subtract",
            "cool", "care", "charge", "smash", "curve", "comfortable", "narrow", "merciful", "material", "fear",
            "exercise", "skinny", "fire", "rainstorm", "tail", "nondescript", "calculating", "pack", "steel",
            "marvelous", "baseball", "furtive", "stitch", "abiding", "empty", "bushes", "painful", "tense",
            "verse", "unwritten", "reproduce", "receptive", "bottle", "silky", "alleged", "stingy", "irritate",
            "expand", "cap", "unsuitable", "gigantic", "exist", "damp", "scrub", "disgusted", "sun", "ink",
            "detailed", "defeated", "economic", "chunky", "stop", "overflow", "numerous", "joyous", "wipe",
            "drink", "error", "branch", "male", "proud", "soggy", "ship", "excite", "industry", "wistful",
            "man", "vacation", "doctor", "naughty", "plane", "ignore", "open", "act", "earthquake", "inconclusive",
            "reflect", "force", "funny", "wonder", "magenta", "near", "dam", "windy", "maid", "wacky", "release",
            "birthday", "statement", "psychotic", "quicksand", "things", "planes", "boundary", "nod", "touch",
            "argue", "sin", "train", "adhoc", "needle", "regret", "stroke", "strengthen", "bruise", "mine",
            "rod", "tax", "twig", "advise", "stamp", "rhyme", "obnoxious", "few", "inform", "fixed"
        };

        public static string GetRndFirstName()
        {
            return FirstNames[_random.Next(FirstNames.Length)];
        }

        public static string GetRndLastName()
        {
            return LastNames[_random.Next(LastNames.Length)];
        }

        public static string GetRndFullName()
        {
            return GetRndFirstName() + " " + GetRndLastName();
        }

        public static string GetRndCountry()
        {
            return Countries[_random.Next(Countries.Length)];
        }

        public static string GetRndEMailAddress()
        {
            string first = GetRndFirstName();
            int len = Math.Min(first.Length, _random.Next(1, 3));
            first = first.Substring(0, len);
            string last = GetRndLastName();
            string domain = GetRndCountry() + _random.Next(1, 4).ToString() + ".com";
            return (first + "." + last + "@" + domain).Replace(" ", "_");
        }

        public static DateTime GetRndDate(ushort initialYear = 1980, ushort yearsSpan = 40)
        {
            int year = initialYear + _random.Next(yearsSpan);
            int dayOfYear = _random.Next(1, 366);
            return new DateTime(year, 1, 1).AddDays(dayOfYear - 1);
        }

        public static int GetRndInteger(int aFrom = 0, int aTo = 1000)
        {
            if (aFrom >= aTo)
            {
                throw new Exception("FROM cannot be greater nor equal to TO");
            }
            return _random.Next(aFrom, aTo);
        }

        public static string GetRndWord()
        {
            return WORDS[_random.Next(WORDS.Length)];
        }

        public static string GetRndPhrase(int aFrom = 0, int aTo = 1000)
        {
            int wordCount = _random.Next(aFrom, aTo);
            var words = new List<string>();
            for (int i = 0; i < wordCount; i++)
            {
                words.Add(GetRndWord());
            }
            string phrase = string.Join(" ", words).Trim();
            if (phrase.Length > 0)
            {
                phrase = char.ToUpper(phrase[0]) + phrase.Substring(1) + ".";
            }
            return phrase;
        }

#if GENERATE_DATASETS
        public static DataTable GetPeople(int count = 20)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("code", typeof(int));
            dt.Columns.Add("first_name", typeof(string));
            dt.Columns.Add("last_name", typeof(string));
            dt.Columns.Add("country", typeof(string));
            dt.Columns.Add("dob", typeof(DateTime));
            for (int i = 1; i <= count; i++)
            {
                dt.Rows.Add(i, GetRndFirstName(), GetRndLastName(), GetRndCountry(), GetRndDate());
            }
            return dt;
        }

        public static DataTable GetUsers(int count = 10)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("first_name", typeof(string));
            dt.Columns.Add("last_name", typeof(string));
            dt.Columns.Add("email", typeof(string));
            for (int i = 1; i <= count; i++)
            {
                dt.Rows.Add(GetRndFirstName(), GetRndLastName(), GetRndEMailAddress());
            }
            return dt;
        }

        public static DataTable GetPosts(int count = 10)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("title", typeof(string));
            dt.Columns.Add("abstract", typeof(string));
            dt.Columns.Add("word_count", typeof(int));
            dt.Columns.Add("comments", typeof(int));
            dt.Columns.Add("post_date", typeof(DateTime));
            for (int i = 1; i <= count; i++)
            {
                dt.Rows.Add(
                    GetRndPhrase(3, 8),
                    GetRndPhrase(30, 50),
                    GetRndInteger(20, 5000),
                    GetRndInteger(0, 20),
                    GetRndDate(2020, 4)
                );
            }
            return dt;
        }
#endif
    }
}
