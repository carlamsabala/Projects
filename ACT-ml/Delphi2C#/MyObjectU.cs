[Flags]
public enum TSetTest
{
    None = 0,
    ptEnumValue1 = 1,
    ptEnumValue2 = 2,
    ptEnumValue3 = 4,
    ptEnumValue4 = 8
}

public class TTestRec
{
    public int Value { get; set; }
    public TTestRec(int value)
    {
        Value = value;
    }
}

public class TTestRecDynArray : List<TTestRec>
{
}

public class TTestRecArray
{
    public TTestRec[] Array;
}

public class TNestedArraysRec
{
    public TTestRec[] ArrayProp1;
    public TTestRec[] ArrayProp2;
    public TTestRec TestRecProp;
}

public class TMultiDataset
{
    public DataTable Customers { get; set; }
    public DataTable People { get; set; }
    public TMultiDataset()
    {
        Customers = new DataTable();
        People = new DataTable();
    }
}

public class TMVCStringDictionary : Dictionary<string, string>
{
}

public class TPerson
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime DOB { get; set; }
    public bool Married { get; set; }
}

public class WebContext
{
}

public class MVCJSONRPCErrorException : Exception
{
    public int Code { get; }
    public MVCJSONRPCErrorException(int code, string message) : base(message)
    {
        Code = code;
    }
}

public static class Constants
{
    public const int JSONRPC_USER_ERROR = 1000;
}

public class MyObject
{
    DataTable GetCustomersDataset()
    {
        DataTable dt = new DataTable();
        dt.Columns.Add("Code", typeof(int));
        dt.Columns.Add("Name", typeof(string));
        dt.Rows.Add(1, "Ford");
        dt.Rows.Add(2, "Ferrari");
        dt.Rows.Add(3, "Lotus");
        dt.Rows.Add(4, "FCA");
        dt.Rows.Add(5, "Hyundai");
        dt.Rows.Add(6, "De Tomaso");
        dt.Rows.Add(7, "Dodge");
        dt.Rows.Add(8, "Tesla");
        dt.Rows.Add(9, "Kia");
        dt.Rows.Add(10, "Tata");
        dt.Rows.Add(11, "Volkswagen");
        dt.Rows.Add(12, "Audi");
        dt.Rows.Add(13, "Skoda");
        dt.AcceptChanges();
        return dt;
    }

    void FillCustomersDataset(DataTable dataSet)
    {
        dataSet.Rows.Add(1, "Ford");
        dataSet.Rows.Add(2, "Ferrari");
        dataSet.Rows.Add(3, "Lotus");
        dataSet.Rows.Add(4, "FCA");
        dataSet.Rows.Add(5, "Hyundai");
        dataSet.Rows.Add(6, "De Tomaso");
        dataSet.Rows.Add(7, "Dodge");
        dataSet.Rows.Add(8, "Tesla");
        dataSet.Rows.Add(9, "Kia");
        dataSet.Rows.Add(10, "Tata");
        dataSet.Rows.Add(11, "Volkswagen");
        dataSet.Rows.Add(12, "Audi");
        dataSet.Rows.Add(13, "Skoda");
        dataSet.AcceptChanges();
    }

    void FillPeopleDataset(DataTable dataSet)
    {
        dataSet.Rows.Add("Daniele", "Teti");
        dataSet.Rows.Add("Peter", "Parker");
        dataSet.Rows.Add("Bruce", "Banner");
        dataSet.Rows.Add("Scott", "Summers");
        dataSet.Rows.Add("Sue", "Storm");
        dataSet.AcceptChanges();
    }

    public void OnBeforeRoutingHook(WebContext context, JObject json)
    {
    }

    public void OnBeforeCallHook(WebContext context, JObject jsonRequest)
    {
        Console.WriteLine("TMyObject.OnBeforeCallHook >>");
        Console.WriteLine(jsonRequest.ToString(Formatting.None));
        Console.WriteLine("TMyObject.OnBeforeCallHook <<");
    }

    public void OnAfterCallHook(WebContext context, JObject jsonResponse)
    {
        Console.WriteLine("TMyObject.OnAfterCallHook >>");
        if (jsonResponse != null)
        {
            Console.WriteLine(jsonResponse.ToString(Formatting.None));
        }
        Console.WriteLine("TMyObject.OnAfterCallHook <<");
    }

    public int Subtract(int value1, int value2)
    {
        return value1 - value2;
    }

    public string ReverseString(string aString, bool aUpperCase)
    {
        char[] chars = aString.ToCharArray();
        Array.Reverse(chars);
        string result = new string(chars);
        if (aUpperCase)
        {
            result = result.ToUpper();
        }
        return result;
    }

    public DateTime GetNextMonday(DateTime aDate)
    {
        DateTime lDate = aDate.AddDays(1);
        while (lDate.DayOfWeek != DayOfWeek.Monday)
        {
            lDate = lDate.AddDays(1);
        }
        return lDate;
    }

    public DateTime PlayWithDatesAndTimes(double aJustAFloat, TimeSpan aTime, DateTime aDate, DateTime aDateAndTime)
    {
        return aDateAndTime.Add(aDate.TimeOfDay).Add(aTime).Add(TimeSpan.FromDays(aJustAFloat));
    }

    public DataTable GetCustomers(string filterString)
    {
        DataTable dt = GetCustomersDataset();
        if (!string.IsNullOrEmpty(filterString))
        {
            dt.DefaultView.RowFilter = filterString;
            dt = dt.DefaultView.ToTable();
        }
        dt.AcceptChanges();
        return dt;
    }

    public TMultiDataset GetMulti()
    {
        TMultiDataset result = new TMultiDataset();
        FillCustomersDataset(result.Customers);
        FillPeopleDataset(result.People);
        return result;
    }

    public TMVCStringDictionary GetStringDictionary()
    {
        var dict = new TMVCStringDictionary();
        dict.Add("key1", "value1");
        dict.Add("key2", "value2");
        dict.Add("key3", "value3");
        dict.Add("key4", "value4");
        return dict;
    }

    public TPerson GetUser(string aUserName)
    {
        TPerson person = new TPerson();
        person.FirstName = "Daniele (a.k.a. " + aUserName + ")";
        person.LastName = "Teti";
        person.DOB = new DateTime(1932, 11, 4);
        person.Married = true;
        return person;
    }

    public int SavePerson(TPerson person)
    {
        Console.WriteLine(person.FirstName + " " + person.LastName);
        return new Random().Next(1000);
    }

    public double FloatsTest(double aDouble, double aExtended)
    {
        return aDouble + aExtended;
    }

    public void DoSomething()
    {
    }

    public void RaiseCustomException()
    {
        throw new MVCJSONRPCErrorException(Constants.JSONRPC_USER_ERROR + 1, "This is an exception message");
    }

    public int RaiseGenericException(int exceptionType)
    {
        switch (exceptionType)
        {
            case 1:
                int l = 0;
                return 10 / l;
            case 2:
                throw new NullReferenceException("Fake Invalid Pointer Operation");
            default:
                throw new Exception("BOOOOM!");
        }
    }

    public JObject SaveObjectWithJSON(JObject withJSON)
    {
        var obj = Utils.JSONObjectAs<ObjectWithJSONObject>(withJSON);
        Console.WriteLine(obj);
        return (JObject)withJSON.DeepClone();
    }

    public TEnumTest PassingEnums(TEnumTest value1, TEnumTest value2)
    {
        if (value1 == value2)
            return TEnumTest.ptEnumValue4;
        else
            return TEnumTest.ptEnumValue3;
    }

    public TSetTest GetSetBySet(TSetTest value)
    {
        TSetTest result = TSetTest.None;
        foreach (TEnumTest item in Enum.GetValues(typeof(TEnumTest)))
        {
            if (value.HasFlag((TSetTest)(1 << (int)item)))
                result &= ~(TSetTest)(1 << (int)item);
            else
                result |= (TSetTest)(1 << (int)item);
        }
        return result;
    }

    public TTestRec SavePersonRec(TTestRec personRec)
    {
        return personRec;
    }

    public TTestRecDynArray GetPeopleRecDynArray()
    {
        TTestRecDynArray arr = new TTestRecDynArray();
        arr.Add(new TTestRec(1));
        arr.Add(new TTestRec(2));
        return arr;
    }

    public TTestRecArray GetPeopleRecStaticArray()
    {
        TTestRecArray result = new TTestRecArray();
        result.Array = new TTestRec[2];
        result.Array[0] = new TTestRec(7);
        result.Array[1] = new TTestRec(8);
        return result;
    }

    public TTestRec GetPersonRec()
    {
        return new TTestRec(99);
    }

    public TNestedArraysRec GetComplex1()
    {
        TNestedArraysRec result = new TNestedArraysRec();
        result.ArrayProp1 = new TTestRec[2];
        result.ArrayProp2 = new TTestRec[2];
        result.ArrayProp1[0] = new TTestRec(1234);
        result.ArrayProp1[1] = new TTestRec(2345);
        result.ArrayProp2[0] = new TTestRec(3456);
        result.ArrayProp2[1] = new TTestRec(4567);
        return result;
    }

    public TTestRecDynArray EchoComplexArrayOfRecords(TTestRecDynArray peopleList)
    {
        return peopleList;
    }

    public TNestedArraysRec EchoComplexArrayOfRecords2(TNestedArraysRec vendorProxiesAndLinks)
    {
        vendorProxiesAndLinks.TestRecProp.Value += 0;
        return vendorProxiesAndLinks;
    }

    public void InvalidMethod1(ref int myVarParam)
    {
    }

    public void InvalidMethod2(out int myOutParam)
    {
        myOutParam = 0;
    }
}

public static class Utils
{
    public static T JSONObjectAs<T>(JObject json) where T : class, new()
    {
        T obj = new T();
        string jsonStr = json.ToString(Formatting.None);
        obj = JsonConvert.DeserializeObject<T>(jsonStr);
        return obj;
    }
}

public class ObjectWithJSONObject
{
    public override string ToString()
    {
        return "ObjectWithJSONObject";
    }
}
