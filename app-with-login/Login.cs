using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace app_with_login
{
    public partial class LoginForm : Form
    {
        public LoginForm()
        {
            InitializeComponent();
            StartPosition = FormStartPosition.Manual;
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            textBoxUid.Text = "Admin";
            textBoxUid.TextChanged += onOnyTextChanged;
            textBoxPswd.TextChanged += onOnyTextChanged;
            buttonLogin.Enabled = false;
            buttonLogin.Click += (sender, e) => DialogResult = DialogResult.OK;
        }
        private void onOnyTextChanged(object sender, EventArgs e)
        {
            buttonLogin.Enabled = !(
                (textBoxUid.Text.Length == 0) || 
                (textBoxPswd.Text.Length == 0)
            );
        }
        public string UserName => textBoxUid.Text;
        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (Visible)
            {
                textBoxPswd.Clear();
                textBoxPswd.PlaceholderText = "********";
            }
        }
    }
}
