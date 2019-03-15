using Android.App;
using Android.Widget;
using Android.OS;
using Android.Support.V7.App;
using Android.Content;
using System;
using Android.Preferences;
using AlertDialog = Android.Support.V7.App.AlertDialog;
using Android.Support.Design.Widget;
using Android.Views.InputMethods;

namespace PWaplication
{
    [Activity(Label = "@string/app_name", Theme = "@style/SplashTheme", MainLauncher = true,
        WindowSoftInputMode = Android.Views.SoftInput.AdjustPan, ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class LoginActivity : AppCompatActivity
    {
        private int MIN_LENGH_TEXT = 3;
        private TextView loginLable;
        private EditText userName;
        private EditText userEmail;
        private EditText password;
        private EditText confirmPassword;
        private Button loginButton;
        private TextView logoutText;
        private TextInputLayout confirmLayout;
        private LoginResiver loginResiver;
        private string savedName;
        private string savedEmail;
        private ProgressDialog progressDialog;


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.activity_login);
            loginLable = FindViewById<TextView>(Resource.Id.main_lable);
            userName = FindViewById<EditText>(Resource.Id.userName);
            userEmail = FindViewById<EditText>(Resource.Id.userEmail);
            password = FindViewById<EditText>(Resource.Id.userPassword);
            confirmPassword = FindViewById<EditText>(Resource.Id.confirmPassword);
            confirmLayout = FindViewById<TextInputLayout>(Resource.Id.confirmLayout);
            loginButton = FindViewById<Button>(Resource.Id.loginButton);
            logoutText = FindViewById<TextView>(Resource.Id.logoutButton);

            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(this);
            savedName = prefs.GetString(Config.KEY_USERNAME, string.Empty);
            savedEmail = prefs.GetString(Config.KEY_EMAIL, string.Empty);

            progressDialog = new ProgressDialog(this);
            progressDialog.SetProgressStyle(ProgressDialogStyle.Spinner);
            progressDialog.SetMessage(GetString(Resource.String.str_loading));

            FillLayout(false, savedEmail);


            loginResiver = new LoginResiver();
            loginResiver.Resive += (status) =>
            {
                progressDialog.Dismiss();
                if (string.IsNullOrEmpty(status))
                {
                    ISharedPreferencesEditor editor = prefs.Edit();
                    editor.PutString(Config.KEY_USERNAME, userName.Text);
                    editor.PutString(Config.KEY_EMAIL, userEmail.Text);
                    editor.Apply();
                    Intent intent = new Intent(this, typeof(MainActivity));
                    intent.SetFlags(ActivityFlags.NewTask | ActivityFlags.ClearTask);
                    StartActivity(intent);
                }
                else
                {
                    AlertDialog.Builder alert = new AlertDialog.Builder(this);
                    alert.SetTitle(Resource.String.str_error).SetMessage(status)
                    .SetPositiveButton(Resource.String.btn_ok, (s, e) => { });
                    alert.Create().Show();
                    loginButton.Enabled = true;
                }
            };
            IntentFilter filter = new IntentFilter(LoginService.RESPONSE_BROADCAST_FILTER);
            RegisterReceiver(loginResiver, filter);

            userName.TextChanged += TextChanged;
            userEmail.TextChanged += TextChanged;
            password.TextChanged += TextChanged;
            confirmPassword.TextChanged += TextChanged;
            loginButton.Click += (sender, e) =>
            {

                HideKeyboard();
                progressDialog.Show();
                string name = string.IsNullOrEmpty(userName.Text) ? null : userName.Text;
                LoginService.PerformAsyncLogin(this, name, password.Text, userEmail.Text);
                loginButton.Enabled = false;

            };
            logoutText.Click += (sender, e) =>
            {
                if (loginButton.Text == GetString(Resource.String.str_login))
                {
                    FillLayout(true, string.Empty);
                }
                else
                {
                    FillLayout(false, userEmail.Text);
                }
            };
        }

        protected override void OnDestroy()
        {
            UnregisterReceiver(loginResiver);
            base.OnDestroy();
        }
        private bool ValidateData()
        {
            var res = true;
            var usernameText = userName.Text;
            var emailText = userEmail.Text;
            var passwordText = password.Text;
            var confirmPasText = confirmPassword.Text;

            if (userName.Visibility == Android.Views.ViewStates.Visible)
            {
                if (usernameText.Length < MIN_LENGH_TEXT)
                {
                    userName.SetError(GetString(Resource.String.Error_login_lenght), null);
                    res = false;
                }
                if (passwordText != confirmPasText)
                {
                    confirmPassword.SetError(GetString(Resource.String.Error_login_pass), null);
                    res = false;
                }
            }

            if (!Android.Util.Patterns.EmailAddress.Matcher(emailText).Matches())
            {
                userEmail.SetError(GetString(Resource.String.Error_login_email), null);
                res = false;
            }

            if (passwordText.Length < MIN_LENGH_TEXT)
            {
                password.SetError(GetString(Resource.String.Error_login_lenght), null);
                res = false;
            }

            return res;
        }


        private void TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            loginButton.Enabled = ValidateData();
        }

        private void FillLayout(bool isRegistration, string email)
        {
            HideKeyboard();
            if (isRegistration)
            {
                loginLable.Text = GetString(Resource.String.str_registration);
                userEmail.Text = email;
                password.Text = string.Empty;
                userName.Visibility = Android.Views.ViewStates.Visible;
                confirmLayout.Visibility = Android.Views.ViewStates.Visible;
                logoutText.Text = GetString(Resource.String.str_login);
                loginButton.Text = GetString(Resource.String.str_registration);
            }
            else
            {
                userName.Text = string.Empty;
                loginLable.Text = GetString(Resource.String.str_login);
                userName.Visibility = Android.Views.ViewStates.Gone;
                confirmLayout.Visibility = Android.Views.ViewStates.Gone;
                logoutText.Text = GetString(Resource.String.str_logout);
                userEmail.Text = email;
                loginButton.Text = GetString(Resource.String.str_login);
            }

        }

        private void HideKeyboard()
        {
            InputMethodManager imm = GetSystemService(Activity.InputMethodService) as InputMethodManager;
            var view = CurrentFocus;
            if (view == null)
            {
                view = new Android.Views.View(this);
            }
            imm.HideSoftInputFromWindow(view.WindowToken, 0);
        }

        private class LoginResiver : BroadcastReceiver
        {
            public Action<string> Resive;
            public override void OnReceive(Context context, Intent intent)
            {
                var status = intent.GetStringExtra(LoginService.EXTR_STATUS);
                Resive.Invoke(status);
            }
        }
    }
}

