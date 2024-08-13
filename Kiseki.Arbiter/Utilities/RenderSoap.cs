using ServiceReference;

namespace Kiseki.Arbiter.Utilities;

public static class RenderSoap
{
    public static void GetScriptFromRenderType(RenderJob job)
    {
        switch (job.RenderType)
        {
            case RenderJobType.Headshot:
                job.JobScript.LoadFromPath("headshot.lua");
                break;
            case RenderJobType.Bodyshot:
                job.JobScript.LoadFromPath("bodyshot.lua");
                break;
            case RenderJobType.Place:
                job.JobScript.LoadFromPath("place.lua");
                break;
            case RenderJobType.Asset:
                if (job.RenderAssetType == AssetType.Hat)
                    job.JobScript.LoadFromPath("xml.lua");
                else
                    job.JobScript.LoadFromPath("bodyasset.lua");
                break;
            case RenderJobType.XML:
                job.JobScript.LoadFromPath("xml.lua");
                break;
            default:
                // ??? don't think this will EVER happen
                throw new Exception("RenderType was not set!");
        }
    }

    public static ScriptExecution GetScriptExecutionFromRenderType(RenderJob job)
    {
        if (job.RenderType == RenderJobType.Headshot || job.RenderType == RenderJobType.Bodyshot)
        {
            return new ScriptExecution
            {
                script    = job.JobScript.Script,
                name      = "RenderJob",
                arguments = 
                [
                    new LuaValue
                    {
                        type = LuaType.LUA_TSTRING,
                        value = job.Uuid
                    },
                    new LuaValue
                    {
                        type = LuaType.LUA_TSTRING,
                        value = "Render"
                    },
                    new LuaValue
                    {
                        type = LuaType.LUA_TSTRING,
                        value = "PNG" // maybe we should make RCC output webp?
                    },
                    new LuaValue
                    {
                        type = LuaType.LUA_TNUMBER,
                        value = "420"
                    },
                    new LuaValue
                    {
                        type = LuaType.LUA_TNUMBER,
                        value = "420"
                    },
                    new LuaValue
                    {
                        type = LuaType.LUA_TSTRING,
// kiseki.local - no tls
#if DEBUG
                        value = "http://" + Constants.BASE_URL
#else
                        value = "https://" + Constants.BASE_URL
#endif
                    },
                    new LuaValue
                    {
                        type = LuaType.LUA_TNUMBER,
                        value = job.AssetId.ToString() // why?
                    },
                ]
            };
        }

        if (job.RenderType == RenderJobType.Place)
        {
            Logger.Write("RenderJobType token shit" + job.PlaceToken);

            return new ScriptExecution
            {
                script    = job.JobScript.Script,
                name      = "RenderJob",
                arguments = 
                [
                    new LuaValue
                    {
                        type = LuaType.LUA_TSTRING,
                        value = job.Uuid
                    },
                    new LuaValue
                    {
                        type = LuaType.LUA_TSTRING,
                        value = "Render"
                    },
                    new LuaValue
                    {
                        type = LuaType.LUA_TSTRING,
                        value = "PNG" // maybe we should make RCC output webp?
                    },
                    new LuaValue
                    {
                        type = LuaType.LUA_TNUMBER,
                        value = "420"
                    },
                    new LuaValue
                    {
                        type = LuaType.LUA_TNUMBER,
                        value = "420"
                    },
                    new LuaValue
                    {
                        type = LuaType.LUA_TSTRING,
// kiseki.local - no tls
#if DEBUG
                        value = "http://" + Constants.BASE_URL
#else
                        value = "https://" + Constants.BASE_URL
#endif
                    },
                    new LuaValue
                    {
                        type = LuaType.LUA_TNUMBER,
                        value = job.AssetId.ToString() // why?
                    },
                    new LuaValue
                    {
                        type = LuaType.LUA_TSTRING,
                        value = job.PlaceToken
                    },
                ]
            };
        }

        if (job.RenderType == RenderJobType.Asset)
        {
            return new ScriptExecution
            {
                script    = job.JobScript.Script,
                name      = "RenderJob",
                arguments = 
                [
                    new LuaValue
                    {
                        type = LuaType.LUA_TSTRING,
                        value = job.Uuid
                    },
                    new LuaValue
                    {
                        type = LuaType.LUA_TSTRING,
                        value = "Render"
                    },
                    new LuaValue
                    {
                        type = LuaType.LUA_TSTRING,
                        value = "PNG" // maybe we should make RCC output webp?
                    },
                    new LuaValue
                    {
                        type = LuaType.LUA_TNUMBER,
                        value = "420"
                    },
                    new LuaValue
                    {
                        type = LuaType.LUA_TNUMBER,
                        value = "420"
                    },
                    new LuaValue
                    {
                        type = LuaType.LUA_TSTRING,
// kiseki.local - no tls
#if DEBUG
                        value = "http://" + Constants.BASE_URL
#else
                        value = "https://" + Constants.BASE_URL
#endif
                    },
                    new LuaValue
                    {
                        type = LuaType.LUA_TNUMBER,
                        value = job.AssetId.ToString() // why?
                    },
                ]
            };
        }

        throw new Exception("not implemented for render type");
    }

}