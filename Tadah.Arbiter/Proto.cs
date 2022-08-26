// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: Resources/Tadah.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace Tadah.Proto {

  /// <summary>Holder for reflection information generated from Resources/Tadah.proto</summary>
  public static partial class TadahReflection {

    #region Descriptor
    /// <summary>File descriptor for Resources/Tadah.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static TadahReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "ChVSZXNvdXJjZXMvVGFkYWgucHJvdG8SBVRhZGFoGh9nb29nbGUvcHJvdG9i",
            "dWYvdGltZXN0YW1wLnByb3RvIvYCCgZTaWduYWwSKQoFbm9uY2UYASABKAsy",
            "Gi5nb29nbGUucHJvdG9idWYuVGltZXN0YW1wEg0KBWpvYklkGAIgASgJEiMK",
            "CW9wZXJhdGlvbhgDIAEoDjIQLlRhZGFoLk9wZXJhdGlvbhIlCgd2ZXJzaW9u",
            "GAQgASgOMhQuVGFkYWguQ2xpZW50VmVyc2lvbhIiCgVwbGFjZRgFIAMoCzIT",
            "LlRhZGFoLlNpZ25hbC5QbGFjZRIqCgl0aHVtYm5haWwYBiADKAsyFy5UYWRh",
            "aC5TaWduYWwuVGh1bWJuYWlsGkUKBVBsYWNlEg8KB3BsYWNlSWQYASABKA0S",
            "DgoGc2NyaXB0GAIgASgJEhsKE2V4cGlyYXRpb25JblNlY29uZHMYAyABKA0a",
            "TwoJVGh1bWJuYWlsEh4KBHR5cGUYASABKA4yEC5UYWRhaC5Bc3NldFR5cGUS",
            "DwoHYXNzZXRJZBgCIAEoDRIRCglhY2Nlc3NLZXkYAyABKAkiXwoIUmVzcG9u",
            "c2USIwoJb3BlcmF0aW9uGAEgASgOMhAuVGFkYWguT3BlcmF0aW9uEg8KB3N1",
            "Y2Nlc3MYAiABKAgSDwoHbWVzc2FnZRgDIAEoCRIMCgRkYXRhGAQgASgJKpkB",
            "CglPcGVyYXRpb24SDAoIT1BFTl9KT0IQABINCglDTE9TRV9KT0IQARISCg5F",
            "WEVDVVRFX1NDUklQVBACEhkKFVJFTkVXX1RBTVBBX0pPQl9MRUFTRRADEhIK",
            "DkNMT1NFX0FMTF9KT0JTEAQSHQoZQ0xPU0VfQUxMX1RBTVBBX1BST0NFU1NF",
            "UxAFEg0KCVRIVU1CTkFJTBAGKjIKDUNsaWVudFZlcnNpb24SCAoETk9ORRAA",
            "EgsKBlRBSVBFSRDbDxIKCgVUQU1QQRDgDypZCglBc3NldFR5cGUSDAoIQ0xP",
            "VEhJTkcQABIICgRIRUFEEAESCAoETUVTSBACEgkKBVBMQUNFEAMSCAoEVVNF",
            "UhAEEgwKCEhFQURTSE9UEAUSBwoDWE1MEAZCDqoCC1RhZGFoLlByb3RvYgZw",
            "cm90bzM="));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { global::Google.Protobuf.WellKnownTypes.TimestampReflection.Descriptor, },
          new pbr::GeneratedClrTypeInfo(new[] {typeof(global::Tadah.Proto.Operation), typeof(global::Tadah.Proto.ClientVersion), typeof(global::Tadah.Proto.AssetType), }, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::Tadah.Proto.Signal), global::Tadah.Proto.Signal.Parser, new[]{ "Nonce", "JobId", "Operation", "Version", "Place", "Thumbnail" }, null, null, new pbr::GeneratedClrTypeInfo[] { new pbr::GeneratedClrTypeInfo(typeof(global::Tadah.Proto.Signal.Types.Place), global::Tadah.Proto.Signal.Types.Place.Parser, new[]{ "PlaceId", "Script", "ExpirationInSeconds" }, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::Tadah.Proto.Signal.Types.Thumbnail), global::Tadah.Proto.Signal.Types.Thumbnail.Parser, new[]{ "Type", "AssetId", "AccessKey" }, null, null, null)}),
            new pbr::GeneratedClrTypeInfo(typeof(global::Tadah.Proto.Response), global::Tadah.Proto.Response.Parser, new[]{ "Operation", "Success", "Message", "Data" }, null, null, null)
          }));
    }
    #endregion

  }
  #region Enums
  public enum Operation {
    [pbr::OriginalName("OPEN_JOB")] OpenJob = 0,
    [pbr::OriginalName("CLOSE_JOB")] CloseJob = 1,
    [pbr::OriginalName("EXECUTE_SCRIPT")] ExecuteScript = 2,
    [pbr::OriginalName("RENEW_TAMPA_JOB_LEASE")] RenewTampaJobLease = 3,
    [pbr::OriginalName("CLOSE_ALL_JOBS")] CloseAllJobs = 4,
    [pbr::OriginalName("CLOSE_ALL_TAMPA_PROCESSES")] CloseAllTampaProcesses = 5,
    [pbr::OriginalName("THUMBNAIL")] Thumbnail = 6,
  }

  public enum ClientVersion {
    [pbr::OriginalName("NONE")] None = 0,
    [pbr::OriginalName("TAIPEI")] Taipei = 2011,
    [pbr::OriginalName("TAMPA")] Tampa = 2016,
  }

  public enum AssetType {
    [pbr::OriginalName("CLOTHING")] Clothing = 0,
    [pbr::OriginalName("HEAD")] Head = 1,
    [pbr::OriginalName("MESH")] Mesh = 2,
    [pbr::OriginalName("PLACE")] Place = 3,
    [pbr::OriginalName("USER")] User = 4,
    [pbr::OriginalName("HEADSHOT")] Headshot = 5,
    [pbr::OriginalName("XML")] Xml = 6,
  }

  #endregion

  #region Messages
  public sealed partial class Signal : pb::IMessage<Signal> {
    private static readonly pb::MessageParser<Signal> _parser = new pb::MessageParser<Signal>(() => new Signal());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<Signal> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Tadah.Proto.TadahReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Signal() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Signal(Signal other) : this() {
      nonce_ = other.nonce_ != null ? other.nonce_.Clone() : null;
      jobId_ = other.jobId_;
      operation_ = other.operation_;
      version_ = other.version_;
      place_ = other.place_.Clone();
      thumbnail_ = other.thumbnail_.Clone();
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Signal Clone() {
      return new Signal(this);
    }

    /// <summary>Field number for the "nonce" field.</summary>
    public const int NonceFieldNumber = 1;
    private global::Google.Protobuf.WellKnownTypes.Timestamp nonce_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::Google.Protobuf.WellKnownTypes.Timestamp Nonce {
      get { return nonce_; }
      set {
        nonce_ = value;
      }
    }

    /// <summary>Field number for the "jobId" field.</summary>
    public const int JobIdFieldNumber = 2;
    private string jobId_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string JobId {
      get { return jobId_; }
      set {
        jobId_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "operation" field.</summary>
    public const int OperationFieldNumber = 3;
    private global::Tadah.Proto.Operation operation_ = 0;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::Tadah.Proto.Operation Operation {
      get { return operation_; }
      set {
        operation_ = value;
      }
    }

    /// <summary>Field number for the "version" field.</summary>
    public const int VersionFieldNumber = 4;
    private global::Tadah.Proto.ClientVersion version_ = 0;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::Tadah.Proto.ClientVersion Version {
      get { return version_; }
      set {
        version_ = value;
      }
    }

    /// <summary>Field number for the "place" field.</summary>
    public const int PlaceFieldNumber = 5;
    private static readonly pb::FieldCodec<global::Tadah.Proto.Signal.Types.Place> _repeated_place_codec
        = pb::FieldCodec.ForMessage(42, global::Tadah.Proto.Signal.Types.Place.Parser);
    private readonly pbc::RepeatedField<global::Tadah.Proto.Signal.Types.Place> place_ = new pbc::RepeatedField<global::Tadah.Proto.Signal.Types.Place>();
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public pbc::RepeatedField<global::Tadah.Proto.Signal.Types.Place> Place {
      get { return place_; }
    }

    /// <summary>Field number for the "thumbnail" field.</summary>
    public const int ThumbnailFieldNumber = 6;
    private static readonly pb::FieldCodec<global::Tadah.Proto.Signal.Types.Thumbnail> _repeated_thumbnail_codec
        = pb::FieldCodec.ForMessage(50, global::Tadah.Proto.Signal.Types.Thumbnail.Parser);
    private readonly pbc::RepeatedField<global::Tadah.Proto.Signal.Types.Thumbnail> thumbnail_ = new pbc::RepeatedField<global::Tadah.Proto.Signal.Types.Thumbnail>();
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public pbc::RepeatedField<global::Tadah.Proto.Signal.Types.Thumbnail> Thumbnail {
      get { return thumbnail_; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as Signal);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(Signal other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (!object.Equals(Nonce, other.Nonce)) return false;
      if (JobId != other.JobId) return false;
      if (Operation != other.Operation) return false;
      if (Version != other.Version) return false;
      if(!place_.Equals(other.place_)) return false;
      if(!thumbnail_.Equals(other.thumbnail_)) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (nonce_ != null) hash ^= Nonce.GetHashCode();
      if (JobId.Length != 0) hash ^= JobId.GetHashCode();
      if (Operation != 0) hash ^= Operation.GetHashCode();
      if (Version != 0) hash ^= Version.GetHashCode();
      hash ^= place_.GetHashCode();
      hash ^= thumbnail_.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      if (nonce_ != null) {
        output.WriteRawTag(10);
        output.WriteMessage(Nonce);
      }
      if (JobId.Length != 0) {
        output.WriteRawTag(18);
        output.WriteString(JobId);
      }
      if (Operation != 0) {
        output.WriteRawTag(24);
        output.WriteEnum((int) Operation);
      }
      if (Version != 0) {
        output.WriteRawTag(32);
        output.WriteEnum((int) Version);
      }
      place_.WriteTo(output, _repeated_place_codec);
      thumbnail_.WriteTo(output, _repeated_thumbnail_codec);
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (nonce_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(Nonce);
      }
      if (JobId.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(JobId);
      }
      if (Operation != 0) {
        size += 1 + pb::CodedOutputStream.ComputeEnumSize((int) Operation);
      }
      if (Version != 0) {
        size += 1 + pb::CodedOutputStream.ComputeEnumSize((int) Version);
      }
      size += place_.CalculateSize(_repeated_place_codec);
      size += thumbnail_.CalculateSize(_repeated_thumbnail_codec);
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(Signal other) {
      if (other == null) {
        return;
      }
      if (other.nonce_ != null) {
        if (nonce_ == null) {
          nonce_ = new global::Google.Protobuf.WellKnownTypes.Timestamp();
        }
        Nonce.MergeFrom(other.Nonce);
      }
      if (other.JobId.Length != 0) {
        JobId = other.JobId;
      }
      if (other.Operation != 0) {
        Operation = other.Operation;
      }
      if (other.Version != 0) {
        Version = other.Version;
      }
      place_.Add(other.place_);
      thumbnail_.Add(other.thumbnail_);
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 10: {
            if (nonce_ == null) {
              nonce_ = new global::Google.Protobuf.WellKnownTypes.Timestamp();
            }
            input.ReadMessage(nonce_);
            break;
          }
          case 18: {
            JobId = input.ReadString();
            break;
          }
          case 24: {
            operation_ = (global::Tadah.Proto.Operation) input.ReadEnum();
            break;
          }
          case 32: {
            version_ = (global::Tadah.Proto.ClientVersion) input.ReadEnum();
            break;
          }
          case 42: {
            place_.AddEntriesFrom(input, _repeated_place_codec);
            break;
          }
          case 50: {
            thumbnail_.AddEntriesFrom(input, _repeated_thumbnail_codec);
            break;
          }
        }
      }
    }

    #region Nested types
    /// <summary>Container for nested types declared in the Signal message type.</summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static partial class Types {
      public sealed partial class Place : pb::IMessage<Place> {
        private static readonly pb::MessageParser<Place> _parser = new pb::MessageParser<Place>(() => new Place());
        private pb::UnknownFieldSet _unknownFields;
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public static pb::MessageParser<Place> Parser { get { return _parser; } }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public static pbr::MessageDescriptor Descriptor {
          get { return global::Tadah.Proto.Signal.Descriptor.NestedTypes[0]; }
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        pbr::MessageDescriptor pb::IMessage.Descriptor {
          get { return Descriptor; }
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public Place() {
          OnConstruction();
        }

        partial void OnConstruction();

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public Place(Place other) : this() {
          placeId_ = other.placeId_;
          script_ = other.script_;
          expirationInSeconds_ = other.expirationInSeconds_;
          _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public Place Clone() {
          return new Place(this);
        }

        /// <summary>Field number for the "placeId" field.</summary>
        public const int PlaceIdFieldNumber = 1;
        private uint placeId_;
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public uint PlaceId {
          get { return placeId_; }
          set {
            placeId_ = value;
          }
        }

        /// <summary>Field number for the "script" field.</summary>
        public const int ScriptFieldNumber = 2;
        private string script_ = "";
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public string Script {
          get { return script_; }
          set {
            script_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
          }
        }

        /// <summary>Field number for the "expirationInSeconds" field.</summary>
        public const int ExpirationInSecondsFieldNumber = 3;
        private uint expirationInSeconds_;
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public uint ExpirationInSeconds {
          get { return expirationInSeconds_; }
          set {
            expirationInSeconds_ = value;
          }
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public override bool Equals(object other) {
          return Equals(other as Place);
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public bool Equals(Place other) {
          if (ReferenceEquals(other, null)) {
            return false;
          }
          if (ReferenceEquals(other, this)) {
            return true;
          }
          if (PlaceId != other.PlaceId) return false;
          if (Script != other.Script) return false;
          if (ExpirationInSeconds != other.ExpirationInSeconds) return false;
          return Equals(_unknownFields, other._unknownFields);
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public override int GetHashCode() {
          int hash = 1;
          if (PlaceId != 0) hash ^= PlaceId.GetHashCode();
          if (Script.Length != 0) hash ^= Script.GetHashCode();
          if (ExpirationInSeconds != 0) hash ^= ExpirationInSeconds.GetHashCode();
          if (_unknownFields != null) {
            hash ^= _unknownFields.GetHashCode();
          }
          return hash;
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public override string ToString() {
          return pb::JsonFormatter.ToDiagnosticString(this);
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public void WriteTo(pb::CodedOutputStream output) {
          if (PlaceId != 0) {
            output.WriteRawTag(8);
            output.WriteUInt32(PlaceId);
          }
          if (Script.Length != 0) {
            output.WriteRawTag(18);
            output.WriteString(Script);
          }
          if (ExpirationInSeconds != 0) {
            output.WriteRawTag(24);
            output.WriteUInt32(ExpirationInSeconds);
          }
          if (_unknownFields != null) {
            _unknownFields.WriteTo(output);
          }
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public int CalculateSize() {
          int size = 0;
          if (PlaceId != 0) {
            size += 1 + pb::CodedOutputStream.ComputeUInt32Size(PlaceId);
          }
          if (Script.Length != 0) {
            size += 1 + pb::CodedOutputStream.ComputeStringSize(Script);
          }
          if (ExpirationInSeconds != 0) {
            size += 1 + pb::CodedOutputStream.ComputeUInt32Size(ExpirationInSeconds);
          }
          if (_unknownFields != null) {
            size += _unknownFields.CalculateSize();
          }
          return size;
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public void MergeFrom(Place other) {
          if (other == null) {
            return;
          }
          if (other.PlaceId != 0) {
            PlaceId = other.PlaceId;
          }
          if (other.Script.Length != 0) {
            Script = other.Script;
          }
          if (other.ExpirationInSeconds != 0) {
            ExpirationInSeconds = other.ExpirationInSeconds;
          }
          _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public void MergeFrom(pb::CodedInputStream input) {
          uint tag;
          while ((tag = input.ReadTag()) != 0) {
            switch(tag) {
              default:
                _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
                break;
              case 8: {
                PlaceId = input.ReadUInt32();
                break;
              }
              case 18: {
                Script = input.ReadString();
                break;
              }
              case 24: {
                ExpirationInSeconds = input.ReadUInt32();
                break;
              }
            }
          }
        }

      }

      public sealed partial class Thumbnail : pb::IMessage<Thumbnail> {
        private static readonly pb::MessageParser<Thumbnail> _parser = new pb::MessageParser<Thumbnail>(() => new Thumbnail());
        private pb::UnknownFieldSet _unknownFields;
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public static pb::MessageParser<Thumbnail> Parser { get { return _parser; } }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public static pbr::MessageDescriptor Descriptor {
          get { return global::Tadah.Proto.Signal.Descriptor.NestedTypes[1]; }
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        pbr::MessageDescriptor pb::IMessage.Descriptor {
          get { return Descriptor; }
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public Thumbnail() {
          OnConstruction();
        }

        partial void OnConstruction();

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public Thumbnail(Thumbnail other) : this() {
          type_ = other.type_;
          assetId_ = other.assetId_;
          accessKey_ = other.accessKey_;
          _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public Thumbnail Clone() {
          return new Thumbnail(this);
        }

        /// <summary>Field number for the "type" field.</summary>
        public const int TypeFieldNumber = 1;
        private global::Tadah.Proto.AssetType type_ = 0;
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public global::Tadah.Proto.AssetType Type {
          get { return type_; }
          set {
            type_ = value;
          }
        }

        /// <summary>Field number for the "assetId" field.</summary>
        public const int AssetIdFieldNumber = 2;
        private uint assetId_;
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public uint AssetId {
          get { return assetId_; }
          set {
            assetId_ = value;
          }
        }

        /// <summary>Field number for the "accessKey" field.</summary>
        public const int AccessKeyFieldNumber = 3;
        private string accessKey_ = "";
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public string AccessKey {
          get { return accessKey_; }
          set {
            accessKey_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
          }
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public override bool Equals(object other) {
          return Equals(other as Thumbnail);
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public bool Equals(Thumbnail other) {
          if (ReferenceEquals(other, null)) {
            return false;
          }
          if (ReferenceEquals(other, this)) {
            return true;
          }
          if (Type != other.Type) return false;
          if (AssetId != other.AssetId) return false;
          if (AccessKey != other.AccessKey) return false;
          return Equals(_unknownFields, other._unknownFields);
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public override int GetHashCode() {
          int hash = 1;
          if (Type != 0) hash ^= Type.GetHashCode();
          if (AssetId != 0) hash ^= AssetId.GetHashCode();
          if (AccessKey.Length != 0) hash ^= AccessKey.GetHashCode();
          if (_unknownFields != null) {
            hash ^= _unknownFields.GetHashCode();
          }
          return hash;
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public override string ToString() {
          return pb::JsonFormatter.ToDiagnosticString(this);
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public void WriteTo(pb::CodedOutputStream output) {
          if (Type != 0) {
            output.WriteRawTag(8);
            output.WriteEnum((int) Type);
          }
          if (AssetId != 0) {
            output.WriteRawTag(16);
            output.WriteUInt32(AssetId);
          }
          if (AccessKey.Length != 0) {
            output.WriteRawTag(26);
            output.WriteString(AccessKey);
          }
          if (_unknownFields != null) {
            _unknownFields.WriteTo(output);
          }
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public int CalculateSize() {
          int size = 0;
          if (Type != 0) {
            size += 1 + pb::CodedOutputStream.ComputeEnumSize((int) Type);
          }
          if (AssetId != 0) {
            size += 1 + pb::CodedOutputStream.ComputeUInt32Size(AssetId);
          }
          if (AccessKey.Length != 0) {
            size += 1 + pb::CodedOutputStream.ComputeStringSize(AccessKey);
          }
          if (_unknownFields != null) {
            size += _unknownFields.CalculateSize();
          }
          return size;
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public void MergeFrom(Thumbnail other) {
          if (other == null) {
            return;
          }
          if (other.Type != 0) {
            Type = other.Type;
          }
          if (other.AssetId != 0) {
            AssetId = other.AssetId;
          }
          if (other.AccessKey.Length != 0) {
            AccessKey = other.AccessKey;
          }
          _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
        }

        [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
        public void MergeFrom(pb::CodedInputStream input) {
          uint tag;
          while ((tag = input.ReadTag()) != 0) {
            switch(tag) {
              default:
                _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
                break;
              case 8: {
                type_ = (global::Tadah.Proto.AssetType) input.ReadEnum();
                break;
              }
              case 16: {
                AssetId = input.ReadUInt32();
                break;
              }
              case 26: {
                AccessKey = input.ReadString();
                break;
              }
            }
          }
        }

      }

    }
    #endregion

  }

  public sealed partial class Response : pb::IMessage<Response> {
    private static readonly pb::MessageParser<Response> _parser = new pb::MessageParser<Response>(() => new Response());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<Response> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Tadah.Proto.TadahReflection.Descriptor.MessageTypes[1]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Response() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Response(Response other) : this() {
      operation_ = other.operation_;
      success_ = other.success_;
      message_ = other.message_;
      data_ = other.data_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Response Clone() {
      return new Response(this);
    }

    /// <summary>Field number for the "operation" field.</summary>
    public const int OperationFieldNumber = 1;
    private global::Tadah.Proto.Operation operation_ = 0;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::Tadah.Proto.Operation Operation {
      get { return operation_; }
      set {
        operation_ = value;
      }
    }

    /// <summary>Field number for the "success" field.</summary>
    public const int SuccessFieldNumber = 2;
    private bool success_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Success {
      get { return success_; }
      set {
        success_ = value;
      }
    }

    /// <summary>Field number for the "message" field.</summary>
    public const int MessageFieldNumber = 3;
    private string message_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string Message {
      get { return message_; }
      set {
        message_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "data" field.</summary>
    public const int DataFieldNumber = 4;
    private string data_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string Data {
      get { return data_; }
      set {
        data_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as Response);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(Response other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (Operation != other.Operation) return false;
      if (Success != other.Success) return false;
      if (Message != other.Message) return false;
      if (Data != other.Data) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (Operation != 0) hash ^= Operation.GetHashCode();
      if (Success != false) hash ^= Success.GetHashCode();
      if (Message.Length != 0) hash ^= Message.GetHashCode();
      if (Data.Length != 0) hash ^= Data.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      if (Operation != 0) {
        output.WriteRawTag(8);
        output.WriteEnum((int) Operation);
      }
      if (Success != false) {
        output.WriteRawTag(16);
        output.WriteBool(Success);
      }
      if (Message.Length != 0) {
        output.WriteRawTag(26);
        output.WriteString(Message);
      }
      if (Data.Length != 0) {
        output.WriteRawTag(34);
        output.WriteString(Data);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (Operation != 0) {
        size += 1 + pb::CodedOutputStream.ComputeEnumSize((int) Operation);
      }
      if (Success != false) {
        size += 1 + 1;
      }
      if (Message.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(Message);
      }
      if (Data.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(Data);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(Response other) {
      if (other == null) {
        return;
      }
      if (other.Operation != 0) {
        Operation = other.Operation;
      }
      if (other.Success != false) {
        Success = other.Success;
      }
      if (other.Message.Length != 0) {
        Message = other.Message;
      }
      if (other.Data.Length != 0) {
        Data = other.Data;
      }
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 8: {
            operation_ = (global::Tadah.Proto.Operation) input.ReadEnum();
            break;
          }
          case 16: {
            Success = input.ReadBool();
            break;
          }
          case 26: {
            Message = input.ReadString();
            break;
          }
          case 34: {
            Data = input.ReadString();
            break;
          }
        }
      }
    }

  }

  #endregion

}

#endregion Designer generated code
