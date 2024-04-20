using Banking.Bitcoin.Views;
using Banking.Ehtereum.Pages;
using Banking.Ehtereum.Services;
using Banking.Ehtereum.Views;
using Banking.Services;
using SubAppInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using EncryptedMessaging;
using EncryptedMessaging.Cloud;
using Xamarin.Forms;

namespace Banking
{
    public class CryptoWallet
    {
        private static SubAppUserInterface UserInterface;

        public static Dictionary<string, NavigationPage> PagesByAbbr;

        private static Context _context;

        public static void Initialize(Page mainPage, Context context)
        {
            _context = context;
            BitcoinWalletService.CreateInstance(context);
            EthereumWalletService.CreateInstance(context);


            var btcPage = new NavigationPage(new ShowBalancePage()) { Title = "BTC" };
            var etcPage = new NavigationPage(new BalancePage()) { Title = "ETH" };

            var toolbarItems = new List<ToolbarItem>();
            var tokensPages = _context.SecureStorage.ObjectStorage.LoadObject(typeof(List<string>), "tokensPages") as List<string>;
            foreach (var abbr in TokenAddrByAbbr.Keys)
            {

                if (tokensPages == null || !tokensPages.Exists(x => x == abbr))
                    toolbarItems.Add(new ToolbarItem(abbr, null, () => AddToken(abbr)));
            }
            UserInterface = new SubAppUserInterface(mainPage, Localization.Resources.Dictionary.Social, new List<NavigationPage> { btcPage, etcPage }, toolbarItems, null);
            //adding already existed tokens from storage
            PagesByAbbr = new Dictionary<string, NavigationPage>();
            if (tokensPages != null)
            {
                foreach (var pageAbbr in tokensPages)
                    AddToken(pageAbbr);
            }
            //UserInterface.AddSubAppPage(etcPage);
        }

        public static void AddToken(string abbr)
        {
            var x = new ERC20BalancePage(abbr);
            var erc20Page = new NavigationPage(x) { Title = abbr };
            PagesByAbbr.Add(abbr, erc20Page);
            UserInterface.AddSubAppPage(erc20Page);
            _context.SecureStorage.ObjectStorage.SaveObject(PagesByAbbr.Keys.ToList(), "tokensPages");
        }

        private static readonly StringComparer comparer = StringComparer.OrdinalIgnoreCase;
        public static IReadOnlyDictionary<string, string> TokenAddrByAbbr => new Dictionary<string, string>(comparer) { { "RMT", "0x4120333f14b8515191790dc326ce8d806a2bb28c" }, { "RNB", "0x8a75f985d5316b1a98bc11fe5364abbad55e1c7a" } };
        public static IReadOnlyDictionary<string, string> AbbrByTokenAddr = TokenAddrByAbbr.ToDictionary((i) => i.Value, (i) => i.Key, comparer);
    }
}
