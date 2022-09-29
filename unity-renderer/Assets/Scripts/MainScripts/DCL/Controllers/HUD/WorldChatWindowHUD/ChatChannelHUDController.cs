using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DCL.Interface;
using SocialFeaturesAnalytics;
using UnityEngine;
using Channel = DCL.Chat.Channels.Channel;

namespace DCL.Chat.HUD
{
    public class ChatChannelHUDController : IHUD
    {
        private const int INITIAL_PAGE_SIZE = 30;
        private const int SHOW_MORE_PAGE_SIZE = 10;
        private const float REQUEST_MESSAGES_TIME_OUT = 2;

        public IChatChannelWindowView View { get; private set; }

        private readonly DataStore dataStore;
        private BaseVariable<HashSet<string>> visibleTaskbarPanels => dataStore.HUDs.visibleTaskbarPanels;
        private BaseVariable<Transform> notificationPanelTransform => dataStore.HUDs.notificationPanelTransform;
        private readonly IUserProfileBridge userProfileBridge;
        private readonly IChatController chatController;
        private readonly IMouseCatcher mouseCatcher;
        private readonly InputAction_Trigger toggleChatTrigger;
        private readonly ISocialAnalytics socialAnalytics;
        private ChatHUDController chatHudController;
        private ChannelMembersHUDController channelMembersHUDController;
        private CancellationTokenSource deactivatePreviewCancellationToken = new CancellationTokenSource();
        private CancellationTokenSource hideLoadingCancellationToken = new CancellationTokenSource();
        private bool skipChatInputTrigger;
        private float lastRequestTime;
        private string channelId;
        private Channel channel;
        private bool mustRequestMessages;
        private ChatMessage oldestMessage;

        public event Action OnPressBack;
        public event Action OnClosed;
        public event Action<bool> OnPreviewModeChanged;
        public event Action<string> OnOpenChannelLeave;

        public ChatChannelHUDController(DataStore dataStore,
            IUserProfileBridge userProfileBridge,
            IChatController chatController,
            IMouseCatcher mouseCatcher,
            InputAction_Trigger toggleChatTrigger,
            ISocialAnalytics socialAnalytics)
        {
            this.dataStore = dataStore;
            this.userProfileBridge = userProfileBridge;
            this.chatController = chatController;
            this.mouseCatcher = mouseCatcher;
            this.toggleChatTrigger = toggleChatTrigger;
            this.socialAnalytics = socialAnalytics;
        }

        public void Initialize(IChatChannelWindowView view = null)
        {
            view ??= ChatChannelComponentView.Create();
            View = view;
            view.OnBack -= HandlePressBack;
            view.OnBack += HandlePressBack;
            view.OnClose -= Hide;
            view.OnClose += Hide;
            view.OnRequireMoreMessages += RequestOldConversations;
            view.OnLeaveChannel += LeaveChannel;
            view.OnShowMembersList += ShowMembersList;
            view.OnHideMembersList += HideMembersList;
            view.OnMuteChanged += MuteChannel;

            if (notificationPanelTransform.Get() == null)
            {
                view.OnFocused += HandleViewFocused;
            }

            chatHudController = new ChatHUDController(dataStore, userProfileBridge, false);
            chatHudController.Initialize(view.ChatHUD);
            chatHudController.OnInputFieldSelected -= HandleInputFieldSelected;
            chatHudController.OnInputFieldSelected += HandleInputFieldSelected;
            chatHudController.OnInputFieldDeselected -= HandleInputFieldDeselected;
            chatHudController.OnInputFieldDeselected += HandleInputFieldDeselected;
            chatHudController.OnSendMessage += HandleSendChatMessage;

            chatController.OnAddMessage -= HandleMessageReceived;
            chatController.OnAddMessage += HandleMessageReceived;
            chatController.OnChannelLeft += HandleChannelLeft;
            chatController.OnChannelUpdated += HandleChannelUpdated;

            if (mouseCatcher != null)
                mouseCatcher.OnMouseLock += Hide;

            toggleChatTrigger.OnTriggered += HandleChatInputTriggered;

            channelMembersHUDController = new ChannelMembersHUDController(view.ChannelMembersHUD, chatController, userProfileBridge);
        }

        public void Setup(string channelId)
        {
            channelMembersHUDController.SetChannelId(channelId);

            if (string.IsNullOrEmpty(channelId) || channelId == this.channelId) return;

            this.channelId = channelId;
            lastRequestTime = 0;

            channel = chatController.GetAllocatedChannel(channelId);
            View.Setup(ToPublicChatModel(channel));

            chatHudController.ClearAllEntries();
            mustRequestMessages = true;
            oldestMessage = null;
        }

        public void SetVisibility(bool visible)
        {
            if (View.IsActive == visible) return;

            SetVisiblePanelList(visible);
            if (visible)
            {
                View?.SetLoadingMessagesActive(false);
                View?.SetOldMessagesLoadingActive(false);

                if (!string.IsNullOrEmpty(channelId))
                {
                    var channel = chatController.GetAllocatedChannel(channelId);
                    View.Setup(ToPublicChatModel(channel));

                    if (mustRequestMessages)
                    {
                        RequestMessages(
                            channelId,
                            INITIAL_PAGE_SIZE);
                        
                        mustRequestMessages = false;
                    }
                }

                View.Show();
                Focus();
            }
            else
            {
                channelMembersHUDController.SetAutomaticReloadingActive(false);
                chatHudController.UnfocusInputField();
                View.Hide();
            }
        }

        public void Focus()
        {
            chatHudController.FocusInputField();
            MarkChannelMessagesAsRead();
        }

        public void Dispose()
        {
            if (chatHudController != null)
            {
                chatHudController.OnInputFieldSelected -= HandleInputFieldSelected;
                chatHudController.OnInputFieldDeselected -= HandleInputFieldDeselected;
            }

            if (chatController != null)
            {
                chatController.OnAddMessage -= HandleMessageReceived;
                chatController.OnChannelLeft -= HandleChannelLeft;
                chatController.OnChannelUpdated -= HandleChannelUpdated;
            }

            if (mouseCatcher != null)
                mouseCatcher.OnMouseLock -= ActivatePreviewMode;

            toggleChatTrigger.OnTriggered -= HandleChatInputTriggered;

            if (View != null)
            {
                View.OnBack -= HandlePressBack;
                View.OnClose -= Hide;
                View.OnFocused -= HandleViewFocused;
                View.OnRequireMoreMessages -= RequestOldConversations;
                View.OnLeaveChannel -= LeaveChannel;
                View.OnMuteChanged -= MuteChannel;
                View.Dispose();
            }

            hideLoadingCancellationToken.Dispose();
            deactivatePreviewCancellationToken.Dispose();
            channelMembersHUDController.Dispose();
        }

        private void HandleSendChatMessage(ChatMessage message)
        {
            message.messageType = ChatMessage.Type.PRIVATE;
            message.recipient = channelId;

            var isValidMessage = !string.IsNullOrEmpty(message.body)
                                 && !string.IsNullOrWhiteSpace(message.body)
                                 && !string.IsNullOrEmpty(message.recipient);

            if (isValidMessage)
            {
                chatHudController.ResetInputField();
                chatHudController.FocusInputField();
            }

            else
            {
                skipChatInputTrigger = true;
                chatHudController.ResetInputField(true);
                ActivatePreviewMode();
                return;
            }

            if (message.body.ToLower().Equals("/leave"))
            {
                LeaveChannelFromCommand();
                return;
            }

            chatController.Send(message);
            socialAnalytics.SendMessageSentToChannel(channel.Name, message.body.Length, "chat");
        }

        private void HandleMessageReceived(ChatMessage message)
        {
            if (!IsMessageFomCurrentChannel(message)) return;

            UpdateOldestMessage(message);

            message.isChannelMessage = true;
            chatHudController.AddChatMessage(message, limitMaxEntries: false);

            if (View.IsActive)
            {
                // The messages from 'channelId' are marked as read if the channel window is currently open
                MarkChannelMessagesAsRead();
            }

            View?.SetLoadingMessagesActive(false);
            View?.SetOldMessagesLoadingActive(false);
        }

        private void UpdateOldestMessage(ChatMessage message)
        {
            if (oldestMessage == null)
                oldestMessage = message;
            else if (message.timestamp < oldestMessage.timestamp)
                oldestMessage = message;
        }

        private void Hide()
        {
            SetVisibility(false);
            OnClosed?.Invoke();
        }

        private void HandlePressBack() => OnPressBack?.Invoke();

        private bool IsMessageFomCurrentChannel(ChatMessage message) =>
            message.sender == channelId || message.recipient == channelId;

        private void MarkChannelMessagesAsRead() => chatController.MarkChannelMessagesAsSeen(channelId);

        private void HandleInputFieldSelected()
        {
            deactivatePreviewCancellationToken.Cancel();
            deactivatePreviewCancellationToken = new CancellationTokenSource();
            DeactivatePreviewMode();
            // The messages from 'channelId' are marked as read if the player clicks on the input field of the channel window
            //MarkChannelMessagesAsRead();
        }

        private void HandleInputFieldDeselected()
        {
            if (View.IsFocused) return;
            WaitThenActivatePreview(deactivatePreviewCancellationToken.Token).Forget();
        }

        private void HandleViewFocused(bool focused)
        {
            if (focused)
            {
                deactivatePreviewCancellationToken.Cancel();
                deactivatePreviewCancellationToken = new CancellationTokenSource();
                DeactivatePreviewMode();
            }
            else
            {
                if (chatHudController.IsInputSelected) return;
                WaitThenActivatePreview(deactivatePreviewCancellationToken.Token).Forget();
            }
        }

        private async UniTaskVoid WaitThenActivatePreview(CancellationToken cancellationToken)
        {
            await UniTask.Delay(3000, cancellationToken: cancellationToken);
            await UniTask.SwitchToMainThread(cancellationToken);
            if (cancellationToken.IsCancellationRequested) return;
            ActivatePreviewMode();
        }

        public void DeactivatePreviewMode()
        {
            View.DeactivatePreview();
            chatHudController.DeactivatePreview();
            OnPreviewModeChanged?.Invoke(false);
        }

        public void ActivatePreviewMode()
        {
            SetVisiblePanelList(false);
            View?.ActivatePreview();
            chatHudController?.ActivatePreview();
            OnPreviewModeChanged?.Invoke(true);
        }

        private void HandleChatInputTriggered(DCLAction_Trigger action)
        {
            // race condition patch caused by unfocusing input field from invalid message on SendChatMessage
            // chat input trigger is the same key as sending the chat message from the input field
            if (skipChatInputTrigger)
            {
                skipChatInputTrigger = false;
                return;
            }

            if (!View.IsActive) return;
            chatHudController.FocusInputField();
        }

        private void RequestMessages(string channelId, int limit, string fromMessageId = null)
        {
            View?.SetLoadingMessagesActive(true);
            chatController.GetChannelMessages(channelId, limit, fromMessageId);
            hideLoadingCancellationToken.Cancel();
            hideLoadingCancellationToken = new CancellationTokenSource();
            WaitForRequestTimeOutThenHideLoadingFeedback(hideLoadingCancellationToken.Token).Forget();
        }

        private void RequestOldConversations()
        {
            if (IsLoadingMessages()) return;

            View?.SetOldMessagesLoadingActive(true);
            lastRequestTime = Time.realtimeSinceStartup;

            chatController.GetChannelMessages(
                channelId,
                SHOW_MORE_PAGE_SIZE,
                oldestMessage?.messageId);

            hideLoadingCancellationToken.Cancel();
            hideLoadingCancellationToken = new CancellationTokenSource();
            WaitForRequestTimeOutThenHideLoadingFeedback(hideLoadingCancellationToken.Token).Forget();
        }

        private bool IsLoadingMessages() =>
            Time.realtimeSinceStartup - lastRequestTime < REQUEST_MESSAGES_TIME_OUT;

        private async UniTaskVoid WaitForRequestTimeOutThenHideLoadingFeedback(CancellationToken cancellationToken)
        {
            lastRequestTime = Time.realtimeSinceStartup;

            await UniTask.WaitUntil(() =>
                    Time.realtimeSinceStartup - lastRequestTime > REQUEST_MESSAGES_TIME_OUT,
                cancellationToken: cancellationToken);
            if (cancellationToken.IsCancellationRequested) return;

            View?.SetLoadingMessagesActive(false);
            View?.SetOldMessagesLoadingActive(false);
        }

        private void LeaveChannel()
        {
            dataStore.channels.channelLeaveSource.Set(ChannelLeaveSource.Chat);
            OnOpenChannelLeave?.Invoke(channelId);
        }

        private void LeaveChannelFromCommand()
        {
            dataStore.channels.channelLeaveSource.Set(ChannelLeaveSource.Command);
            chatController.LeaveChannel(channelId);
        }

        private void HandleChannelLeft(string channelId)
        {
            if (channelId != this.channelId) return;
            OnPressBack?.Invoke();
        }

        private void HandleChannelUpdated(Channel updatedChannel)
        {
            if (updatedChannel.ChannelId != channelId)
                return;

            var channel = chatController.GetAllocatedChannel(channelId);
            View.Setup(ToPublicChatModel(updatedChannel));
        }

        private void ShowMembersList() => channelMembersHUDController.SetVisibility(true);

        private void HideMembersList() => channelMembersHUDController.SetVisibility(false);

        private void MuteChannel(bool muted)
        {
            if (muted)
                chatController.MuteChannel(channelId);
            else
                chatController.UnmuteChannel(channelId);
        }

        private void SetVisiblePanelList(bool visible)
        {
            var newSet = visibleTaskbarPanels.Get();

            if (visible)
                newSet.Add("ChatChannel");
            else
                newSet.Remove("ChatChannel");

            visibleTaskbarPanels.Set(newSet, true);
        }

        private PublicChatModel ToPublicChatModel(Channel channel)
        {
            return new PublicChatModel(channelId, channel.Name, channel.Description,
                channel.Joined, channel.MemberCount, channel.Muted);
        }
    }
}