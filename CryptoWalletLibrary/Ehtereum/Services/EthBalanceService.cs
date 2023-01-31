using CryptoWalletLibrary.Ehtereum.Models;
using CryptoWalletLibrary.Ehtereum.ViewModels;
using NBitcoin;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CryptoWalletLibrary.Ehtereum.Services
{
    internal class EthBalanceService
    {
        private readonly EthCommonService ethCommonService;
        private static readonly StringComparer comparer = StringComparer.OrdinalIgnoreCase;
        public static EthBalanceService CreateInstance(EthCommonService ethCommonService) => new(ethCommonService);
        public static EthBalanceService Instance { get; private set; }

        public EthBalanceService(EthCommonService ethCommonService)
        {
            this.ethCommonService = ethCommonService;
            Instance = this;
        }

        /// <summary>
        ///  Calculates user's Ether and ERC20 token balances
        /// </summary>
        internal void GetBalanceOfAddresses()
        {
            ethCommonService.TotalBalanceByToken = new Dictionary<string, double>();
            ethCommonService.TotalConfirmedBalanceByToken = new Dictionary<string, double>();

            var txByToken = ethCommonService.TransactionsFromStorage.Where(tx => tx.Erc == ERC.ERC20).GroupBy(tx => tx.TxTo);
            foreach (var TokenTx in txByToken)
            {
                GetERC20TokenBalanceOfAddresses(TokenTx.Key);
            }

            GetEtherBalanceOfAddresses();
        }

        /// <summary>
        /// Based on fetched transcations calculates confirmed (transactions with confirmed status) and total
        /// (transactions with confirmed + unconfirmed status) balance for user's each adress. Pending transactions are ommited while calculations are made. In addtion total
        /// and confirmed balance of all addresses are calculated.
        /// </summary>
        private void GetEtherBalanceOfAddresses()
        {
            ethCommonService.TotalBalance = 0;
            ethCommonService.TotalConfirmedBalance = 0;
            ethCommonService.BalanceByAddress = new Dictionary<string, EthBalance>(comparer);

            void txReceived(EthTransactionForStoring tx)
            {
                var address = tx.TxTo;
                var amountReceived = decimal.ToDouble(tx.Value) / Math.Pow(10, 18);
                if (ethCommonService.BalanceByAddress.TryGetValue(address, out var balance))
                {
                    if (tx.ConfirmationStatus == EthTxConfirmationStatus.CONFIRMED)
                        balance.Confirmed += amountReceived;
                    balance.Total += amountReceived;
                }
                else
                {
                    ethCommonService.BalanceByAddress[address] = tx.ConfirmationStatus == EthTxConfirmationStatus.CONFIRMED
                        ? new EthBalance() { Confirmed = amountReceived, Total = amountReceived }
                        : new EthBalance() { Confirmed = .0, Total = amountReceived };
                }
            }

            void txSent(EthTransactionForStoring tx)
            {
                var address = tx.TxFrom;
                var amountSpent = (decimal.ToDouble(tx.Value) + tx.GasPrice * (ulong)tx.GasUsed) / Math.Pow(10, 18);
                if (ethCommonService.BalanceByAddress.TryGetValue(address, out var balance))
                {
                    if (tx.ConfirmationStatus == EthTxConfirmationStatus.CONFIRMED)
                        balance.Confirmed -= amountSpent;
                    balance.Total -= amountSpent;
                }
                else
                {
                    ethCommonService.BalanceByAddress[address] = tx.ConfirmationStatus == EthTxConfirmationStatus.CONFIRMED
                        ? new EthBalance() { Confirmed = -amountSpent, Total = -amountSpent }
                        : new EthBalance() { Confirmed = .0, Total = -amountSpent };
                }
            }

            foreach (var tx in ethCommonService.TransactionsFromStorage.Where(tx => tx.ConfirmationStatus != EthTxConfirmationStatus.PENDING))
            {
                if (tx.ConfirmationStatus == EthTxConfirmationStatus.PENDING) continue;
                if (tx.Sent == EthTxSentStatus.RECEIVED)
                    txReceived(tx);
                else if (tx.Sent == EthTxSentStatus.SENT)
                    txSent(tx);
                else
                {
                    txReceived(tx);
                    txSent(tx);
                }
            }

            ethCommonService.TotalBalance = ethCommonService.BalanceByAddress.Sum(x => x.Value.Total);
            ethCommonService.TotalConfirmedBalance = ethCommonService.BalanceByAddress.Sum(x => x.Value.Confirmed);
        }

        /// <summary>
        /// Based on fetched ERC20 transcations calculates confirmed ERC20 token balance(transactions with confirmed status) and total 
        /// (transactions with confirmed + unconfirmed statuses) for user's each adress. Pending transactions are ommited while calculations are made. In addtion total
        /// and confirmed balance of all addresses are calculated.
        /// </summary>
        /// <param name="tokenAddr">abbreviation for ERC20 token</param>
        private void GetERC20TokenBalanceOfAddresses(string tokenAddr)
        {
            var erc20TokenTxs = ethCommonService.TransactionsFromStorage.Where(tx => tx.TxTo == tokenAddr).ToList();
            ethCommonService.TotalBalanceByToken[tokenAddr] = 0;
            ethCommonService.TotalConfirmedBalanceByToken[tokenAddr] = 0;
            ethCommonService.AddrBalanceByToken = new Dictionary<string, Dictionary<string, EthBalance>>
            {
                [tokenAddr] = new Dictionary<string, EthBalance>(comparer)
            };
            void txReceived(EthTransactionForStoring tx)
            {
                var address = tx.ContractTo;
                var amountReceived = double.Parse(tx.ContractValue);
                if (ethCommonService.AddrBalanceByToken[tokenAddr].TryGetValue(address, out var balance))
                {
                    if (tx.ConfirmationStatus == EthTxConfirmationStatus.CONFIRMED)
                        balance.Confirmed += amountReceived;
                    balance.Total += amountReceived;
                }
                else
                {
                    ethCommonService.AddrBalanceByToken[tokenAddr][address] = tx.ConfirmationStatus == EthTxConfirmationStatus.CONFIRMED
                        ? new EthBalance() { Confirmed = amountReceived, Total = amountReceived }
                        : new EthBalance() { Confirmed = .0, Total = amountReceived };
                }
            }

            void txSent(EthTransactionForStoring tx)
            {
                var address = tx.TxFrom;
                var amountSpent = double.Parse(tx.ContractValue);
                if (ethCommonService.AddrBalanceByToken[tokenAddr].TryGetValue(address, out var balance))
                {
                    if (tx.ConfirmationStatus == EthTxConfirmationStatus.CONFIRMED)
                        balance.Confirmed -= amountSpent;
                    balance.Total -= amountSpent;
                }
                else
                {
                    ethCommonService.AddrBalanceByToken[tokenAddr][address] = tx.ConfirmationStatus == EthTxConfirmationStatus.CONFIRMED
                        ? new EthBalance() { Confirmed = -amountSpent, Total = -amountSpent }
                        : new EthBalance() { Confirmed = .0, Total = -amountSpent };
                }
            }

            foreach (var tx in erc20TokenTxs.Where(tx => tx.ConfirmationStatus != EthTxConfirmationStatus.PENDING))
            {
                if (tx.ConfirmationStatus == EthTxConfirmationStatus.PENDING) continue;
                if (tx.Sent == EthTxSentStatus.RECEIVED)
                    txReceived(tx);
                else if (tx.Sent == EthTxSentStatus.SENT)
                    txSent(tx);
                else
                {
                    txReceived(tx);
                    txSent(tx);
                }
            }

            ethCommonService.TotalBalanceByToken[tokenAddr] = ethCommonService.AddrBalanceByToken[tokenAddr].Sum(x => x.Value.Total);
            ethCommonService.TotalConfirmedBalanceByToken[tokenAddr] = ethCommonService.AddrBalanceByToken[tokenAddr].Sum(x => x.Value.Confirmed);

            if (CryptoWalletLibInit.TokenAbbrByAddr.ContainsKey(tokenAddr))
                ERC20ViewModelLocator.ERC20ViewModelConstruc(CryptoWalletLibInit.TokenAbbrByAddr[tokenAddr]).UpdateBalance();
        }
    }
}
