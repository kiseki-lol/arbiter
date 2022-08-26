local jobId, type, format, x, y, baseUrl, assetId = ...

print(("[%s] Started RenderJob for type '%s' with assetId %d ..."):format(jobId, type, assetId))

game:GetService("ScriptInformationProvider"):SetAssetUrl(baseUrl .. "/asset/")
game:GetService("InsertService"):SetAssetUrl(baseUrl .. "/asset/?id=%d")
game:GetService("InsertService"):SetAssetVersionUrl(baseUrl .. "/Asset/?assetversionid=%d")
game:GetService("ContentProvider"):SetBaseUrl(baseUrl)
game:GetService("ScriptContext").ScriptsDisabled = true

local Player = game.Players:CreateLocalPlayer(0)
Player.CharacterAppearance = ("%s/users/%d/character"):format(baseUrl, assetId)
Player:LoadCharacter(false)

game:GetService("RunService"):Run()

Player.Character.Animate.Disabled = true 
Player.Character.Torso.Anchored = true

-- Headshot Camera
local FOV = 52.5
local AngleOffsetX = 0
local AngleOffsetY = 0
local AngleOffsetZ = 0

local CameraAngle = Player.Character.Head.CFrame * CFrame.new(AngleOffsetX, AngleOffsetY, AngleOffsetZ)
local CameraPosition = Player.Character.Head.CFrame + Vector3.new(0, 0, 0) + (CFrame.Angles(0, 0, 0).lookVector.unit * 3)

local Camera = Instance.new("Camera", Player.Character)
Camera.Name = "ThumbnailCamera"
Camera.CameraType = Enum.CameraType.Scriptable

Camera.CoordinateFrame = CFrame.new(CameraPosition.p, CameraAngle.p)
Camera.FieldOfView = FOV
workspace.CurrentCamera = Camera

print(("[%s] Rendering ..."):format(jobId))
local result = game:GetService("ThumbnailGenerator"):Click(format, x, y, true)
print(("[%s] Done!"):format(jobId))

return result