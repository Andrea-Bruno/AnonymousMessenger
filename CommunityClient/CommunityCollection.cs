using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CommunityClient
{
    public class CommunityCollection
    {
        public CommunityCollection()
        {
            RootPosts = new ObservableCollection<Post>();
            Communities = new Dictionary<Post, BaseCommunity>();
            AllCommunities.Add(Communities);
        }
        public readonly ObservableCollection<Post> RootPosts;
        public readonly Dictionary<Post, BaseCommunity> Communities;
        public void Add(BaseCommunity community)
        {
            Communities.Add(community.Root, community);
            RootPosts.Add(community.Root);
        }
        public void Remove(BaseCommunity community)
        {
            _ = Communities.Remove(community.Root);
            _ = RootPosts.Remove(community.Root);
        }
        private static readonly List<Dictionary<Post, BaseCommunity>> AllCommunities = new List<Dictionary<Post, BaseCommunity>>();
        /// <summary>
        /// Get the community from its root post.
        /// </summary>
        /// <param name="rootPost">The root post, i.e. the post containing information describing the community, This is the post with ID 0</param>
        /// <returns>Returns null if the post is not a root post from any community</returns>
        public static BaseCommunity GetCommunity(Post rootPost)
        {
            foreach (var collection in AllCommunities)
            {
                if (collection.ContainsKey(rootPost))
                    return collection[rootPost];
            }
            return null;
        }
    }
}
