using System;
using System.Collections.Generic;
using System.Text.Json;

namespace YourNamespace
{
    public static class Storage
    {
        private static readonly string[] Titles = { "IBM", "AAPL", "GOOG", "MSFT" };
        private static readonly Random RandomInstance = new Random();

        public static string GetNextDataToSend(int lastID, out int currentEventID)
        {
            int index = lastID;
            while (index == lastID)
            {
                index = RandomInstance.Next(1, Titles.Length + 1);
            }
            double value = (500 + RandomInstance.Next(0, 200)) + (RandomInstance.Next(0, 50) / 100.0);
            var jsonData = new Dictionary<string, object>
            {
                { "stock", Titles[index - 1] },
                { "value", value }
            };
            currentEventID = lastID + 1;
            return JsonSerializer.Serialize(jsonData);
        }
    }
}
