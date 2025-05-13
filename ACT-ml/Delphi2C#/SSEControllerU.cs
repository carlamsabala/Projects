using System;
using System.Collections.Generic;
using System.Threading;
using MVCFramework;
using MVCFramework.Commons;
using MVCFramework.SSEController;
using StorageU;

[Route("stocks")]
public class MySSEController : MVCSSEController
{
    protected override List<SSEMessage> GetServerSentEvents(string lastEventId)
    {
        Thread.Sleep(500 + new Random().Next(2000));
        int currentEventId;
        SSEMessage sseMessage = new SSEMessage();
        sseMessage.Event = "stockupdate";
        sseMessage.Data = Storage.GetNextDataToSend(ParseIntOrDefault(lastEventId, 0), out currentEventId);
        sseMessage.Id = currentEventId.ToString();
        return new List<SSEMessage> { sseMessage };
    }

    private int ParseIntOrDefault(string s, int defaultValue)
    {
        return int.TryParse(s, out int result) ? result : defaultValue;
    }
}
