namespace WalletSystemAPI.Models
{
    public class AccountDetailsModel
    {
        public string LoginName { get; set; }
        public string UserPassword { get; set; }
        public long AccountNumber { get; set; }
        public DateTime RegDate { get; set; }
        public decimal UserBalance { get; set; }
    }
}
