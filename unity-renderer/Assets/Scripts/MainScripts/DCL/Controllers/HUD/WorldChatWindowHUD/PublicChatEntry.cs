﻿using System;
using DCL.Chat.HUD;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PublicChatEntry : BaseComponentView, IComponentModelConfig
{
    [SerializeField] internal Button openChatButton;
    [SerializeField] internal TMP_Text nameLabel;
    [SerializeField] internal PublicChatEntryModel model;
    [SerializeField] internal UnreadNotificationBadge unreadNotifications;
    [SerializeField] internal string namePrefix = "#";
    [SerializeField] internal TMP_Text memberCountLabel;
    [SerializeField] internal Button optionsButton;
    [SerializeField] internal Button leaveButton;
    [SerializeField] internal GameObject leaveButtonContainer;
    [SerializeField] internal GameObject openChatContainer;
    [SerializeField] internal GameObject joinedContainer;
    
    private IChatController chatController;

    public PublicChatEntryModel Model => model;

    public event Action<PublicChatEntry> OnOpenChat;
    public event Action<PublicChatEntry> OnLeave;
    public event Action<PublicChatEntry> OnOpenOptions;

    public static PublicChatEntry Create()
    {
        return Instantiate(Resources.Load<PublicChatEntry>("SocialBarV1/PublicChannelElement"));
    }

    public override void Awake()
    {
        base.Awake();
        openChatButton.onClick.AddListener(() => OnOpenChat?.Invoke(this));
        
        if (optionsButton)
            optionsButton.onClick.AddListener(() => OnOpenOptions?.Invoke(this));
        
        if (leaveButton)
            leaveButton.onClick.AddListener(() => OnLeave?.Invoke(this));
    }

    public void Initialize(IChatController chatController)
    {
        this.chatController = chatController;
    }

    public void Configure(BaseComponentModel newModel)
    {
        model = (PublicChatEntryModel) newModel;
        RefreshControl();
    }

    public override void RefreshControl()
    {
        nameLabel.text = $"{namePrefix}{model.name}";
        if (unreadNotifications)
            unreadNotifications.Initialize(chatController, model.channelId);
        if (memberCountLabel)
            memberCountLabel.SetText($"{model.memberCount} members");
        if (joinedContainer)
            joinedContainer.SetActive(model.isJoined);
        if (leaveButtonContainer)
            leaveButtonContainer.SetActive(model.isJoined);
        if (openChatContainer)
            openChatContainer.SetActive(!model.isJoined);
    }

    public void Dock(ChannelContextualMenu contextualMenu)
    {
        contextualMenu.transform.position = optionsButton.transform.position;
    }

    [Serializable]
    public class PublicChatEntryModel : BaseComponentModel
    {
        public string channelId;
        public string name;
        public long lastMessageTimestamp;
        public bool isJoined;
        public int memberCount;

        public PublicChatEntryModel(string channelId, string name, long lastMessageTimestamp, bool isJoined, int memberCount)
        {
            this.channelId = channelId;
            this.name = name;
            this.lastMessageTimestamp = lastMessageTimestamp;
            this.isJoined = isJoined;
            this.memberCount = memberCount;
        }
    }
}