using Cysharp.Threading.Tasks;
using DCLServices.WearablesCatalogService;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.TestTools;

namespace DCL.Backpack
{
    public class WearableGridControllerShould
    {
        private string OWN_USER_ID = "ownUserId";

        private WearableGridController controller;
        private IWearableGridView view;
        private IUserProfileBridge userProfileBridge;
        private IWearablesCatalogService wearablesCatalogService;
        private DataStore_BackpackV2 dataStore;
        private UserProfile ownUserProfile;

        [SetUp]
        public void SetUp()
        {
            view = Substitute.For<IWearableGridView>();

            ownUserProfile = ScriptableObject.CreateInstance<UserProfile>();

            ownUserProfile.UpdateData(new UserProfileModel
            {
                userId = OWN_USER_ID,
                name = "ownUserName",
            });

            userProfileBridge = Substitute.For<IUserProfileBridge>();
            userProfileBridge.GetOwn().Returns(ownUserProfile);

            wearablesCatalogService = Substitute.For<IWearablesCatalogService>();

            dataStore = new DataStore_BackpackV2();

            controller = new WearableGridController(view,
                userProfileBridge,
                wearablesCatalogService,
                dataStore);
        }

        [TearDown]
        public void TearDown()
        {
            controller.Dispose();
        }

        [UnityTest]
        public IEnumerator SetWearableBreadcrumbWhenLoadingWearables()
        {
            const int WEARABLE_TOTAL_AMOUNT = 18;

            IReadOnlyList<WearableItem> wearableList = Array.Empty<WearableItem>();

            wearablesCatalogService.RequestOwnedWearablesAsync(OWN_USER_ID, 1, 15, true,
                                        Arg.Any<CancellationToken>())
                                   .Returns(UniTask.FromResult<(IReadOnlyList<WearableItem> wearables, int totalAmount)>((wearableList, WEARABLE_TOTAL_AMOUNT)));

            controller.LoadWearables();
            yield return null;

            view.Received(1)
                .SetWearableBreadcrumb(Arg.Is<NftBreadcrumbModel>(n => n.Current == 0
                                                                       && n.ResultCount == WEARABLE_TOTAL_AMOUNT
                                                                       && n.Path[0].Name == "All"
                                                                       && n.Path[0].Filter == "all"));
        }

        [UnityTest]
        public IEnumerator RequestFirstPageWhenLoadingWearables()
        {
            const int WEARABLE_TOTAL_AMOUNT = 18;

            IReadOnlyList<WearableItem> wearableList = Array.Empty<WearableItem>();

            wearablesCatalogService.RequestOwnedWearablesAsync(OWN_USER_ID, 1, 15, true,
                                        Arg.Any<CancellationToken>())
                                   .Returns(UniTask.FromResult<(IReadOnlyList<WearableItem> wearables, int totalAmount)>((wearableList, WEARABLE_TOTAL_AMOUNT)));

            controller.LoadWearables();
            yield return null;

            wearablesCatalogService.Received(1)
                                   .RequestOwnedWearablesAsync(OWN_USER_ID, 1, 15, true,
                                        Arg.Any<CancellationToken>());
        }

        [UnityTest]
        [TestCase(14, 1, ExpectedResult = null)]
        [TestCase(0, 1, ExpectedResult = null)]
        [TestCase(16, 2, ExpectedResult = null)]
        [TestCase(87, 6, ExpectedResult = null)]
        [TestCase(1356, 91, ExpectedResult = null)]
        public IEnumerator SetWearablePagesWhenLoadingWearables(int wearableTotalAmount, int expectedTotalPages)
        {
            IReadOnlyList<WearableItem> wearableList = Array.Empty<WearableItem>();

            wearablesCatalogService.RequestOwnedWearablesAsync(OWN_USER_ID, 1, 15, true,
                                        Arg.Any<CancellationToken>())
                                   .Returns(UniTask.FromResult<(IReadOnlyList<WearableItem> wearables, int totalAmount)>((wearableList, wearableTotalAmount)));

            controller.LoadWearables();
            yield return null;

            view.Received(1).SetWearablePages(1, expectedTotalPages);
        }

        [UnityTest]
        public IEnumerator ShowListOfWearablesWhenLoadingWearables()
        {
            const int WEARABLE_TOTAL_AMOUNT = 3;

            IReadOnlyList<WearableItem> wearableList = new[]
            {
                new WearableItem
                {
                    id = "w1",
                    rarity = "common",
                    description = "super wearable",
                    thumbnail = "w1thumbnail",
                    baseUrl = "http://localimages",
                    i18n = new[]
                    {
                        new i18n
                        {
                            text = "idk",
                            code = "wtf",
                        }
                    },
                    data = new WearableItem.Data
                    {
                        replaces = new[] { "category1", "category2", "category3" },
                        representations = new[]
                        {
                            new WearableItem.Representation
                            {
                                bodyShapes = new[] { "bodyShape1" },
                                overrideReplaces = new[] { "override1", "override2", "override3" },
                            }
                        }
                    }
                },
                new WearableItem
                {
                    id = "w2",
                    rarity = "rare",
                    description = "wo wah iii",
                    thumbnail = "w2thumbnail",
                    baseUrl = "http://localimages",
                    i18n = new[]
                    {
                        new i18n
                        {
                            text = "123",
                            code = "567",
                        }
                    },
                    data = new WearableItem.Data
                    {
                        representations = new[]
                        {
                            new WearableItem.Representation
                            {
                                bodyShapes = new[] { "bodyShape1" },
                                overrideReplaces = null,
                            }
                        }
                    }
                },
                new WearableItem
                {
                    id = "w3",
                    rarity = "unique",
                    description = "lalal lolo",
                    thumbnail = "w3thumbnail",
                    baseUrl = "http://localimages",
                    i18n = new[]
                    {
                        new i18n
                        {
                            text = "23p2",
                            code = "oi3j2",
                        }
                    },
                    data = new WearableItem.Data
                    {
                        representations = new[]
                        {
                            new WearableItem.Representation
                            {
                                bodyShapes = new[] { "bodyShape2" },
                                overrideReplaces = new[] { "override1", "override2", "override3" },
                            }
                        }
                    }
                }
            };

            wearablesCatalogService.RequestOwnedWearablesAsync(OWN_USER_ID, 1, 15, true,
                                        Arg.Any<CancellationToken>())
                                   .Returns(UniTask.FromResult<(IReadOnlyList<WearableItem> wearables, int totalAmount)>((wearableList, WEARABLE_TOTAL_AMOUNT)));

            controller.LoadWearables();
            yield return null;

            view.Received(1).ClearWearables();

            view.Received(1)
                .ShowWearables(Arg.Is<IEnumerable<WearableGridItemModel>>(i =>
                     i.ElementAt(0).WearableId == "w1"
                     && i.ElementAt(1).WearableId == "w2"
                     && i.ElementAt(2).WearableId == "w3"
                     && i.Count() == 3));
        }

        [UnityTest]
        public IEnumerator ShowOnEquippedWearableWhenLoadingWearables()
        {
            const int WEARABLE_TOTAL_AMOUNT = 1;

            dataStore.previewEquippedWearables.Add("w1");

            IReadOnlyList<WearableItem> wearableList = new[]
            {
                new WearableItem
                {
                    id = "w1",
                    rarity = "common",
                    description = "super wearable",
                    thumbnail = "w1thumbnail",
                    baseUrl = "http://localimages",
                    i18n = new[]
                    {
                        new i18n
                        {
                            text = "idk",
                            code = "wtf",
                        }
                    },
                    data = new WearableItem.Data
                    {
                        replaces = new[] { "category1", "category2", "category3" },
                        representations = new[]
                        {
                            new WearableItem.Representation
                            {
                                bodyShapes = new[] { "bodyShape1" },
                                overrideReplaces = new[] { "override1", "override2", "override3" },
                            }
                        }
                    }
                },
            };

            wearablesCatalogService.RequestOwnedWearablesAsync(OWN_USER_ID, 1, 15, true,
                                        Arg.Any<CancellationToken>())
                                   .Returns(UniTask.FromResult<(IReadOnlyList<WearableItem> wearables, int totalAmount)>((wearableList, WEARABLE_TOTAL_AMOUNT)));

            controller.LoadWearables();
            yield return null;

            view.Received(1).ClearWearables();

            view.Received(1)
                .ShowWearables(Arg.Is<IEnumerable<WearableGridItemModel>>(i =>
                     i.ElementAt(0).WearableId == "w1"
                     && i.ElementAt(0).IsEquipped
                     && i.ElementAt(0).IsSelected == false
                     && i.ElementAt(0).IsNew == false
                     && i.ElementAt(0).ImageUrl == "http://localimagesw1thumbnail"
                     && i.Count() == 1));
        }

        [UnityTest]
        public IEnumerator EquipWearable()
        {
            IReadOnlyList<WearableItem> wearableList = new[]
            {
                new WearableItem
                {
                    id = "w1",
                    rarity = "epic",
                    description = "super wearable",
                    thumbnail = "w1thumbnail",
                    baseUrl = "http://localimages",
                },
            };

            wearablesCatalogService.RequestOwnedWearablesAsync(OWN_USER_ID, 1, 15, true,
                                        Arg.Any<CancellationToken>())
                                   .Returns(UniTask.FromResult<(IReadOnlyList<WearableItem> wearables, int totalAmount)>((wearableList, 1)));

            controller.LoadWearables();
            yield return null;
            view.ClearReceivedCalls();

            controller.Equip("w1");

            view.Received(1)
                .SetWearable(Arg.Is<WearableGridItemModel>(w => w.WearableId == "w1"
                                                                && w.Rarity == NftRarity.Epic
                                                                && w.IsSelected == false
                                                                && w.IsEquipped == true
                                                                && w.ImageUrl == "http://localimagesw1thumbnail"));
        }

        [UnityTest]
        public IEnumerator DontEquipWearableWhenNotBeingShown()
        {
            IReadOnlyList<WearableItem> wearableList = new[]
            {
                new WearableItem
                {
                    id = "w1",
                    rarity = "epic",
                    description = "super wearable",
                    thumbnail = "w1thumbnail",
                    baseUrl = "http://localimages",
                },
            };

            wearablesCatalogService.RequestOwnedWearablesAsync(OWN_USER_ID, 1, 15, true,
                                        Arg.Any<CancellationToken>())
                                   .Returns(UniTask.FromResult<(IReadOnlyList<WearableItem> wearables, int totalAmount)>((wearableList, 1)));

            controller.LoadWearables();
            yield return null;
            view.ClearReceivedCalls();

            controller.Equip("w2");

            view.DidNotReceiveWithAnyArgs().SetWearable(default(WearableGridItemModel));
        }

        [UnityTest]
        public IEnumerator UnEquipWearable()
        {
            IReadOnlyList<WearableItem> wearableList = new[]
            {
                new WearableItem
                {
                    id = "w1",
                    rarity = "epic",
                    description = "super wearable",
                    thumbnail = "w1thumbnail",
                    baseUrl = "http://localimages",
                },
            };

            wearablesCatalogService.RequestOwnedWearablesAsync(OWN_USER_ID, 1, 15, true,
                                        Arg.Any<CancellationToken>())
                                   .Returns(UniTask.FromResult<(IReadOnlyList<WearableItem> wearables, int totalAmount)>((wearableList, 1)));

            controller.LoadWearables();
            yield return null;
            view.ClearReceivedCalls();

            controller.UnEquip("w1");

            view.Received(1)
                .SetWearable(Arg.Is<WearableGridItemModel>(w => w.WearableId == "w1"
                                                                && w.Rarity == NftRarity.Epic
                                                                && w.IsSelected == false
                                                                && w.IsEquipped == false
                                                                && w.ImageUrl == "http://localimagesw1thumbnail"));
        }

        [UnityTest]
        public IEnumerator DontUnEquipWearableWhenNotBeingShown()
        {
            IReadOnlyList<WearableItem> wearableList = new[]
            {
                new WearableItem
                {
                    id = "w1",
                    rarity = "epic",
                    description = "super wearable",
                    thumbnail = "w1thumbnail",
                    baseUrl = "http://localimages",
                },
            };

            wearablesCatalogService.RequestOwnedWearablesAsync(OWN_USER_ID, 1, 15, true,
                                        Arg.Any<CancellationToken>())
                                   .Returns(UniTask.FromResult<(IReadOnlyList<WearableItem> wearables, int totalAmount)>((wearableList, 1)));

            controller.LoadWearables();
            yield return null;
            view.ClearReceivedCalls();

            controller.UnEquip("w2");

            view.DidNotReceiveWithAnyArgs().SetWearable(default(WearableGridItemModel));
        }

        [Test]
        public void ChangePageWhenViewRequestsIt()
        {
            wearablesCatalogService.RequestOwnedWearablesAsync(OWN_USER_ID, 3, 15, true,
                                        Arg.Any<CancellationToken>())
                                   .Returns(UniTask.FromResult<(IReadOnlyList<WearableItem> wearables, int totalAmount)>((Array.Empty<WearableItem>(), 50)));

            view.OnWearablePageChanged += Raise.Event<Action<int>>(3);

            wearablesCatalogService.Received(1)
                                   .RequestOwnedWearablesAsync(OWN_USER_ID,
                                        3, 15, true, Arg.Any<CancellationToken>());

            view.Received(1).SetWearablePages(3, 4);
        }

        [Test]
        public void SelectWearableAndFillInfoCard()
        {
            wearablesCatalogService.WearablesCatalog.Returns(new BaseDictionary<string, WearableItem>(new[]
            {
                ("w1", new WearableItem
                {
                    id = "w1",
                    baseUrl = "http://localimages/",
                    data = new WearableItem.Data
                    {
                        category = "upper_body",
                        hides = new[] { "lower_body" },
                        replaces = new[] { "eyes" },
                    },
                    rarity = "legendary",
                    thumbnail = "w1.png",
                    description = "awesome wearable",
                    i18n = new[]
                    {
                        new i18n
                        {
                            code = "en",
                            text = "w1name",
                        }
                    }
                }),
            }));

            dataStore.previewEquippedWearables.Add("w1");

            view.OnWearableSelected += Raise.Event<Action<WearableGridItemModel>>(new WearableGridItemModel
            {
                WearableId = "w1",
                ImageUrl = "http://localimages/w1.png",
                IsEquipped = false,
                IsNew = false,
                IsSelected = false,
                Rarity = NftRarity.Legendary,
            });

            view.Received(1).ClearWearableSelection();
            view.Received(1).SelectWearable("w1");

            view.Received(1)
                .FillInfoCard(Arg.Is<InfoCardComponentModel>(i =>
                     i.category == "upper_body"
                     && i.rarity == "legendary"
                     && i.description == "awesome wearable"
                     && i.isEquipped == true
                     && i.name == "w1name"
                     && i.hiddenBy == null
                     && i.hideList[0] == "lower_body"
                     && i.removeList[0] == "eyes"));
        }

        [UnityTest]
        public IEnumerator FilterAllWearablesFromBreadcrumb()
        {
            wearablesCatalogService.RequestOwnedWearablesAsync(OWN_USER_ID, 1, 15, true,
                                        Arg.Any<CancellationToken>())
                                   .Returns(UniTask.FromResult<(IReadOnlyList<WearableItem> wearables, int totalAmount)>((Array.Empty<WearableItem>(), 50)));

            view.OnFilterWearables += Raise.Event<Action<string>>("all");
            yield return null;

            view.Received(1).SetWearableBreadcrumb(Arg.Is<NftBreadcrumbModel>(n =>
                n.ResultCount == 50
                && n.Current == 0
                && n.Path[0].Filter == "all"
                && n.Path[0].Name == "All"));

            wearablesCatalogService.Received(1).RequestOwnedWearablesAsync(OWN_USER_ID, 1, 15, true, Arg.Any<CancellationToken>());
        }
    }
}