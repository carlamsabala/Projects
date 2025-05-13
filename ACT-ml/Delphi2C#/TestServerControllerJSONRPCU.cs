using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace TestServer.JSONRPC
{
    public enum TEnumTest
    {
        ptEnumValue0,
        ptEnumValue1,
        ptEnumValue2,
        ptEnumValue3
    }

    public class TSetOfEnumTest : HashSet<TEnumTest>
    {
    }

    public class TSimpleRecord
    {
        public int IntegerProperty { get; set; }
    }

    public class TComplexRecord
    {
        public TComplexRecord() { }
    }

    public class TCustomerIssue648
    {
        public int Id { get; set; }
        public DateTime Added { get; set; }
        public string Name { get; set; }
        public DateTime ExpirationDate { get; set; }
        public DateTime? MaxUpdateDate { get; set; }
        public string AppVersion { get; set; }
        public string Activated { get; set; }
    }

    public class TPerson
    {
        public int ID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DOB { get; set; }
        public bool Married { get; set; }
    }

    public enum JSONRPCRequestType
    {
        Request,
        Notification
    }

    [Route("api/jsonrpc")]
    [ApiController]
    public class TestJsonRpcController : ControllerBase
    {
        [HttpPost("subtract")]
        [MVCInheritable]
        public ActionResult<int> Subtract(long value1, long value2)
        {
            return (int)(value1 - value2);
        }

        [HttpPost("mynotify")]
        [MVCInheritable]
        public IActionResult MyNotify()
        {
            var _ = GetType().Name;
            return Ok();
        }

        [HttpPost("myrequest")]
        [MVCInheritable]
        public ActionResult<bool> MyRequest()
        {
            return true;
        }

        [HttpPost("add")]
        [MVCInheritable]
        public ActionResult<JObject> Add(long value1, long value2, long value3)
        {
            var obj = new JObject();
            obj["res"] = value1 + value2 + value3;
            return obj;
        }

        [HttpPost("getlistfromto")]
        [MVCInheritable]
        public ActionResult<JArray> GetListFromTo(long aFrom, long aTo)
        {
            var arr = new JArray();
            for (long i = aFrom; i <= aTo; i++)
                arr.Add(i);
            return arr;
        }

        [HttpPost("multiplystring")]
        [MVCInheritable]
        public ActionResult<string> MultiplyString(string aString, long multiplier)
        {
            var result = aString;
            for (int i = 2; i <= multiplier; i++)
                result += aString;
            return result;
        }
    }

    [Route("api/jsonrpcwithget")]
    [ApiController]
    [MVCJSONRPCAllowGET]
    public class TestJsonRpcControllerWithGet : TestJsonRpcController
    {
    }

    public class TestJsonRpcClass
    {
        [MVCInheritable]
        public int Subtract(long value1, long value2)
        {
            return (int)(value1 - value2);
        }

        [MVCInheritable]
        public void MyNotify()
        {
            var _ = GetType().Name;
        }

        [MVCInheritable]
        public JObject Add(long value1, long value2, long value3)
        {
            var obj = new JObject();
            obj["res"] = value1 + value2 + value3;
            return obj;
        }

        [MVCInheritable]
        public JArray GetListFromTo(long aFrom, long aTo)
        {
            var arr = new JArray();
            for (long i = aFrom; i <= aTo; i++)
                arr.Add(i);
            return arr;
        }

        [MVCInheritable]
        public string MultiplyString(string aString, long multiplier)
        {
            var result = aString;
            for (int i = 2; i <= multiplier; i++)
                result += aString;
            return result;
        }

        [MVCInheritable]
        public DateTime AddTimeToDateTime(DateTime aDateTime, TimeSpan aTime)
        {
            return aDateTime + aTime;
        }

        [MVCInheritable]
        public TPerson DoError(TPerson myObj)
        {
            throw new Exception("BOOOM!! (TestJsonRpcClass.DoError)");
        }

        [MVCInheritable]
        public TPerson HandlingObjects(TPerson myObj)
        {
            return new TPerson
            {
                ID = myObj.ID,
                FirstName = myObj.FirstName,
                LastName = myObj.LastName,
                DOB = myObj.DOB,
                Married = myObj.Married
            };
        }

        [MVCInheritable]
        public TEnumTest ProcessEnums(TEnumTest value1, TEnumTest value2)
        {
            return (TEnumTest)(((int)value1 + (int)value2) % 3);
        }

        [MVCInheritable]
        public TSetOfEnumTest ProcessSets(TSetOfEnumTest value1, TEnumTest value2)
        {
            value1.Add(value2);
            return value1;
        }

        [MVCInheritable]
        public TSimpleRecord GetSingleRecord()
        {
            return new TSimpleRecord();
        }

        [MVCInheritable]
        public TSimpleRecord[] GetArrayOfRecords()
        {
            return new TSimpleRecord[] { new TSimpleRecord(), new TSimpleRecord(), new TSimpleRecord() };
        }

        [MVCInheritable]
        public TSimpleRecord EchoSingleRecord(TSimpleRecord simpleRecord)
        {
            return simpleRecord;
        }

        [MVCInheritable]
        public TComplexRecord GetSingleComplexRecord()
        {
            return new TComplexRecord();
        }

        [MVCInheritable]
        public TComplexRecord EchoSingleComplexRecord(TComplexRecord complexRecord)
        {
            return complexRecord;
        }

        [MVCInheritable]
        public TComplexRecord[] EchoArrayOfRecords(TComplexRecord[] complexRecordArray)
        {
            return complexRecordArray;
        }

        [MVCInheritable]
        public TCustomerIssue648 GetTCustomer_ISSUE648()
        {
            return new TCustomerIssue648
            {
                Id = 155,
                Added = DateTime.Now,
                Name = "Daniele Teti",
                ExpirationDate = DateTime.Now.AddDays(7),
                MaxUpdateDate = null,
                AppVersion = null,
                Activated = null
            };
        }
    }

    [MVCJSONRPCAllowGET]
    public class TestJsonRpcClassWithGet : TestJsonRpcClass
    {
    }

    public class TestJsonRpcHookClass
    {
        private JObject fJSONReq;
        private string fHistory;
        private JSONRPCRequestType fJSONRPCKind;
        public void OnBeforeRoutingHook(WebContext context, JObject json)
        {
            fJSONReq = (JObject)json.DeepClone();
            if (string.Equals(json.Value<string>("method"), "error_OnBeforeRoutingHook", StringComparison.OrdinalIgnoreCase))
                throw new Exception("error_OnBeforeRoutingHook");
            fHistory = "OnBeforeRoutingHook";
            if (json.ContainsKey("id"))
                fJSONRPCKind = JSONRPCRequestType.Request;
            else
                fJSONRPCKind = JSONRPCRequestType.Notification;
        }
        public void OnBeforeCallHook(WebContext context, JObject json)
        {
            if (string.Equals(json.Value<string>("method"), "error_OnBeforeCallHook", StringComparison.OrdinalIgnoreCase))
                throw new Exception("error_OnBeforeCallHook");
            fHistory += "|OnBeforeCallHook";
        }
        public void OnAfterCallHook(WebContext context, JObject json)
        {
            try
            {
                if (string.Equals(fJSONReq.Value<string>("method"), "error_OnAfterCallHook", StringComparison.OrdinalIgnoreCase))
                    throw new Exception("error_OnAfterCallHook");
                fHistory += "|OnAfterCallHook";
                if (fJSONRPCKind == JSONRPCRequestType.Request)
                {
                    if (json == null)
                        throw new Exception("JSON is null");
                }
                else
                {
                    if (json != null)
                        if (!json.ContainsKey("error"))
                            throw new Exception("ERROR! Notification has a response but is not an error");
                }
                if (json != null && json.ContainsKey("error"))
                    fHistory += "|error";
                context.Response.Headers["x-history"] = fHistory;
            }
            finally
            {
                fJSONReq = null;
            }
        }
        [MVCInheritable]
        public bool error_OnBeforeRoutingHook()
        {
            return true;
        }
        [MVCInheritable]
        public bool error_OnBeforeCallHook()
        {
            return true;
        }
        [MVCInheritable]
        public bool error_OnAfterCallHook()
        {
            return true;
        }
        [MVCInheritable]
        public void Notif1()
        {
        }
        [MVCInheritable]
        public void NotifWithError()
        {
            throw new Exception("BOOM NOTIF");
        }
        [MVCInheritable]
        public string Request1()
        {
            return "empty";
        }
        [MVCInheritable]
        public string RequestWithError()
        {
            throw new Exception("BOOM REQUEST");
        }
    }

    [MVCJSONRPCAllowGET]
    public class TestJsonRpcHookClassWithGet : TestJsonRpcHookClass
    {
    }
}
