using DCL.Quests;
using UnityEngine;

namespace DCLServices.QuestsService.TestScene
{
    public class QuestServiceTestScene : MonoBehaviour
    {
        [SerializeField] private ClientQuestsServiceMock client = null;
        [SerializeField] private QuestTrackerComponentView questTrackerComponentView;
        [SerializeField] private QuestCompletedComponentView questCompletedComponentView;
        [SerializeField] private QuestOfferComponentView questOfferComponentView;
        private QuestsService service;
        private QuestsController questController;

        private void Awake()
        {
            service = new QuestsService(client);
            service.SetUserId("Test");
            service.OnQuestUpdated += (questUpdate) => {Debug.Log($"QuestUdpated: {questUpdate.Name}"); };
            questController = new QuestsController(service, questTrackerComponentView, questCompletedComponentView, questOfferComponentView);
        }
    }
}
