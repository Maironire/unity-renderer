using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using DCL;
using DCL.Controllers;
using DCL.CRDT;
using DCL.ECS7;
using DCL.ECSComponents;
using DCL.ECSRuntime;
using DCL.Models;
using Google.Protobuf;
using KernelCommunication;
using NSubstitute;
using NUnit.Framework;
using RPC;
using rpc_csharp;
using rpc_csharp.transport;
using RPC.Services;
using UnityEngine;
using UnityEngine.TestTools;
using BinaryWriter = KernelCommunication.BinaryWriter;
using Environment = DCL.Environment;

namespace Tests
{
    public class ECS7HotReload
    {

        [UnityTest]
        public IEnumerator HotReloadSceneCorrectly()
        {
            yield return UniTask.ToCoroutine(async () =>
            {
                const int SCENE_NUMBER = 666;
                const int ENTITY_ID = 500;
                const int COMPONENT_ID = ComponentID.MESH_RENDERER;
                IMessage COMPONENT_DATA = new PBMeshRenderer()
                {
                    Box = new PBMeshRenderer.Types.BoxMesh()
                    {
                        Uvs = { }
                    }
                };

                RPCContext context = DataStore.i.rpc.context;

                var (clientTransport, serverTransport) = MemoryTransport.Create();

                RpcServer<RPCContext> rpcServer = new RpcServer<RPCContext>();
                rpcServer.AttachTransport(serverTransport, context);

                rpcServer.SetHandler((port, t, c) =>
                {
                    CRDTServiceCodeGen.RegisterService(port, new CRDTServiceImpl());
                });

                try
                {
                    LoadEnvironment();
                    ECSComponentsManager componentsManager = LoadEcs7Dependencies();

                    context.crdt.MessagingControllersManager = Environment.i.messaging.manager;
                    
                    ClientCRDTService clientCrdtService = await CreateClientCrdtService(clientTransport);
                    await LoadScene(SCENE_NUMBER).ToCoroutine();

                    IParcelScene scene = Environment.i.world.state.GetScene(SCENE_NUMBER);
                    Assert.NotNull(scene);

                    CRDTMessage crdtCreateBoxMessage = new CRDTMessage()
                    {
                        key1 = ENTITY_ID,
                        key2 = COMPONENT_ID,
                        timestamp = 1,
                        data = ProtoSerialization.Serialize(COMPONENT_DATA)
                    };

                    await clientCrdtService.SendCrdt(new CRDTManyMessages()
                    {
                        SceneNumber = SCENE_NUMBER,
                        Payload = ByteString.CopyFrom(CreateCRDTMessage(crdtCreateBoxMessage))
                    });

                    IDCLEntity entity = scene.GetEntityById(ENTITY_ID);
                    Assert.NotNull(entity);

                    var component = componentsManager.GetComponent(COMPONENT_ID);
                    Assert.IsTrue(component.HasComponent(scene, entity));

                    // Do hot reload
                    await UnloadScene(SCENE_NUMBER);
                    await LoadScene(SCENE_NUMBER);

                    scene = Environment.i.world.state.GetScene(SCENE_NUMBER);
                    Assert.NotNull(scene);

                    crdtCreateBoxMessage = new CRDTMessage()
                    {
                        key1 = ENTITY_ID,
                        key2 = COMPONENT_ID,
                        timestamp = 1,
                        data = ProtoSerialization.Serialize(COMPONENT_DATA)
                    };

                    await clientCrdtService.SendCrdt(new CRDTManyMessages()
                    {
                        SceneNumber = SCENE_NUMBER,
                        Payload = ByteString.CopyFrom(CreateCRDTMessage(crdtCreateBoxMessage))
                    });

                    entity = scene.GetEntityById(ENTITY_ID);
                    Assert.NotNull(entity);

                    component = componentsManager.GetComponent(COMPONENT_ID);
                    Assert.IsTrue(component.HasComponent(scene, entity));

                }
                catch (Exception e)
                {
                    throw e;
                }
                finally
                {
                    rpcServer.Dispose();
                    DataStore.Clear();
                }
            });
        }

        private static ECSComponentsManager LoadEcs7Dependencies()
        {
            ISceneController sceneController = Environment.i.world.sceneController;
            Dictionary<int, ICRDTExecutor> crdtExecutors = new Dictionary<int, ICRDTExecutor>(1);

            ECSComponentsFactory componentsFactory = new ECSComponentsFactory();
            ECSComponentsManager componentsManager = new ECSComponentsManager(componentsFactory.componentBuilders);
            var crdtExecutorsManager = new CrdtExecutorsManager(crdtExecutors, componentsManager, sceneController,
                Environment.i.world.state, DataStore.i.rpc.context.crdt);
            var componentsComposer = new ECS7ComponentsComposer(componentsFactory,
                Substitute.For<IECSComponentWriter>(),
                Substitute.For<IInternalECSComponents>());
            return componentsManager;
        }

        private static void LoadEnvironment()
        {
            ServiceLocator serviceLocator = ServiceLocatorFactory.CreateDefault();
            Environment.Setup(serviceLocator);
            serviceLocator.Initialize();
        }

        private static async UniTask LoadScene(int sceneNumber)
        {
            LoadParcelScenesMessage.UnityParcelScene scene = new LoadParcelScenesMessage.UnityParcelScene()
            {
                basePosition = new Vector2Int(0, 0),
                parcels = new Vector2Int[] { new Vector2Int(0, 0) },
                sceneNumber = sceneNumber
            };

            Environment.i.world.sceneController.LoadParcelScenes(JsonUtility.ToJson(scene));

            var message = new QueuedSceneMessage_Scene
            {
                sceneNumber = scene.sceneNumber,
                tag = "",
                type = QueuedSceneMessage.Type.SCENE_MESSAGE,
                method = MessagingTypes.INIT_DONE,
                payload = new Protocol.SceneReady()
            };
            Environment.i.world.sceneController.EnqueueSceneMessage(message);

            await UniTask.WaitWhile(() => Environment.i.messaging.manager.HasScenePendingMessages(sceneNumber));
        }

        private static async UniTask UnloadScene(int sceneNumber)
        {
            Environment.i.world.sceneController.UnloadScene(sceneNumber);
            await UniTask.WaitWhile(() => Environment.i.messaging.manager.hasPendingMessages);
        }

        private static byte[] CreateCRDTMessage(CRDTMessage message)
        {
            using (MemoryStream msgStream = new MemoryStream())
            {
                using (BinaryWriter msgWriter = new BinaryWriter(msgStream))
                {
                    KernelBinaryMessageSerializer.Serialize(msgWriter, message);
                    return msgStream.ToArray();
                }
            }
        }
        
        static async UniTask<ClientCRDTService> CreateClientCrdtService(ITransport transport)
        {
            RpcClient client = new RpcClient(transport);
            RpcClientPort port = await client.CreatePort("test-port");
            RpcClientModule module = await port.LoadModule(CRDTServiceCodeGen.ServiceName);
            ClientCRDTService crdtService = new ClientCRDTService(module);
            return crdtService;
        }
    }
}