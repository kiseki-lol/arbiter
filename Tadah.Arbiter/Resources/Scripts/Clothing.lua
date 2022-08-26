local jobId, type, format, x, y, baseUrl, assetId = ...

print(("[%s] Started RenderJob for type '%s' with assetId %d"):format(jobId, type, assetId))

game:GetService("ScriptInformationProvider"):SetAssetUrl(baseUrl .. "/asset/")
game:GetService("InsertService"):SetAssetUrl(baseUrl .. "/asset/?id=%d")
game:GetService("InsertService"):SetAssetVersionUrl(baseUrl .. "/Asset/?assetversionid=%d")
game:GetService("ContentProvider"):SetBaseUrl(baseUrl)
game:GetService("ScriptContext").ScriptsDisabled = true

local Player = game.Players:CreateLocalPlayer(0)
Player.CharacterAppearance = ("%s/thumbnail/clothingcharapp/%d"):format(baseUrl, assetId)
Player:LoadCharacter(false)

game:GetService("RunService"):Run()

Player.Character.Animate.Disabled = true 
Player.Character.Torso.Anchored = true

print(("[%s] Rendering ..."):format(jobId))
local result = game:GetService("ThumbnailGenerator"):Click(format, x, y, true)
print(("[%s] Done!"):format(jobId))

return result