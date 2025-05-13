using System;
using System.Drawing;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;

namespace MainSenderFormApp
{
    public class MainSenderForm : Form
    {
        private TextBox edtMessage;
        private Label lblMessage;
        private Button btnSend;
        private HttpClient httpClient;

        public MainSenderForm()
        {
            
            httpClient = new HttpClient();

            
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            
            this.edtMessage = new TextBox();
            this.lblMessage = new Label();
            this.btnSend = new Button();

            
            this.lblMessage.Text = "Enter Message:";
            this.lblMessage.Location = new Point(20, 33);
            this.lblMessage.AutoSize = true;

            
            this.edtMessage.Location = new Point(100, 30);
            this.edtMessage.Size = new Size(250, 20);

            
            this.btnSend.Text = "Send Message";
            this.btnSend.Location = new Point(100, 70);
            this.btnSend.Click += new EventHandler(btnSend_Click);

            
            this.ClientSize = new Size(400, 120);
            this.Controls.Add(this.lblMessage);
            this.Controls.Add(this.edtMessage);
            this.Controls.Add(this.btnSend);
            this.Text = "Main Sender Form";
        }

        private async void btnSend_Click(object sender, EventArgs e)
        {
            var jsonObject = new { value = edtMessage.Text };
            string jsonString = JsonSerializer.Serialize(jsonObject);

            using (var content = new StringContent(jsonString, Encoding.UTF8, "application/json"))
            {
                try
                {
                    HttpResponseMessage response = await httpClient.PostAsync("http://localhost:8080/api/notifications", content);
                    
                    if (response.StatusCode != HttpStatusCode.Created)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();
                        MessageBox.Show(
                            $"{(int)response.StatusCode}: {response.ReasonPhrase} ({responseContent})",
                            "Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            httpClient.Dispose();
            base.OnFormClosed(e);
        }
    }

    static class Program
    {
        
        [STAThread]
        static void Main()
        {
            
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainSenderForm());
        }
    }
}
