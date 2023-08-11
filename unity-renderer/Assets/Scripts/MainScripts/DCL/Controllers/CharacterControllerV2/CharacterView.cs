﻿using DCL;
using DCL.CameraTool;
using DCL.Helpers;
using DCL.Interface;
using JetBrains.Annotations;
using MainScripts.DCL.Controllers.CharacterController;
using System;
using System.Collections.Generic;
using UnityEngine;
using Environment = DCL.Environment;

namespace MainScripts.DCL.Controllers.CharacterControllerV2
{
    public class CharacterView : MonoBehaviour, ICharacterView
    {
        private const int FIELD_HEIGHT = 30;
        private const int LABEL_SIZE = 500;
        [SerializeField] private UnityEngine.CharacterController characterController;
        [SerializeField] private CharacterAnimationController animationController;

        [Header("Data")]
        [SerializeField] private CharacterControllerData data;
        [SerializeField] private GameObject shadowBlob;

        [Header("InputActions")]
        [SerializeField] private InputAction_Hold jumpAction;
        [SerializeField] private InputAction_Hold sprintAction;
        [SerializeField] private InputAction_Hold walkAction;
        [SerializeField] private InputAction_Measurable characterYAxis;
        [SerializeField] private InputAction_Measurable characterXAxis;

        [SerializeField] private Vector3Variable cameraForward;
        [SerializeField] private Vector3Variable cameraRight;

        [Header("Camera")]
        public CameraMode CameraMode;

        [Header("Old References")]
        public GameObject avatarGameObject;
        public GameObject firstPersonCameraGameObject;

        private DCLCharacterControllerV2 controller;
        private readonly DataStore_Player dataStorePlayer = DataStore.i.player;
        private bool initialPositionAlreadySet;
        private Vector3 lastPosition;
        private float originalFOV;

        // todo: delete this once the ui thing is done
        private Dictionary<string, string> valuesTemp = new ();
        private CharacterState characterState;

        // end delete

        private void Awake()
        {
            controller = new DCLCharacterControllerV2(this, data, jumpAction, sprintAction, walkAction, characterXAxis, characterYAxis, cameraForward, cameraRight, CameraMode, shadowBlob);

            characterState = controller.GetCharacterState();
            animationController.SetupCharacterState(characterState);

            // TODO: Remove this?
            CommonScriptableObjects.playerUnityPosition.Set(Vector3.zero);
            dataStorePlayer.playerWorldPosition.Set(Vector3.zero);
            CommonScriptableObjects.playerCoords.Set(Vector2Int.zero);
            dataStorePlayer.playerGridPosition.Set(Vector2Int.zero);
            CommonScriptableObjects.playerUnityEulerAngles.Set(Vector3.zero);
            CommonScriptableObjects.worldOffset.OnChange += OnWorldReposition;
            var worldData = DataStore.i.Get<DataStore_World>();
            worldData.avatarTransform.Set(avatarGameObject.transform);
            worldData.fpsTransform.Set(firstPersonCameraGameObject.transform);
            CommonScriptableObjects.rendererState.OnChange += OnRenderingStateChanged;
            OnRenderingStateChanged(CommonScriptableObjects.rendererState.Get(), false);

            dataStorePlayer.lastTeleportPosition.OnChange += OnTeleport;
        }

        public void PostStart(FollowWithDamping cameraFollow)
        {
            cameraFollow.additionalHeight = characterController.height * 0.5f;
            animationController.PostStart(cameraFollow);
        }

        void OnRenderingStateChanged(bool isEnable, bool prevState)
        {
            this.enabled = isEnable && !DataStore.i.common.isSignUpFlow.Get();
        }

        [Obsolete]
        private void OnWorldReposition(Vector3 current, Vector3 previous)
        {
            transform.position = CharacterGlobals.characterPosition.unityPosition;
        }

        // sent by kernel
        [UsedImplicitly]
        public void Teleport(string teleportPayload)
        {
            var tpsCamera = DataStore.i.camera.tpsCamera.Get();
            originalFOV = tpsCamera.m_Lens.FieldOfView;

            var newPosition = Utils.FromJsonWithNulls<Vector3>(teleportPayload);
            dataStorePlayer.lastTeleportPosition.Set(newPosition, notifyEvent: true);

            if (!initialPositionAlreadySet) { initialPositionAlreadySet = true; }
        }

        private void OnTeleport(Vector3 current, Vector3 previous)
        {
            ReportPosition(current);
        }

        private void ReportPosition(Vector3 newPosition)
        {
            lastPosition = CharacterGlobals.characterPosition.worldPosition;
            CharacterGlobals.characterPosition.worldPosition = newPosition;
            transform.position = CharacterGlobals.characterPosition.unityPosition;
            Environment.i.platform.physicsSyncController?.MarkDirty();
            CommonScriptableObjects.playerUnityPosition.Set(CharacterGlobals.characterPosition.unityPosition);
            dataStorePlayer.playerWorldPosition.Set(CharacterGlobals.characterPosition.worldPosition);
            Vector2Int playerPosition = Utils.WorldToGridPosition(CharacterGlobals.characterPosition.worldPosition);
            CommonScriptableObjects.playerCoords.Set(playerPosition);
            dataStorePlayer.playerGridPosition.Set(playerPosition);
            dataStorePlayer.playerUnityPosition.Set(CharacterGlobals.characterPosition.unityPosition);

            if (Moved(lastPosition))
            {
                if (Moved(lastPosition, useThreshold: true))
                    ReportMovement();

                lastPosition = transform.position;
            }
        }

        bool Moved(Vector3 previousPosition, bool useThreshold = false)
        {
            if (useThreshold)
                return Vector3.Distance(CharacterGlobals.characterPosition.worldPosition, previousPosition) > 0.001f;
            else
                return CharacterGlobals.characterPosition.worldPosition != previousPosition;
        }

        void ReportMovement()
        {
            var reportPosition = CharacterGlobals.characterPosition.worldPosition;
            var compositeRotation = Quaternion.LookRotation(transform.forward);
            var cameraRotation = Quaternion.LookRotation(cameraForward.Get());

            if (initialPositionAlreadySet)
                WebInterface.ReportPosition(reportPosition, compositeRotation, characterController.height, cameraRotation);

            //lastMovementReportTime = DCLTime.realtimeSinceStartup;
        }

        private bool isRagdoll = false;
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.N))
                showDebug = !showDebug;

            if (Input.GetKeyDown(KeyCode.R))
            {
                if (!isRagdoll)
                    Ragdollize();
                else
                    DeRagdollize();
            }

            if (isRagdoll) return;

            characterController.radius = data.characterControllerRadius;
            characterController.skinWidth = characterController.radius * 0.1f; // its recommended that its 10% of the radius
            controller.Update(Time.deltaTime);

            var tpsCamera = DataStore.i.camera.tpsCamera.Get();
            bool isFovHigher = characterState.SpeedState == SpeedState.RUN && Flat(characterState.TotalVelocity).magnitude >= characterState.MaxVelocity * 0.35f;
            float targetFov = isFovHigher ? originalFOV + 15 : originalFOV;
            float fovSpeed = isFovHigher ? 20 : 50;
            tpsCamera.m_Lens.FieldOfView = Mathf.MoveTowards(tpsCamera.m_Lens.FieldOfView, targetFov, fovSpeed * Time.deltaTime);
        }

        private void DeRagdollize()
        {
            Animator animator = GetComponentInChildren<Animator>();

            Physics.autoSimulation = false;
            var cameraTarget = GameObject.Find("CharacterCameraTarget ");
            var damp = cameraTarget.GetComponent<FollowWithDamping>();
            damp.target = animator.transform;

            isRagdoll = false;
            GetComponentInChildren<RagdollController>().DeRagdollize();
            animator.enabled = true;
            characterController.enabled = true;
            damp.additionalHeight = 0.835f;
        }

        private void Ragdollize()
        {
            Physics.autoSimulation = true;
            var cameraTarget = GameObject.Find("CharacterCameraTarget ");
            var damp = cameraTarget.GetComponent<FollowWithDamping>();
            damp.target = RecursiveFindChild(transform, "Avatar_Neck");

            isRagdoll = true;
            GetComponentInChildren<Animator>().enabled = false;
            characterController.enabled = false;
            GetComponentInChildren<RagdollController>().Ragdollize();
            damp.additionalHeight = 0;
        }


        Transform RecursiveFindChild(Transform parent, string childName)
        {
            foreach (Transform child in parent)
            {
                if(child.name == childName)
                {
                    return child;
                }

                Transform found = RecursiveFindChild(child, childName);
                if (found != null)
                    return found;
            }
            return null;
        }
        private Vector3 Flat(Vector3 vector3)
        {
            vector3.y = 0;
            return vector3;
        }

        public (bool isGrounded, Vector3 deltaPosition, bool wallHit) Move(Vector3 delta)
        {
            if (!initialPositionAlreadySet) return (true, transform.position, false);

            var previousPosition = transform.position;
            var collisionFlags = characterController.Move(delta);
            Vector3 currentPosition = transform.position;
            var deltaPosition = previousPosition - currentPosition;

            if (currentPosition.y < 0) { transform.position = new Vector3(currentPosition.x, 0, currentPosition.z); }

            ReportPosition(PositionUtils.UnityToWorldPosition(currentPosition));

            return (characterController.isGrounded, deltaPosition, collisionFlags.HasFlag(CollisionFlags.Sides));
        }

        public void SetForward(Vector3 forward)
        {
            transform.forward = forward;
            CommonScriptableObjects.characterForward.Set(forward);
            CommonScriptableObjects.playerUnityEulerAngles.Set(transform.eulerAngles);
        }

        /*private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, characterController.radius);
            Gizmos.DrawWireSphere(transform.position + Vector3.down, characterController.radius);
        }*/

        public Vector3 GetSpherecastPosition() =>
            transform.position;

        public Vector3 GetPosition() =>
            transform.position;

        public (Vector3 center, float radius, float skinWidth, float height) GetCharacterControllerSettings() =>
            (characterController.center, characterController.radius, characterController.skinWidth, characterController.height);

        private bool showDebug = true;

        private void OnGUI()
        {
            if (showDebug)
            {
                var coef = Screen.width / 1920f; // values are made for 1920
                var firstColumnPosition = Mathf.RoundToInt(1920 * 0.12f);
                var secondColumnPosition = Mathf.RoundToInt(1920 * 0.6f);
                var fontSize = Mathf.RoundToInt(22 * coef);

                GUI.skin.label.fontSize = fontSize;
                GUI.skin.textField.fontSize = fontSize;
                var firstColumnYPos = 25;

                DrawLabel(firstColumnPosition, ref firstColumnYPos, "\"Z\" to walk");
                DrawLabel(firstColumnPosition, ref firstColumnYPos, "\"Shift\" to run");
                DrawLabel(firstColumnPosition, ref firstColumnYPos, "Keep pressing spacebar for higher jumps");

                DrawLabel(firstColumnPosition, ref firstColumnYPos, "\"Q\" to launch towards camera");
                DrawLabel(firstColumnPosition, ref firstColumnYPos, "\"E\" to apply a constant force");
                DrawLabel(firstColumnPosition, ref firstColumnYPos, "\"J\" to change camera");
                DrawLabel(firstColumnPosition, ref firstColumnYPos, "\"K\" for feet camera");
                DrawLabel(firstColumnPosition, ref firstColumnYPos, "\"R\" for Ragdoll");
                DrawLabel(firstColumnPosition, ref firstColumnYPos, "\"N\" to toggle this panel");

                /*data.walkSpeed = DrawFloatField(firstColumnPosition, ref firstColumnYPos, data.walkSpeed, "walkSpeed");
                data.jogSpeed = DrawFloatField(firstColumnPosition, ref firstColumnYPos, data.jogSpeed, "jogSpeed");
                data.runSpeed = DrawFloatField(firstColumnPosition, ref firstColumnYPos, data.runSpeed, "runSpeed");
                /*data.acceleration = DrawFloatField(firstColumnPosition, ref firstColumnYPos, data.acceleration, "acceleration");
                data.maxAcceleration = DrawFloatField(firstColumnPosition, ref firstColumnYPos, data.maxAcceleration, "maxAcceleration");
                data.accelerationTime = DrawFloatField(firstColumnPosition, ref firstColumnYPos, data.accelerationTime, "accelerationTime");

                //data.airAcceleration = DrawFloatField(firstColumnPosition, ref firstColumnYPos, data.airAcceleration, "airAcceleration");
                data.gravity = DrawFloatField(firstColumnPosition, ref firstColumnYPos, data.gravity, "gravity");

                data.jogJumpHeight = DrawFloatField(firstColumnPosition, ref firstColumnYPos, data.jogJumpHeight, "jogJumpHeight");
                data.runJumpHeight = DrawFloatField(firstColumnPosition, ref firstColumnYPos, data.runJumpHeight, "runJumpHeight");
                data.jumpGravityFactor = DrawFloatField(firstColumnPosition, ref firstColumnYPos, data.jumpGravityFactor, "jumpGravityFactor");

                //data.jumpPadForce = DrawFloatField(firstColumnPosition, ref firstColumnYPos, data.jumpPadForce, "jumpPadForce");
                data.jumpHeightStun = DrawFloatField(firstColumnPosition, ref firstColumnYPos, data.jumpHeightStun, "jumpHeightStun");
                data.longFallStunTime = DrawFloatField(firstColumnPosition, ref firstColumnYPos, data.longFallStunTime, "longFallStunTime");
                data.noSlipDistance = DrawFloatField(firstColumnPosition, ref firstColumnYPos, data.noSlipDistance, "noSlipDistance");
                data.slipSpeedMultiplier = DrawFloatField(firstColumnPosition, ref firstColumnYPos, data.slipSpeedMultiplier, "slipSpeedMultiplier");
                data.jumpVelocityDrag = DrawFloatField(firstColumnPosition, ref firstColumnYPos, data.jumpVelocityDrag, "jumpVelocityDrag");
                data.movAnimBlendSpeed = DrawFloatField(firstColumnPosition, ref firstColumnYPos, data.movAnimBlendSpeed, "movAnimBlendSpeed");
                data.characterControllerRadius = DrawFloatField(firstColumnPosition, ref firstColumnYPos, data.characterControllerRadius, "characterRadius");

                var secondColumnYPos = 0;
                DrawObjectValue(secondColumnPosition, ref secondColumnYPos, "State", characterState.SpeedState);
                DrawObjectValue(secondColumnPosition, ref secondColumnYPos, "velocity", characterState.TotalVelocity);
                DrawObjectValue(secondColumnPosition, ref secondColumnYPos, "isGrounded", characterState.IsGrounded);
                DrawObjectValue(secondColumnPosition, ref secondColumnYPos, "isFalling", characterState.IsJumping);
                DrawObjectValue(secondColumnPosition, ref secondColumnYPos, "ExternalImpulse", characterState.ExternalImpulse);
                DrawObjectValue(secondColumnPosition, ref secondColumnYPos, "ExternalVelocity", characterState.ExternalVelocity);
                DrawObjectValue(secondColumnPosition, ref secondColumnYPos, "currentAcceleration", characterState.currentAcceleration);*/
            }
        }

        private void DrawObjectValue(int xPos, ref int yPos, string label, object obj)
        {
            DrawLabel(xPos, ref yPos, label);

            GUI.Label(
                new Rect(Width(xPos + FIELD_HEIGHT + LABEL_SIZE), Height(FIELD_HEIGHT + yPos),
                    Width(500), Height(FIELD_HEIGHT)), obj.ToString());

            yPos += FIELD_HEIGHT + 2;
        }

        private float DrawFloatField(int xPos, ref int yPos, float value, string label)
        {
            if (!valuesTemp.ContainsKey(label))
                valuesTemp.Add(label, value.ToString());

            DrawLabel(xPos, ref yPos, label);
            string result = GUI.TextField(new Rect(Width(xPos + FIELD_HEIGHT + LABEL_SIZE), Height(FIELD_HEIGHT + yPos), Width(90), Height(FIELD_HEIGHT)), valuesTemp[label]);
            valuesTemp[label] = result;
            yPos += FIELD_HEIGHT + 2;
            return float.TryParse(result, out float newNumber) ? newNumber : value;
        }

        private void DrawLabel(int xPos, ref int yPos, string label)
        {
            GUI.Label(new Rect(Width(xPos + FIELD_HEIGHT), Height(FIELD_HEIGHT + yPos), Width(LABEL_SIZE), Height(FIELD_HEIGHT)), label);
            yPos += FIELD_HEIGHT + 2;
        }

        private float Width(float value) =>
            value * Screen.width / 1920f;

        private float Height(float value) =>
            value * Screen.height / 1080f;
    }

    public interface ICharacterView
    {
        (bool isGrounded, Vector3 deltaPosition, bool wallHit) Move(Vector3 delta);

        void SetForward(Vector3 forward);

        Vector3 GetPosition();

        (Vector3 center, float radius, float skinWidth, float height) GetCharacterControllerSettings();

        Vector3 GetSpherecastPosition();
    }
}