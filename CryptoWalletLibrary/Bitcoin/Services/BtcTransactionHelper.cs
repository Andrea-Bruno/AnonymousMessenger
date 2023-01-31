using CryptoWalletLibrary.Bitcoin.Services;
using CryptoWalletLibrary.Models;
using ElectrumXClient.Response;
using NBitcoin;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace CryptoWalletLibrary.Services
{
    internal class BtcTransactionHelper
    {
        /// <summary>
        /// Gets transaction inputs from given transaction. Input details are fetch from ElectrumX server.
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="address">adress to which transaction belongs</param>
        /// <returns></returns>
        internal static async Task<List<TransactionInput>> GetTransactionInputDetails(Transaction transaction, string address)
        {
            var trInputs = new List<TransactionInput>();
            foreach (var vin in transaction.Inputs)
            {
                var transactrionResponse = new BlockchainTransactionGetResponse();
                try
                {
                    transactrionResponse = await BtcCommonService.Client.GetBlockchainTransactionGet(vin.PrevOut.Hash.ToString());
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("bitcoin back-end issue: " + ex.Message);
                    break;
                }
                var trDetails = transactrionResponse.Result;
                var vout = trDetails.VoutValue.Find(v => v.N == vin.PrevOut.N);
                trInputs.Add(new TransactionInput()
                {
                    Address = BitcoinAddress.Create(vout.ScriptPubKey.Addresses.First(), BtcCommonService.BitcoinNetwork).ToString(),
                    Amount = vout.Value,
                    OutputIdx = (int)vin.PrevOut.N,
                    TrId = vin.PrevOut.Hash.ToString(),
                    IsUsersAddress = BitcoinAddress.Create(vout.ScriptPubKey.Addresses.First(), BtcCommonService.BitcoinNetwork).ToString() == address,

                });
            }
            return trInputs;
        }

        /// <summary>
        /// Gets transaction ouptus from given transaction.
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="address">adress to which transaction belongs</param>
        /// <returns></returns>
        public static List<TransactionOutput> GetTransactionOutputDetails(Transaction transaction, string address)
        {
            var trOutputs = new List<TransactionOutput>();
            foreach (var output in transaction.Outputs)
                trOutputs.Add(new TransactionOutput()
                {
                    Address = output.ScriptPubKey.GetDestinationAddress(BtcCommonService.BitcoinNetwork).ToString(),
                    Amount = (double)output.Value.ToDecimal(MoneyUnit.BTC),
                    IsUsersAddress = output.ScriptPubKey.GetDestinationAddress(BtcCommonService.BitcoinNetwork).ToString() == address
                });
            return trOutputs;
        }
    }
}