using CryptoWalletLibrary;
using CryptoWalletLibrary.Ehtereum;
using CryptoWalletLibrary.Ehtereum.ViewModels;
using CryptoWalletUI.Bitcoin.Views;
using CryptoWalletUI.Ehtereum.Pages;
using EncryptedMessaging;
using SubAppInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;

namespace CryptoWalletUI
{
    public static class CryptoWalletUIinit
    {
        private static SubAppUserInterface UserInterface;

        public static Dictionary<string, NavigationPage> PagesByAbbr;

        private static Context _context;

        /// <summary>
        /// Initializes Wallet UI.
        /// </summary>
        /// <param name="mainPage"></param>
        /// <param name="context"></param>
        public static void Initialize(Page mainPage, Context context)
        {
            _context = context;

            CryptoWalletLibInit.Initialize(_context);

            var tokensPages = _context.SecureStorage.ObjectStorage.LoadObject(typeof(List<string>), "tokensPages") as List<string>;
            var nftPage = _context.SecureStorage.Values.Get("nftPage", false);

            //prepairng toolbar
            var toolbarItems = new List<ToolbarItem>();
            if (!nftPage)
                toolbarItems.Add(new ToolbarItem("NFT", null, () => AddNFTs()));
            foreach (var abbr in TokenAddrByAbbr.Keys)
            {
                if (tokensPages == null || !tokensPages.Exists(x => x == abbr))
                    toolbarItems.Add(new ToolbarItem(abbr, null, () => AddToken(abbr)));
            }

            var imageButtons = new List<ImageButton>();

            var button = new ImageButton();
            void OnImageButtonClicked(object sender, EventArgs e)
            {
                AddToken("RMT");
            }
            button.Clicked += OnImageButtonClicked;
            imageButtons.Add(button);

            var btcPage = new NavigationPage(new ShowBalancePage()) { Title = "BTC" };
            var etcPage = new NavigationPage(new BalancePage()) { Title = "ETH" };
            UserInterface = new SubAppUserInterface(mainPage, Localization.Resources.Dictionary.Social, new List<NavigationPage> { btcPage, etcPage }, /*toolbarItems*/new List<ToolbarItem>(), imageButtons);

            //adding already existed NFT and tokens from storage
            //PagesByAbbr = new Dictionary<string, NavigationPage>();
            //if (tokensPages != null)
            //{
            //    foreach (var pageAbbr in tokensPages)
            //        AddToken(pageAbbr);
            //}
            //if (nftPage) AddNFTs();

            //------adding all pages for mobile without toolbar(for testing)---------
            PagesByAbbr = new Dictionary<string, NavigationPage>();
            AddNFTs();
            foreach (var pageAbbr in TokenAddrByAbbr.Keys)
                AddToken(pageAbbr);

            //var erc20PageMethodsByAbbr = new Dictionary<string, Dictionary<string, object>>();
            //foreach (var pageAbbr in PagesByAbbr.Keys)
            //{
            //    Action<EthTransactionForStoring> addTx = (PagesByAbbr[pageAbbr].RootPage.BindingContext as ERC20ViewModel).AddTransaction;
            //    Action<EthTransactionForStoring> updateTx = (PagesByAbbr[pageAbbr].RootPage.BindingContext as ERC20ViewModel).UpdateTransaction;
            //    Action<string> removeTx = (PagesByAbbr[pageAbbr].RootPage.BindingContext as ERC20ViewModel).RemoveTransaction;
            //    Action updateBalance = (PagesByAbbr[pageAbbr].RootPage.BindingContext as ERC20ViewModel).UpdateBalance;

            //    CryptoWalletLibInit.ERC20PageMethodsByAbbr.Add(pageAbbr, new Dictionary<string, object>() { { "addTransaction", addTx }, { "updateTransaction", updateTx }, { "removeTransaction", removeTx }, { "updateBalance", updateBalance } });
            //}
        }

        /// <summary>
        /// Adds ERC20 token page to UI.
        /// </summary>
        /// <param name="abbr">Abbreavation of token</param>
        public static void AddToken(string abbr)
        {
            var erc20Page = new NavigationPage(new ERC20BalancePage(abbr)) { Title = abbr };
            PagesByAbbr.Add(abbr, erc20Page);
            UserInterface.AddSubAppPage(erc20Page);
            _context.SecureStorage.ObjectStorage.SaveObject(PagesByAbbr.Keys.ToList(), "tokensPages");

            Action<EthTransactionForStoring> addTx = (PagesByAbbr[abbr].RootPage.BindingContext as ERC20ViewModel).AddTransaction;
            Action<EthTransactionForStoring> updateTx = (PagesByAbbr[abbr].RootPage.BindingContext as ERC20ViewModel).UpdateTransaction;
            Action<string> removeTx = (PagesByAbbr[abbr].RootPage.BindingContext as ERC20ViewModel).RemoveTransaction;
            Action updateBalance = (PagesByAbbr[abbr].RootPage.BindingContext as ERC20ViewModel).UpdateBalance;

            CryptoWalletLibInit.ERC20PageMethodsByAbbr.Add(abbr, new Dictionary<string, object>() { { "addTransaction", addTx }, { "updateTransaction", updateTx }, { "removeTransaction", removeTx }, { "updateBalance", updateBalance } });
            
            // Remove this token from toolbar coz its already added u cant add twice same page! add method to SubAppUserInteface for removing item from toolbar
        }

        /// <summary>
        /// Adds ERC721(NFT) page to UI.
        /// </summary>
        public static void AddNFTs()
        {
            var nftPage = new NavigationPage(new ERC721AssetsPage()) { Title = "NFT" };
            UserInterface.AddSubAppPage(nftPage);
            _context.SecureStorage.Values.Set("nftPage", true);
        }

        private static readonly StringComparer comparer = StringComparer.OrdinalIgnoreCase;
        public static IReadOnlyDictionary<string, string> TokenAddrByAbbr => new Dictionary<string, string>(comparer) { { "RMT", "0xF48B2fECfc655F9F186424C69bdd80d78F0D5F7E" },/* { "RNB", "0x8a75f985d5316b1a98bc11fe5364abbad55e1c7a" }*/ };
        public static IReadOnlyDictionary<string, string> AbbrByTokenAddr = TokenAddrByAbbr.ToDictionary((i) => i.Value, (i) => i.Key, comparer);
    }
}
