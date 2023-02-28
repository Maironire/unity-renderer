import { RpcClient, RpcClientPort, Transport } from '@dcl/rpc'
import type { UnityGame } from '@dcl/unity-renderer/src/index'
import { profileToRendererFormat } from 'lib/decentraland/profiles/transformations/profileToRendererFormat'
import { NewProfileForRenderer } from 'lib/decentraland/profiles/transformations/types'
import defaultLogger from 'lib/logger'
import { call, fork, put, select, take, takeEvery, takeLatest } from 'redux-saga/effects'
import { createRendererRpcClient } from 'renderer-protocol/rpcClient'
import { registerEmotesService } from 'renderer-protocol/services/emotesService'
import { registerFriendRequestRendererService } from 'renderer-protocol/services/friendRequestService'
import { createRpcTransportService } from 'renderer-protocol/services/transportService'
import { trackEvent } from 'shared/analytics'
import { receivePeerUserData } from 'shared/comms/peers'
import { getExploreRealmsService } from 'shared/dao/selectors'
import { waitingForRenderer } from 'shared/loading/types'
import { getAllowedContentServer } from 'shared/meta/selectors'
import {
  addProfileToLastSentProfileVersionAndCatalog,
  SendProfileToRenderer,
  sendProfileToRenderer,
  SEND_PROFILE_TO_RENDERER_REQUEST
} from 'shared/profiles/actions'
import { getLastSentProfileVersion, getProfileFromStore } from 'shared/profiles/selectors'
import { SET_REALM_ADAPTER } from 'shared/realm/actions'
import { getFetchContentServerFromRealmAdapter, getFetchContentUrlPrefixFromRealmAdapter } from 'shared/realm/selectors'
import { IRealmAdapter } from 'shared/realm/types'
import { waitForRealm } from 'shared/realm/waitForRealmAdapter'
import { isFeatureToggleEnabled } from 'shared/selectors'
import { SignUpSetIsSignUp, SIGNUP_SET_IS_SIGNUP } from 'shared/session/actions'
import { getCurrentIdentity, getCurrentUserId } from 'shared/session/selectors'
import { RootState } from 'shared/store/rootTypes'
import { CurrentRealmInfoForRenderer, NotificationType, VOICE_CHAT_FEATURE_TOGGLE } from 'shared/types'
import {
  SetVoiceChatErrorAction,
  SET_VOICE_CHAT_ERROR,
  SET_VOICE_CHAT_HANDLER,
  VoicePlayingUpdateAction,
  VoiceRecordingUpdateAction,
  VOICE_PLAYING_UPDATE,
  VOICE_RECORDING_UPDATE
} from 'shared/voiceChat/actions'
import { getVoiceHandler } from 'shared/voiceChat/selectors'
import { VoiceHandler } from 'shared/voiceChat/VoiceHandler'
import { SetCurrentScene, SET_CURRENT_SCENE } from 'shared/world/actions'
import { getSceneWorkerBySceneID } from 'shared/world/parcelSceneManager'
import { SceneWorker } from 'shared/world/SceneWorker'
import { initializeEngine } from 'unity-interface/dcl'
import { getUnityInstance } from 'unity-interface/IUnityInterface'
import { InitializeRenderer, registerRendererModules, registerRendererPort, REGISTER_RPC_PORT } from './actions'
import { waitForRendererInstance } from './sagas-helper'
import { getClientPort } from './selectors'
import { RendererModules, RENDERER_INITIALIZE } from './types'

export function* rendererSaga() {
  yield takeEvery(SEND_PROFILE_TO_RENDERER_REQUEST, handleSubmitProfileToRenderer)
  yield takeLatest(SIGNUP_SET_IS_SIGNUP, sendSignUpToRenderer)
  yield takeEvery(VOICE_PLAYING_UPDATE, updateUserVoicePlayingRenderer)
  yield takeEvery(VOICE_RECORDING_UPDATE, updatePlayerVoiceRecordingRenderer)
  yield takeEvery(SET_VOICE_CHAT_ERROR, handleVoiceChatError)
  yield takeEvery(REGISTER_RPC_PORT, handleRegisterRpcPort)

  const action: InitializeRenderer = yield take(RENDERER_INITIALIZE)
  yield call(initializeRenderer, action)

  yield takeLatest(SET_CURRENT_SCENE, listenToWhetherSceneSupportsVoiceChat)

  yield fork(reportRealmChangeToRenderer)
  yield fork(updateChangeVoiceChatHandlerProcess)
}

/**
 * On every new port we register the services for it and starts the inverse RPC
 */
function* handleRegisterRpcPort() {
  const port: RpcClientPort | undefined = yield select(getClientPort)

  if (!port) {
    return
  }

  if (createRpcTransportService(port)) {
    const modules: RendererModules = {
      emotes: registerEmotesService(port),
      friendRequest: registerFriendRequestRendererService(port)
    }

    yield put(registerRendererModules(modules))
  }
}

/**
 * This saga sends the BFF configuration changes to the renderer upon every change
 */
function* reportRealmChangeToRenderer() {
  yield call(waitForRendererInstance)

  while (true) {
    const realmAdapter: IRealmAdapter = yield call(waitForRealm)

    try {
      const configuredContentServer: string = getFetchContentServerFromRealmAdapter(realmAdapter)
      const contentServerUrl: string = yield select(getAllowedContentServer, configuredContentServer)
      const current = convertCurrentRealmType(realmAdapter, contentServerUrl)
      defaultLogger.info('UpdateRealmsInfo', current)
      getUnityInstance().UpdateRealmsInfo({ current })
      getUnityInstance().UpdateRealmAbout(realmAdapter.about)

      const realmsService = yield select(getExploreRealmsService)

      if (realmsService) {
        yield call(fetchAndReportRealmsInfo, realmsService)
      }

      // wait for the next context
    } catch (err: any) {
      defaultLogger.error(err)
    }

    yield take(SET_REALM_ADAPTER)
  }
}

async function fetchAndReportRealmsInfo(url: string) {
  try {
    const response = await fetch(url)
    if (response.ok) {
      const value = await response.json()
      getUnityInstance().UpdateRealmsInfo({ realms: value })
    }
  } catch (e) {
    defaultLogger.error(url, e)
  }
}

function convertCurrentRealmType(realmAdapter: IRealmAdapter, contentServerUrl: string): CurrentRealmInfoForRenderer {
  return {
    serverName: realmAdapter.about.configurations?.realmName || realmAdapter.baseUrl,
    layer: '',
    domain: realmAdapter.baseUrl,
    contentServerUrl: contentServerUrl
  }
}

function* updateUserVoicePlayingRenderer(action: VoicePlayingUpdateAction) {
  const { playing, userId } = action.payload
  yield call(waitForRendererInstance)
  getUnityInstance().SetUserTalking(userId, playing)
}

function* updatePlayerVoiceRecordingRenderer(action: VoiceRecordingUpdateAction) {
  yield call(waitForRendererInstance)
  getUnityInstance().SetPlayerTalking(action.payload.recording)
}

function* updateChangeVoiceChatHandlerProcess() {
  let prevHandler: VoiceHandler | undefined = undefined
  while (true) {
    // wait for a new VoiceHandler
    yield take(SET_VOICE_CHAT_HANDLER)

    const handler: VoiceHandler | undefined = yield select(getVoiceHandler)

    if (handler !== prevHandler) {
      if (prevHandler) {
        yield prevHandler.destroy()
      }
      prevHandler = handler
    }

    yield call(waitForRendererInstance)

    if (handler) {
      getUnityInstance().SetVoiceChatStatus({ isConnected: true })
    } else {
      getUnityInstance().SetVoiceChatStatus({ isConnected: false })
    }
  }
}

function* handleVoiceChatError(action: SetVoiceChatErrorAction) {
  const message = action.payload.message
  yield call(waitForRendererInstance)
  if (message) {
    getUnityInstance().ShowNotification({
      type: NotificationType.GENERIC,
      message,
      buttonMessage: 'OK',
      timer: 5
    })
  }
}

function* listenToWhetherSceneSupportsVoiceChat(data: SetCurrentScene) {
  const currentScene: SceneWorker | undefined = data.payload.currentScene
    ? yield call(getSceneWorkerBySceneID, data.payload.currentScene)
    : undefined

  const nowEnabled = currentScene
    ? isFeatureToggleEnabled(VOICE_CHAT_FEATURE_TOGGLE, currentScene?.metadata)
    : isFeatureToggleEnabled(VOICE_CHAT_FEATURE_TOGGLE)

  yield call(waitForRendererInstance)

  getUnityInstance().SetVoiceChatEnabledByScene(nowEnabled)
}

function* initializeRenderer(action: InitializeRenderer) {
  const { delegate, container } = action.payload

  // will start loading
  yield put(waitingForRenderer())

  // start loading the renderer
  try {
    const { renderer, transport }: { renderer: UnityGame; transport: Transport } = yield call(delegate, container)

    const startTime = performance.now()

    trackEvent('renderer_initializing_start', {})

    // register the RPC port
    const rpcHandles: {
      rpcClient: RpcClient
      rendererInterfacePort: RpcClientPort
    } = yield call(createRendererRpcClient, transport)
    yield put(registerRendererPort(rpcHandles.rpcClient, rpcHandles.rendererInterfacePort))

    // wire the kernel to the renderer, at some point, the `initializeEngine`
    // function _MUST_ send the `signalRendererInitializedCorrectly` action
    // to signal that the renderer successfuly loaded
    yield call(initializeEngine, renderer)

    // wait for renderer start
    yield call(waitForRendererInstance)

    trackEvent('renderer_initializing_end', {
      loading_time: performance.now() - startTime
    })
  } catch (e) {
    trackEvent('renderer_initialization_error', {
      message: e + ''
    })
    if (e instanceof Error) {
      throw e
    } else {
      throw new Error('Error initializing rendering')
    }
  }
}

function* sendSignUpToRenderer(action: SignUpSetIsSignUp) {
  if (action.payload.isSignUp) {
    getUnityInstance().ShowAvatarEditorInSignIn()

    const userId: string = yield select(getCurrentUserId)
    yield put(sendProfileToRenderer(userId))
  }
}

let lastSentProfile: NewProfileForRenderer | null = null
export function* handleSubmitProfileToRenderer(action: SendProfileToRenderer): any {
  const { userId, forceSend } = action.payload

  yield call(waitForRendererInstance)
  const bff: IRealmAdapter = yield call(waitForRealm)
  const { profile, identity, isCurrentUser, lastSentProfileVersion } = yield select(
    getInformationToSubmitProfileFromStore,
    userId
  )

  if (!profile || !profile.data) {
    return
  }
  const fetchContentServerWithPrefix = getFetchContentUrlPrefixFromRealmAdapter(bff)

  if (isCurrentUser) {
    const forRenderer = profileToRendererFormat(profile.data, {
      address: identity.address,
      baseUrl: fetchContentServerWithPrefix
    })
    forRenderer.hasConnectedWeb3 = identity.hasConnectedWeb3

    if (lastSentProfile && lastSentProfile.version < forRenderer.version) {
      queueMicrotask(() => {
        getUnityInstance().LoadProfile(forRenderer)
      })
      lastSentProfile = forRenderer
    }
  } else {
    const alreadySentWithVersion =
      typeof lastSentProfileVersion === 'number' && lastSentProfileVersion >= profile.data.version

    if (!alreadySentWithVersion || forceSend) {
      const profileInRendererFormat = profileToRendererFormat(profile.data, {
        baseUrl: fetchContentServerWithPrefix
      })

      queueMicrotask(() => {
        getUnityInstance().AddUserProfileToCatalog(profileInRendererFormat)
      })
      yield put(addProfileToLastSentProfileVersionAndCatalog(userId, profileInRendererFormat.version))
    }

    // Send to Avatars scene
    // TODO: Consider refactor this so that it's distinguishable from a message received over the network
    // (`receivePeerUserData` is a handler for a comms message!!!)
    receivePeerUserData(profile.data, fetchContentServerWithPrefix)
  }
}

function getInformationToSubmitProfileFromStore(state: RootState, userId: string) {
  const identity = getCurrentIdentity(state)
  const isCurrentUser = identity?.address.toLowerCase() === userId.toLowerCase()
  return {
    profile: getProfileFromStore(state, userId),
    identity,
    isCurrentUser,
    lastSentProfileVersion: getLastSentProfileVersion(state, userId)
  }
}
