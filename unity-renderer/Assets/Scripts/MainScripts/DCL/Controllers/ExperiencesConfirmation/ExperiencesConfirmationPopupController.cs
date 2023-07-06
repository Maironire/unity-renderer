using System;
using System.Collections.Generic;

namespace DCL.PortableExperiences.Confirmation
{
    public class ExperiencesConfirmationPopupController
    {
        private readonly IExperiencesConfirmationPopupView view;
        private readonly DataStore dataStore;
        private readonly List<string> descriptionBuffer = new ();
        private Action acceptCallback;
        private Action rejectCallback;

        public ExperiencesConfirmationPopupController(IExperiencesConfirmationPopupView view,
            DataStore dataStore)
        {
            this.view = view;
            this.dataStore = dataStore;

            view.Hide(true);

            view.OnAccepted += () =>
            {
                view.Hide();
                acceptCallback?.Invoke();
            };
            view.OnRejected += () =>
            {
                view.Hide();
                rejectCallback?.Invoke();
            };

            dataStore.PendingPortableExperienceToBeConfirmed.Confirm.OnChange += OnConfirmRequested;
        }

        public void Dispose()
        {
            view.Dispose();

            dataStore.PendingPortableExperienceToBeConfirmed.Confirm.OnChange -= OnConfirmRequested;
        }

        private void OnConfirmRequested(ExperiencesConfirmationData current, ExperiencesConfirmationData previous)
        {
            ExperiencesConfirmationData.ExperienceMetadata metadata = current.Experience;

            acceptCallback = current.OnAcceptCallback;
            rejectCallback = current.OnRejectCallback;

            descriptionBuffer.Clear();

            foreach (string permission in metadata.Permissions)
                descriptionBuffer.Add(ConvertPermissionIdToDescription(permission));

            view.Show();
            view.SetModel(new ExperiencesConfirmationViewModel
            {
                Name = metadata.ExperienceName,
                IconUrl = metadata.IconUrl,
                Permissions = descriptionBuffer,
                Description = metadata.Description,
            });
        }

        private string ConvertPermissionIdToDescription(string permissionId)
        {
            switch (permissionId)
            {
                case "USE_FETCH":
                    return "Let the scene perform external HTTP requests.";
                case "USE_WEBSOCKET":
                    return "Let the scene use the Websocket API to establish external connections.";
                case "OPEN_EXTERNAL_LINK":
                    return "Let the scene open a URL (in a browser tab or web view).";
                case "USE_WEB3_API":
                    return "Let the scene communicate with a wallet.";
                case "ALLOW_TO_TRIGGER_AVATAR_EMOTE":
                    return "Let the scene to animate the player’s avatar with an emote.";
                case "ALLOW_TO_MOVE_PLAYER_INSIDE_SCENE":
                    return "Let the scene to change the player’s position.";
            }

            return permissionId;
        }
    }
}
