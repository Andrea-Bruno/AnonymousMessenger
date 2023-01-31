using CryptoWalletLibrary.Ehtereum.Models;
using CryptoWalletLibrary.Ehtereum.Utilies;
using CryptoWalletLibrary.Ehtereum.ViewModels;

using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using Nethereum.Geth;

using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.RPC.NonceServices;
using Nethereum.Signer;
using Nethereum.Web3;
using System;

using System.Numerics;

using System.Threading.Tasks;

namespace CryptoWalletLibrary.Ehtereum.Services
{
    public class EthTransferService
    {

        private readonly EthCommonService ethCommonService;
        private readonly EthBalanceService ethBalanceService;

        internal static EthTransferService CreateInstance(EthCommonService ethCommonService, EthBalanceService ethBalanceService) => new(ethCommonService, ethBalanceService);
        public static EthTransferService Instance { get; private set; }

        internal EthTransferService(EthCommonService ethCommonService, EthBalanceService ethBalanceService)
        {
            this.ethCommonService = ethCommonService;
            this.ethBalanceService = ethBalanceService;
            Task.Run(() => GetRecommendedFee());
        }

        /// <summary>
        /// Sends ether to specified address.
        /// </summary>
        /// <param name="sourceAddress"> sender address </param>
        /// <param name="destinationAddress"> destination address</param>
        /// <param name="ethAmount"></param>
        /// <param name="gasPrice"></param>
        /// <param name="gasLimit"></param>
        /// <param name="nonce"></param>
        /// <returns></returns>
        public async Task<string> SendEther(string sourceAddress, string destinationAddress, decimal ethAmount, int? gasPrice = null, int? gasLimit = null, int? nonce = null)
        {
            var account = ethCommonService.Wallet.GetAccount(sourceAddress, chainId: (int)Chain.Rinkeby);
            var web3 = new Web3Geth(account, url: EthCommonService.FULL_NODE_URL);

            string transactionHash;
            try
            {
                transactionHash = await web3.Eth.GetEtherTransferService()
                .TransferEtherAsync(destinationAddress, ethAmount, gas: gasLimit, gasPriceGwei: gasPrice /*0.005m*/, nonce: nonce);
            }
            catch
            {
                return null;
            }

            var (transaction, transactionReceipt, totalBlockN) = await GetSentTxDataAsync(transactionHash);

            if (transaction == null && transactionReceipt == null) return null;

            var transactionForStorage = new EthTransactionForStoring()
            {
                Block = transaction.BlockNumber?.ToUlong(),
                ContractTo = "",
                ContractValue = "",
                GasUsed = transactionReceipt?.GasUsed.ToUlong(),
                GasPrice = transaction.GasPrice.ToUlong(),
                Sent = EnumExtensions.AdressesToSSentStatus(sourceAddress, destinationAddress),
                SuccessStatus = (transactionReceipt?.Status != null) ? transactionReceipt.Status.ToUlong() == 1 ? EthTxSuccessStatus.SUCCESS : EthTxSuccessStatus.FAIL : EthTxSuccessStatus.PENDING, // when tx is pending full node responses with null block number
                Time = (ulong)DateTimeOffset.Now.ToUnixTimeSeconds(),
                TxFrom = sourceAddress,
                TxHash = transaction.TransactionHash,
                TxTo = destinationAddress,
                Value = ethAmount * (decimal)Math.Pow(10, 18),
                Nonce = transaction.Nonce.ToUlong(),
                ConfirmedBlockN = transaction.BlockNumber != null && transactionReceipt?.BlockNumber != null ? totalBlockN.ToUlong() - transactionReceipt.BlockNumber.ToUlong() : null,
            };

            ethCommonService.TransactionsFromStorage.Add(transactionForStorage);
            _ = ethCommonService.context.SecureStorage.ObjectStorage.SaveObject(ethCommonService.TransactionsFromStorage, "etcTransactions");

            ethCommonService.context.InvokeOnMainThread(() => EthTxViewModelLocator.EthTxViewModel.AddTransaction(transactionForStorage));
            ethBalanceService.GetBalanceOfAddresses();
            ethCommonService.context.InvokeOnMainThread(() => EthTxViewModelLocator.EthTxViewModel.UpdateBalance());

            return transaction.TransactionHash;
        }


        /// <summary>
        /// Sends ERC20 Token.
        /// </summary>
        /// <param name="sourceAddress"></param>
        /// <param name="destinationAddress"></param>
        /// <param name="tokenAmount"></param>
        /// <param name="gasPrice"></param>
        /// <param name="gasLimit"></param>
        /// <param name="nonce"></param>
        /// <param name="tokenAbbr"></param>
        /// <returns></returns>
        public async Task<string> SendToken(string sourceAddress, string destinationAddress, int tokenAmount, int? gasPrice, int? gasLimit, int? nonce, string tokenAbbr)
        {
            //var web3 = new Web3(wallet.GetAccount(sourceAddress, chainId: (int)Chain.Rinkeby), url: FULL_NODE_URL);
            var contractAddr = CryptoWalletLibInit.TokenAddrByAbbr[tokenAbbr];


            var account = ethCommonService.Wallet.GetAccount(sourceAddress, chainId: (int)Chain.Rinkeby);
            var web3 = new Web3Geth(account, url: EthCommonService.FULL_NODE_URL);
            var transferHandler = web3.Eth.GetContractTransactionHandler<TransferFunction>();

            var transfer = new TransferFunction()
            {
                To = destinationAddress,
                TokenAmount = tokenAmount,
                GasPrice = (BigInteger?)(gasPrice * Math.Pow(10, 9)),
                Gas = gasLimit,
                Nonce = nonce,
            };
            string transactionHash;
            try
            {
                transactionHash = await transferHandler.SendRequestAsync(contractAddr, transfer);
            }
            catch
            {
                return null;
            }

            var (transaction, transactionReceipt, totalBlockN) = await GetSentTxDataAsync(transactionHash);

            if (transaction == null && transactionReceipt == null) return null;

            var transactionForStorage = new EthTransactionForStoring()
            {
                Block = transaction.BlockNumber?.ToUlong(),
                ContractTo = destinationAddress,
                ContractValue = tokenAmount.ToString(),
                GasUsed = transactionReceipt?.GasUsed.ToUlong(),
                GasPrice = transaction.GasPrice.ToUlong(),
                Sent = EnumExtensions.AdressesToSSentStatus(sourceAddress, destinationAddress),
                SuccessStatus = (transactionReceipt?.Status != null) ? transactionReceipt.Status.ToUlong() == 1 ? EthTxSuccessStatus.SUCCESS : EthTxSuccessStatus.FAIL : EthTxSuccessStatus.PENDING, // when tx is pending full node responses with null block number
                Time = (ulong)DateTimeOffset.Now.ToUnixTimeSeconds(),
                TxFrom = sourceAddress,
                TxHash = transaction.TransactionHash,
                TxTo = contractAddr,
                Nonce = transaction.Nonce.ToUlong(),
                ConfirmedBlockN = transaction.BlockNumber != null && transactionReceipt?.BlockNumber != null ? totalBlockN.ToUlong() - transactionReceipt.BlockNumber.ToUlong() : null,
            };

            ethCommonService.TransactionsFromStorage.Add(transactionForStorage);
            _ = ethCommonService.context.SecureStorage.ObjectStorage.SaveObject(ethCommonService.TransactionsFromStorage, "etcTransactions");

            var addTransactionERC20 = (Action<EthTransactionForStoring>)CryptoWalletLibInit.ERC20PageMethodsByAbbr[tokenAbbr]["addTransaction"];
            var updateBalanceERC20 = (Action)CryptoWalletLibInit.ERC20PageMethodsByAbbr[tokenAbbr]["updateBalance"];
            ethCommonService.context.InvokeOnMainThread(() => addTransactionERC20(transactionForStorage));
            ethBalanceService.GetBalanceOfAddresses();
            ethCommonService.context.InvokeOnMainThread(() => updateBalanceERC20());
            ethCommonService.context.InvokeOnMainThread(() => EthTxViewModelLocator.EthTxViewModel.UpdateBalance());


            return transaction.TransactionHash;
        }

        private static async Task<(Nethereum.RPC.Eth.DTOs.Transaction, TransactionReceipt, HexBigInteger)> GetSentTxDataAsync(string transactionHash)
        {
            var nAttemptsToGetTxBlock = 4;
            var delayPeriod = 2000;
            Nethereum.RPC.Eth.DTOs.Transaction transaction = null;
            TransactionReceipt transactionReceipt = null;
            HexBigInteger totalBlockN = null;
            for (var i = 0; i < nAttemptsToGetTxBlock; i++)
            {
                await Task.Delay(delayPeriod);
                try
                {
                    transaction = await EthCommonService.Web3Clinet.Eth.Transactions.GetTransactionByHash.SendRequestAsync(transactionHash);
                    if (transaction == null) continue;
                }
                catch
                {
                    continue;
                }
                if (transaction.BlockNumber != null)
                {
                    try
                    {
                        transactionReceipt = await EthCommonService.Web3Clinet.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transactionHash);
                    }
                    catch
                    {
                        transactionReceipt = new TransactionReceipt();
                    }
                    try
                    {
                        totalBlockN = await EthCommonService.Web3Clinet.Eth.Blocks.GetBlockNumber.SendRequestAsync(transactionHash);
                    }
                    catch
                    {
                        totalBlockN = transactionReceipt.BlockNumber;
                    }

                    break;
                }
            }
            return (transaction, transactionReceipt, totalBlockN);

        }

        /// <summary>
        /// Estimates gas limit for contract to which token will be sent.
        /// </summary>
        /// <param name="sourceAddress"></param>
        /// <param name="destinationAddress"></param>
        /// <param name="tokenAmount"></param>
        /// <param name="tokenAbbr"></param>
        /// <returns></returns>
        public async Task<BigInteger> EstimateContractGasLimit(string sourceAddress, string destinationAddress, int tokenAmount, string tokenAbbr)
        {
            var web3 = new Web3(ethCommonService.Wallet.GetAccount(sourceAddress, chainId: (int)Chain.Rinkeby), url: EthCommonService.FULL_NODE_URL);
            var contractAddr = CryptoWalletLibInit.TokenAddrByAbbr[tokenAbbr];
            var transferHandler = web3.Eth.GetContractTransactionHandler<TransferFunction>();
            var transfer = new TransferFunction()
            {
                To = destinationAddress,
                TokenAmount = tokenAmount
            };
            var estimatedGasFee = await transferHandler.EstimateGasAsync(contractAddr, transfer);

            return estimatedGasFee.Value;
        }

        /// <summary>
        /// Gets recommended bit fee to make transaction
        /// </summary>
        /// <returns></returns>
        public async Task GetRecommendedFee()
        {
            ethCommonService.Fee = await EthCommonService.Web3Clinet.FeeSuggestion.GetSimpleFeeSuggestionStrategy().SuggestFeeAsync();
        }

        /// <summary>
        /// Checks if there are any pending transactions of the given address in the Ethereum full node Mempool.
        /// </summary>
        /// <param name="address"></param>
        /// <returns> done + pending transactions count and only done ones counts </returns>
        public async Task<(int, int)> CheckPendingTxAsync(string address)
        {
            var account = ethCommonService.Wallet.GetAccount(address, chainId: (int)Chain.Rinkeby);
            var web3 = new Web3Geth(account, url: EthCommonService.FULL_NODE_URL);
            account.NonceService = new InMemoryNonceService(address, web3.Client);
            var doneAndPendingTxCount = (int)(await web3.Eth.Transactions.GetTransactionCount.SendRequestAsync(account.Address, BlockParameter.CreatePending())).Value;
            var doneTxCount = (int)(await web3.Eth.Transactions.GetTransactionCount.SendRequestAsync(address)).Value;

            return (doneAndPendingTxCount, doneTxCount);
        }

        [Function("transfer", "bool")]
        public class TransferFunction : FunctionMessage
        {
            [Parameter("address", "_to", 1)]
            public string To { get; set; }

            [Parameter("uint256", "_value", 2)]
            public BigInteger TokenAmount { get; set; }
        }

        [Function("publicMintByUser")]
        public class MintFunction : FunctionMessage
        {
            [Parameter("uint256", "mintQuantity", 1)]
            public BigInteger MintQuantity { get; set; }
        }

        [Event("Transfer")]
        public class TransferEventDTO : IEventDTO
        {
            [Parameter("address", "from", 1, true)]
            public string From { get; set; }

            [Parameter("address", "to", 2, true)]
            public string To { get; set; }

            [Parameter("uint256", "tokenId", 3, true)]
            public BigInteger TokenId { get; set; }
        }
    }
}

/*! public async Task<string> MintNFT(string sourceAddress, string destinationAddress, int tokenAmount, int? gasPrice, int? gasLimit, string tokenAbbr)
     {
         sourceAddress = "0xFdBd76F867D6bE7863f181C7D667079edef951c7";
         destinationAddress = "0x76b24618aB2613443832aD306D72dD7a002099e9";
         var web3 = new Web3(wallet.GetAccount(sourceAddress, chainId: (int)Chain.Rinkeby), url: FULL_NODE_URL);
         //var contractAddr = CryptoWallet.TokenAddrByAbbr[tokenAbbr];
         var transferHandler = web3.Eth.GetContractTransactionHandler<MintFunction>();
         var NFTContractAddr = "0x2F40DD9bD7656b4B6682944643F8F58cF8Bc347a";
         var abi = @"[ { ""inputs"": [ { ""internalType"": ""uint256"", ""name"": ""mintQuantity"", ""type"": ""uint256"" } ], ""name"": ""publicMintByUser"", ""outputs"": [], ""stateMutability"": ""payable"", ""type"": ""function"" }, { ""inputs"": [ { ""internalType"": ""address"", ""name"": ""from"", ""type"": ""address"" }, { ""internalType"": ""address"", ""name"": ""to"", ""type"": ""address"" }, { ""internalType"": ""uint256"", ""name"": ""tokenId"", ""type"": ""uint256"" } ], ""name"": ""transferFrom"", ""outputs"": [], ""stateMutability"": ""nonpayable"", ""type"": ""function"" } ]";
         var transfer = new MintFunction() { MintQuantity = 1 };
         var contract = web3.Eth.GetContract(abi, NFTContractAddr);
         var transferFunction = contract.GetFunction("publicMintByUser");
         //var gas = await transferFunction.EstimateGasAsync(from: sourceAddress, gas: null, value: null, 1);
         var gas = await transferFunction.EstimateGasAsync(sourceAddress, new Nethereum.Hex.HexTypes.HexBigInteger(21000), new Nethereum.Hex.HexTypes.HexBigInteger(100));
         var receiptSecondAmountSend = await transferFunction.SendTransactionAndWaitForReceiptAsync(sourceAddress, null, null, null, destinationAddress, 0.0002);

         Nethereum.Hex.HexTypes.HexBigInteger hexBigInteger = new Nethereum.Hex.HexTypes.HexBigInteger((BigInteger)0.07);
         var receiptAmountSend =
         await transferFunction.SendTransactionAndWaitForReceiptAsync(sourceAddress, gas: null, value: hexBigInteger);

         return receiptAmountSend.BlockHash;
     }*/