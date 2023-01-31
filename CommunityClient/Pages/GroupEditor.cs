using System;
using System.Collections.Generic;
using System.Text;

namespace CommunityClient.Pages
{
    class GroupEditor : TextAndDescription
    {
        public GroupEditor(Group group = null)
        {
            Initialize(group);
        }
    }
}
