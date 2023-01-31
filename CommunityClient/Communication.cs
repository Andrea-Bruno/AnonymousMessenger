using System;
using System.Collections.Generic;
using static CommunityClient.BaseCommunity;
namespace CommunityClient
{
    public static class Communication
    {
        public enum Command : ushort // 2 byte - the names must start with Get or Set
        {
            SetCommunity,
            GetCommunity,
            GetPosts, // Get posts starting from a specific location
            GetPostData, // Get single post data only (used to update posts that have been edited)
            GetFlags,
            SetPost,
            SetPostId,
            DeletePost,
            UpdateEmoticon,
        }

        public enum Emoticon : byte
        {
            Like,
            Dislike,
            Love,
            Haha,
            Yay,
            Wow,
            Sad,
            Angry
        }

        // =======================================================
        // =========    Messages for the cloud server    =========
        // =======================================================

        public static void JoinAndSend(byte[] communityId, Command command, params byte[][] values)
        {
            var listValues = new List<byte[]>(values);
            //			listValues.Insert(0, BitConverter.GetBytes(MyId));
            listValues.Insert(0, BitConverter.GetBytes((ushort)command));
            listValues.Insert(0, communityId);
            listValues.Insert(0, new byte[] { CurrentVersion });
            var data = EncryptedMessaging.Functions.JoinData(false, listValues.ToArray());
            var cmd = Enum.GetName(typeof(Command), command);
            SendMessage(data, cmd.StartsWith("Get"));
        }

        public static void GetPosts(BaseCommunity baseCommunity, bool AlsoPostsInQueue = false)
        {
            if (AlsoPostsInQueue)
                JoinAndSend(baseCommunity.CommunityId, Command.GetPosts, BitConverter.GetBytes(baseCommunity.LastPost.NextPostId), BitConverter.GetBytes(baseCommunity.PostIdQueueLastTrain));
            else
                JoinAndSend(baseCommunity.CommunityId, Command.GetPosts, BitConverter.GetBytes(baseCommunity.LastPost.NextPostId));
        }
        public static void GetPostData(BaseCommunity baseCommunity, long postId)
        {
            JoinAndSend(baseCommunity.CommunityId, Command.GetPostData, postId.GetBytes());
        }

        public static void GetFlags(BaseCommunity baseCommunity, byte[] chainedPostsIndexes)
        {
            JoinAndSend(baseCommunity.CommunityId, Command.GetFlags, chainedPostsIndexes);
        }
        /// <summary>
        /// Submit a new post to the server and set an action that will be performed when the server responds
        /// </summary>
        /// <param name="baseCommunity">the social of post</param>
        /// <param name="onAnswer">Action that will be performed when the server replies with the id number assigned to the post</param>        
        public static void SetPost(BaseCommunity baseCommunity, Action<long, int> onAnswer, long id, Post.ExportType exportType, byte[] export)
        {    //[0]=communityId, [1]=command, [2]=Version (implemented in JoinAndSend)), [3]=RequestId, [4] postId, [5]=exportType, [6]=export, [7] communityType, [8] communityModality
            var SetNewCommunity = id == 0;
            if (SetNewCommunity)
                JoinAndSend(baseCommunity.CommunityId, Command.SetCommunity, BitConverter.GetBytes(RequestId), id.GetBytes(), new byte[] { (byte)exportType }, export, new byte[] { (byte)baseCommunity.Type }, new byte[] { (byte)baseCommunity.Modality });
            else
                JoinAndSend(baseCommunity.CommunityId, Command.SetPost, BitConverter.GetBytes(RequestId), id.GetBytes(), new byte[] { (byte)exportType }, export);
            if (onAnswer != null)
                OnAnswerEvents.Add(RequestId, onAnswer);
            RequestId++;
        }
        private static uint RequestId;
        internal static Dictionary<uint, Action<long, int>> OnAnswerEvents = new Dictionary<uint, Action<long, int>>();
        internal static readonly List<Tuple<byte[], byte[]>> idToPrivateKeyTable = new List<Tuple<byte[], byte[]>>();

        public static void DeletePost(BaseCommunity baseCommunity, Post post)
        {
            JoinAndSend(baseCommunity.CommunityId, Command.DeletePost, BitConverter.GetBytes(post.Id));
        }

        public static void UpdateEmoticon(BaseCommunity baseCommunity, Post post, Emoticon emoticon, bool myAction)
        {
            var status = (byte)emoticon;
            if (myAction)
                status |= 0b10000000; // this bit indicates my action on the emoticon
            JoinAndSend(baseCommunity.CommunityId, Command.UpdateEmoticon, BitConverter.GetBytes(post.Id), new byte[] { status });
        }

        // =======================================================
        // =========    Answers from the cloud server    =========
        // =======================================================

        public static void SendMessage(byte[] data, bool isGet)
        {
            Social.Context.Messaging.SendBinary(Social.CommunityCloud, data, isGet, false);
        }

        public static void ProcessIncomingMessage(EncryptedMessaging.Message message)
        {
            if (message.Type == EncryptedMessaging.MessageFormat.MessageType.Binary && message.ChatId == Social.CommunityCloud.ChatId)
            {
                var parts = EncryptedMessaging.Functions.SplitData(message.GetData(), false);
                var answerTo = (Command)BitConverter.ToInt16(parts[0], 0);
                if (answerTo == Command.GetCommunity)
                    AnswerToGetCommunity(parts);
                if (answerTo == Command.SetPostId)
                {
                    var replyToRequestId = BitConverter.ToUInt32(parts[1], 0);
                    var postId = BitConverter.ToInt64(parts[2], 0);
                    var recordLength = BitConverter.ToInt32(parts[3], 0);
                    if (OnAnswerEvents.TryGetValue(replyToRequestId, out var onAnswer))
                    {
                        OnAnswerEvents[replyToRequestId]?.Invoke(postId, recordLength);
                        _ = OnAnswerEvents.Remove(replyToRequestId);
                    }
                }
                if (answerTo == Command.GetFlags || answerTo == Command.GetPosts)
                    ImportPosts(parts[1], parts[2]);

            }
        }

        private static void AnswerToGetCommunity(List<byte[]> parts)
        {
            // [1]=id, [2]=type, [3]=modality,[4]=total users,5[export]
            var id = parts[1];
            var privateKey = idToPrivateKeyTable.Find(x => x.Item1.SequenceEqual(id))?.Item2;
            var type = (CommunityType)parts[2][0];
            var modality = (ModalitySubscription)parts[3][0];

            var community = Social.FindSubscriptions(id);
            if (community == null)
            {
                switch (type)
                {
                    case CommunityType.Wall:
                        community = new Page();
                        break;
                    case CommunityType.Group:
                        community = new Group();
                        break;
                    case CommunityType.Page:
                        community = new Page();
                        break;
                }
                community.Modality = modality;
                community.PrivateKey = Convert.ToBase64String(privateKey);
                community.ImportPosts(parts[5]);
            }
            else
            {
                community.ImportPosts(parts[5]);
            }
            community.TotalUsers = BitConverter.ToUInt32(parts[4], 0);
            Social.AddCommunity(community, false);
        }

    }
}
