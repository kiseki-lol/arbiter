using System.ServiceModel;
using ServiceReference;

namespace Kiseki.Arbiter;

public abstract class Job
{
    protected DateTime _started;
    protected DateTime _closed;
    protected JobStatus _status;
    protected string _result;

    public string Uuid { get; protected set; }
    public int Port { get; protected set; }
    public Process? Process { get; protected set; } = null;
    public bool IsRunning { get; protected set; } = false;

    // Job Script (the initial script that will be ran on the Job)
    public LuaScript JobScript;
    public string? PlaceToken { get; protected set; } = ""; // placed here because renders also require

    // SOAP
    public int SoapPort { get; protected set; }
    public bool SoapReady { get; protected set; } = false;
    public BasicHttpBinding SoapBinding;
    public EndpointAddress SoapEndpoint;
    public RCCServiceSoapClient SoapClient;

    public virtual DateTime Started {
        get => _started;
        protected set {
            _started = value;
        }
    }

    public virtual DateTime Closed {
        get => _closed;
        protected set {
            _closed = value;
        }
    }

    public virtual JobStatus Status { 
        get => _status;
        protected set {
            _status = value;
        }
    }

    public virtual string Base64Result { 
        get => _result;
        protected set {
            _result = value;
        }
    }

    public Job(string uuid, int port, int soapPort)
    {
        Uuid = uuid;
        
        // SOAP
        SoapPort = soapPort;
        Port     = port;
        SoapBinding = new BasicHttpBinding();
        SoapEndpoint = new EndpointAddress("http://localhost:" + SoapPort);
        SoapClient = new RCCServiceSoapClient(SoapBinding, SoapEndpoint);
        JobScript = new LuaScript();
    }

    public abstract void Start();

    public void Close()
    {
        Logger.Write(Uuid, $"Closing...", LogSeverity.Event);

        // set status closed just in case process doesn't actually exist        
        Status = JobStatus.Closed;
        IsRunning = false;
        Closed = DateTime.UtcNow;

        try
        {
            Process!.Kill();

            Logger.Write(Uuid, $"Closed!", LogSeverity.Event);
        }
        catch (Exception e)
        {
            Logger.Write(Uuid, $"Error while closing job process! { e }", LogSeverity.Error);
        }
    }
}