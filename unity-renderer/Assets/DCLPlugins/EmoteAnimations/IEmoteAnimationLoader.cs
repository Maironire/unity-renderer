﻿using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace DCL.Emotes
{
    public interface IEmoteAnimationLoader : IDisposable
    {
        AnimationClip loadedAnimationClip { get; }
        GameObject container { get; }
        AudioClip audioClip { get; }
        UniTask LoadEmote(GameObject targetContainer, WearableItem emote, string bodyShapeId, CancellationToken ct = default);
    }
}
