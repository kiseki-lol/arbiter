local jobId, type, format, x, y, baseUrl, assetId = ...

print(("[%s] Started RenderJob for type '%s' with assetId %d ..."):format(jobId, type, assetId))

game:GetService("ScriptInformationProvider"):SetAssetUrl(baseUrl .. "/asset/")
game:GetService("InsertService"):SetAssetUrl(baseUrl .. "/asset/?id=%d")
game:GetService("InsertService"):SetAssetVersionUrl(baseUrl .. "/Asset/?assetversionid=%d")
game:GetService("ContentProvider"):SetBaseUrl(baseUrl)
game:GetService("ScriptContext").ScriptsDisabled = true

local meshPart = Instance.new("Part", workspace)
meshPart.Anchored = true
meshPart.Size = Vector3.new(10, 10, 10)

local mesh = Instance.new("SpecialMesh", meshPart)
mesh.MeshType = "FileMesh"
mesh.MeshId = ("%s/asset?id=%d"):format(baseUrl, assetId)

print(("[%s] Rendering ..."):format(jobId))
local result = game:GetService("ThumbnailGenerator"):Click(format, x, y, true)
print(("[%s] Done!"):format(jobId))

return result