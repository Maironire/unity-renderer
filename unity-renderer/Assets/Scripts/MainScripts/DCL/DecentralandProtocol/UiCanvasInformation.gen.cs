// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: decentraland/sdk/components/ui_canvas_information.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace DCL.ECSComponents {

  /// <summary>Holder for reflection information generated from decentraland/sdk/components/ui_canvas_information.proto</summary>
  public static partial class UiCanvasInformationReflection {

    #region Descriptor
    /// <summary>File descriptor for decentraland/sdk/components/ui_canvas_information.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static UiCanvasInformationReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "CjdkZWNlbnRyYWxhbmQvc2RrL2NvbXBvbmVudHMvdWlfY2FudmFzX2luZm9y",
            "bWF0aW9uLnByb3RvEhtkZWNlbnRyYWxhbmQuc2RrLmNvbXBvbmVudHMaJWRl",
            "Y2VudHJhbGFuZC9jb21tb24vYm9yZGVyX3JlY3QucHJvdG8ijgEKFVBCVWlD",
            "YW52YXNJbmZvcm1hdGlvbhIaChJkZXZpY2VfcGl4ZWxfcmF0aW8YASABKAIS",
            "DQoFd2lkdGgYAiABKAUSDgoGaGVpZ2h0GAMgASgFEjoKEWludGVyYWN0YWJs",
            "ZV9hcmVhGAQgASgLMh8uZGVjZW50cmFsYW5kLmNvbW1vbi5Cb3JkZXJSZWN0",
            "QhSqAhFEQ0wuRUNTQ29tcG9uZW50c2IGcHJvdG8z"));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { global::Decentraland.Common.BorderRectReflection.Descriptor, },
          new pbr::GeneratedClrTypeInfo(null, null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::DCL.ECSComponents.PBUiCanvasInformation), global::DCL.ECSComponents.PBUiCanvasInformation.Parser, new[]{ "DevicePixelRatio", "Width", "Height", "InteractableArea" }, null, null, null, null)
          }));
    }
    #endregion

  }
  #region Messages
  /// <summary>
  /// This component is created by the renderer and used by the scenes to know the resolution of the UI canvas
  /// </summary>
  public sealed partial class PBUiCanvasInformation : pb::IMessage<PBUiCanvasInformation>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<PBUiCanvasInformation> _parser = new pb::MessageParser<PBUiCanvasInformation>(() => new PBUiCanvasInformation());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pb::MessageParser<PBUiCanvasInformation> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::DCL.ECSComponents.UiCanvasInformationReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public PBUiCanvasInformation() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public PBUiCanvasInformation(PBUiCanvasInformation other) : this() {
      devicePixelRatio_ = other.devicePixelRatio_;
      width_ = other.width_;
      height_ = other.height_;
      interactableArea_ = other.interactableArea_ != null ? other.interactableArea_.Clone() : null;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public PBUiCanvasInformation Clone() {
      return new PBUiCanvasInformation(this);
    }

    /// <summary>Field number for the "device_pixel_ratio" field.</summary>
    public const int DevicePixelRatioFieldNumber = 1;
    private float devicePixelRatio_;
    /// <summary>
    /// informs the scene about the resolution used for the UI rendering
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public float DevicePixelRatio {
      get { return devicePixelRatio_; }
      set {
        devicePixelRatio_ = value;
      }
    }

    /// <summary>Field number for the "width" field.</summary>
    public const int WidthFieldNumber = 2;
    private int width_;
    /// <summary>
    /// informs about the width of the canvas, in virtual pixels. this value does not change when the pixel ratio changes.
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int Width {
      get { return width_; }
      set {
        width_ = value;
      }
    }

    /// <summary>Field number for the "height" field.</summary>
    public const int HeightFieldNumber = 3;
    private int height_;
    /// <summary>
    /// informs about the height of the canvas, in virtual pixels. this value does not change when the pixel ratio changes.
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int Height {
      get { return height_; }
      set {
        height_ = value;
      }
    }

    /// <summary>Field number for the "interactable_area" field.</summary>
    public const int InteractableAreaFieldNumber = 4;
    private global::Decentraland.Common.BorderRect interactableArea_;
    /// <summary>
    /// informs the sdk about the interactable area. some implementations may change
    /// this area depending on the HUD that is being shown. this value may change at
    /// any time by the Renderer to create reactive UIs. as an example, an explorer with the
    /// chat UI hidden and with no minimap could have a rect that covers the whole screen.
    /// on the contrary, if the chat UI is shown, the rect would be smaller.
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public global::Decentraland.Common.BorderRect InteractableArea {
      get { return interactableArea_; }
      set {
        interactableArea_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override bool Equals(object other) {
      return Equals(other as PBUiCanvasInformation);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Equals(PBUiCanvasInformation other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (!pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.Equals(DevicePixelRatio, other.DevicePixelRatio)) return false;
      if (Width != other.Width) return false;
      if (Height != other.Height) return false;
      if (!object.Equals(InteractableArea, other.InteractableArea)) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override int GetHashCode() {
      int hash = 1;
      if (DevicePixelRatio != 0F) hash ^= pbc::ProtobufEqualityComparers.BitwiseSingleEqualityComparer.GetHashCode(DevicePixelRatio);
      if (Width != 0) hash ^= Width.GetHashCode();
      if (Height != 0) hash ^= Height.GetHashCode();
      if (interactableArea_ != null) hash ^= InteractableArea.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void WriteTo(pb::CodedOutputStream output) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      output.WriteRawMessage(this);
    #else
      if (DevicePixelRatio != 0F) {
        output.WriteRawTag(13);
        output.WriteFloat(DevicePixelRatio);
      }
      if (Width != 0) {
        output.WriteRawTag(16);
        output.WriteInt32(Width);
      }
      if (Height != 0) {
        output.WriteRawTag(24);
        output.WriteInt32(Height);
      }
      if (interactableArea_ != null) {
        output.WriteRawTag(34);
        output.WriteMessage(InteractableArea);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalWriteTo(ref pb::WriteContext output) {
      if (DevicePixelRatio != 0F) {
        output.WriteRawTag(13);
        output.WriteFloat(DevicePixelRatio);
      }
      if (Width != 0) {
        output.WriteRawTag(16);
        output.WriteInt32(Width);
      }
      if (Height != 0) {
        output.WriteRawTag(24);
        output.WriteInt32(Height);
      }
      if (interactableArea_ != null) {
        output.WriteRawTag(34);
        output.WriteMessage(InteractableArea);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(ref output);
      }
    }
    #endif

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int CalculateSize() {
      int size = 0;
      if (DevicePixelRatio != 0F) {
        size += 1 + 4;
      }
      if (Width != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(Width);
      }
      if (Height != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(Height);
      }
      if (interactableArea_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(InteractableArea);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(PBUiCanvasInformation other) {
      if (other == null) {
        return;
      }
      if (other.DevicePixelRatio != 0F) {
        DevicePixelRatio = other.DevicePixelRatio;
      }
      if (other.Width != 0) {
        Width = other.Width;
      }
      if (other.Height != 0) {
        Height = other.Height;
      }
      if (other.interactableArea_ != null) {
        if (interactableArea_ == null) {
          InteractableArea = new global::Decentraland.Common.BorderRect();
        }
        InteractableArea.MergeFrom(other.InteractableArea);
      }
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(pb::CodedInputStream input) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      input.ReadRawMessage(this);
    #else
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 13: {
            DevicePixelRatio = input.ReadFloat();
            break;
          }
          case 16: {
            Width = input.ReadInt32();
            break;
          }
          case 24: {
            Height = input.ReadInt32();
            break;
          }
          case 34: {
            if (interactableArea_ == null) {
              InteractableArea = new global::Decentraland.Common.BorderRect();
            }
            input.ReadMessage(InteractableArea);
            break;
          }
        }
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalMergeFrom(ref pb::ParseContext input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
            break;
          case 13: {
            DevicePixelRatio = input.ReadFloat();
            break;
          }
          case 16: {
            Width = input.ReadInt32();
            break;
          }
          case 24: {
            Height = input.ReadInt32();
            break;
          }
          case 34: {
            if (interactableArea_ == null) {
              InteractableArea = new global::Decentraland.Common.BorderRect();
            }
            input.ReadMessage(InteractableArea);
            break;
          }
        }
      }
    }
    #endif

  }

  #endregion

}

#endregion Designer generated code