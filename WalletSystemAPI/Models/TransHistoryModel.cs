namespace WalletSystemAPI.Models
{
    public class TransHistoryModel
    {
        public string TransType { get; set; }
        public long AccntNumber { get; set; }
        public decimal Amount { get; set; }
        public long AccntFrom { get; set; }
        public long AccntTo { get; set; }
        public decimal EndBalance { get; set; }
        public DateTime TransDate { get; set; }
    }
}
