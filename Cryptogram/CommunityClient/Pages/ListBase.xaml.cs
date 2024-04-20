using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;


namespace CommunityClient.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ListBase : ContentPage
    {
        public ListBase(CommunityType communityType)
        {
            InitializeComponent();
            CommunityType = communityType;
            if (communityType == CommunityType.Group)
            {
                Title.Text = Localization.Resources.Dictionary.Groups;
#if DEBUG
                if (communityType == CommunityType.Group)
                {
                    //GetTestGroup.Clicked += (s, e) => BaseCommunity.GetCommunityFromCloud("Rc4oLqt+8d4FSlciC0zumxvVFeRljrsF798T406MbbA=", Community.LastPost);
                    //GetTestGroup.IsVisible = true;
                }
#endif
                NewItem.Text = Localization.Resources.Dictionary.NewGroup;
                MyListView.ItemsSource = Social.Groups.RootPosts;
                NewItem.Clicked += (object sender, EventArgs e) => Navigation.PushAsync(new GroupEditor());
            }
            else if (communityType == CommunityType.Wall)
            {
                Title.Text = Localization.Resources.Dictionary.PeopleFollowed;
                MyListView.ItemsSource = Social.Walls.RootPosts;
            }
            else if (communityType == CommunityType.Page)
            {
                Title.Text = Localization.Resources.Dictionary.Pages;
                MyListView.ItemsSource = Social.Pages.RootPosts;
            }
        }
        private readonly int Level;
        private CommunityType CommunityType;
        /// <summary>
        /// Show the list of socials
        /// </summary>
        /// <param name="community"></param>
        public ListBase(BaseCommunity community, Post post = null, int level = 0)
        {
            InitializeComponent();
            Level = level;
            Community = community;
            Title.Text = community.Root.Text;
            if (level == 1)
            {
                NewItem.IsVisible = true;
                NewItem.Text = Localization.Resources.Dictionary.CreateNewTopic;
                NewItem.Clicked += (object sender, EventArgs e) => Navigation.PushAsync(new TopicEditor(community));
            }
            else if (level >= 1)
            {
                Text.IsVisible = true;
                NewItem.IsVisible = true;
                NewItem.Text = Localization.Resources.Dictionary.CreateNewPost;
                NewItem.Clicked += NewPost_Clicked;
            }
            community.GetCommunityFromCloud();
            CurrentPost = post ?? community.Root;
            if (post != null && level > 1)
            {
                ParentText.IsVisible = !string.IsNullOrEmpty(post.Text);
                ParentText.Text = post.Text;
                ParentDescription.IsVisible = !string.IsNullOrEmpty(post.Description);
                ParentDescription.Text = post.Description;
            }
            MyListView.ItemsSource = CurrentPost?.SubPosts;
            MyListView.RefreshCommand = RefreshFromServer();
            if (CurrentPost != null)
            {
                void RefreshLastMessageTimeDistance(object n)
                {
                    foreach (var subPost in CurrentPost.SubPosts)
                    {
                        post.UpdateTimeLiteral();
                    }
                }
                var timerRefreshLastMessageTimeDistance = new System.Threading.Timer(RefreshLastMessageTimeDistance, null, 300000, 300000); //If the garbage collector eliminates it, put it as static
            }
            Refresh.IsVisible = true;
            Refresh.Command = RefreshFromServer();
            Share.IsVisible = true;
            Share.Command = new Command(() =>
            {
                var share = new Xamarin.Essentials.ShareTextRequest
                {
                    Text = community.Address
                };
            });
            CurrentPost?.UpdateFlagsOfSubPosts(community);
        }

        private Command RefreshFromServer()
        {
            return new Command(() =>
            {
                MyListView.IsRefreshing = true;
                Community.RefreshFromServer();
                MyListView.IsRefreshing = false;
            });

        }

        private readonly Post CurrentPost;

        private readonly BaseCommunity Community;
        //private BaseCommunity BaseCommunity
        //{
        //    get
        //    {
        //            return Community; 
        //    }
        //}
        private async void Handle_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item == null)
                return;
            ((ListView)sender).SelectedItem = null;
            //var community = e.Item as BaseCommunity;

            if (MyListView.ItemsSource is IEnumerable<BaseCommunity>)
            {
                await Navigation.PushAsync(new ListBase(e.Item as BaseCommunity)).ConfigureAwait(false);
            }
            else if (Level < 3 && MyListView.ItemsSource is IEnumerable<Post>)
            {
                var community = Community;
                if (community == null)
                {
                    if (CommunityType == CommunityType.Group)
                    {
                        if (!Social.Groups.Communities.TryGetValue(e.Item as Post, out community))
                            return;
                    }
                    else if (CommunityType == CommunityType.Wall)
                    {
                        if (!Social.Walls.Communities.TryGetValue(e.Item as Post, out community))
                            return;
                    }
                    else if (CommunityType == CommunityType.Page)
                    {
                        if (!Social.Pages.Communities.TryGetValue(e.Item as Post, out community))
                            return;
                    }
                }
                await Navigation.PushAsync(new ListBase(community, e.Item as Post, Level + 1)).ConfigureAwait(false);
            }
        }

        private void NewPost_Clicked(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(Text.Text))
            {
                var editableProperties = new EditableProperties() { Text = Text.Text };
                Community.CreateNewPost(editableProperties, CurrentPost);
            }
            Text.Text = "";
        }

        private async void Edit_Clicked(object sender, EventArgs e)
        {
            var post = (sender as MenuItem)?.CommandParameter as Post;
            //var mi = ((MenuItem)sender);
            //_lastItemSelected = mi.CommandParameter as Contact;
            //if (_lastItemSelected != null)
            //	await Application.Current.MainPage.Navigation.PushAsync(new ChatUserProfilePage(new ItemDetailViewModel(_lastItemSelected)), false).ConfigureAwait(true);
        }

        private async void Report_Clicked(object sender, EventArgs e)
        {
            var post = (sender as MenuItem)?.CommandParameter as Post;
            if (await Application.Current.MainPage.DisplayAlert(Localization.Resources.Dictionary.Alert, Localization.Resources.Dictionary.ConfirmReport, Localization.Resources.Dictionary.Yes, Localization.Resources.Dictionary.No).ConfigureAwait(false))
            {

            }
        }

        private async void Delete_Clicked(object sender, EventArgs e)
        {
            var post = (sender as MenuItem)?.CommandParameter as Post;
            if (await Application.Current.MainPage.DisplayAlert(Localization.Resources.Dictionary.Alert, Localization.Resources.Dictionary.DeleteElement, Localization.Resources.Dictionary.Yes, Localization.Resources.Dictionary.No).ConfigureAwait(false))
            {
                post.Delete(Community);
            }
        }

        //private void Subscribe_OnClick(object sender, EventArgs e)
        //{
        //    BaseCommunity.GetCommunityFromCloud((sender as Editor).Text, null);
        //}

        private Post GetPost(object sender, out BaseCommunity baseCommunity)
        {
            baseCommunity = Community;
            var post = (sender as Button)?.CommandParameter as Post;
            if (baseCommunity == null && post.Id == 0)
                baseCommunity = CommunityCollection.GetCommunity(post);
            return post;
        }

        private void OnLikeTapped(object sender, EventArgs e)
        {
            var post = GetPost(sender, out var baseCommunity);
            post.ILike = !post.ILike;
            post.Like = post.ILike ? post.Like + 1 : post.Like - 1;
            post.OnPropertyChanged(nameof(post.Like));
            post.SavePostLocal(baseCommunity);
            Communication.UpdateEmoticon(baseCommunity, post, Communication.Emoticon.Like, post.ILike);
        }
        private void OnDislikeTapped(object sender, EventArgs e)
        {
            var post = GetPost(sender, out var baseCommunity);
            post.IDislike = !post.IDislike;
            post.Dislike = post.IDislike ? post.Dislike + 1 : post.Dislike - 1;
            post.OnPropertyChanged(nameof(post.Dislike));
            post.SavePostLocal(baseCommunity);
            Communication.UpdateEmoticon(baseCommunity, post, Communication.Emoticon.Dislike, post.IDislike);
        }
        private void OnLoveTapped(object sender, EventArgs e)
        {
            var post = GetPost(sender, out var baseCommunity);
            post.ILove = !post.ILove;
            post.Love = post.ILove ? post.Love + 1 : post.Love - 1;
            post.OnPropertyChanged(nameof(post.Love));
            post.SavePostLocal(baseCommunity);
            Communication.UpdateEmoticon(baseCommunity, post, Communication.Emoticon.Love, post.ILove);
        }
        private void OnHahaTapped(object sender, EventArgs e)
        {
            var post = GetPost(sender, out var baseCommunity);
            post.IHaha = !post.IHaha;
            post.Haha = post.IHaha ? post.Haha + 1 : post.Haha - 1;
            post.OnPropertyChanged(nameof(post.Haha));
            post.SavePostLocal(baseCommunity);
            Communication.UpdateEmoticon(baseCommunity, post, Communication.Emoticon.Haha, post.IHaha);
        }
        private void OnYayTapped(object sender, EventArgs e)
        {
            var post = GetPost(sender, out var baseCommunity);
            post.IYay = !post.IYay;
            post.Yay = post.IYay ? post.Yay + 1 : post.Yay - 1;
            post.OnPropertyChanged(nameof(post.Yay));
            post.SavePostLocal(baseCommunity);
            Communication.UpdateEmoticon(baseCommunity, post, Communication.Emoticon.Yay, post.IYay);
        }
        private void OnWowTapped(object sender, EventArgs e)
        {
            var post = GetPost(sender, out var baseCommunity);
            post.IWow = !post.IWow;
            post.Wow = post.IWow ? post.Wow + 1 : post.Wow - 1;
            post.OnPropertyChanged(nameof(post.Wow));
            post.SavePostLocal(baseCommunity);
            Communication.UpdateEmoticon(baseCommunity, post, Communication.Emoticon.Wow, post.IWow);
        }
        private void OnSadTapped(object sender, EventArgs e)
        {
            var post = GetPost(sender, out var baseCommunity);
            post.ISad = !post.ISad;
            post.Sad = post.ISad ? post.Sad + 1 : post.Sad - 1;
            post.OnPropertyChanged(nameof(post.Sad));
            post.SavePostLocal(baseCommunity);
            Communication.UpdateEmoticon(baseCommunity, post, Communication.Emoticon.Sad, post.ISad);
        }
        private void OnAngryTapped(object sender, EventArgs e)
        {
            var post = GetPost(sender, out var baseCommunity);
            post.IAngry = !post.IAngry;
            post.Angry = post.IAngry ? post.Angry + 1 : post.Angry - 1;
            post.OnPropertyChanged(nameof(post.Angry));
            post.SavePostLocal(baseCommunity);
            Communication.UpdateEmoticon(baseCommunity, post, Communication.Emoticon.Angry, post.IAngry);
        }
    }
}