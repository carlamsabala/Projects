using System;
using System.Threading;
using System.Windows.Forms;
using LoggerPro;
using LoggerPro.VCLMemoAppender;
using LoggerPro.VCLListViewAppender;

namespace VCLAppenders
{
    public partial class MainForm : Form
    {
        private ILogWriter _log;
        public MainForm()
        {
            InitializeComponent();
            Load += MainForm_Load;
        }
        private void MainForm_Load(object sender, EventArgs e)
        {
            _log = BuildLogWriter(new ILogAppender[] { new VCLListViewAppender(ListView1), new VCLMemoLogAppender(Memo1) });
        }
        public ILogWriter Log() => _log;
        private void Button1_Click(object sender, EventArgs e)
        {
            Log().Debug("This is a debug message with TAG1", "TAG1");
            Log().Debug("This is a debug message with TAG2", "TAG2");
        }
        private void Button2_Click(object sender, EventArgs e)
        {
            Log().Info("This is a info message with TAG1", "TAG1");
            Log().Info("This is a info message with TAG2", "TAG2");
        }
        private void Button3_Click(object sender, EventArgs e)
        {
            Log().Warn("This is a warning message with TAG1", "TAG1");
            Log().Warn("This is a warning message with TAG2", "TAG2");
        }
        private void Button4_Click(object sender, EventArgs e)
        {
            Log().Error("This is an error message with TAG1", "TAG1");
            Log().Error("This is an error message with TAG2", "TAG2");
        }
        private void Button5_Click(object sender, EventArgs e)
        {
            Action threadProc = () =>
            {
                string threadID = Thread.CurrentThread.ManagedThreadId.ToString();
                for (int i = 1; i <= 50; i++)
                {
                    Log().Debug("log message " + DateTime.Now.ToLongTimeString() + " ThreadID: " + threadID, "MULTITHREADING");
                    Log().Info("log message " + DateTime.Now.ToLongTimeString() + " ThreadID: " + threadID, "MULTITHREADING");
                    Log().Warn("log message " + DateTime.Now.ToLongTimeString() + " ThreadID: " + threadID, "MULTITHREADING");
                    Log().Error("log message " + DateTime.Now.ToLongTimeString() + " ThreadID: " + threadID, "MULTITHREADING");
                    Log().Fatal("log message " + DateTime.Now.ToLongTimeString() + " ThreadID: " + threadID, "MULTITHREADING");
                }
            };
            new Thread(new ThreadStart(threadProc)).Start();
            new Thread(new ThreadStart(threadProc)).Start();
            new Thread(new ThreadStart(threadProc)).Start();
            new Thread(new ThreadStart(threadProc)).Start();
        }
        private void Button6_Click(object sender, EventArgs e)
        {
            Log().Fatal("This is a fatal message with TAG1", "TAG1");
            Log().Fatal("This is a fatal message with TAG2", "TAG2");
        }
    }
}
