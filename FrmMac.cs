using MacTool.Mac;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace MacTool
{
    public partial class FrmMac : Form
    {
        [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
        static extern IntPtr FindWindowByCaption(IntPtr ZeroOnly, string lpWindowName);

        [DllImport("user32.Dll")]
        static extern int PostMessage(IntPtr hWnd, UInt32 msg, int wParam, int lParam);
        private string CAPTION = "MAC解绑";
        public FrmMac()
        {
            InitializeComponent();
            this.Load += (sender, e) =>
            {
                this.txtUser.Text = AppSetting.User;
                this.txtPwd.Text = AppSetting.Pwd;
                this.txtFile.Text = AppSetting.FilePath;
                this.txtInterval.Text = AppSetting.Interval;
                this.txtHour.Text = AppSetting.Hour;
                this.chkTimer.Checked = AppSetting.IsTimerEnabled;
            };
            this.FormClosing += (sender, e) =>
            {
                if (null != macTool)
                {
                    macTool.Stop();
                }
            };


        }
        MacUnbind macTool = null;
        private void btnStart_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(FilePath))
            {
                MessageBox.Show("当前测试账号文件不能为空", CAPTION);
                return;
            }
            StartUnbind(this.FilePath, true);
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            string acount = this.txtTestAccount.Text.Trim();
            if (string.IsNullOrEmpty(acount))
            {
                MessageBox.Show("当前测试账号不能为空", CAPTION);
                return;
            }
            StartUnbind(acount, false);
        }


        private void StartUnbind(string account, bool isFile)
        {
            macTool = new MacUnbind(account, this.User, this.Pwd, isFile);
            int second = 0;
            if (!int.TryParse(this.Interval, out second))
            {
                second = 5;
            }
            macTool.Interval = second;
            macTool.OnChanged += MacTool_OnChanged;
            macTool.OnError += MacTool_OnError;

            DateTime startDT = DateTime.Now;
            Thread thread = new Thread(() =>
            {
                Tuple<int, int, int> tuple = macTool.Start();
                Action act = () =>
                {
                    DateTime endDt = DateTime.Now;
                    string interval = new DateTime(endDt.Subtract(startDT).Ticks).ToString("HH:mm:ss");
                    MessageBox.Show(string.Format("共处理:{0}, 成功:{1}, 失败:{2},用时:{3}", tuple.Item1, tuple.Item2, tuple.Item3, interval), CAPTION);
                };
                this.BeginInvoke(act);

            });
            thread.Start();
            SaveSetting();

            txtLog.Text = "";
            lbCount.Text = "0";
            StartTimer();
        }

        private void StopUnbind()
        {
            if (null != macTool)
            {
                macTool.Stop();
                macTool.OnChanged -= MacTool_OnChanged;
                macTool.OnError -= MacTool_OnError;
            }
            StopTimer();

        }

        private void MacTool_OnError(object sender, MacUnbindEventArg e)
        {
            if (e == null)
            {
                return;
            }

            Action act = () =>
            {
                txtLog.Text = "  " + e.Msg + "\r\n" + "  " +
                              e.Detail + "\r\n" + txtLog.Text;
                txtLog.Text = " 出异常，停止运行" + "\r\n" + txtLog.Text;
                macTool.Stop();
                macTool.OnChanged -= MacTool_OnChanged;
                macTool.OnError -= MacTool_OnError;
            };
            this.BeginInvoke(act);
        }

        int lineCount = 0;
        int account = 0;
        const int LineMax = 50;
        private void MacTool_OnChanged(object sender, MacUnbindEventArg e)
        {
            if (e == null)
            {
                return;
            }

            Action act = () =>
            {

                if (string.IsNullOrEmpty(txtLog.Text))
                {
                    txtLog.Text = "  " + e.Msg;
                }
                else
                {
                    txtLog.Text = "  " + e.Msg + "\r\n" + txtLog.Text;
                }
                if (e.Msg.Contains("解绑成功") || e.Msg.Contains("解绑失败"))
                {
                    account++;
                }

                lbCount.Text = account.ToString();
                if (lineCount++ / LineMax > 0)
                {
                    txtLog.Text = "  " + e.Msg;
                    lineCount = 0;
                }
            };
            this.BeginInvoke(act);
        }

        private string User
        {
            get { return this.txtUser.Text.Trim(); }
        }
        private string Pwd
        {
            get { return this.txtPwd.Text.Trim(); }
        }
        private string TestAccount
        {
            get { return this.txtTestAccount.Text.Trim(); }
        }

        private string FilePath
        {
            get { return this.txtFile.Text.Trim(); }
        }

        private string Interval
        {
            get { return this.txtInterval.Text.Trim(); }
        }

        private bool IsTimerEnabled
        {
            get { return this.chkTimer.Checked; }
        }

        private string Hour
        {
            get { return this.txtHour.Text; }
        }

        private void SaveSetting()
        {
            AppSetting.User = this.User;
            AppSetting.Pwd = this.Pwd;
            AppSetting.FilePath = this.FilePath;
            AppSetting.Interval = this.Interval;
            AppSetting.Hour = this.Hour;
            AppSetting.IsTimerEnabled = this.IsTimerEnabled;
            AppSetting.Save();
        }

        private void btnFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog openDlg = new OpenFileDialog();
            openDlg.AddExtension = true;
            openDlg.CheckFileExists = true;
            openDlg.DefaultExt = ".txt";
            openDlg.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            DialogResult dResult = openDlg.ShowDialog();
            if (dResult == DialogResult.OK)
            {
                this.txtFile.Text = openDlg.FileName;
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            StopUnbind();
        }

        System.Windows.Forms.Timer timer;
        int timerCount = 0;
        bool isTrigged = false;
        private void StartTimer()
        {
            if (!this.IsTimerEnabled)
            {
                return;
            }

            int hour = 22;
            int.TryParse(txtHour.Text.Trim(), out hour);
            hour = (hour < 0 ? 22 : hour) % 24;
            if (timer == null)
            {
                timer = new System.Windows.Forms.Timer();
                timer.Interval = 3 * 60 * 1000;
                timer.Tick += (sender, e) =>
                {
                    int newHour = DateTime.Now.Hour;
                    if (hour == newHour)
                    {
                        if ((null != macTool && macTool.IsRunning) ||
                             isTrigged)
                        {
                            return;
                        }
                        CloseMessageBox();
                        isTrigged = true;
                        btnStart_Click(null, null);
                        return;
                    }
                    isTrigged = false;
                    if (timerCount++ / LineMax > 0)
                    {
                        txtLog.Text = "";
                    }
                    txtLog.Text = string.Format("定时检测:{0} \r\n", DateTime.Now.ToString("yyyy-MM-dd HH: mm:ss")) + txtLog.Text;
                };
            }
            if (null != timer && !timer.Enabled)
            {
                timer.Start();
            }
        }


        private void StopTimer()
        {
            if (null != timer && timer.Enabled)
            {
                timer.Stop();
            }
            isTrigged = false;
        }

        private void chkTimer_CheckedChanged(object sender, EventArgs e)
        {
            txtHour.Enabled = chkTimer.Checked;
        }

        void CloseMessageBox()
        {
            const UInt32 WM_CLOSE = 0x0010;
            IntPtr hWnd = FindWindowByCaption(IntPtr.Zero, CAPTION);
            if (hWnd != IntPtr.Zero)
                PostMessage(hWnd, WM_CLOSE, 0, 0);
        }

    }
}
