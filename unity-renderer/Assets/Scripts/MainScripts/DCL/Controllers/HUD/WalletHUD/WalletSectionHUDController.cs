﻿using Cysharp.Threading.Tasks;
using DCL.Browser;
using DCL.Helpers;
using DCL.Tasks;
using System;
using System.Threading;
using UnityEngine;

namespace DCL.Wallet
{
    public class WalletSectionHUDController
    {
        private const string URL_MANA_INFO = "https://docs.decentraland.org/examples/get-a-wallet";
        private const string URL_MANA_PURCHASE = "https://account.decentraland.org";

        private readonly IWalletSectionHUDComponentView view;
        private readonly DataStore dataStore;
        private readonly IUserProfileBridge userProfileBridge;
        private readonly IClipboard clipboard;
        private readonly IBrowserBridge browserBridge;
        private readonly ITheGraph theGraph;

        private CancellationTokenSource fetchEthereumManaCancellationToken;
        private CancellationTokenSource fetchPolygonManaCancellationToken;

        private UserProfile ownUserProfile => userProfileBridge.GetOwn();

        public WalletSectionHUDController(
            IWalletSectionHUDComponentView view,
            DataStore dataStore,
            IUserProfileBridge userProfileBridge,
            IClipboard clipboard,
            IBrowserBridge browserBridge,
            ITheGraph theGraph)
        {
            this.view = view;
            this.dataStore = dataStore;
            this.userProfileBridge = userProfileBridge;
            this.clipboard = clipboard;
            this.browserBridge = browserBridge;
            this.theGraph = theGraph;

            dataStore.exploreV2.configureWalletSectionInFullscreenMenu.OnChange += ConfigureWalletSectionInFullscreenMenuChanged;
            ConfigureWalletSectionInFullscreenMenuChanged(dataStore.exploreV2.configureWalletSectionInFullscreenMenu.Get(), null);

            dataStore.wallet.isInitialized.Set(true);

            dataStore.wallet.currentEthereumManaBalance.OnChange += OnCurrentEthereumManaBalanceChanged;
            OnCurrentEthereumManaBalanceChanged(dataStore.wallet.currentEthereumManaBalance.Get(), 0f);
            dataStore.wallet.currentPolygonManaBalance.OnChange += OnCurrentPolygonManaBalanceChanged;
            OnCurrentPolygonManaBalanceChanged(dataStore.wallet.currentPolygonManaBalance.Get(), 0f);

            dataStore.wallet.isWalletSectionVisible.OnChange += OnWalletSectionVisible;

            ownUserProfile.OnUpdate += OnProfileUpdated;
            OnProfileUpdated(ownUserProfile);

            view.OnCopyWalletAddress += CopyWalletAddress;
            view.OnBuyManaClicked += GoToManaPurchaseUrl;
            view.OnLearnMoreClicked += GoToLearnMoreUrl;
        }

        public void Dispose()
        {
            dataStore.exploreV2.configureWalletSectionInFullscreenMenu.OnChange -= ConfigureWalletSectionInFullscreenMenuChanged;
            dataStore.wallet.currentEthereumManaBalance.OnChange -= OnCurrentEthereumManaBalanceChanged;
            dataStore.wallet.currentPolygonManaBalance.OnChange -= OnCurrentPolygonManaBalanceChanged;
            dataStore.wallet.isWalletSectionVisible.OnChange -= OnWalletSectionVisible;
            ownUserProfile.OnUpdate -= OnProfileUpdated;
            view.OnBuyManaClicked -= GoToManaPurchaseUrl;
            view.OnLearnMoreClicked -= GoToLearnMoreUrl;

            fetchEthereumManaCancellationToken.SafeCancelAndDispose();
            fetchPolygonManaCancellationToken.SafeCancelAndDispose();
        }

        private void ConfigureWalletSectionInFullscreenMenuChanged(Transform currentParentTransform, Transform _) =>
            view.SetAsFullScreenMenuMode(currentParentTransform);

        private void OnCurrentEthereumManaBalanceChanged(double currentBalance, double _) =>
            view.SetEthereumManaBalance(currentBalance);

        private void OnCurrentPolygonManaBalanceChanged(double currentBalance, double _) =>
            view.SetPolygonManaBalance(currentBalance);

        private void OnWalletSectionVisible(bool isVisible, bool _)
        {
            if (ownUserProfile.isGuest)
                return;

            if (isVisible)
            {
                fetchEthereumManaCancellationToken = fetchEthereumManaCancellationToken.SafeRestart();
                fetchPolygonManaCancellationToken = fetchPolygonManaCancellationToken.SafeRestart();
                RequestManaAsync(TheGraphNetwork.Ethereum, fetchEthereumManaCancellationToken.Token).Forget();
                RequestManaAsync(TheGraphNetwork.Polygon, fetchPolygonManaCancellationToken.Token).Forget();
            }
            else
            {
                fetchEthereumManaCancellationToken.SafeCancelAndDispose();
                fetchPolygonManaCancellationToken.SafeCancelAndDispose();
            }
        }

        private async UniTask RequestManaAsync(TheGraphNetwork network, CancellationToken cancellationToken)
        {
            while (true)
            {
                await UniTask.WaitUntil(() =>
                        ownUserProfile != null && !string.IsNullOrEmpty(ownUserProfile.userId),
                    cancellationToken: cancellationToken);

                if (network == TheGraphNetwork.Ethereum)
                    view.SetEthereumManaLoadingActive(true);
                else
                    view.SetPolygonManaLoadingActive(true);

                Promise<double> promise = theGraph.QueryMana(ownUserProfile.userId, network);
                if (promise != null)
                {
                    await promise;

                    if (network == TheGraphNetwork.Ethereum)
                    {
                        dataStore.wallet.currentEthereumManaBalance.Set(promise.value);
                        view.SetEthereumManaLoadingActive(false);
                    }
                    else
                    {
                        dataStore.wallet.currentPolygonManaBalance.Set(promise.value);
                        view.SetPolygonManaLoadingActive(false);
                    }
                }

                await UniTask.Delay(TimeSpan.FromSeconds(WalletUtils.FETCH_MANA_INTERVAL), cancellationToken: cancellationToken);

                if (cancellationToken.IsCancellationRequested)
                    break;
            }
        }

        private void OnProfileUpdated(UserProfile userProfile)
        {
            if (string.IsNullOrEmpty(userProfile.userId))
                return;

            view.SetWalletAddress(userProfile.userId);
            view.SetWalletSectionAsGuest(userProfile.isGuest);
        }

        private void CopyWalletAddress()
        {
            if (string.IsNullOrEmpty(ownUserProfile.userId))
                return;

            clipboard.WriteText(ownUserProfile.userId);
        }

        private void GoToManaPurchaseUrl() =>
            browserBridge.OpenUrl(URL_MANA_PURCHASE);

        private void GoToLearnMoreUrl() =>
            browserBridge.OpenUrl(URL_MANA_INFO);
    }
}