using NBitcoin;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Xamarin.Forms;

namespace CommunityClient
{
    public static class Social
    {
#if DEBUG
        public static bool Reset = false;
#endif

        public static void Initialize(Xamarin.Forms.Page mainPage, EncryptedMessaging.Context context)
        {
            Context = context;
            if (CommunityCloud == null)
                CommunityCloud = Context.Contacts.AddContact(BaseCommunity.DefaultServerPublicKey, "CommunityCloud", EncryptedMessaging.Modality.Server, EncryptedMessaging.Contacts.SendMyContact.None);

            System.Diagnostics.Debug.WriteLine("Cloud user ID " + CommunityCloud.UserId);
            System.Diagnostics.Debug.WriteLine("Cloud PubKet " + BaseCommunity.DefaultServerPublicKey);
            Context.OnContactEvent += Communication.ProcessIncomingMessage;
#if DEBUG
            // Community are stored here C:\Users\USER\AppData\Local\Packages\8459ecf1-a9a6-4678-8446-d8ecaa6d1ef5_x3hap7anq5jnt\LocalState\IsolatedStorage\Url.ogzsfvvb2sbtneywqkh4ucch1kbiopd0\Url.ogzsfvvb2sbtneywqkh4ucch1kbiopd0\Files\0
            if (Reset)
                Context.SecureStorage.ObjectStorage.DeleteAllObject(typeof(Group)); // reset the groups

#endif

#if DEBUG_RAM_SOCIAL
            Context.SecureStorage.ObjectStorage.DeleteAllObject(typeof(Group));    // reset the Group  
            Context.SecureStorage.ObjectStorage.DeleteAllObject(typeof(Page));   // reset the Page  
            Context.SecureStorage.ObjectStorage.DeleteAllObject(typeof(Wall));   // reset the Wall  

#endif

            // Load groups
            var groups = Context.SecureStorage.ObjectStorage.GetAllObjects(typeof(Group));
            foreach (Group item in groups)
                Groups.Add(item);

            // Load pages
            var pages = Context.SecureStorage.ObjectStorage.GetAllObjects(typeof(Page));
            foreach (Page item in pages)
                Pages.Add(item);

            // Load wall
            var walls = Context.SecureStorage.ObjectStorage.GetAllObjects(typeof(Wall));
            foreach (Wall item in walls)
                Walls.Add(item);
#if DEBUG
            var n = 0;
            void addGroup(string privateKey)
            {
                n++;
                CreateNewCommunity(CommunityType.Group, "♥", "Test group " + n, "Community for test use " + n, Convert.FromBase64String(privateKey), context.EntryPoint, modality: BaseCommunity.ModalitySubscription.NoSubscriptionRequired);
            }
            addGroup("Rc4oLqt+8d4FSlciC0zumxvVFeRljrsF798T406MjXA=");
            addGroup("Rc4oLqt+8d4FSlciC0zumxvVFeRljrsF798T406MbbA=");
#endif

            if (MyWall == null)
            {
                var myPrivatKey = context.My.GetPrivateKey();
                var hash = NBitcoin.Crypto.Hashes.DoubleSHA256(Encoding.ASCII.GetBytes(myPrivatKey + "wallet key")).ToBytes();
                var wallKey = new Key(hash);
                var myWall = new Wall();
                myWall.Initialize(null, context.My.Name, null, wallKey.ToBytes(), context.EntryPoint);
                MyWall = myWall;
            }
            var myWallPage = new NavigationPage(new Pages.ListBase(MyWall)) { Title = Localization.Resources.Dictionary.MyWall };
            var wallPage = new NavigationPage(new Pages.ListBase(CommunityType.Wall)) { Title = Localization.Resources.Dictionary.PeopleFollowed };
            var groupsPage = new NavigationPage(new Pages.ListBase(CommunityType.Group)) { Title = Localization.Resources.Dictionary.Groups };
            var pagesPage = new NavigationPage(new Pages.ListBase(CommunityType.Page)) { Title = Localization.Resources.Dictionary.Pages };
            var itemMenu = new ToolbarItem(Localization.Resources.Dictionary.Subscribe, null, async () =>
            {
                void subscribe(string code)
                {
                    BaseCommunity.GetCommunityFromCloud(code, null);
                }
                var inputPage = new Pages.Input(subscribe, Localization.Resources.Dictionary.Subscribe);
                if (mainPage is TabbedPage tabbedPage)
                    await tabbedPage.CurrentPage.Navigation.PushAsync(inputPage);
            });
            UserInterface = new SubAppInterface.SubAppUserInterface(mainPage, Localization.Resources.Dictionary.Social, new List<NavigationPage> { myWallPage, wallPage, groupsPage, pagesPage }, new List<ToolbarItem> { itemMenu }, null);
        }
        static SubAppInterface.SubAppUserInterface UserInterface;

        /// <summary>
        /// Create a new community which can be a personal group, page or wall
        /// </summary>
        /// <param name="type"></param>
        /// <param name="emoticon"></param>
        /// <param name="text"></param>
        /// <param name="description"></param>
        /// <param name="privateKey">It is the community encryption key, only members have this key. The server does not give the key and therefore on the server side the data is encrypted no one except members can decrypt the posts in the community</param>
        /// <param name="entryPoint">Web address that gives access to the community. The community is decentralized so there are several access points, even a private individual can keep a mini server at home for their communities</param>
        /// <param name="serverPublicKey">The public key is part of the asymmetric cryptography: It is the public encryption key of the cloud where the community is saved, this key allows you to communicate with the cloud and send community management commands</param>
        private static void CreateNewCommunity(CommunityType type, string emoticon, string text, string description, byte[] privateKey, Uri entryPoint, byte[] serverPublicKey = null, BaseCommunity.ModalitySubscription modality = BaseCommunity.ModalitySubscription.Default)
        {
            var exists = Exists(privateKey);
            if (exists == null)
            {
                BaseCommunity community = null;
                if (type == CommunityType.Group)
                    community = new Group();
                else if (type == CommunityType.Page)
                    community = new Page();
                else if (type == CommunityType.Wall)
                    community = new Wall();
                community.Initialize(emoticon, text, description, privateKey, entryPoint, serverPublicKey, modality);
                AddCommunity(community);
            }
#if DEBUG
            // in the debug phase, however, I send the group created to the cloud because I could have deleted it for testing
            else
            {
                exists.SaveToCloud();
            }
#endif

        }
        internal static EncryptedMessaging.Contact CommunityCloud;
        internal static EncryptedMessaging.Context Context;
        public static readonly CommunityCollection Groups = new CommunityCollection();
        public static readonly CommunityCollection Pages = new CommunityCollection();
        public static readonly CommunityCollection Walls = new CommunityCollection();

        //public static ObservableCollection<Group> Groups { get; set; }
        //public static ObservableCollection<Page> Pages { get; set; }
        //public static ObservableCollection<Wall> Walls { get; set; }
        private static Wall _myWall;
        public static Wall MyWall
        {
            get
            {
                return _myWall;
            }
            set
            {
                _myWall = value;
                Walls.Add(value);
            }
        }

        /// <summary>
        /// public List<BaseCommunity> Communities;
        /// public List<BaseCommunity> GetCommunities(CommunityType type) => Communities.FindAll(x => x.Type == type);
        /// </summary>
        /// <param name="group"></param>
        /// <param name="saveRemote"></param>
        public static void AddGroup(Group group, bool saveRemote = true)
        {
            AddCommunity(group, saveRemote);
        }
        public static void AddPage(Page page, bool saveRemote = true)
        {
            AddCommunity(page, saveRemote);
        }
        public static void AddWall(Wall wall, bool saveRemote = true)
        {
            AddCommunity(wall, saveRemote);
        }

        /// <summary>
        /// Add a community, it can be saved on the cloud server if it is new
        /// </summary>
        /// <param name="community">Community</param>
        /// <param name="saveRemote">Save the new community to the cloud server</param>
        public static void AddCommunity(BaseCommunity community, bool saveRemote = true)
        {
            var exists = Exists(community.PrivateKeyBytes);
            if (exists == null)
            {
                if (community is Wall)
                    Walls.Add(community as Wall);
                else if (community is Group)
                    Groups.Add(community as Group);
                else if (community is Page)
                    Pages.Add(community as Page);
            }
            if (saveRemote)
                community.SaveToCloud(); // When the server assigns the id it will automatically be saved locally as well
            community.SaveLocal();
        }
        public static BaseCommunity Exists(byte[] privateKeyBytes)
        {
            return Subscriptions.Find(x => x.PrivateKeyBytes.SequenceEqual(privateKeyBytes));
        }
        public static BaseCommunity Exists(string privateKey)
        {
            return Subscriptions.Find(x => x.PrivateKey == privateKey);
        }

        //private static SubAppInterface.SubAppUserInterface UserInterface;

        public static List<BaseCommunity> Subscriptions { get { var result = new List<BaseCommunity>(Walls.Communities.Values); result.AddRange(Groups.Communities.Values); result.AddRange(Pages.Communities.Values); return result; } }
        public static BaseCommunity FindSubscriptions(byte[] id) { return Subscriptions.Find(x => x.CommunityId.SequenceEqual(id)); }

    }
}
