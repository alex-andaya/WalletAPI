using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WalletSystemAPI.Controllers;
using NUnit.Framework;
using WalletSystemAPI;
using Microsoft.Extensions.Configuration;
using WalletSystemAPI.Models;
using System.Net;
using Newtonsoft.Json;

namespace NUnitWalletTestProject
{   
    public class WalletTest
    {
        private readonly IConfiguration _configuration;
        private AccountDetailsController controller;
        public WalletTest()
        {
            _configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(@"appsettings.json",false,false).AddEnvironmentVariables().Build();
        }
        [SetUp]
        public void Setup()
        {
            controller = new AccountDetailsController(_configuration);
        }

        [Test]
        public void SaveAccountDetailsGetResponseIsSuccess()
        {
            AccountDetailsModel item = new AccountDetailsModel();
            item.LoginName = "Test Name";
            item.UserPassword = "123456";
            var result = controller.SaveAccountDetails(item);

            Assert.That(result, Is.EqualTo("Account Details Save!"));
        }
        [Test]
        public void DepositAccountDetailsGetResponseIsSuccess()
        {

            var result = controller.Deposit("Test Name", "123456", 500);

            Assert.That(result, Is.EqualTo("Transaction Save! Your Current Balance is :500.00"));
        }
        [Test]
        public void WithdrawtAccountDetailsGetResponseIsSuccess()
        {

            var result = controller.Withdraw("Test Name", "123456", 200);

            Assert.That(result, Is.EqualTo("Account Details Save! Your Current Balance is :300.00"));
        }
        [Test]
        public void TransferAccountDetailsGetResponseIsSuccess()
        {

            var result = controller.Transfer("Test Name", "123456", 100, 1010101); //add other account here

            Assert.That(result, Is.EqualTo("Account Details Save! Your Current Balance is :200.00"));
        }
    }
}
