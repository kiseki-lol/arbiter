﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ServiceReference
{
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "2.1.0")]
    [System.ServiceModel.ServiceContractAttribute(Namespace="http://roblox.com/", ConfigurationName="ServiceReference.RCCServiceSoap")]
    public interface RCCServiceSoap
    {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://roblox.com/HelloWorld", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        System.Threading.Tasks.Task<string> HelloWorldAsync();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://roblox.com/GetVersion", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        System.Threading.Tasks.Task<string> GetVersionAsync();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://roblox.com/GetStatus", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        System.Threading.Tasks.Task<ServiceReference.Status> GetStatusAsync();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://roblox.com/OpenJob", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        System.Threading.Tasks.Task<ServiceReference.OpenJobResponse> OpenJobAsync(ServiceReference.OpenJobRequest request);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://roblox.com/OpenJobEx", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        System.Threading.Tasks.Task<ServiceReference.LuaValue[]> OpenJobExAsync(ServiceReference.Job job, ServiceReference.ScriptExecution script);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://roblox.com/RenewLease", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        System.Threading.Tasks.Task<double> RenewLeaseAsync(string jobID, double expirationInSeconds);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://roblox.com/Execute", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        System.Threading.Tasks.Task<ServiceReference.ExecuteResponse> ExecuteAsync(ServiceReference.ExecuteRequest request);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://roblox.com/ExecuteEx", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        System.Threading.Tasks.Task<ServiceReference.LuaValue[]> ExecuteExAsync(string jobID, ServiceReference.ScriptExecution script);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://roblox.com/CloseJob", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        System.Threading.Tasks.Task CloseJobAsync(string jobID);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://roblox.com/BatchJob", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        System.Threading.Tasks.Task<ServiceReference.BatchJobResponse> BatchJobAsync(ServiceReference.BatchJobRequest request);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://roblox.com/BatchJobEx", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        System.Threading.Tasks.Task<ServiceReference.LuaValue[]> BatchJobExAsync(ServiceReference.Job job, ServiceReference.ScriptExecution script);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://roblox.com/GetExpiration", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        System.Threading.Tasks.Task<double> GetExpirationAsync(string jobID);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://roblox.com/GetAllJobs", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        System.Threading.Tasks.Task<ServiceReference.GetAllJobsResponse> GetAllJobsAsync(ServiceReference.GetAllJobsRequest request);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://roblox.com/GetAllJobsEx", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        System.Threading.Tasks.Task<ServiceReference.Job[]> GetAllJobsExAsync();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://roblox.com/CloseExpiredJobs", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        System.Threading.Tasks.Task<int> CloseExpiredJobsAsync();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://roblox.com/CloseAllJobs", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        System.Threading.Tasks.Task<int> CloseAllJobsAsync();
        
        [System.ServiceModel.OperationContractAttribute(Action="http://roblox.com/Diag", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        System.Threading.Tasks.Task<ServiceReference.DiagResponse> DiagAsync(ServiceReference.DiagRequest request);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://roblox.com/DiagEx", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        System.Threading.Tasks.Task<ServiceReference.LuaValue[]> DiagExAsync(int type, string jobID);
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "2.1.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://roblox.com/")]
    public partial class Status
    {
        
        private string versionField;
        
        private int environmentCountField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=0)]
        public string version
        {
            get
            {
                return this.versionField;
            }
            set
            {
                this.versionField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=1)]
        public int environmentCount
        {
            get
            {
                return this.environmentCountField;
            }
            set
            {
                this.environmentCountField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "2.1.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://roblox.com/")]
    public partial class LuaValue
    {
        
        private LuaType typeField;
        
        private string valueField;
        
        private LuaValue[] tableField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=0)]
        public LuaType type
        {
            get
            {
                return this.typeField;
            }
            set
            {
                this.typeField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=1)]
        public string value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayAttribute(Order=2)]
        public LuaValue[] table
        {
            get
            {
                return this.tableField;
            }
            set
            {
                this.tableField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "2.1.0")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://roblox.com/")]
    public enum LuaType
    {
        
        /// <remarks/>
        LUA_TNIL,
        
        /// <remarks/>
        LUA_TBOOLEAN,
        
        /// <remarks/>
        LUA_TNUMBER,
        
        /// <remarks/>
        LUA_TSTRING,
        
        /// <remarks/>
        LUA_TTABLE,
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "2.1.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://roblox.com/")]
    public partial class ScriptExecution
    {
        
        private string nameField;
        
        private string scriptField;
        
        private LuaValue[] argumentsField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=0)]
        public string name
        {
            get
            {
                return this.nameField;
            }
            set
            {
                this.nameField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=1)]
        public string script
        {
            get
            {
                return this.scriptField;
            }
            set
            {
                this.scriptField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayAttribute(Order=2)]
        public LuaValue[] arguments
        {
            get
            {
                return this.argumentsField;
            }
            set
            {
                this.argumentsField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "2.1.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://roblox.com/")]
    public partial class Job
    {
        
        private string idField;
        
        private double expirationInSecondsField;
        
        private int categoryField;
        
        private double coresField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=0)]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=1)]
        public double expirationInSeconds
        {
            get
            {
                return this.expirationInSecondsField;
            }
            set
            {
                this.expirationInSecondsField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=2)]
        public int category
        {
            get
            {
                return this.categoryField;
            }
            set
            {
                this.categoryField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order=3)]
        public double cores
        {
            get
            {
                return this.coresField;
            }
            set
            {
                this.coresField = value;
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "2.1.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(WrapperName="OpenJob", WrapperNamespace="http://roblox.com/", IsWrapped=true)]
    public partial class OpenJobRequest
    {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://roblox.com/", Order=0)]
        public ServiceReference.Job job;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://roblox.com/", Order=1)]
        public ServiceReference.ScriptExecution script;
        
        public OpenJobRequest()
        {
        }
        
        public OpenJobRequest(ServiceReference.Job job, ServiceReference.ScriptExecution script)
        {
            this.job = job;
            this.script = script;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "2.1.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(WrapperName="OpenJobResponse", WrapperNamespace="http://roblox.com/", IsWrapped=true)]
    public partial class OpenJobResponse
    {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://roblox.com/", Order=0)]
        [System.Xml.Serialization.XmlElementAttribute("OpenJobResult")]
        public ServiceReference.LuaValue[] OpenJobResult;
        
        public OpenJobResponse()
        {
        }
        
        public OpenJobResponse(ServiceReference.LuaValue[] OpenJobResult)
        {
            this.OpenJobResult = OpenJobResult;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "2.1.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(WrapperName="Execute", WrapperNamespace="http://roblox.com/", IsWrapped=true)]
    public partial class ExecuteRequest
    {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://roblox.com/", Order=0)]
        public string jobID;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://roblox.com/", Order=1)]
        public ServiceReference.ScriptExecution script;
        
        public ExecuteRequest()
        {
        }
        
        public ExecuteRequest(string jobID, ServiceReference.ScriptExecution script)
        {
            this.jobID = jobID;
            this.script = script;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "2.1.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(WrapperName="ExecuteResponse", WrapperNamespace="http://roblox.com/", IsWrapped=true)]
    public partial class ExecuteResponse
    {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://roblox.com/", Order=0)]
        [System.Xml.Serialization.XmlElementAttribute("ExecuteResult", IsNullable=true)]
        public ServiceReference.LuaValue[] ExecuteResult;
        
        public ExecuteResponse()
        {
        }
        
        public ExecuteResponse(ServiceReference.LuaValue[] ExecuteResult)
        {
            this.ExecuteResult = ExecuteResult;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "2.1.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(WrapperName="BatchJob", WrapperNamespace="http://roblox.com/", IsWrapped=true)]
    public partial class BatchJobRequest
    {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://roblox.com/", Order=0)]
        public ServiceReference.Job job;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://roblox.com/", Order=1)]
        public ServiceReference.ScriptExecution script;
        
        public BatchJobRequest()
        {
        }
        
        public BatchJobRequest(ServiceReference.Job job, ServiceReference.ScriptExecution script)
        {
            this.job = job;
            this.script = script;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "2.1.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(WrapperName="BatchJobResponse", WrapperNamespace="http://roblox.com/", IsWrapped=true)]
    public partial class BatchJobResponse
    {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://roblox.com/", Order=0)]
        [System.Xml.Serialization.XmlElementAttribute("BatchJobResult", IsNullable=true)]
        public ServiceReference.LuaValue[] BatchJobResult;
        
        public BatchJobResponse()
        {
        }
        
        public BatchJobResponse(ServiceReference.LuaValue[] BatchJobResult)
        {
            this.BatchJobResult = BatchJobResult;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "2.1.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(WrapperName="GetAllJobs", WrapperNamespace="http://roblox.com/", IsWrapped=true)]
    public partial class GetAllJobsRequest
    {
        
        public GetAllJobsRequest()
        {
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "2.1.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(WrapperName="GetAllJobsResponse", WrapperNamespace="http://roblox.com/", IsWrapped=true)]
    public partial class GetAllJobsResponse
    {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://roblox.com/", Order=0)]
        [System.Xml.Serialization.XmlElementAttribute("GetAllJobsResult", IsNullable=true)]
        public ServiceReference.Job[] GetAllJobsResult;
        
        public GetAllJobsResponse()
        {
        }
        
        public GetAllJobsResponse(ServiceReference.Job[] GetAllJobsResult)
        {
            this.GetAllJobsResult = GetAllJobsResult;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "2.1.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(WrapperName="Diag", WrapperNamespace="http://roblox.com/", IsWrapped=true)]
    public partial class DiagRequest
    {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://roblox.com/", Order=0)]
        public int type;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://roblox.com/", Order=1)]
        public string jobID;
        
        public DiagRequest()
        {
        }
        
        public DiagRequest(int type, string jobID)
        {
            this.type = type;
            this.jobID = jobID;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "2.1.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(WrapperName="DiagResponse", WrapperNamespace="http://roblox.com/", IsWrapped=true)]
    public partial class DiagResponse
    {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://roblox.com/", Order=0)]
        [System.Xml.Serialization.XmlElementAttribute("DiagResult", IsNullable=true)]
        public ServiceReference.LuaValue[] DiagResult;
        
        public DiagResponse()
        {
        }
        
        public DiagResponse(ServiceReference.LuaValue[] DiagResult)
        {
            this.DiagResult = DiagResult;
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "2.1.0")]
    public interface RCCServiceSoapChannel : ServiceReference.RCCServiceSoap, System.ServiceModel.IClientChannel
    {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "2.1.0")]
    public partial class RCCServiceSoapClient : System.ServiceModel.ClientBase<ServiceReference.RCCServiceSoap>, ServiceReference.RCCServiceSoap
    {
        
        public RCCServiceSoapClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress)
        {
        }
        
        public System.Threading.Tasks.Task<string> HelloWorldAsync()
        {
            return base.Channel.HelloWorldAsync();
        }
        
        public System.Threading.Tasks.Task<string> GetVersionAsync()
        {
            return base.Channel.GetVersionAsync();
        }
        
        public System.Threading.Tasks.Task<ServiceReference.Status> GetStatusAsync()
        {
            return base.Channel.GetStatusAsync();
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        System.Threading.Tasks.Task<ServiceReference.OpenJobResponse> ServiceReference.RCCServiceSoap.OpenJobAsync(ServiceReference.OpenJobRequest request)
        {
            return base.Channel.OpenJobAsync(request);
        }
        
        public System.Threading.Tasks.Task<ServiceReference.OpenJobResponse> OpenJobAsync(ServiceReference.Job job, ServiceReference.ScriptExecution script)
        {
            ServiceReference.OpenJobRequest inValue = new ServiceReference.OpenJobRequest();
            inValue.job = job;
            inValue.script = script;
            return ((ServiceReference.RCCServiceSoap)(this)).OpenJobAsync(inValue);
        }
        
        public System.Threading.Tasks.Task<ServiceReference.LuaValue[]> OpenJobExAsync(ServiceReference.Job job, ServiceReference.ScriptExecution script)
        {
            return base.Channel.OpenJobExAsync(job, script);
        }
        
        public System.Threading.Tasks.Task<double> RenewLeaseAsync(string jobID, double expirationInSeconds)
        {
            return base.Channel.RenewLeaseAsync(jobID, expirationInSeconds);
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        System.Threading.Tasks.Task<ServiceReference.ExecuteResponse> ServiceReference.RCCServiceSoap.ExecuteAsync(ServiceReference.ExecuteRequest request)
        {
            return base.Channel.ExecuteAsync(request);
        }
        
        public System.Threading.Tasks.Task<ServiceReference.ExecuteResponse> ExecuteAsync(string jobID, ServiceReference.ScriptExecution script)
        {
            ServiceReference.ExecuteRequest inValue = new ServiceReference.ExecuteRequest();
            inValue.jobID = jobID;
            inValue.script = script;
            return ((ServiceReference.RCCServiceSoap)(this)).ExecuteAsync(inValue);
        }
        
        public System.Threading.Tasks.Task<ServiceReference.LuaValue[]> ExecuteExAsync(string jobID, ServiceReference.ScriptExecution script)
        {
            return base.Channel.ExecuteExAsync(jobID, script);
        }
        
        public System.Threading.Tasks.Task CloseJobAsync(string jobID)
        {
            return base.Channel.CloseJobAsync(jobID);
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        System.Threading.Tasks.Task<ServiceReference.BatchJobResponse> ServiceReference.RCCServiceSoap.BatchJobAsync(ServiceReference.BatchJobRequest request)
        {
            return base.Channel.BatchJobAsync(request);
        }
        
        public System.Threading.Tasks.Task<ServiceReference.BatchJobResponse> BatchJobAsync(ServiceReference.Job job, ServiceReference.ScriptExecution script)
        {
            ServiceReference.BatchJobRequest inValue = new ServiceReference.BatchJobRequest();
            inValue.job = job;
            inValue.script = script;
            return ((ServiceReference.RCCServiceSoap)(this)).BatchJobAsync(inValue);
        }
        
        public System.Threading.Tasks.Task<ServiceReference.LuaValue[]> BatchJobExAsync(ServiceReference.Job job, ServiceReference.ScriptExecution script)
        {
            return base.Channel.BatchJobExAsync(job, script);
        }
        
        public System.Threading.Tasks.Task<double> GetExpirationAsync(string jobID)
        {
            return base.Channel.GetExpirationAsync(jobID);
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        System.Threading.Tasks.Task<ServiceReference.GetAllJobsResponse> ServiceReference.RCCServiceSoap.GetAllJobsAsync(ServiceReference.GetAllJobsRequest request)
        {
            return base.Channel.GetAllJobsAsync(request);
        }
        
        public System.Threading.Tasks.Task<ServiceReference.GetAllJobsResponse> GetAllJobsAsync()
        {
            ServiceReference.GetAllJobsRequest inValue = new ServiceReference.GetAllJobsRequest();
            return ((ServiceReference.RCCServiceSoap)(this)).GetAllJobsAsync(inValue);
        }
        
        public System.Threading.Tasks.Task<ServiceReference.Job[]> GetAllJobsExAsync()
        {
            return base.Channel.GetAllJobsExAsync();
        }
        
        public System.Threading.Tasks.Task<int> CloseExpiredJobsAsync()
        {
            return base.Channel.CloseExpiredJobsAsync();
        }
        
        public System.Threading.Tasks.Task<int> CloseAllJobsAsync()
        {
            return base.Channel.CloseAllJobsAsync();
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        System.Threading.Tasks.Task<ServiceReference.DiagResponse> ServiceReference.RCCServiceSoap.DiagAsync(ServiceReference.DiagRequest request)
        {
            return base.Channel.DiagAsync(request);
        }
        
        public System.Threading.Tasks.Task<ServiceReference.DiagResponse> DiagAsync(int type, string jobID)
        {
            ServiceReference.DiagRequest inValue = new ServiceReference.DiagRequest();
            inValue.type = type;
            inValue.jobID = jobID;
            return ((ServiceReference.RCCServiceSoap)(this)).DiagAsync(inValue);
        }
        
        public System.Threading.Tasks.Task<ServiceReference.LuaValue[]> DiagExAsync(int type, string jobID)
        {
            return base.Channel.DiagExAsync(type, jobID);
        }
        
        public virtual System.Threading.Tasks.Task OpenAsync()
        {
            return System.Threading.Tasks.Task.Factory.FromAsync(((System.ServiceModel.ICommunicationObject)(this)).BeginOpen(null, null), new System.Action<System.IAsyncResult>(((System.ServiceModel.ICommunicationObject)(this)).EndOpen));
        }
    }
}
