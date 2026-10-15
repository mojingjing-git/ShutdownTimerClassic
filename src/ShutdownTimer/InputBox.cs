using ShutdownTimer.Helpers;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace ShutdownTimer
{
    public partial class InputBox : Form
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public bool PasswordMode { get; set; }

        public string ReturnValue { get; set; }

        public InputBox()
        {
            InitializeComponent();
        }

        private void InputBox_Load(object sender, EventArgs e)
        {
            Text = Strings.T("Shutdown Timer - ") + Title;
            titleLabel.Text = Title;
            messageLabel.Text = Message;
            if (PasswordMode) { inputTextBox.PasswordChar = Convert.ToChar("*"); }

            // 按当前语言统一应用字体，替换掉 Designer 里硬编码的微软雅黑。
            Localization.ApplyTo(this);
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            this.ReturnValue = inputTextBox.Text;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void inputTextBox_TextChanged(object sender, EventArgs e)
        {
            okButton.Enabled = !inputTextBox.Text.Equals("");
        }
    }
}
