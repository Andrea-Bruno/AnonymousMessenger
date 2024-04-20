using Nethereum.RPC.Fee1559Suggestions;
using Nethereum.Web3;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Banking.Ehtereum.Services
{
    public class EthereumCommonWalletService
    {
        public Fee1559 Fee { get; private set; }
        private static Web3 web3Clinet;
        private static HttpClient httpClient;
        private const string FULL_NODE_URL = "https://rinkeby.infura.io/v3/27cf7d3b72cf4537a2110e6713920fab";

        public EthereumCommonWalletService()
        {
            web3Clinet = new Web3(FULL_NODE_URL);
            httpClient = new HttpClient();
        }
        public async Task GetRecommendedFee()
        {
            Fee = await web3Clinet.FeeSuggestion.GetSimpleFeeSuggestionStrategy().SuggestFeeAsync();
            //var a = web3Clinet.Eth.GasPrice.SendRequestAsync().Result;
            //var x = web3Clinet.FeeSuggestion.GetMedianPriorityFeeHistorySuggestionStrategy().SuggestFeeAsync().Result;
            //var y = web3Clinet.FeeSuggestion.GeTimePreferenceFeeSuggestionStrategy().SuggestFeeAsync().Result;
        }
    }
}
