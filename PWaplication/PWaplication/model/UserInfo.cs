namespace PWaplication
{
    public class TokenInfo<T>
    {
        public T user_info_token;
    }

    public class UserInfo
    {
        public string id;
        public string name;
        public string email;
        public string balance;
    }

    public class Users
    {
        public string id;
        public string name;
    }
    public class TransactionInfo<T>
    {
        public T trans_token;
    }

    public class Transaction
    {
        public string id;
        public string date;
        public string username;
        public string amount;
        public string balance;
    }
}