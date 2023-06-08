using Cysharp.Threading.Tasks;
using DCL.Tasks;
using DCLServices.QuestsService;
using Decentraland.Quests;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace DCL.Quests
{
    public class QuestsController : IDisposable
    {
        private readonly IQuestsService questsService;
        private readonly IQuestTrackerComponentView questTrackerComponentView;
        private readonly IQuestCompletedComponentView questCompletedComponentView;
        private readonly IQuestStartedPopupComponentView questStartedPopupComponentView;
        private readonly IQuestLogComponentView questLogComponentView;
        private readonly IUserProfileBridge userProfileBridge;
        private readonly DataStore dataStore;

        private CancellationTokenSource disposeCts = null;
        private BaseVariable<string> pinnedQuestId => dataStore.Quests.pinnedQuest;
        private Dictionary<string, QuestInstance> quests;

        public QuestsController(
            IQuestsService questsService,
            IQuestTrackerComponentView questTrackerComponentView,
            IQuestCompletedComponentView questCompletedComponentView,
            IQuestStartedPopupComponentView questStartedPopupComponentView,
            IQuestLogComponentView questLogComponentView,
            IUserProfileBridge userProfileBridge,
            DataStore dataStore)
        {
            this.questsService = questsService;
            this.questTrackerComponentView = questTrackerComponentView;
            this.questCompletedComponentView = questCompletedComponentView;
            this.questStartedPopupComponentView = questStartedPopupComponentView;
            this.questLogComponentView = questLogComponentView;
            this.userProfileBridge = userProfileBridge;
            this.dataStore = dataStore;
            disposeCts = new CancellationTokenSource();
            quests = new ();
            if (questsService != null)
            {
                StartTrackingQuests(disposeCts.Token).Forget();
                StartTrackingStartedQuests(disposeCts.Token).Forget();
            }
            questLogComponentView.SetIsGuest(userProfileBridge.GetOwn().isGuest);
            questStartedPopupComponentView.OnOpenQuestLog += () => { dataStore.HUDs.questsPanelVisible.Set(true); };
            dataStore.exploreV2.configureQuestInFullscreenMenu.OnChange += ConfigureQuestLogInFullscreenMenuChanged;
            ConfigureQuestLogInFullscreenMenuChanged(dataStore.exploreV2.configureQuestInFullscreenMenu.Get(), null);
            questLogComponentView.OnPinChange += ChangePinnedQuest;
        }

        private void ChangePinnedQuest(string questId, bool isPinned)
        {
            string previousPinnedQuestId = pinnedQuestId.Get();

            pinnedQuestId.Set(isPinned ? questId : "");
            if(!string.IsNullOrEmpty(previousPinnedQuestId))
                AddOrUpdateQuestToLog(quests[previousPinnedQuestId]);

            if (string.IsNullOrEmpty(pinnedQuestId.Get()))
            {
                questTrackerComponentView.SetVisible(false);
                return;
            }

            questTrackerComponentView.SetVisible(true);
            questTrackerComponentView.SetQuestTitle(quests[questId].Quest.Name);
            questTrackerComponentView.SetQuestSteps(GetQuestSteps(quests[questId]));
            AddOrUpdateQuestToLog(quests[pinnedQuestId.Get()]);
        }

        private void ConfigureQuestLogInFullscreenMenuChanged(Transform current, Transform previous) =>
            questLogComponentView.SetAsFullScreenMenuMode(current);

        private async UniTaskVoid StartTrackingQuests(CancellationToken ct)
        {
            await foreach (var questInstance in questsService.QuestUpdated.WithCancellation(ct))
            {
                AddOrUpdateQuestToLog(questInstance);
                if (questInstance.Id != pinnedQuestId.Get())
                    continue;

                questTrackerComponentView.SetQuestTitle(questInstance.Quest.Name);
                questTrackerComponentView.SetQuestSteps(GetQuestSteps(questInstance));
            }
        }

        private List<QuestStepComponentModel> GetQuestSteps(QuestInstance questInstance)
        {
            List<QuestStepComponentModel> questSteps = new List<QuestStepComponentModel>();
            foreach (var step in questInstance.State.CurrentSteps)
            {
                foreach (Task task in step.Value.TasksCompleted)
                    questSteps.Add(new QuestStepComponentModel { isCompleted = true, text = task.Id });

                foreach (Task task in step.Value.ToDos)
                    questSteps.Add(new QuestStepComponentModel { isCompleted = false, text = task.Id });
            }

            return questSteps;
        }

        private async UniTaskVoid StartTrackingStartedQuests(CancellationToken ct)
        {
            await foreach (var questStateUpdate in questsService.QuestStarted.WithCancellation(ct))
            {
                questStartedPopupComponentView.SetQuestName(questStateUpdate.Quest.Name);
                questStartedPopupComponentView.SetVisible(true);
                AddOrUpdateQuestToLog(questStateUpdate);
            }
        }

        private void AddOrUpdateQuestToLog(QuestInstance questInstance)
        {
            if (!quests.ContainsKey(questInstance.Id))
                quests.Add(questInstance.Id, questInstance);

            quests[questInstance.Id] = questInstance;

            QuestDetailsComponentModel quest = new QuestDetailsComponentModel()
            {
                questName = questInstance.Quest.Name,
                //questCreator = questInstance.Quest.,
                questDescription = questInstance.Quest.Description,
                questId = questInstance.Id,
                isPinned = questInstance.Id == pinnedQuestId.Get(),
                //coordinates = questInstance.Quest.Coordinates,
                questSteps = GetQuestSteps(questInstance),
                questRewards = new List<QuestRewardComponentModel>()
            };
            questLogComponentView.AddActiveQuest(quest);
        }

        public void Dispose() =>
            disposeCts?.SafeCancelAndDispose();
    }
}
