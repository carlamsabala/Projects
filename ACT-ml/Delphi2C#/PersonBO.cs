    [JsonProperty("id")]
    public int ID
    {
        get => _id;
        set => _id = value;
    }

    [JsonProperty("firstName")]
    public string FIRST_NAME
    {
        get => _firstName;
        set => _firstName = value;
    }

    [JsonProperty("lastName")]
    public string LAST_NAME
    {
        get => _lastName;
        set => _lastName = value;
    }

    [JsonProperty("workPhoneNumber")]
    public string WORK_PHONE_NUMBER
    {
        get => _workPhoneNumber;
        set => _workPhoneNumber = value;
    }

    [JsonProperty("mobilePhoneNumber")]
    public string MOBILE_PHONE_NUMBER
    {
        get => _mobilePhoneNumber;
        set => _mobilePhoneNumber = value;
    }

    [JsonProperty("email")]
    public string EMAIL
    {
        get => _email;
        set => _email = value;
    }
}
