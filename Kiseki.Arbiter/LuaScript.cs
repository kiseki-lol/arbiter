namespace Kiseki.Arbiter;

public class LuaScript
{
    public string Script { get; private set; }

    public LuaScript(string script = "")
    {
        Script = script;
    }

    public void LoadFromPath(string filename)
    {
        try
        {
            Script = File.ReadAllText(Paths.Scripts + "/" + filename);
        }
        catch (FileNotFoundException)
        {
            throw new("Failed to find Lua Script");
        }
    }
}