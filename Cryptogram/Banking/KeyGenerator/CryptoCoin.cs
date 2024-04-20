using NBitcoin;
using System.Collections.Generic;

using EncryptedMessaging;

namespace Banking.KeyGenerator
{
    public class CryptoCoin
    {
        private Context Context;
        private  BitcoinSecret secretKey;
        private  ExtKey masterKey;

        private  int changeAddrCount;
        private  int mainAddrCount;
        private  BitcoinAddress changeAddress;
        private  List<BitcoinAddress> mainAddresses;
        private  List<BitcoinAddress> changeAddresses;

        private  ExtKey mainAdressesParentExtKey;
        private  ExtKey changeAdressesParentExtKey;
        private  ExtPubKey mainAdressesParentExtPubKey;
        private  ExtPubKey changeAdressesParentExtPubKey;

        private const int MAX_RANGE_EMPTY_ADRR = 5;

    }
}
