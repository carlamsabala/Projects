using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace frmLoadSwaggerJson
{
    public class SwagInfo
    {
        public string Title { get; set; } = "";
        public string Version { get; set; } = "";
        public string TermsOfService { get; set; } = "";
        public string Description { get; set; } = "";
    }

    public class SwagDoc
    {
        public SwagInfo Info { get; set; } = new SwagInfo();
        public string Host { get; set; } = "";
        public string BasePath { get; set; } = "";
        public List<string> Consumes { get; set; } = new List<string>();
        public List<string> Produces { get; set; } = new List<string>();
        public List<string> Schemes { get; set; } = new List<string>();
        public List<object> Definitions { get; set; } = new List<object>();
        public List<object> Paths { get; set; } = new List<object>();

        public JObject SwaggerJson { get; set; }

        
        public void LoadFromFile(string filename)
        {
            string content = File.ReadAllText(filename);
            
            SwaggerJson = JObject.Parse(content);
            
            if (SwaggerJson["info"] is JObject infoObj)
            {
                Info = new SwagInfo
                {
                    Title = infoObj.Value<string>("title") ?? "",
                    Version = infoObj.Value<string>("version") ?? "",
                    TermsOfService = infoObj.Value<string>("termsOfService") ?? "",
                    Description = infoObj.Value<string>("description") ?? ""
                };
            }
        }

        
        public void GenerateSwaggerJson()
        {
            var doc = new JObject
            {
                ["swagger"] = "2.0",
                ["info"] = new JObject
                {
                    ["title"] = Info.Title,
                    ["version"] = Info.Version,
                    ["termsOfService"] = Info.TermsOfService,
                    ["description"] = Info.Description
                },
                ["host"] = Host,
                ["basePath"] = BasePath,
                ["consumes"] = new JArray(Consumes),
                ["produces"] = new JArray(Produces),
                ["schemes"] = new JArray(Schemes),
                ["definitions"] = new JObject(), 
                ["paths"] = new JObject()        
            };

            SwaggerJson = doc;
        }
    }
    public class SimpleSwaggerDocDemoForm : Form
    {
        private TextBox memo1;
        private Button btnLoadJSON;
        private Label lblApiDescription;

        public SimpleSwaggerDocDemoForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.memo1 = new TextBox();
            this.btnLoadJSON = new Button();
            this.lblApiDescription = new Label();
            
            this.memo1.Multiline = true;
            this.memo1.ScrollBars = ScrollBars.Both;
            this.memo1.Location = new Point(12, 50);
            this.memo1.Size = new Size(760, 400);
            this.memo1.Font = new Font("Consolas", 10);
             
            this.btnLoadJSON.Location = new Point(12, 10);
            this.btnLoadJSON.Size = new Size(120, 30);
            this.btnLoadJSON.Text = "Load JSON";
            this.btnLoadJSON.Click += new EventHandler(this.btnLoadJSON_Click);
             
            this.lblApiDescription.Location = new Point(150, 10);
            this.lblApiDescription.Size = new Size(400, 30);
            this.lblApiDescription.Text = "API Description";
            
            this.ClientSize = new Size(784, 461);
            this.Controls.Add(this.memo1);
            this.Controls.Add(this.btnLoadJSON);
            this.Controls.Add(this.lblApiDescription);
            this.Text = "Swagger JSON Loader";
        }

        private void btnLoadJSON_Click(object sender, EventArgs e)
        {
            try
            {
                SwagDoc swagDoc = new SwagDoc();
                swagDoc.LoadFromFile("swagger.json");
                lblApiDescription.Text = swagDoc.Info.Description;
                swagDoc.GenerateSwaggerJson();
                memo1.Text = swagDoc.SwaggerJson.ToString(Formatting.Indented);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading JSON: " + ex.Message);
            }
        }
    }
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new SimpleSwaggerDocDemoForm());
        }
    }
}
