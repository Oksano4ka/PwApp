using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PWaplication
{
    public class PWAPI
    {
        class ApiKey
        {
            public string id_token { get; set; }
        }

        public const string BASE_URL = "http://193.124.114.46:3001/";
        public const string POST_LOGIN_CREATE = "users";
        public const string POST_LOGIN = "sessions/create";
        public const string POST_TRANSACTION = "api/protected/transactions";
        public const string GET_USER_INFO = "api/protected/user-info";
        public const string GET_USER_TRANSACTION = "api/protected/transactions";
        public const string GET_USERS_LIST = "api/protected/users/list";
        public const string FIELD_USERNAME = "username";
        public const string FIELD_PASSWORD = "password";
        public const string FIELD_EMAIL = "email";
        public const string FIELD_FILTER = "filter";
        public const string FIELD_NAME = "name";
        public const string FIELD_AMOUNT = "amount";
        public const string AUTH = "bearer";

        public static string mApiKey { get; private set; }

        public static async Task<string> LoginCreate(string username, string password, string email)
        {

            var values = new Dictionary<string, string>
                {
                   { FIELD_USERNAME, username },
                   { FIELD_PASSWORD, password },
                   { FIELD_EMAIL, email}
                };
            var errorMessage = string.Empty;

            try
            {
                var content = new FormUrlEncodedContent(values);
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(BASE_URL + POST_LOGIN_CREATE);
                var response = await client.PostAsync(client.BaseAddress, content);

                if (response.StatusCode == HttpStatusCode.Created)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    var token = JsonConvert.DeserializeObject<ApiKey>(responseString);
                    mApiKey = token.id_token;
                }
                else
                {
                    errorMessage = await response.Content.ReadAsStringAsync();
                }
            }
            catch (Exception e)
            {
                errorMessage = e.Message;
            }
            return errorMessage;
        }

        public static async Task<string> Login(string email, string password)
        {
            var values = new Dictionary<string, string>
                {
                   { FIELD_EMAIL, email },
                   { FIELD_PASSWORD, password }
                };
            var errorMessage = string.Empty;

            try
            {
                var content = new FormUrlEncodedContent(values);
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(BASE_URL + POST_LOGIN);
                var response = await client.PostAsync(client.BaseAddress, content);

                if (response.StatusCode == HttpStatusCode.Created)
                {
                    var responseString = await response.Content.ReadAsStringAsync();

                    var token = JsonConvert.DeserializeObject<ApiKey>(responseString);

                    mApiKey = token.id_token;
                }
                else
                {
                    errorMessage = await response.Content.ReadAsStringAsync();
                }
            }
            catch (Exception e)
            {
                errorMessage = e.Message;
            }
            return errorMessage;
        }

        public static async void GetUserInfo(Action<UserInfo, string> listener)
        {
            var errorMessage = string.Empty;
            UserInfo user = null;
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(BASE_URL + GET_USER_INFO);
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue(AUTH, mApiKey);
                var response = await client.GetAsync(client.BaseAddress);

                var responseString = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var info = JsonConvert.DeserializeObject<TokenInfo<UserInfo>>(responseString);
                    user = info.user_info_token;
                }
                else
                {
                    errorMessage = responseString;
                }
            }
            catch (Exception e)
            {
                errorMessage = e.Message;
            }
            listener.Invoke(user, errorMessage);
        }

        public static async void GetFilteredUserList(string filter, Action<List<Users>, string> listener)
        {
            var values = new Dictionary<string, string>
                {
                   { FIELD_FILTER, filter }
                };
            List<Users> users = null;
            var errorMessage = string.Empty;
            try
            {
                HttpClient client = new HttpClient();
                var content = new FormUrlEncodedContent(values);
                client.BaseAddress = new Uri(BASE_URL + GET_USERS_LIST);
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue(AUTH, mApiKey);
                var response = await client.PostAsync(client.BaseAddress, content);
                var responseString = await response.Content.ReadAsStringAsync();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    users = JsonConvert.DeserializeObject<List<Users>>(responseString);
                }
                else
                {
                    errorMessage = responseString;
                }
            }
            catch (Exception e)
            {
                errorMessage = e.Message;
            }
            listener.Invoke(users, errorMessage);
        }

        public static async void GetListTransaction(Action<List<Transaction>, string> listener)
        {
            var errorMessage = string.Empty;
            List<Transaction> list = null;
            try
            {
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(BASE_URL + GET_USER_TRANSACTION);
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue(AUTH, mApiKey);
                var response = await client.GetAsync(client.BaseAddress);
                var responseString = await response.Content.ReadAsStringAsync();

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var transactions = JsonConvert.DeserializeObject<TransactionInfo<List<Transaction>>>(responseString);
                    list = transactions.trans_token;
                    list.Reverse();
                }
                else
                {
                    errorMessage = responseString;
                }
            }
            catch (Exception e)
            {
                errorMessage = e.Message;
            }
            listener.Invoke(list, errorMessage);
        }

        public static async void PayTransaction(string name, string amount, Action<Transaction, string> listener)
        {
            var values = new Dictionary<string, string>
                {
                   { FIELD_NAME, name },
                   { FIELD_AMOUNT, amount }
                };
            var errorMessage = string.Empty;
            Transaction transaction = null;
            try
            {
                var content = new FormUrlEncodedContent(values);
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(BASE_URL + POST_TRANSACTION);
                client.DefaultRequestHeaders.Authorization =
                   new System.Net.Http.Headers.AuthenticationHeaderValue(AUTH, mApiKey);
                var response = await client.PostAsync(client.BaseAddress, content);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    var transactionInfo = JsonConvert.DeserializeObject<TransactionInfo<Transaction>>(responseString);
                    transaction = transactionInfo.trans_token;
                }
                else
                {
                    errorMessage = await response.Content.ReadAsStringAsync();
                }
            }
            catch (Exception e)
            {
                errorMessage = e.Message;
            }
            listener.Invoke(transaction, errorMessage);
        }

    }
}