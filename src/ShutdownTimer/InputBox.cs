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
            // 用查表覆盖 Designer 里的硬编码中文。titleLabel / messageLabel 不在此列：
            // 它们紧接着就被调用方传进来的 Title / Message 整体覆盖，Designer 里的
            // “标题” / “描述” 只是设计时占位，运行时永远显示不出来。
            ApplyTexts();

            Text = Strings.T("Shutdown Timer - ") + Title;
            titleLabel.Text = Title;
            messageLabel.Text = Message;
            if (PasswordMode) { inputTextBox.PasswordChar = Convert.ToChar("*"); }

            // 按当前语言统一应用字体，替换掉 Designer 里硬编码的微软雅黑。
            Localization.ApplyTo(this);
        }

        /// <summary>
        /// 用 <see cref="Strings"/> 查表覆盖 <c>InputBox.Designer.cs</c> 里硬编码的中文文案。
        ///
        /// 为什么不在 Designer 里直接改：<c>*.Designer.cs</c> 是 VS 自动生成的，
        /// 手工写进去的改动在下次打开设计器、拖动控件时会连同 InitializeComponent()
        /// 一起被重新生成覆盖掉。Designer 里的中文因此原样保留（开发者在设计器里
        /// 看到的仍是中文），运行时由本方法用同一份查表结果覆盖，界面行为零变化。
        ///
        /// key 一律是上游的英文原文，与 <c>upstream/master</c> 的 Designer 逐字一致。
        /// </summary>
        private void ApplyTexts()
        {
            okButton.Text = Strings.T("OK");
            cancelButton.Text = Strings.T("Cancel");
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
