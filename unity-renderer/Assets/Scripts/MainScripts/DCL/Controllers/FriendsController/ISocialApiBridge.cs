using Cysharp.Threading.Tasks;
using DCl.Social.Friends;
using System;
using System.Threading;

namespace DCL.Social.Friends
{
    public interface ISocialApiBridge : IService
    {
        public event Action<FriendRequest> OnIncomingFriendRequestAdded;
        public event Action<FriendRequest> OnOutgoingFriendRequestAdded;
        event Action<UserStatus> OnFriendAdded;
        event Action<string> OnFriendRequestRemoved;
        event Action<string, UserProfile> OnFriendRequestAccepted;
        event Action<string> OnFriendRequestRejected;
        event Action<string> OnDeletedByFriend;
        event Action<string> OnFriendRequestCanceled;

        UniTask<FriendshipInitializationMessage> GetInitializationInformationAsync(CancellationToken cancellationToken = default);

        UniTask RejectFriendshipAsync(string friendRequestId, CancellationToken cancellationToken = default);

        UniTask CancelFriendshipAsync(string friendRequestId, CancellationToken cancellationToken = default);

        UniTask<UserProfile> AcceptFriendshipAsync(string friendId, CancellationToken cancellationToken = default);

        UniTask DeleteFriendshipAsync(string friendId, CancellationToken cancellationToken = default);

        UniTask<FriendRequest> RequestFriendshipAsync(string friendId, string messageBody, CancellationToken cancellationToken = default);
    }
}
