using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using WalletSystemAPI.Models;
using System.Runtime.Serialization;
using System.Text;

namespace WalletSystemAPI.Controllers
{
    [ApiController]
    public class AccountDetailsController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public AccountDetailsController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost]
        [Route("RegisterAccountDetails")]
        public string SaveAccountDetails(AccountDetailsModel item)
        {
            try
            {
                SqlConnection cn = new SqlConnection(_configuration.GetConnectionString("WalletConnectionString"));
                cn.Open();
                SqlCommand cmd = new SqlCommand("ReadUserLogin", cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@loginname", item.LoginName);
                int result = Convert.ToInt32(cmd.ExecuteScalar());
                cmd.Dispose();
                cn.Dispose();
                if (result == 0)
                {
                    Random rnd = new Random();
                    double rndNumber = Convert.ToDouble(DateTime.Now.ToString("ddMMyyyyHH00")) + rnd.Next();

                    cn = new SqlConnection(_configuration.GetConnectionString("WalletConnectionString"));
                    cn.Open();
                    cmd = new SqlCommand("CreateDetails", cn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@loginname", item.LoginName);
                    byte[] pass = ASCIIEncoding.ASCII.GetBytes(item.UserPassword);
                    string password = Convert.ToBase64String(pass);
                    cmd.Parameters.AddWithValue("@pass", password);
                    cmd.Parameters.AddWithValue("@datereg", DateTime.Now);
                    cmd.Parameters.AddWithValue("@acntnum", rndNumber);
                    cmd.Parameters.AddWithValue("@userbal", 0);
                    int results = cmd.ExecuteNonQuery();
                    cmd.Dispose();
                    cn.Dispose();
                    if (results == 0)
                    {
                        return "Account Details Not Save!";
                    }
                    else
                    {
                        CreateTransaction("Account Created", rndNumber, 0, 0, 0, 0);
                        return "Account Details Save!";
                    }
                }
                else
                {
                    return "Login Name Already Exist!";
                }
            }
            catch (Exception ex)
            {
                return ex.Message.ToString();
            }        
        }
        [HttpPost]
        [Route("AccountDeposit")]
        public string Deposit(string login,string Pass,decimal total)
        {
            try
            {
                SqlConnection cn = new SqlConnection(_configuration.GetConnectionString("WalletConnectionString"));
                cn.Open();
                SqlCommand cmd = new SqlCommand("ReadLogin", cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@loginname", login);
                byte[] pass = ASCIIEncoding.ASCII.GetBytes(Pass);
                string password = Convert.ToBase64String(pass);
                cmd.Parameters.AddWithValue("@pass", password);

                SqlDataReader dr = cmd.ExecuteReader();

                long acntnum = 0;
                while (dr.Read())
                {
                    acntnum = Convert.ToInt64(dr["AccountNumber"]);
                }
                if(acntnum == 0)
                {
                    return "Invalid Login";
                }
                else
                {
                    try
                    {
                        cn = new SqlConnection(_configuration.GetConnectionString("WalletConnectionString"));
                        cn.Open();
                        cmd = new SqlCommand("ReadBalance", cn);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@acntnum", acntnum);
                        dr = cmd.ExecuteReader();
                        decimal balance = 0;

                        if (total != 0)
                        {
                            if (dr.Read())
                            {
                                balance = Convert.ToDecimal(dr["UserBalance"]);
                            }
                            else
                            {
                                return "Account Number Not Exist!";
                            }
                            decimal currenttotal = balance + total;
                            cn = new SqlConnection(_configuration.GetConnectionString("WalletConnectionString"));
                            cn.Open();
                            cmd = new SqlCommand("UpdateAddBalance", cn);
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@acntnum", acntnum);
                            cmd.Parameters.AddWithValue("@total", total);
                            int results = cmd.ExecuteNonQuery();
                            cmd.Dispose();
                            cn.Dispose();
                            if (results < 0)
                            {
                                return "Deposit not save!";
                            }
                            else
                            {
                                CreateTransaction("Account Deposit", acntnum, total, 0, 0, currenttotal);
                                return "Transaction Save! Your Current Balance is :" + currenttotal;
                            }
                        }
                        else
                        {
                            return "Invalid Total Deposit!";
                        }

                    }
                    catch (Exception ex)
                    {
                        return ex.Message.ToString();
                    }
                }
            
            }
            catch (Exception ex)
            {
                return ex.Message.ToString();
            }

        }

        [HttpPost]
        [Route("AccountWithdraw")]
        public string Withdraw(string login, string Pass, decimal total)
        {
            try
            {
                SqlConnection cn = new SqlConnection(_configuration.GetConnectionString("WalletConnectionString"));
                cn.Open();
                SqlCommand cmd = new SqlCommand("ReadLogin", cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@loginname", login);
                byte[] pass = ASCIIEncoding.ASCII.GetBytes(Pass);
                string password = Convert.ToBase64String(pass);
                cmd.Parameters.AddWithValue("@pass", password);

                SqlDataReader dr = cmd.ExecuteReader();

                long acntnum = 0;
                while (dr.Read())
                {
                    acntnum = Convert.ToInt64(dr["AccountNumber"]);
                }
                if (acntnum == 0)
                {
                    return "Invalid Login";
                }
                else
                {
                    try
                    {
                        cn = new SqlConnection(_configuration.GetConnectionString("WalletConnectionString"));
                        cn.Open();
                        cmd = new SqlCommand("ReadBalance", cn);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@acntnum", acntnum);
                        dr = cmd.ExecuteReader();
                        decimal balance = 0;
                        if (total != 0)
                        {
                            if (dr.Read())
                            {
                                balance = Convert.ToDecimal(dr["UserBalance"]);
                            }
                            else
                            {
                                return "Account Number Not Exist!";
                            }
                            if (balance >= total)
                            {
                                cn = new SqlConnection(_configuration.GetConnectionString("WalletConnectionString"));
                                cn.Open();
                                cmd = new SqlCommand("UpdateDeductBalance", cn);
                                cmd.CommandType = CommandType.StoredProcedure;
                                balance = balance - total;
                                cmd.Parameters.AddWithValue("@acntnum", acntnum);
                                cmd.Parameters.AddWithValue("@total", balance);

                                int results = cmd.ExecuteNonQuery();
                                cmd.Dispose();
                                cn.Dispose();
                                if (results < 0)
                                {
                                    return "Invalid Transaction!";
                                }
                                else
                                {
                                    CreateTransaction("Account Withdraw", acntnum, total, 0, 0, balance);
                                    return "Account Details Save! Your Current Balance is :" + balance;
                                }
                            }
                            else
                            {

                                return "Insufficient Balance! Your Current Balance is :" + balance;
                            }
                        }
                        else
                        {
                            return "Invalid Total Withdraw!";
                        }
                    }
                    catch (Exception ex)
                    {
                        return ex.Message.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                return ex.Message.ToString();
            }

        }
        [HttpPost]
        [Route("AccountTransferFund")]
           
        public string Transfer(string login, string Pass, decimal total, long toacntnum)
        {
            try
            {
                SqlConnection cn = new SqlConnection(_configuration.GetConnectionString("WalletConnectionString"));
                cn.Open();
                SqlCommand cmd = new SqlCommand("ReadLogin", cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@loginname", login);
                byte[] pass = ASCIIEncoding.ASCII.GetBytes(Pass);
                string password = Convert.ToBase64String(pass);
                cmd.Parameters.AddWithValue("@pass", password);

                SqlDataReader dr = cmd.ExecuteReader();

                long acntnum = 0;
                while (dr.Read())
                {
                    acntnum = Convert.ToInt64(dr["AccountNumber"]);
                }
                if (acntnum == 0)
                {
                    return "Invalid Login";
                }
                else
                {
                    if (acntnum == toacntnum)
                    {
                        return "Invalid Account Recipient!";
                    }
                    else
                    {
                        try
                        {
                            cn = new SqlConnection(_configuration.GetConnectionString("WalletConnectionString"));
                            cn.Open();
                            cmd = new SqlCommand("ReadBalance", cn);
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@acntnum", acntnum);
                            dr = cmd.ExecuteReader();
                            decimal balance = 0;
                            if (total != 0)
                            {
                                if (dr.Read())
                                {
                                    balance = Convert.ToDecimal(dr["UserBalance"]);
                                }
                                else
                                {
                                    return "Account Number Not Exist!";
                                }
                                if (balance >= total)
                                {
                                    cn = new SqlConnection(_configuration.GetConnectionString("WalletConnectionString"));
                                    cn.Open();
                                    cmd = new SqlCommand("ReadAccountNumber", cn);
                                    cmd.CommandType = CommandType.StoredProcedure;
                                    cmd.Parameters.AddWithValue("@acntnum", toacntnum);
                                    int result = (int)cmd.ExecuteScalar();
                                    if (result > 0)
                                    {
                                        cmd = new SqlCommand("UpdateAddBalance", cn);
                                        cmd.CommandType = CommandType.StoredProcedure;
                                        cmd.Parameters.AddWithValue("@acntnum", toacntnum);
                                        cmd.Parameters.AddWithValue("@total", total);
                                        int results = cmd.ExecuteNonQuery();
                                        if (results < 0)
                                        {
                                            return "Invalid Transaction!";
                                        }
                                        else
                                        {
                                            cmd = new SqlCommand("UpdateDeductBalance", cn);
                                            cmd.CommandType = CommandType.StoredProcedure;
                                            balance = balance - total;
                                            cmd.Parameters.AddWithValue("@acntnum", acntnum);
                                            cmd.Parameters.AddWithValue("@total", balance);
                                            results = cmd.ExecuteNonQuery();
                                            cmd.Dispose();
                                            cn.Dispose();
                                            if (results < 0)
                                            {
                                                return "Invalid Transaction!";
                                            }
                                            else
                                            {
                                                CreateTransaction("Transfer Fund", acntnum, total, acntnum, toacntnum, balance);
                                                return "Account Details Save! Your Current Balance is :" + balance;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        return "Account Recipient Number Not Exist!";
                                    }
                                }
                                else
                                {
                                    return "Insufficient Balance! Your Current Balance is :" + balance;
                                }
                            }
                            else
                            {
                                return "Invalid Total Transfer!";
                            }
                        }
                        catch (Exception ex)
                        {
                            return ex.Message.ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return ex.Message.ToString();
            }


        }
        [HttpGet]
        [Route("TransactionHistory")]
        public List<TransHistoryModel> TransHistory(string loginName, string Pass)
        {
            try
            {
                SqlConnection cn = new SqlConnection(_configuration.GetConnectionString("WalletConnectionString"));
                cn.Open();
                SqlCommand cmd = new SqlCommand("ReadTransaction", cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@login", loginName);
                byte[] pass = ASCIIEncoding.ASCII.GetBytes(Pass);
                string password = Convert.ToBase64String(pass);
                cmd.Parameters.AddWithValue("@pass", password);
                SqlDataReader dr = cmd.ExecuteReader();
                List<TransHistoryModel> list = new List<TransHistoryModel>();
                while (dr.Read())
                {
                    TransHistoryModel item = new TransHistoryModel();
                    item.AccntNumber = Convert.ToInt64(dr["AccntNumber"]);
                    item.TransType = dr["TrasactionType"].ToString();
                    item.Amount = Convert.ToDecimal(dr["Amount"]);
                    if(!string.IsNullOrEmpty(dr["AccntFrom"].ToString()))
                    {
                        item.AccntFrom = Convert.ToInt64(dr["AccntFrom"]);
                    }
                    else
                    {
                        item.AccntFrom = 0;
                    }
                    if (!string.IsNullOrEmpty(dr["AccntTo"].ToString()))
                    {
                        item.AccntTo = Convert.ToInt64(dr["AccntTo"]);
                    }
                                     
                    item.EndBalance = Convert.ToDecimal(dr["EndBalance"]);
                    item.TransDate = Convert.ToDateTime(dr["TransDate"]);
                    list.Add(item);
                }
                return list;
            }
            catch (Exception ex)
            {
               return new List<TransHistoryModel>();

            }
           
        }
        private string CreateTransaction(string transname, double accntnum,decimal amount,double accntfrom,long accountto,decimal endbal)
        {
            try
            {
                SqlConnection cn = new SqlConnection(_configuration.GetConnectionString("WalletConnectionString"));
                cn.Open();
                SqlCommand cmd = new SqlCommand("CreateTransaction", cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@AccntNumber", accntnum);
                cmd.Parameters.AddWithValue("@TrasactionType", transname);
                cmd.Parameters.AddWithValue("@Amount", amount);
                if(accntfrom == 0)
                {
                    cmd.Parameters.AddWithValue("@AccntFrom", DBNull.Value);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@AccntFrom", accntfrom);
                }
                if (accountto == 0)
                {
                    cmd.Parameters.AddWithValue("@AccntTo", DBNull.Value);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@AccntTo", accountto);
                }
               
                cmd.Parameters.AddWithValue("@EndBalance", endbal);
                cmd.Parameters.AddWithValue("@TransDate", DateTime.Now);
                int result = cmd.ExecuteNonQuery();
                cmd.Dispose();
                cn.Dispose();
                if(result < 0)
                {
                    return "Transaction Details Not Save!";
                }
                else
                {
                    if (transname == "Transfer Fund")
                    {
                        cn = new SqlConnection(_configuration.GetConnectionString("WalletConnectionString"));
                        cn.Open();
                        cmd = new SqlCommand("ReadBalance", cn);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@acntnum", accountto);
                        SqlDataReader dr = cmd.ExecuteReader();
                        decimal balance = 0;
                        if (dr.Read())
                        {
                            balance = Convert.ToDecimal(dr["UserBalance"]);
                        }
                        cn = new SqlConnection(_configuration.GetConnectionString("WalletConnectionString"));
                        cn.Open();
                        cmd = new SqlCommand("CreateTransaction", cn);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@AccntNumber", accountto);
                        cmd.Parameters.AddWithValue("@TrasactionType", transname);
                        cmd.Parameters.AddWithValue("@Amount", amount);
                        cmd.Parameters.AddWithValue("@AccntFrom", accntfrom);
                        cmd.Parameters.AddWithValue("@AccntTo", DBNull.Value);
                        cmd.Parameters.AddWithValue("@EndBalance", balance);
                        cmd.Parameters.AddWithValue("@TransDate", DateTime.Now);
                        result = cmd.ExecuteNonQuery();
                        cmd.Dispose();
                        cn.Dispose();
                        if (result < 0)
                        {
                            return "Transaction Details Not Save!";
                        }
                        else
                        {
                            return "Transaction Details Save!";
                        }
                    }
                    return "Transaction Details Save!";
                }
               

            }
            catch (Exception ex)
            {
                return ex.Message.ToString();
            }

        }
    }
}
