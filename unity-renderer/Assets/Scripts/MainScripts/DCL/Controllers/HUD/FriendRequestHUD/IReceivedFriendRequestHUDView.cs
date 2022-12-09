using System;
using UIComponents.Scripts.Components;
using static DCL.Social.Friends.ReceivedFriendRequestHUDModel;

namespace DCL.Social.Friends
{
    public interface IReceivedFriendRequestHUDView : IBaseComponentView<ReceivedFriendRequestHUDModel>
    {
        event Action OnClose;
        event Action OnOpenProfile;
        event Action OnRejectFriendRequest;
        event Action OnConfirmFriendRequest;

        void SetBodyMessage(string messageBody);
        void SetTimestamp(DateTime timestamp);
        void SetRecipientName(string userName);
        void SetOtherProfilePicture(string uri);
        void SetOwnProfilePicture(string uri);
        void SetState(LayoutState state);
        void Show();
        void Close();
    }
}