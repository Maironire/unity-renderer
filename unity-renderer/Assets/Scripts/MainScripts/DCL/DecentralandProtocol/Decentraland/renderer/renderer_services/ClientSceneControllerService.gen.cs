
// AUTOGENERATED, DO NOT EDIT
// Type definitions for server implementations of ports.
// package: decentraland.renderer.renderer_services
// file: decentraland/renderer/renderer_services/scene_controller.proto
using Cysharp.Threading.Tasks;
using rpc_csharp;

namespace Decentraland.Renderer.RendererServices {
public interface IClientRpcSceneControllerService
{
  UniTask<LoadSceneResult> LoadScene(LoadSceneMessage request);

  UniTask<UnloadSceneResult> UnloadScene(UnloadSceneMessage request);

  UniTask<CRDTSceneMessage> SendCrdt(CRDTSceneMessage request);

  UniTask<CRDTSceneCurrentState> GetCurrentState(GetCurrentStateMessage request);

  UniTask<SendBatchResponse> SendBatch(SendBatchRequest request);
}

public class ClientRpcSceneControllerService : IClientRpcSceneControllerService
{
  private readonly RpcClientModule module;

  public ClientRpcSceneControllerService(RpcClientModule module)
  {
      this.module = module;
  }

  
  public UniTask<LoadSceneResult> LoadScene(LoadSceneMessage request)
  {
      return module.CallUnaryProcedure<LoadSceneResult>("LoadScene", request);
  }

  public UniTask<UnloadSceneResult> UnloadScene(UnloadSceneMessage request)
  {
      return module.CallUnaryProcedure<UnloadSceneResult>("UnloadScene", request);
  }

  public UniTask<CRDTSceneMessage> SendCrdt(CRDTSceneMessage request)
  {
      return module.CallUnaryProcedure<CRDTSceneMessage>("SendCrdt", request);
  }

  public UniTask<CRDTSceneCurrentState> GetCurrentState(GetCurrentStateMessage request)
  {
      return module.CallUnaryProcedure<CRDTSceneCurrentState>("GetCurrentState", request);
  }

  public UniTask<SendBatchResponse> SendBatch(SendBatchRequest request)
  {
      return module.CallUnaryProcedure<SendBatchResponse>("SendBatch", request);
  }

}
}
