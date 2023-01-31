namespace CryptoWalletLibrary.Ehtereum.Models
{
    public class EthTransactionDTO
    {
        public ulong Time { get; set; }
        public string TxFrom { get; set; }
        public string TxTo { get; set; }
        public ulong GasUsed { get; set; }
        public ulong GasPrice { get; set; }
        public ulong Block { get; set; }
        public string TxHash { get; set; }
        public decimal Value { get; set; }
        public string ContractTo { get; set; }
        public string ContractValue { get; set; }
        public bool Status { get; set; }
        public ERC? Erc { get; set; }
        public InputDataMethod? InputDataMethod { get; set; }
        public ulong Nonce { get; set; }
        public ulong ConfirmedBlockN { get; set; }
    }

    public enum ERC
    {
        ETH, ERC20, ERC721
    }

    public enum InputDataMethod
    {
        safeTransferFrom, setApprovalForAll, transferFrom, transferOwnership,
        buy, createToken, toggleTokenSellStatus, changeTokenPrice, undefined
    }
}