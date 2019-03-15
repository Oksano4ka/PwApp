using System;
using System.Collections.Generic;
using Android.App;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using AlertDialog = Android.Support.V7.App.AlertDialog;
using SearchView = Android.Support.V7.Widget.SearchView;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace PWaplication
{
    [Activity(Label = "PayActivity", Theme = "@style/AppTheme",
        ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class PayActivity : AppCompatActivity
    {
        private SearchView searchView;
        private EmptyRecyclerView userList;
        private Toolbar toolbar;
        private TextView balance;
        private UserListAdapter adapter;
        AppBarLayout appBar;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.activity_pay);
            userList = FindViewById<EmptyRecyclerView>(Resource.Id.userList);
            userList.SetEmptView(FindViewById<EmptyView>(Resource.Id.empty_view));
            userList.ShowLoading(EmptyView.StyleEmptyView.ONLY_TEXT, Resource.String.str_default_search);
            toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            appBar = FindViewById<AppBarLayout>(Resource.Id.app_bar);
            balance = FindViewById<TextView>(Resource.Id.balance);
            SetSupportActionBar(toolbar);
            SupportActionBar.SetTitle(Resource.String.pay_activity_title);
            adapter = new UserListAdapter(new List<Users>());
            adapter.ItemClick += (sender, pos) =>
                CreatePayDialog(adapter.UsersList[pos].name);
            userList.SetAdapter(adapter);
            DividerItemDecoration dividerItemDecoration = new DividerItemDecoration(userList.Context, LinearLayoutManager.Vertical);
            dividerItemDecoration.SetDrawable(GetDrawable(Resource.Drawable.divider));
            userList.AddItemDecoration(dividerItemDecoration);
            PWAPI.GetUserInfo(FillHeader);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_search, menu);
            var searchItem = menu.FindItem(Resource.Id.action_search);
            searchView = searchItem.ActionView as SearchView;
            searchView.FocusChange += (sender, e) =>
            {
                if (e.HasFocus)
                {
                    appBar.SetExpanded(false);
                }
            };
            searchView.QueryTextSubmit += (sender, args) =>
            {
                userList.ShowLoading(EmptyView.StyleEmptyView.TEXT_AND_PROGRESS, Resource.String.str_search);
                PWAPI.GetFilteredUserList(args.Query, FillList);
            };
            return true;
        }

        private void FillHeader(UserInfo user, string errorMessage)
        {
            if (string.IsNullOrEmpty(errorMessage))
            {
                balance.Text = GetString(Resource.String.balance_format, user.balance);
            }
            else
            {
                ShowErrorMessage(errorMessage);
            }
        }
        private void FillList(List<Users> users, string errorMessage)
        {
            if (string.IsNullOrEmpty(errorMessage))
            {
                adapter.UsersList = users;
                adapter.NotifyDataSetChanged();
            }
            else
            {
                ShowErrorMessage(errorMessage);
            }
        }

        private void CreatePayDialog(string name)
        {
            var editText = new EditText(this);
            editText.InputType = Android.Text.InputTypes.ClassNumber;
            AlertDialog.Builder alert = new AlertDialog.Builder(this, Resource.Style.Dialog);
            alert.SetTitle(Resource.String.pay_dialog_title)
                .SetView(editText)
            .SetPositiveButton(Resource.String.btn_send, (s, e) =>
            {
                PWAPI.PayTransaction(name, editText.Text, TransacrionComplete);
            })
            .SetNegativeButton(Resource.String.btn_cancel, (s, e) => { });
            alert.Show();
        }

        private void TransacrionComplete(Transaction t, string erroorMessage)
        {
            if (string.IsNullOrEmpty(erroorMessage))
            {
                balance.Text = GetString(Resource.String.balance_format, t.balance);
                AlertDialog.Builder alert = new AlertDialog.Builder(this, Resource.Style.Dialog)
                    .SetTitle(Resource.String.pay_success)
                    .SetMessage(GetString(Resource.String.pay_success_format, t.username, t.amount, t.balance))
                    .SetPositiveButton(Resource.String.btn_ok, (sender, e) => { });
                alert.Show();
            }
            else
            {
                ShowErrorMessage(erroorMessage);
            }
        }

        private void ShowErrorMessage(string msg)
        {
            AlertDialog.Builder alert = new AlertDialog.Builder(this, Resource.Style.Dialog);
            alert.SetTitle(Resource.String.str_error).SetMessage(msg)
            .SetPositiveButton(Resource.String.btn_ok, (s, e) => { });
            alert.Show();

            if (adapter.ItemCount == 0)
            {
                userList.ShowLoading(EmptyView.StyleEmptyView.ONLY_TEXT, Resource.String.str_default_search);
            }
        }

        private class UsersHolder : RecyclerView.ViewHolder
        {
            public TextView name;
            public TextView id;

            public UsersHolder(View itemView, Action<int> listener) : base(itemView)
            {
                name = itemView.FindViewById<TextView>(Resource.Id.userName);
                id = itemView.FindViewById<TextView>(Resource.Id.userId);
                itemView.Click += (sender, e) => listener(AdapterPosition);
            }
        }

        private class UserListAdapter : RecyclerView.Adapter
        {
            public List<Users> UsersList { get; set; }
            public event EventHandler<int> ItemClick;

            public UserListAdapter(List<Users> list)
            {
                UsersList = list;
            }

            public override int ItemCount => UsersList.Count;

            public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            {
                UsersHolder vh = holder as UsersHolder;
                vh.name.Text = UsersList[position].name;
                vh.id.Text = UsersList[position].id;
            }

            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {
                View itemView = LayoutInflater.From(parent.Context).
                    Inflate(Resource.Layout.user_item, parent, false);
                UsersHolder vh = new UsersHolder(itemView, OnClick);
                return vh;
            }

            private void OnClick(int pos)
            {
                ItemClick?.Invoke(this, pos);
            }
        }
    }
}