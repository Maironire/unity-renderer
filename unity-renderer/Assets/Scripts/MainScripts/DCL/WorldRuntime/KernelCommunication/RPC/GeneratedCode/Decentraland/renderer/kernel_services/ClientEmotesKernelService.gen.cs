
// AUTOGENERATED, DO NOT EDIT
// Type definitions for server implementations of ports.
// package: 
// file: decentraland/renderer/kernel_services/emotes_kernel.proto
using Cysharp.Threading.Tasks;
using rpc_csharp;

public class ClientEmotesKernelService
{
  private readonly RpcClientModule module;

  public ClientEmotesKernelService(RpcClientModule module)
  {
      this.module = module;
  }

  public UniTask<TriggerExpressionResponse> TriggerExpression(TriggerExpressionRequest request)
  {
      return module.CallUnaryProcedure<TriggerExpressionResponse>("TriggerExpression", request);
  }
}