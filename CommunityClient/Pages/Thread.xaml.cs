using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
namespace CommunityClient.Pages
{
    /// <summary>
    /// This class represents the unfolding of posts in a social
    /// </summary>
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Thread : ContentPage
    {
        public BaseCommunity BaseCommunity { get; set; }
        public Post PostToExplore;
        public int Level;
        /// <summary>
        /// Instantiate a view to display a list of posts
        /// </summary>
        /// <param name="baseCommunity">The Social to which the posts belong</param>
        /// <param name="postToExplore">Optional parameter that indicates whether the posts are sub-posts of an open post (posts can be responses that form a tree structure)</param>
        /// <param name="level">The level of the post tree</param>
        public Thread(BaseCommunity baseCommunity, Post postToExplore = null, int level = 0)
        {
            InitializeComponent();
            Level = level;
            BaseCommunity = baseCommunity;
            PostToExplore = postToExplore;
            if (postToExplore != null)
                Posts.ItemsSource = postToExplore.SubPosts;
            else
                Posts.ItemsSource = baseCommunity.FirstPost.SubPosts;
        }

        private async void Handle_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item == null)
                return;
            //await DisplayAlert("Item Tapped", "An item was tapped.", "OK").ConfigureAwait(true);
            //((ListView)sender).SelectedItem = null;
            if (Level < 2)
            {
                await Navigation.PushAsync(new Thread(BaseCommunity, e.Item as Post, Level + 1)).ConfigureAwait(false);
            }
        }

        private void NewPost_Clicked(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(Text.Text))
                BaseCommunity.CreateNewPost(Post.PostType.Text, System.Text.Encoding.Unicode.GetBytes(Text.Text), PostToExplore);
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
                post.Delete(BaseCommunity);
            }
        }

        private void OnLikeTapped(object sender, EventArgs e)
        {
            var post = (sender as Button)?.CommandParameter as Post;
            post.ILike = !post.ILike;
            post.Like = post.ILike ? post.Like + 1 : post.Like - 1;
            post.OnPropertyChanged(nameof(post.Like));
            post.SavePostLocal(BaseCommunity);
            Communication.UpdateEmoticon(BaseCommunity, post, Communication.Emoticon.Like, post.ILike);
        }
        private void OnDislikeTapped(object sender, EventArgs e)
        {
            var post = (sender as Button)?.CommandParameter as Post;
            post.IDislike = !post.IDislike;
            post.Dislike = post.IDislike ? post.Dislike + 1 : post.Dislike - 1;
            post.OnPropertyChanged(nameof(post.Dislike));
            post.SavePostLocal(BaseCommunity);
            Communication.UpdateEmoticon(BaseCommunity, post, Communication.Emoticon.Dislike, post.IDislike);
        }
        private void OnLoveTapped(object sender, EventArgs e)
        {
            var post = (sender as Button)?.CommandParameter as Post;
            post.ILove = !post.ILove;
            post.Love = post.ILove ? post.Love + 1 : post.Love - 1;
            post.OnPropertyChanged(nameof(post.Love));
            post.SavePostLocal(BaseCommunity);
            Communication.UpdateEmoticon(BaseCommunity, post, Communication.Emoticon.Love, post.ILove);
        }
        private void OnHahaTapped(object sender, EventArgs e)
        {
            var post = (sender as Button)?.CommandParameter as Post;
            post.IHaha = !post.IHaha;
            post.Haha = post.IHaha ? post.Haha + 1 : post.Haha - 1;
            post.OnPropertyChanged(nameof(post.Haha));
            post.SavePostLocal(BaseCommunity);
            Communication.UpdateEmoticon(BaseCommunity, post, Communication.Emoticon.Haha, post.IHaha);
        }
        private void OnYayTapped(object sender, EventArgs e)
        {
            var post = (sender as Button)?.CommandParameter as Post;
            post.IYay = !post.IYay;
            post.Yay = post.IYay ? post.Yay + 1 : post.Yay - 1;
            post.OnPropertyChanged(nameof(post.Yay));
            post.SavePostLocal(BaseCommunity);
            Communication.UpdateEmoticon(BaseCommunity, post, Communication.Emoticon.Yay, post.IYay);
        }
        private void OnWowTapped(object sender, EventArgs e)
        {
            var post = (sender as Button)?.CommandParameter as Post;
            post.IWow = !post.IWow;
            post.Wow = post.IWow ? post.Wow + 1 : post.Wow - 1;
            post.OnPropertyChanged(nameof(post.Wow));
            post.SavePostLocal(BaseCommunity);
            Communication.UpdateEmoticon(BaseCommunity, post, Communication.Emoticon.Wow, post.IWow);
        }
        private void OnSadTapped(object sender, EventArgs e)
        {
            var post = (sender as Button)?.CommandParameter as Post;
            post.ISad = !post.ISad;
            post.Sad = post.ISad ? post.Sad + 1 : post.Sad - 1;
            post.OnPropertyChanged(nameof(post.Sad));
            post.SavePostLocal(BaseCommunity);
            Communication.UpdateEmoticon(BaseCommunity, post, Communication.Emoticon.Sad, post.ISad);
        }
        private void OnAngryTapped(object sender, EventArgs e)
        {
            var post = (sender as Button)?.CommandParameter as Post;
            post.IAngry = !post.IAngry;
            post.Angry = post.IAngry ? post.Angry + 1 : post.Angry - 1;
            post.OnPropertyChanged(nameof(post.Angry));
            post.SavePostLocal(BaseCommunity);
            Communication.UpdateEmoticon(BaseCommunity, post, Communication.Emoticon.Angry, post.IAngry);
        }
    }
}
