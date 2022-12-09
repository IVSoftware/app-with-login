Your [post](https://stackoverflow.com/q/74735590/5438626) states that your objective is to show the login form _first_, requiring user credentials _before showing the main form_. One way to do that is to intercept the first `VisibleChanged` event of the application main window which is `HomeForm`. Since the user isn't logged in, `HomeForm` will hide and the `LoginForm` will be superimposed on the rectangle it leaves behind. Exit the app if login is cancelled (or if user validation fails of course). With a valid login, the app proceeds with _HomeForm_ as the main application window as it should be.

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

Disposing windows is important and failure to do so can cause the kind of exit hang you describe. For this reason, and "_because a form displayed as a dialog box is hidden instead of closed_" (per [Microsoft](https://learn.microsoft.com/en-us/dotnet/api/system.windows.forms.form.showdialog?view=windowsdesktop-7.0)) the `using` block ensures the popup window will properly dispose. This should help avoid any issues with exiting the app.


    public partial class LoginForm : Form
    {
        public LoginForm()
        {
            InitializeComponent();
            StartPosition = FormStartPosition.Manual;
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
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
    }

![login flow](https://github.com/IVSoftware/app-with-login/blob/master/app-with-login/Screenshots/screenshot.png?raw=true)

