namespace Kiseki.Arbiter;

public class LuaScript
{
    public string Script { get; private set; }

    public LuaScript(string script)
    {
        Script = script;
    }
}