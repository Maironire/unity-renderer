// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: AvatarModifierArea.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace DCL.ECSComponents {

  /// <summary>Holder for reflection information generated from AvatarModifierArea.proto</summary>
  public static partial class AvatarModifierAreaReflection {

    #region Descriptor
    /// <summary>File descriptor for AvatarModifierArea.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static AvatarModifierAreaReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "ChhBdmF0YXJNb2RpZmllckFyZWEucHJvdG8SEGRlY2VudHJhbGFuZC5lY3Ma",
            "FGNvbW1vbi9WZWN0b3IzLnByb3RvIngKFFBCQXZhdGFyTW9kaWZpZXJBcmVh",
            "EhYKBGFyZWEYASABKAsyCC5WZWN0b3IzEhMKC2V4Y2x1ZGVfaWRzGAIgAygJ",
            "EjMKCW1vZGlmaWVycxgDIAMoDjIgLmRlY2VudHJhbGFuZC5lY3MuQXZhdGFy",
            "TW9kaWZpZXIqOQoOQXZhdGFyTW9kaWZpZXISEAoMSElERV9BVkFUQVJTEAAS",
            "FQoRRElTQUJMRV9QQVNTUE9SVFMQAUIUqgIRRENMLkVDU0NvbXBvbmVudHNi",
            "BnByb3RvMw=="));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { global::Vector3Reflection.Descriptor, },
          new pbr::GeneratedClrTypeInfo(new[] {typeof(global::DCL.ECSComponents.AvatarModifier), }, null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::DCL.ECSComponents.PBAvatarModifierArea), global::DCL.ECSComponents.PBAvatarModifierArea.Parser, new[]{ "Area", "ExcludeIds", "Modifiers" }, null, null, null, null)
          }));
    }
    #endregion

  }
  #region Enums
  public enum AvatarModifier {
    [pbr::OriginalName("HIDE_AVATARS")] HideAvatars = 0,
    [pbr::OriginalName("DISABLE_PASSPORTS")] DisablePassports = 1,
  }

  #endregion

  #region Messages
  public sealed partial class PBAvatarModifierArea : pb::IMessage<PBAvatarModifierArea>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<PBAvatarModifierArea> _parser = new pb::MessageParser<PBAvatarModifierArea>(() => new PBAvatarModifierArea());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pb::MessageParser<PBAvatarModifierArea> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::DCL.ECSComponents.AvatarModifierAreaReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public PBAvatarModifierArea() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public PBAvatarModifierArea(PBAvatarModifierArea other) : this() {
      area_ = other.area_ != null ? other.area_.Clone() : null;
      excludeIds_ = other.excludeIds_.Clone();
      modifiers_ = other.modifiers_.Clone();
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public PBAvatarModifierArea Clone() {
      return new PBAvatarModifierArea(this);
    }

    /// <summary>Field number for the "area" field.</summary>
    public const int AreaFieldNumber = 1;
    private global::Vector3 area_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public global::Vector3 Area {
      get { return area_; }
      set {
        area_ = value;
      }
    }

    /// <summary>Field number for the "exclude_ids" field.</summary>
    public const int ExcludeIdsFieldNumber = 2;
    private static readonly pb::FieldCodec<string> _repeated_excludeIds_codec
        = pb::FieldCodec.ForString(18);
    private readonly pbc::RepeatedField<string> excludeIds_ = new pbc::RepeatedField<string>();
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public pbc::RepeatedField<string> ExcludeIds {
      get { return excludeIds_; }
    }

    /// <summary>Field number for the "modifiers" field.</summary>
    public const int ModifiersFieldNumber = 3;
    private static readonly pb::FieldCodec<global::DCL.ECSComponents.AvatarModifier> _repeated_modifiers_codec
        = pb::FieldCodec.ForEnum(26, x => (int) x, x => (global::DCL.ECSComponents.AvatarModifier) x);
    private readonly pbc::RepeatedField<global::DCL.ECSComponents.AvatarModifier> modifiers_ = new pbc::RepeatedField<global::DCL.ECSComponents.AvatarModifier>();
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public pbc::RepeatedField<global::DCL.ECSComponents.AvatarModifier> Modifiers {
      get { return modifiers_; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override bool Equals(object other) {
      return Equals(other as PBAvatarModifierArea);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Equals(PBAvatarModifierArea other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (!object.Equals(Area, other.Area)) return false;
      if(!excludeIds_.Equals(other.excludeIds_)) return false;
      if(!modifiers_.Equals(other.modifiers_)) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override int GetHashCode() {
      int hash = 1;
      if (area_ != null) hash ^= Area.GetHashCode();
      hash ^= excludeIds_.GetHashCode();
      hash ^= modifiers_.GetHashCode();
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
      if (area_ != null) {
        output.WriteRawTag(10);
        output.WriteMessage(Area);
      }
      excludeIds_.WriteTo(output, _repeated_excludeIds_codec);
      modifiers_.WriteTo(output, _repeated_modifiers_codec);
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalWriteTo(ref pb::WriteContext output) {
      if (area_ != null) {
        output.WriteRawTag(10);
        output.WriteMessage(Area);
      }
      excludeIds_.WriteTo(ref output, _repeated_excludeIds_codec);
      modifiers_.WriteTo(ref output, _repeated_modifiers_codec);
      if (_unknownFields != null) {
        _unknownFields.WriteTo(ref output);
      }
    }
    #endif

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int CalculateSize() {
      int size = 0;
      if (area_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(Area);
      }
      size += excludeIds_.CalculateSize(_repeated_excludeIds_codec);
      size += modifiers_.CalculateSize(_repeated_modifiers_codec);
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(PBAvatarModifierArea other) {
      if (other == null) {
        return;
      }
      if (other.area_ != null) {
        if (area_ == null) {
          Area = new global::Vector3();
        }
        Area.MergeFrom(other.Area);
      }
      excludeIds_.Add(other.excludeIds_);
      modifiers_.Add(other.modifiers_);
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
          case 10: {
            if (area_ == null) {
              Area = new global::Vector3();
            }
            input.ReadMessage(Area);
            break;
          }
          case 18: {
            excludeIds_.AddEntriesFrom(input, _repeated_excludeIds_codec);
            break;
          }
          case 26:
          case 24: {
            modifiers_.AddEntriesFrom(input, _repeated_modifiers_codec);
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
          case 10: {
            if (area_ == null) {
              Area = new global::Vector3();
            }
            input.ReadMessage(Area);
            break;
          }
          case 18: {
            excludeIds_.AddEntriesFrom(ref input, _repeated_excludeIds_codec);
            break;
          }
          case 26:
          case 24: {
            modifiers_.AddEntriesFrom(ref input, _repeated_modifiers_codec);
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
