using System;
using UnityEngine;

public interface IPublicChatWindowView
{
    event Action OnClose;
    event Action OnBack;
    event Action<bool> OnFocused;
    event Action OnClickOverWindow;
    bool IsActive { get; }
    IChatHUDComponentView ChatHUD { get; }
    RectTransform Transform { get; }
    bool IsFocused { get; }
    void Dispose();
    void Hide();
    void Show();
    void Configure(PublicChatModel model);
}