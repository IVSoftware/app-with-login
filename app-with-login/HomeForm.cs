using System;
using System.Windows.Forms;

namespace app_with_login
{
    public partial class HomeForm : Form
    {
        public HomeForm()
        {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterScreen;
        }
        public bool IsLoggedIn { get; private set; }
        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (Visible && !IsLoggedIn)
            {
                // Don't block the visible event.
                BeginInvoke(new Action(() =>
                {
                    execLoginFlow();
                }));
            }
        }
        private void execLoginFlow()
        {
            Visible = false;
            while (!IsLoggedIn)
            {
                using (var loginForm = new LoginForm())
                {
                    loginForm.Size = Size;
                    loginForm.Location = Location;

                    if (DialogResult.Cancel == loginForm.ShowDialog(this))
                    {
                        switch (MessageBox.Show(
                            this,
                            "Invalid Credentials",
                            "Error",
                            buttons: MessageBoxButtons.RetryCancel))
                        {
                            case DialogResult.Cancel: Application.Exit(); return;
                            case DialogResult.Retry: break;
                        }
                    }
                    else
                    {
                        WindowState = FormWindowState.Maximized;
                        IsLoggedIn = true;
                        Text = $"Welcome {loginForm.UserName}";
                        Visible = true;
                    }
                }
            }
        }
    }
}
