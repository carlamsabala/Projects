using System;
using System.Drawing;
using System.Windows.Forms;

namespace YourNamespace
{
    public class WaitingForm : Form
    {
        private Label lblMessage;
        private Label lblRunningRequests;
        private Timer TimerWaiting;
        private Panel Shape1;
        private int FWaitingCount;
        private int fPoints;

        public int WaitingCount
        {
            get { return FWaitingCount; }
            set { SetWaitingCount(value); }
        }

        public WaitingForm()
        {
            lblMessage = new Label();
            lblRunningRequests = new Label();
            TimerWaiting = new Timer();
            Shape1 = new Panel();
            TimerWaiting.Interval = 1000;
            TimerWaiting.Tick += TimerWaiting_Tick;
            this.FormClosed += WaitingForm_FormClosed;
            lblMessage.AutoSize = true;
            lblMessage.Location = new Point(10, 10);
            lblRunningRequests.AutoSize = true;
            lblRunningRequests.Location = new Point(10, 40);
            Shape1.Size = new Size(100, 10);
            Shape1.Location = new Point(10, 70);
            Shape1.BackColor = Color.Black;
            this.Controls.Add(lblMessage);
            this.Controls.Add(lblRunningRequests);
            this.Controls.Add(Shape1);
        }

        private void SetWaitingCount(int value)
        {
            FWaitingCount = Math.Max(0, value);
            if (FWaitingCount == 0)
            {
                TimerWaiting.Enabled = false;
                this.Hide();
                Cursor = Cursors.Default;
            }
            else
            {
                if (!this.Visible)
                {
                    Cursor = Cursors.WaitCursor;
                    fPoints = 0;
                    TimerWaiting.Enabled = true;
                    this.Show();
                }
                lblRunningRequests.Text = FWaitingCount.ToString() + " running request" + (FWaitingCount > 1 ? "s" : "");
                lblRunningRequests.Refresh();
            }
        }

        public void IncreaseWaitingCount()
        {
            WaitingCount = WaitingCount + 1;
        }

        public void DecreaseWaitingCount()
        {
            WaitingCount = WaitingCount - 1;
        }

        private void TimerWaiting_Tick(object sender, EventArgs e)
        {
            if (fPoints == 3)
                fPoints = 0;
            else
                fPoints++;
            lblMessage.Text = "Please wait" + new string('.', fPoints);
        }

        private void WaitingForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Cursor = Cursors.Default;
        }
    }
}
