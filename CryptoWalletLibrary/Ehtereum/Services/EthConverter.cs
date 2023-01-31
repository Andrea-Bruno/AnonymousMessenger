using CryptoWalletLibrary.Ehtereum.Models;
using CryptoWalletLibrary.Ehtereum.Utilies;
using System;

namespace CryptoWalletLibrary.Ehtereum.Services
{
    internal static class EthConverter
    {
        public static void TxDTOToStorageTx(EthTransactionDTO ethTransactionDTO, ref EthTransactionForStoring txFromStorage)
        {
            txFromStorage.Block = ethTransactionDTO.Block;
            txFromStorage.ContractTo = ethTransactionDTO.ContractTo;
            txFromStorage.ContractValue = ethTransactionDTO.ContractValue;
            txFromStorage.Erc = ethTransactionDTO.Erc;
            txFromStorage.GasUsed = ethTransactionDTO.GasUsed;
            txFromStorage.GasPrice = ethTransactionDTO.GasPrice;
            txFromStorage.SuccessStatus = EnumExtensions.BoolToSuccessStatus(ethTransactionDTO.Status);
            txFromStorage.Nonce = ethTransactionDTO.Nonce;
            txFromStorage.Time = ethTransactionDTO.Time;
            txFromStorage.TxFrom = ethTransactionDTO.TxFrom;
            txFromStorage.TxTo = ethTransactionDTO.TxTo;
            txFromStorage.TxHash = ethTransactionDTO.TxHash;
            txFromStorage.Value = ethTransactionDTO.Value;
            if (ethTransactionDTO.TxFrom.Equals(ethTransactionDTO.TxTo))
                txFromStorage.Sent = EnumExtensions.AdressesToSSentStatus(ethTransactionDTO.TxFrom, ethTransactionDTO.TxTo);
        }

        public static EthTransactionForStoring TxDTOToStorageTx(EthTransactionDTO ethTransactionDTO)
        {
            return new EthTransactionForStoring()
            {
                Block = ethTransactionDTO.Block,
                ContractTo = ethTransactionDTO.ContractTo,
                ContractValue = ethTransactionDTO.ContractValue,
                Erc = ethTransactionDTO.Erc,
                GasUsed = ethTransactionDTO.GasUsed,
                GasPrice = ethTransactionDTO.GasPrice,
                SuccessStatus = EnumExtensions.BoolToSuccessStatus(ethTransactionDTO.Status),
                Nonce = ethTransactionDTO.Nonce,
                Time = ethTransactionDTO.Time,
                TxFrom = ethTransactionDTO.TxFrom,
                TxTo = ethTransactionDTO.TxTo,
                TxHash = ethTransactionDTO.TxHash,
                Value = ethTransactionDTO.Value,
                ConfirmedBlockN = ethTransactionDTO.ConfirmedBlockN,
                Sent = EnumExtensions.AdressesToSSentStatus(ethTransactionDTO.TxFrom, ethTransactionDTO.TxTo)
            };
        }

        public static void StorageTxToTx(EthTransactionForStoring transactionForStoring, ref EthTransaction ethTransaction)
        {
            ethTransaction.Block = transactionForStoring.Block;
            ethTransaction.Gas = transactionForStoring.GasUsed;
            ethTransaction.GasPrice = transactionForStoring.GasPrice;
            ethTransaction.Status = transactionForStoring.SuccessStatus;
            ethTransaction.Time = DateTimeOffset.FromUnixTimeSeconds((long)transactionForStoring.Time).DateTime;
            ethTransaction.TxFrom = transactionForStoring.TxFrom;
            ethTransaction.TxTo = transactionForStoring.TxTo;
            ethTransaction.TxHash = transactionForStoring.TxHash;
            ethTransaction.ContractTo = transactionForStoring.ContractTo;
            ethTransaction.ContractValue = transactionForStoring.ContractValue;
            ethTransaction.Value = transactionForStoring.Value / ((decimal)Math.Pow(10, 18));
            ethTransaction.Sent = transactionForStoring.Sent;
            ethTransaction.ConfirmedBlockN = transactionForStoring.ConfirmedBlockN;
        }
        public static EthTransaction StorageTxToTx(EthTransactionForStoring transactionForStoring)
        {
            return new EthTransaction()
            {
                Block = transactionForStoring.Block,
                Gas = transactionForStoring.GasUsed,
                GasPrice = transactionForStoring.GasPrice,
                Status = transactionForStoring.SuccessStatus,
                Time = DateTimeOffset.FromUnixTimeSeconds((long)transactionForStoring.Time).DateTime,
                TxFrom = transactionForStoring.TxFrom,
                TxTo = transactionForStoring.TxTo,
                TxHash = transactionForStoring.TxHash,
                ContractTo = transactionForStoring.ContractTo,
                ContractValue = transactionForStoring.ContractValue,
                Value = transactionForStoring.Value / ((decimal)Math.Pow(10, 18)),
                Sent = transactionForStoring.Sent,
                ConfirmedBlockN = transactionForStoring.ConfirmedBlockN,
            };
        }

        public static NFTAsset StorageNFTtoNFT(NFTAssetForStoring nftAssetForStoring)
        {
            return new NFTAsset()
            {
                TokenId = nftAssetForStoring.TokenId,
                TokenURI = nftAssetForStoring.TokenURI,
                OwnerAddr = nftAssetForStoring.OwnerAddr,
                CollcetionAbbr = nftAssetForStoring.CollcetionAbbr,
                CollcetionName = nftAssetForStoring.CollcetionName,
                ContractAddr = nftAssetForStoring.ContractAddr,
            };
        }
    }
}
