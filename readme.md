Your [post](https://stackoverflow.com/q/74735590/5438626) states that your objective is to show the login form _first_, requiring user credentials _before showing the main form_. One way to achieve this is to force the creation of the main window handle while also preventing it from becoming visible by overriding `SetVisibleCore` until the user succeeds in logging in.  Exit the app if login is cancelled (or if user validation fails of course). With a valid login, the app proceeds with _HomeForm_ as the main application window as it should be.

    public partial class HomeForm : Form
    {
        public HomeForm()
        {
            InitializeComponent();
            // Ordinarily we don't get the handle until
            // window is shown. But we want it now.
            _ = Handle;
            // Call BeginInvoke on the new handle so as not to block the CTor.
            BeginInvoke(new Action(()=> execLoginFlow()));
            // Ensure final disposal of login form. Failure to properly dispose of window 
            // handles is the leading cause of the kind of exit hang you describe.
            Disposed += (sender, e) => _loginForm.Dispose();
            buttonSignOut.Click += (sender, e) => IsLoggedIn = false;
        }
        private LoginForm _loginForm = new LoginForm();
        protected override void SetVisibleCore(bool value) =>
            base.SetVisibleCore(value && IsLoggedIn);

        bool _isLoggedIn = false;
        public bool IsLoggedIn
        {
            get => _isLoggedIn;
            set
            {
                if (!Equals(_isLoggedIn, value))
                {
                    _isLoggedIn = value;
                    onIsLoggedInChanged();
                }
            }
        }

        private void onIsLoggedInChanged()
        {
            if (IsLoggedIn)
            {
                WindowState = FormWindowState.Maximized;
                Text = $"Welcome {_loginForm.UserName}";
                Visible = true;
            }
            else execLoginFlow();
        }

        private void execLoginFlow()
        {
            Visible = false;
            while (!IsLoggedIn)
            {
                _loginForm.StartPosition = FormStartPosition.CenterScreen;
                if (DialogResult.Cancel == _loginForm.ShowDialog(this))
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
                    IsLoggedIn = true;
                }
            }
        }
    }

***
**Login form**

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

[![login flow][1]][1]


  [1]: https://i.stack.imgur.com/4XLKv.png