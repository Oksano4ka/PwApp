using Android.App;
using Android.Content;

namespace PWaplication
{
    [Service(Exported = true, Enabled = true)]
    class LoginService : IntentService
    {
        public const string REG_ACTION = "REG_ACTION";
        public const string LOGIN_ACTION = "LOGIN_ACTION";
        public const string RESPONSE_BROADCAST_FILTER = "RESPONSE_BROADCAST_FILTER";
        public const string EXTR_USERNAME = "username";
        public const string EXTR_PASSWORD = "password";
        public const string EXTR_EMAIL = "email";
        public const string EXTR_STATUS = "status";
        public const string EXTR_STATUS_MESSAGE = "status_message";

        public LoginService() : base()
        {

        }

        public static void PerformAsyncLogin(Context context, string username, string password, string email)
        {
            Intent loginIntent = new Intent(context, typeof(LoginService));
            if (string.IsNullOrEmpty(username))
            {
                loginIntent.SetAction(LOGIN_ACTION);
            }
            else
            {
                loginIntent.SetAction(REG_ACTION);
                loginIntent.PutExtra(EXTR_USERNAME, username);
            }
            loginIntent.PutExtra(EXTR_EMAIL, email);
            loginIntent.PutExtra(EXTR_PASSWORD, password);
            context.StartService(loginIntent);
        }

        protected override void OnHandleIntent(Intent intent)
        {
            var password = intent.GetStringExtra(EXTR_PASSWORD);
            var email = intent.GetStringExtra(EXTR_EMAIL);
            var status = string.Empty;
            switch (intent.Action)
            {
                case REG_ACTION:
                    var username = intent.GetStringExtra(EXTR_USERNAME);
                    status = PWAPI.LoginCreate(username, password, email).Result;
                    break;
                case LOGIN_ACTION:
                    status = PWAPI.Login(email, password).Result;
                    break;
                default:
                    break;
            }
            Intent loginResponce = new Intent(RESPONSE_BROADCAST_FILTER);
            loginResponce.PutExtra(EXTR_STATUS, status);
            SendBroadcast(loginResponce);
        }
    }
}