local jobId, type, format, x, y, baseUrl, assetId, token = ...

print(("[%s] Started RenderJob for type '%s' with assetId %d ..."):format(jobId, type, assetId))

game:GetService("ScriptInformationProvider"):SetAssetUrl(baseUrl .. "/asset/")
game:GetService("InsertService"):SetAssetUrl(baseUrl .. "/asset/?id=%d")
game:GetService("InsertService"):SetAssetVersionUrl(baseUrl .. "/Asset/?assetversionid=%d")
game:GetService("ContentProvider"):SetBaseUrl(baseUrl)

-- do this twice for security
game:GetService("ScriptContext").ScriptsDisabled = true
game:GetService("StarterGui").ShowDevelopmentGui = false

game:Load(("%s/asset/?id=%d&token=%s"):format(baseUrl, assetId, token))

game:GetService("ScriptContext").ScriptsDisabled = true
game:GetService("StarterGui").ShowDevelopmentGui = false

print(("[%s] Rendering ..."):format(jobId))
local result = game:GetService("ThumbnailGenerator"):Click(format, x, y, false)
print(("[%s] Done!"):format(jobId))

return result