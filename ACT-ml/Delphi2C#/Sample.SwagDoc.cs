using System;
using Swag.Doc;
using Swag.Common.Types;
using Sample.Api.Employee;

namespace Sample
{
    public class SampleApiSwagDocBuilder
    {
        private TSwagDoc _swagDoc;
        private string _deployFolder;
        public string DeployFolder { get => _deployFolder; set => _deployFolder = value; }
        public string Generate()
        {
            _swagDoc = new TSwagDoc();
            try
            {
                DocumentApiInfo();
                DocumentApiSettings();
                DocumentApi();
                _swagDoc.GenerateSwaggerJson();
                SaveSwaggerJson();
                return _swagDoc.SwaggerJson.Format;
            }
            finally
            {
                _swagDoc.Dispose();
            }
        }
        private void DocumentApiInfo()
        {
            _swagDoc.Info.Title = "Sample API";
            _swagDoc.Info.Version = "v1";
            _swagDoc.Info.TermsOfService = "http://www.apache.org/licenses/LICENSE-2.0.txt";
            _swagDoc.Info.Description = "Sample API Description";
            _swagDoc.Info.Contact.Name = "Marcelo Jaloto";
            _swagDoc.Info.Contact.Email = "marcelojaloto@gmail.com";
            _swagDoc.Info.Contact.Url = "https://github.com/marcelojaloto/SwagDoc";
            _swagDoc.Info.License.Name = "Apache License - Version 2.0, January 2004";
            _swagDoc.Info.License.Url = "http://www.apache.org/licenses/LICENSE-2.0";
        }
        private void DocumentApiSettings()
        {
            _swagDoc.Host = "localhost";
            _swagDoc.BasePath = "/api";
            _swagDoc.Consumes.Add("application/json");
            _swagDoc.Produces.Add("application/json");
            _swagDoc.Schemes = new TSwagProtocol[] { TSwagProtocol.tpsHttp };
        }
        private void DocumentApi()
        {
            DocumentApiEmployee();
        }
        private void DocumentApiEmployee()
        {
            using (TFakeApiEmployee vApiEmployee = new TFakeApiEmployee())
            {
                vApiEmployee.DocumentApi(_swagDoc);
            }
        }
        private void SaveSwaggerJson()
        {
            _swagDoc.SwaggerFilesFolder = _deployFolder;
            _swagDoc.SaveSwaggerJsonToFile();
        }
    }
}
