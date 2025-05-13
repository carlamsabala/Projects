
using System;
using System.Threading;
using System.Windows.Forms;
using LoggerPro; 

namespace LoggerPro
{
    
    public class VCLListBoxAppender : LoggerProAppenderBase
    {
        private readonly ListBox _listBox;
        private readonly int _maxLogLines;

        public VCLListBoxAppender(ListBox listBox, int maxLogLines = 500, ILogItemRenderer logItemRenderer = null)
            : base(logItemRenderer)
        {
            _listBox = listBox ?? throw new ArgumentNullException(nameof(listBox));
            _maxLogLines = maxLogLines;
        }
        public override void Setup()
        {
            base.Setup();
            
            if (_listBox.InvokeRequired)
            {
                _listBox.Invoke(new Action(() => _listBox.Items.Clear()));
            }
            else
            {
                _listBox.Items.Clear();
            }
        }

        
        public override void TearDown()
        {
            base.TearDown();
            
        }

        
        public override void WriteLog(LogItem logItem)
        {
            
            string formattedLog = FormatLog(logItem);

            
            if (_listBox.InvokeRequired)
            {
                _listBox.BeginInvoke(new Action(() => AppendLog(formattedLog)));
            }
            else
            {
                AppendLog(formattedLog);
            }
        }

        private void AppendLog(string logText)
        {
            _listBox.BeginUpdate();
            try
            {
                
                while (_listBox.Items.Count >= _maxLogLines)
                {
                    _listBox.Items.RemoveAt(0);
                }
                _listBox.Items.Add(logText);
                
                _listBox.SelectedIndex = _listBox.Items.Count - 1;
            }
            finally
            {
                _listBox.EndUpdate();
            }
        }
    }
}
