
namespace CommunityClient.Pages
{
    class TopicEditor : TextAndDescription
    {
        public TopicEditor(BaseCommunity community)
        {
            Initialize(community, null, community.Root.Text);
        }
    }
}
