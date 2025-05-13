using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Swag.Doc;
using Swag.Common.Types;
using Swag.Doc.Path.Operation;
using Swag.Doc.Path.Operation.Response;
using Swag.Doc.Path.Operation.RequestParameter;
using Sample.DelphiUnit.Generate;
using System.Text;

namespace Sample.MvcControllerClientFileBuilder
{
    public class SwagDocToDelphiRESTClientBuilder
    {
        private TSwagDoc _swagDoc;
        public SwagDocToDelphiRESTClientBuilder(TSwagDoc pSwagDoc)
        {
            _swagDoc = pSwagDoc;
        }
        private string CapitalizeFirstLetter(string pTypeName)
        {
            if (pTypeName.Length > 2)
                return char.ToUpper(pTypeName[0]) + pTypeName.Substring(1);
            return pTypeName;
        }
        private string RewriteUriToSwaggerWay(string pUri)
        {
            return pUri.Replace("{", "($").Replace("}", ")");
        }
        private string OperationIdToFunctionName(TSwagPathOperation pOperation)
        {
            string result = pOperation.OperationId.Replace("{", "").Replace("}", "").Replace("-", "");
            if (!char.IsLetter(result[0]))
                result = "F" + result;
            return CapitalizeFirstLetter(result);
        }
        private string GenerateUnitText(TDelphiUnit pDelphiUnit)
        {
            pDelphiUnit.Title = _swagDoc.Info.Title;
            pDelphiUnit.Description = _swagDoc.Info.Description;
            pDelphiUnit.License = _swagDoc.Info.License.Name;
            return pDelphiUnit.Generate();
        }
        private TUnitTypeDefinition ConvertSwaggerTypeToDelphiType(TSwagRequestParameter pSwaggerType)
        {
            TUnitTypeDefinition result = new TUnitTypeDefinition();
            TSwagTypeParameter vSwaggerType = pSwaggerType.TypeParameter;
            switch (vSwaggerType)
            {
                case TSwagTypeParameter.stpNotDefined:
                    if (pSwaggerType.Schema.JsonSchema.TryGetValue("$ref", out string refValue) && !string.IsNullOrEmpty(refValue))
                        result.TypeName = ConvertRefToType(refValue);
                    else
                    {
                        result.TypeName = pSwaggerType.Schema.JsonSchema.GetValue("type").ToString();
                        if (result.TypeName.ToLower() == "array")
                        {
                            if (pSwaggerType.Schema.JsonSchema.TryGetValue("items", out string itemsJson))
                                result.TypeName = "array of " + pSwaggerType.Schema.JsonSchema.GetValue("type").ToString();
                        }
                    }
                    break;
                case TSwagTypeParameter.stpString:
                    result.TypeName = "String";
                    break;
                case TSwagTypeParameter.stpNumber:
                    result.TypeName = "Double";
                    break;
                case TSwagTypeParameter.stpInteger:
                    result.TypeName = "Integer";
                    break;
                case TSwagTypeParameter.stpBoolean:
                    result.TypeName = "Boolean";
                    break;
                case TSwagTypeParameter.stpArray:
                    {
                        var vJson = pSwaggerType.Schema.JsonSchema;
                        if (vJson != null)
                            result.TypeName = "array of " + vJson.GetValue("type").ToString();
                        else
                        {
                            if (pSwaggerType.Items.Values.TryGetValue("type", out object typeObj))
                                result.TypeName = "array of " + typeObj.ToString();
                            else
                                result.TypeName = "array of ";
                        }
                    }
                    break;
                case TSwagTypeParameter.stpFile:
                    result.TypeName = "err File";
                    break;
            }
            return result;
        }
        private string ConvertRefToType(string pRef)
        {
            int idx = pRef.LastIndexOf('/');
            string result = pRef.Substring(idx + 1);
            result = char.ToUpper(result[0]) + result.Substring(1);
            if (result.ToLower() != "string")
                result = "T" + result;
            return result;
        }
        private string ConvertRefToVarName(string pRef)
        {
            int idx = pRef.LastIndexOf('/');
            return pRef.Substring(idx + 1);
        }
        private void ChildType(TDelphiUnit pDelphiUnit, TJsonPair pJson)
        {
            TUnitTypeDefinition vTypeInfo = new TUnitTypeDefinition();
            vTypeInfo.TypeName = "T" + CapitalizeFirstLetter(pJson.JsonString.Value);
            TJsonObject vJsonProps = pJson.JsonValue.GetValue("properties") as TJsonObject;
            for (int i = 0; i < vJsonProps.Count; i++)
            {
                TUnitFieldDefinition vFieldInfo = new TUnitFieldDefinition();
                vFieldInfo.FieldName = vJsonProps.Pairs[i].JsonString.Value;
                TJsonObject vTypeObj = vJsonProps.Pairs[i].JsonValue as TJsonObject;
                vFieldInfo.FieldType = vTypeObj.GetValue("type").ToString();
                if (vFieldInfo.FieldType == "number")
                    vFieldInfo.FieldType = "Double";
                else if (vFieldInfo.FieldType == "object")
                {
                    vFieldInfo.FieldType = "T" + CapitalizeFirstLetter(vJsonProps.Pairs[i].JsonString.Value);
                    ChildType(pDelphiUnit, vJsonProps.Pairs[i]);
                }
                if (vTypeObj.TryGetValue("description", out object vValue))
                    vFieldInfo.AddAttribute("[MVCDoc(" + Quote(vValue.ToString()) + ")]");
                if (vTypeObj.TryGetValue("format", out vValue))
                {
                    if (vFieldInfo.FieldType.ToLower() == "integer" && vValue.ToString().ToLower() == "int64")
                        vFieldInfo.FieldType = "Int64";
                    vFieldInfo.AddAttribute("[MVCFormat(" + Quote(vValue.ToString()) + ")]");
                }
                if (vTypeObj.TryGetValue("maxLength", out vValue))
                    vFieldInfo.AddAttribute("[MVCMaxLength(" + vValue.ToString() + ")]");
                vTypeInfo.Fields.Add(vFieldInfo);
            }
            pDelphiUnit.AddType(vTypeInfo);
        }
        private void HandleArray(TUnitFieldDefinition pField, TJsonPair pJson)
        {
            TJsonObject jsonObj = pJson.JsonValue as TJsonObject;
            if (jsonObj.Values.ContainsKey("items") && (jsonObj.Values["items"] as TJsonObject).Values.ContainsKey("type"))
            {
                string vType = (jsonObj.Values["items"] as TJsonObject).Values["type"].ToString();
                if (vType.ToLower() != "string")
                    vType = "T" + vType;
                pField.FieldType = "array of " + vType;
            }
            else
            {
                TJsonObject vJsonVal = jsonObj.Values["items"] as TJsonObject;
                string vRef = vJsonVal.Values["$ref"].ToString();
                pField.FieldType = "array of " + ConvertRefToType(vRef);
            }
        }
        private void ConvertSwaggerDefinitionsToTypeDefinitions(TDelphiUnit pDelphiUnit)
        {
            for (int i = 0; i < _swagDoc.Definitions.Count; i++)
            {
                TUnitTypeDefinition vTypeInfo = new TUnitTypeDefinition();
                vTypeInfo.TypeName = "T" + CapitalizeFirstLetter(_swagDoc.Definitions[i].Name);
                TJsonObject vJsonProps = _swagDoc.Definitions[i].JsonSchema.GetValue("properties") as TJsonObject;
                for (int j = 0; j < vJsonProps.Count; j++)
                {
                    TUnitFieldDefinition vFieldInfo = new TUnitFieldDefinition();
                    vFieldInfo.FieldName = vJsonProps.Pairs[j].JsonString.Value;
                    TJsonObject vTypeObj = vJsonProps.Pairs[j].JsonValue as TJsonObject;
                    if (vTypeObj.Values.ContainsKey("type"))
                        vFieldInfo.FieldType = vTypeObj.Values["type"].ToString();
                    else
                        vFieldInfo.FieldType = ConvertRefToType(vTypeObj.Values["$ref"].ToString());
                    if (vFieldInfo.FieldType == "number")
                        vFieldInfo.FieldType = "Double";
                    else if (vFieldInfo.FieldType == "object")
                    {
                        vFieldInfo.FieldType = "T" + CapitalizeFirstLetter(vJsonProps.Pairs[j].JsonString.Value);
                        ChildType(pDelphiUnit, vJsonProps.Pairs[j]);
                    }
                    else if (vFieldInfo.FieldType == "array")
                    {
                        HandleArray(vFieldInfo, vJsonProps.Pairs[j]);
                    }
                    if (vTypeObj.TryGetValue("description", out object vValue) && vValue.ToString().Trim().Length > 0)
                        vFieldInfo.AddAttribute("[MVCDoc(" + Quote(vValue.ToString()) + ")]");
                    if (vTypeObj.TryGetValue("format", out vValue))
                    {
                        if (vFieldInfo.FieldType.ToLower() == "integer" && vValue.ToString().ToLower() == "int64")
                            vFieldInfo.FieldType = "Int64";
                        vFieldInfo.AddAttribute("[MVCFormat(" + Quote(vValue.ToString()) + ")]");
                    }
                    if (vTypeObj.TryGetValue("maxLength", out vValue))
                        vFieldInfo.AddAttribute("[MVCMaxLength(" + vValue.ToString() + ")]");
                    if (vTypeObj.TryGetValue("minimum", out vValue))
                        vFieldInfo.AddAttribute("[MVCMinimum(" + vValue.ToString() + ")]");
                    if (vTypeObj.TryGetValue("maximum", out vValue))
                        vFieldInfo.AddAttribute("[MVCMaximum(" + vValue.ToString() + ")]");
                    vTypeInfo.Fields.Add(vFieldInfo);
                }
                pDelphiUnit.AddType(vTypeInfo);
            }
        }
        private string Quote(string s)
        {
            return "\"" + s + "\"";
        }
        public string Generate()
        {
            TDelphiUnit vDelphiUnit = new TDelphiUnit();
            vDelphiUnit.UnitFile = "UnitFilenameMvcControllerClient";
            vDelphiUnit.AddInterfaceUnit("IPPeerClient");
            vDelphiUnit.AddInterfaceUnit("REST.Client");
            vDelphiUnit.AddInterfaceUnit("REST.Authenticator.OAuth");
            vDelphiUnit.AddInterfaceUnit("REST.Types");
            vDelphiUnit.AddInterfaceUnit("MVCFramework");
            vDelphiUnit.AddInterfaceUnit("MVCFramework.Commons");
            ConvertSwaggerDefinitionsToTypeDefinitions(vDelphiUnit);
            TUnitTypeDefinition vMVCControllerClient = new TUnitTypeDefinition();
            vMVCControllerClient.TypeName = "TMyMVCControllerClient";
            vMVCControllerClient.TypeInherited = "TObject";
            vMVCControllerClient.AddAttribute("  [MVCPath('" + RewriteUriToSwaggerWay(_swagDoc.BasePath) + "')]");

            TUnitFieldDefinition vField = new TUnitFieldDefinition();
            vField.FieldName = "RESTClient";
            vField.FieldType = "TRESTClient";
            vMVCControllerClient.Fields.Add(vField);

            vField = new TUnitFieldDefinition();
            vField.FieldName = "RESTRequest";
            vField.FieldType = "TRESTRequest";
            vMVCControllerClient.Fields.Add(vField);

            vField = new TUnitFieldDefinition();
            vField.FieldName = "RESTResponse";
            vField.FieldType = "TRESTResponse";
            vMVCControllerClient.Fields.Add(vField);

            vDelphiUnit.AddType(vMVCControllerClient);
            ConvertSwaggerDefinitionsToTypeDefinitions(vDelphiUnit);
            for (int pathIndex = 0; pathIndex < _swagDoc.Paths.Count; pathIndex++)
            {
                for (int opIndex = 0; opIndex < _swagDoc.Paths[pathIndex].Operations.Count; opIndex++)
                {
                    TUnitMethod vMethod = new TUnitMethod();
                    if (_swagDoc.Paths[pathIndex].Operations[opIndex].Description.Trim().Length > 0)
                        vMethod.AddAttribute("    [MVCDoc(" + Quote(_swagDoc.Paths[pathIndex].Operations[opIndex].Description) + ")]");
                    vMethod.AddAttribute("    [MVCPath('" + _swagDoc.Paths[pathIndex].Uri + "')]"); 
                    vMethod.AddAttribute("    [MVCHTTPMethod([http" + _swagDoc.Paths[pathIndex].Operations[opIndex].OperationToString + "])]");
                    vMethod.Name = OperationIdToFunctionName(_swagDoc.Paths[pathIndex].Operations[opIndex]);
                    for (int paramIndex = 0; paramIndex < _swagDoc.Paths[pathIndex].Operations[opIndex].Parameters.Count; paramIndex++)
                    {
                        TUnitParameter vResultParam = new TUnitParameter();
                        vResultParam.ParamName = CapitalizeFirstLetter(_swagDoc.Paths[pathIndex].Operations[opIndex].Parameters[paramIndex].Name);
                        vResultParam.ParamType = ConvertSwaggerTypeToDelphiType(_swagDoc.Paths[pathIndex].Operations[opIndex].Parameters[paramIndex]);
                        vMethod.AddParameter(vResultParam);
                    }
                    foreach (var response in _swagDoc.Paths[pathIndex].Operations[opIndex].Responses)
                    {
                        TJsonObject vSchemaObj = response.Value.Schema.JsonSchema;
                        if (vSchemaObj == null)
                            continue;
                        if (vSchemaObj.TryGetValue("$ref", out string vRef))
                        {
                            vMethod.AddAttribute("    [MVCResponse(" + response.Key + ", " + Quote(response.Value.Description) + ", " + ConvertRefToType(vRef) + ")]");
                            TUnitParameter vResultParam = new TUnitParameter();
                            vResultParam.ParamName = ConvertRefToVarName(vRef);
                            vResultParam.ParamType = new TUnitTypeDefinition { TypeName = ConvertRefToType(vRef) };
                            vMethod.AddLocalVariable(vResultParam);
                            vMethod.Content.Add("  " + ConvertRefToVarName(vRef) + " := " + ConvertRefToType(vRef) + ".Create;");
                        }
                        else
                        {
                            if (!vSchemaObj.TryGetValue("properties", out _))
                                continue;
                            if (!vSchemaObj.TryGetValue("employees", out _))
                                continue;
                            if (!vSchemaObj.TryGetValue("items", out _))
                                continue;
                            if (vSchemaObj.TryGetValue("$ref", out vRef))
                            {
                                vMethod.AddAttribute("    [MVCResponseList(" + response.Key + ", " + Quote(response.Value.Description) + ", " + ConvertRefToType(vRef) + ")]");
                                TUnitParameter vResultParam = new TUnitParameter();
                                vResultParam.ParamName = ConvertRefToVarName(vRef);
                                vResultParam.ParamType = new TUnitTypeDefinition { TypeName = "TObjectList<" + ConvertRefToType(vRef) + ">" };
                                vMethod.AddLocalVariable(vResultParam);
                                vDelphiUnit.AddInterfaceUnit("Generics.Collections");
                                vMethod.Content.Add("  " + ConvertRefToVarName(vRef) + " := TObjectList<" + ConvertRefToType(vRef) + ">.Create;");
                            }
                        }
                    }
                    vMVCControllerClient.Methods.Add(vMethod);
                }
            }
            vDelphiUnit.SortTypeDefinitions();
            return GenerateUnitText(vDelphiUnit);
        }
    }
}
