using Microsoft.AspNetCore.Mvc;
using Controllers.Base;    
using EntitiesU;          
using System;

public static class TSwaggerConst
{
    public const string USE_DEFAULT_SUMMARY_TAGS = "USE_DEFAULT_SUMMARY_TAGS";
    public const string PLURAL_MODEL_NAME = "People"; 
}

public class SWAGUseDefaultControllerModel { }

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class MVCSwagDefaultModelAttribute : Attribute
{
    public Type ModelType { get; }
    public string SingularName { get; }
    public string PluralName { get; }
    public MVCSwagDefaultModelAttribute(Type modelType, string singularName, string pluralName)
    {
        ModelType = modelType;
        SingularName = singularName;
        PluralName = pluralName;
    }
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class MVCSwagDefaultSummaryTagsAttribute : Attribute
{
    public string Tags { get; }
    public MVCSwagDefaultSummaryTagsAttribute(string tags)
    {
        Tags = tags;
    }
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class MVCSwagSummaryAttribute : Attribute
{
    public string UseDefaultSummaryTags { get; }
    public string Description { get; }
    public string OperationId { get; }
    public MVCSwagSummaryAttribute(string useDefaultSummaryTags, string description, string operationId)
    {
        UseDefaultSummaryTags = useDefaultSummaryTags;
        Description = description;
        OperationId = operationId;
    }
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class MVCSwagResponsesAttribute : Attribute
{
    public int StatusCode { get; }
    public string Description { get; }
    public object ResponseModel { get; }
    public bool UseDefault { get; }
    public MVCSwagResponsesAttribute(int statusCode, string description)
    {
        StatusCode = statusCode;
        Description = description;
    }
    public MVCSwagResponsesAttribute(int statusCode, string description, object responseModel, bool useDefault)
    {
        StatusCode = statusCode;
        Description = description;
        ResponseModel = responseModel;
        UseDefault = useDefault;
    }
}

namespace ControllersU
{
    
    [Route("mypeople")]
    [ApiController]
    [MVCSwagDefaultModel(typeof(PersonModel), "MyPerson", "MyPeople")]
    [MVCSwagDefaultSummaryTags("MyPeople")]
    public class MyPeopleController : BaseController<TPerson, TPersonRec>
    {
        // No additional actions are defined.
    }

    
    [Route("people")]
    [ApiController]
    [MVCSwagDefaultModel(typeof(PersonModel), "Person", "People")]
    [MVCSwagDefaultSummaryTags("People")]
    public class PeopleController : BaseController<TPerson, TPersonRec>
    {
        
        [HttpGet("all2")]
        [MVCSwagSummary(TSwaggerConst.USE_DEFAULT_SUMMARY_TAGS,
            "List all " + TSwaggerConst.PLURAL_MODEL_NAME + " (using route /all2, with model specified in controller)",
            "getAll2" + TSwaggerConst.PLURAL_MODEL_NAME)]
        [MVCSwagResponses(200, "Success", typeof(SWAGUseDefaultControllerModel), true)]
        [MVCSwagResponses(500, "Internal Server Error")]
        public virtual IActionResult GetAll2()
        {
            
            return Ok();
        }

        
        [HttpGet("all3")]
        [MVCSwagSummary(TSwaggerConst.USE_DEFAULT_SUMMARY_TAGS,
            "List all PersonWithNickNameModel (using route /all3, custom model specified on action)",
            "getAllPeopleWithNickName")]
        [MVCSwagResponses(200, "Success", typeof(PersonWithNickNameModel), true)]
        [MVCSwagResponses(500, "Internal Server Error")]
        public virtual IActionResult GetAll3()
        {
            
            return Ok();
        }
    }

    
    [Route("tallpeople")]
    [Route("tallpeoplexxx")]
    [ApiController]
    [MVCSwagDefaultModel(typeof(TallPersonModel), "TallPerson", "TallPeople")]
    [MVCSwagDefaultSummaryTags("Tall People")]
    public class TallPeopleController : PeopleController
    {
        
        [HttpGet("all2")]
        [MVCSwagSummary(TSwaggerConst.USE_DEFAULT_SUMMARY_TAGS,
            "List all " + TSwaggerConst.PLURAL_MODEL_NAME + " (child controller, default controller model)",
            "getAll2" + TSwaggerConst.PLURAL_MODEL_NAME)]
        [MVCSwagResponses(200, "Success", typeof(SWAGUseDefaultControllerModel), true)]
        [MVCSwagResponses(500, "Internal Server Error")]
        public override IActionResult GetAll2()
        {
            
            return base.GetAll2();
        }

        
        [HttpGet("all3")]
        [MVCSwagSummary(TSwaggerConst.USE_DEFAULT_SUMMARY_TAGS,
            "List all PersonWithNickNameModel", "getAllPersonWithNickNameModel")]
        [MVCSwagResponses(200, "Success", typeof(PersonWithNickNameModel), true)]
        [MVCSwagResponses(500, "Internal Server Error")]
        public override IActionResult GetAll3()
        {
            
            return base.GetAll3();
        }
    }
}
