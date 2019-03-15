using System;
using System.Collections.Generic;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using AlertDialog = Android.Support.V7.App.AlertDialog;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace PWaplication
{
    [Activity(Label = "MainActivity", Theme = "@style/AppTheme", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class MainActivity : AppCompatActivity, AppBarLayout.IOnOffsetChangedListener
    {
        private const float PERCENTAGE_TO_SHOW_TITLE_AT_TOOLBAR = 0.9f;
        private const float PERCENTAGE_TO_HIDE_TITLE_DETAILS = 0.3f;
        private const int ALPHA_ANIMATIONS_DURATION = 200;

        public const int REQUEST_CODE_PAY = 101;
        private TextView username;
        private TextView email;
        private TextView balance;
        private LinearLayout titleContainer;
        private AppBarLayout appBar;
        private Toolbar toolbar;
        private TextView toolbarLable;
        private FloatingActionButton buttonSend;
        private RecyclerView historylist;
        private HistoryAdapter adapter;
        private bool isTheTitleContainerVisible = true;
        private bool isTheTitleVisible = false;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            username = FindViewById<TextView>(Resource.Id.userName);
            email = FindViewById<TextView>(Resource.Id.email);
            balance = FindViewById<TextView>(Resource.Id.balance);
            titleContainer = FindViewById<LinearLayout>(Resource.Id.main_linearlayout_title);
            appBar = FindViewById<AppBarLayout>(Resource.Id.app_bar);
            toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            toolbarLable = FindViewById<TextView>(Resource.Id.toolbarLable);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayShowTitleEnabled(false);

            appBar.AddOnOffsetChangedListener(this);
            startAlphaAnimation(toolbarLable, 0, ViewStates.Invisible);

            historylist = FindViewById<RecyclerView>(Resource.Id.histotyList);
            adapter = new HistoryAdapter(new List<Transaction>());
            historylist.SetAdapter(adapter);
            adapter.ItemClick += (sender, item) =>
             {
                 var editText = new EditText(this);
                 editText.InputType = Android.Text.InputTypes.ClassNumber;
                 int sum = 0;
                 if (int.TryParse(item.amount, out sum))
                 {
                     sum = Math.Abs(sum);
                 }
                 editText.Text = sum.ToString();
                 AlertDialog.Builder alert = new AlertDialog.Builder(this, Resource.Style.Dialog);
                 alert.SetTitle(Resource.String.repeat_dialog_title)
                     .SetView(editText)
                     .SetMessage(GetString(Resource.String.repeat_msg_format, item.username))
                 .SetPositiveButton(Resource.String.btn_send, (s, e) =>
                 {
                     PWAPI.PayTransaction(item.username, editText.Text, TransacrionComplete);
                 })
                 .SetNegativeButton(Resource.String.btn_cancel, (s, e) => { });

                 alert.Show();
             };
            buttonSend = FindViewById<FloatingActionButton>(Resource.Id.fab);
            buttonSend.Click += (sender, e) =>
            {
                StartPay();
            };
        }

        protected override void OnResume()
        {
            base.OnResume();
            PWAPI.GetUserInfo(FillHeader);
            PWAPI.GetListTransaction(FillHistoryList);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_send, menu);
            menu.FindItem(Resource.Id.action_send).SetVisible(isTheTitleVisible);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == Resource.Id.action_send)
            {
                StartPay();
            }
            return base.OnOptionsItemSelected(item);
        }

        private void StartPay()
        {
            Intent intent = new Intent(this, typeof(PayActivity));
            StartActivity(intent);
        }

        private void TransacrionComplete(Transaction t, string erroorMessage)
        {
            if (string.IsNullOrEmpty(erroorMessage))
            {
                balance.Text = GetString(Resource.String.balance_format, t.balance);
                PWAPI.GetListTransaction(FillHistoryList);
                balance.Text = GetString(Resource.String.balance_format, t.balance);
                AlertDialog.Builder alert = new AlertDialog.Builder(this, Resource.Style.Dialog)
                    .SetTitle(Resource.String.pay_success)
                    .SetMessage(GetString(Resource.String.pay_success_format, t.username, t.amount, t.balance))
                    .SetPositiveButton(Resource.String.btn_ok, (sender, e) => { });
                alert.Show();
            }
            else
            {
                AlertDialog.Builder alert = new AlertDialog.Builder(this, Resource.Style.Dialog);
                alert.SetTitle(Resource.String.str_error).SetMessage(erroorMessage)
                .SetPositiveButton(Resource.String.btn_ok, (s, e) => { });
                alert.Show();
            }
        }

        private void FillHeader(UserInfo user, string errorMessage)
        {
            if (string.IsNullOrEmpty(errorMessage))
            {
                username.Text = user.name;
                email.Text = user.email;
                balance.Text = GetString(Resource.String.balance_format, user.balance);
                toolbarLable.Text = username.Text + " " + balance.Text;
            }
            else
            {
                ShowErrorMessage(errorMessage);
            }
        }

        private void FillHistoryList(List<Transaction> list, string errorMessage)
        {
            if (string.IsNullOrEmpty(errorMessage))
            {
                adapter.TransactionList = list;
                adapter.NotifyDataSetChanged();
            }
            else
            {
                ShowErrorMessage(errorMessage);
            }
        }

        private void ShowErrorMessage(string msg)
        {
            AlertDialog.Builder alert = new AlertDialog.Builder(this, Resource.Style.Dialog);
            alert.SetTitle(Resource.String.str_error).SetMessage(msg)
            .SetPositiveButton(Resource.String.btn_ok, (s, e) => { });
            alert.Show();
        }

        public void OnOffsetChanged(AppBarLayout appBarLayout, int verticalOffset)
        {
            int maxScroll = appBarLayout.TotalScrollRange;
            var rect = (float)Math.Abs(verticalOffset) / (float)maxScroll;

            HandleAlphaOnTitle(rect);
            HandleToolbarTitleVisibility(rect);
        }

        private void HandleAlphaOnTitle(float rect)
        {
            if (rect >= PERCENTAGE_TO_HIDE_TITLE_DETAILS)
            {
                if (isTheTitleContainerVisible)
                {
                    startAlphaAnimation(titleContainer, ALPHA_ANIMATIONS_DURATION, ViewStates.Invisible);
                    isTheTitleContainerVisible = false;
                }
            }
            else
            {
                if (!isTheTitleContainerVisible)
                {
                    startAlphaAnimation(titleContainer, ALPHA_ANIMATIONS_DURATION, ViewStates.Visible);
                    isTheTitleContainerVisible = true;
                }
            }
        }

        private void HandleToolbarTitleVisibility(float percentage)
        {
            if (percentage >= PERCENTAGE_TO_SHOW_TITLE_AT_TOOLBAR)
            {
                if (!isTheTitleVisible)
                {
                    startAlphaAnimation(toolbarLable, ALPHA_ANIMATIONS_DURATION, ViewStates.Visible);
                    isTheTitleVisible = true;
                    InvalidateOptionsMenu();
                }
            }
            else
            {
                if (isTheTitleVisible)
                {
                    startAlphaAnimation(toolbarLable, ALPHA_ANIMATIONS_DURATION, ViewStates.Invisible);
                    isTheTitleVisible = false;
                    InvalidateOptionsMenu();
                }
            }
        }

        private void startAlphaAnimation(View v, long duration, ViewStates visibility)
        {
            AlphaAnimation alphaAnimation = (visibility == ViewStates.Visible)
                    ? new AlphaAnimation(0f, 1f)
                    : new AlphaAnimation(1f, 0f);

            alphaAnimation.Duration = duration;
            alphaAnimation.FillAfter = true;
            v.StartAnimation(alphaAnimation);
        }

        private class HistoryItemHolder : RecyclerView.ViewHolder
        {
            public TextView date;
            public TextView user;
            public TextView amount;
            public TextView balance;

            public HistoryItemHolder(View itemView, Action<int> listener) : base(itemView)
            {
                date = itemView.FindViewById<TextView>(Resource.Id.card_date);
                user = itemView.FindViewById<TextView>(Resource.Id.card_user);
                amount = itemView.FindViewById<TextView>(Resource.Id.card_amount);
                balance = itemView.FindViewById<TextView>(Resource.Id.card_balance);
                itemView.Click += (sender, e) => listener(AdapterPosition);
            }
        }

        private class HistoryAdapter : RecyclerView.Adapter
        {
            public List<Transaction> TransactionList { get; set; }
            public event EventHandler<Transaction> ItemClick;

            public override int ItemCount => TransactionList != null ?
                TransactionList.Count : 0;

            public HistoryAdapter(List<Transaction> list)
            {
                TransactionList = list;
            }

            public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            {
                var vh = holder as HistoryItemHolder;
                vh.date.Text = TransactionList[position].date;
                vh.user.Text = TransactionList[position].username;
                vh.amount.Text = TransactionList[position].amount;
                vh.balance.Text = TransactionList[position].balance;
                int amount = 0;
                if (int.TryParse(TransactionList[position].amount, out amount))
                {
                    vh.amount.SetTextColor(amount < 0 ? Color.Red : Color.Green);
                }
            }

            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {
                View itemView = LayoutInflater.From(parent.Context).
                    Inflate(Resource.Layout.card_history_item, parent, false);
                var vh = new HistoryItemHolder(itemView, OnClick);
                return vh;
            }

            private void OnClick(int pos)
            {
                ItemClick?.Invoke(this, TransactionList[pos]);
            }
        }
    }
}