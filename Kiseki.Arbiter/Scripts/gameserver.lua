local jobId, type, baseUrl, placeId, port, token = ...

print(("[%s] Started Gameserver for type '%s' with placeId %d ..."):format(jobId, type, placeId))

------------------- UTILITY FUNCTIONS --------------------------
function waitForChild(parent, childName)
    while true do
        local child = parent:findFirstChild(childName)
        if child then return child end
        parent.ChildAdded:wait()
    end
end


-----------------------------------END UTILITY FUNCTIONS -------------------------

-----------------------------------"CUSTOM" SHARED CODE----------------------------------

pcall(function() settings().Network.UseInstancePacketCache = true end)
pcall(function() settings().Network.UsePhysicsPacketCache = true end)
pcall(function() settings()["Task Scheduler"].PriorityMethod = Enum.PriorityMethod.AccumulatedError end)

settings().Network.PhysicsSend = Enum.PhysicsSendMethod.TopNErrors
settings().Network.ExperimentalPhysicsEnabled = true
settings().Network.WaitingForCharacterLogRate = 100
pcall(function() settings().Diagnostics:LegacyScriptMode() end)

-----------------------------------START GAME SHARED SCRIPT------------------------------

local scriptContext = game:GetService("ScriptContext")
scriptContext.ScriptsDisabled = true

game:SetPlaceID(placeId, false)
game:GetService("ChangeHistoryService"):SetEnabled(false)

local ns = game:GetService("NetworkServer")

if baseUrl ~= nil then
    pcall(function() game:GetService("Players"):SetAbuseReportUrl(baseUrl .. "/AbuseReport/InGameChatHandler.ashx") end)
    pcall(function() game:GetService("ScriptInformationProvider"):SetAssetUrl(baseUrl .. "/Asset/") end)
    pcall(function() game:GetService("ContentProvider"):SetBaseUrl(baseUrl .. "/") end)
    -- pcall(function() game:GetService("Players"):SetChatFilterUrl(baseUrl .. "/Game/ChatFilter.ashx") end)

    game:GetService("BadgeService"):SetPlaceId(placeId)
    game:GetService("BadgeService"):SetIsBadgeLegalUrl("")
    game:GetService("BadgeService"):SetHasBadgeUrl(baseUrl .. "/assets/has-badge")
    game:GetService("BadgeService"):SetAwardBadgeUrl(baseUrl .. "/assets/award-badge?userId=%d&badgeId=%d")
    game:GetService("InsertService"):SetBaseSetsUrl(baseUrl .. "/Game/Tools/InsertAsset.ashx?nsets=10&type=base")
    game:GetService("InsertService"):SetUserSetsUrl(baseUrl .. "/Game/Tools/InsertAsset.ashx?nsets=20&type=user&userid=%d")
    game:GetService("InsertService"):SetCollectionUrl(baseUrl .. "/Game/Tools/InsertAsset.ashx?sid=%d")
    game:GetService("InsertService"):SetAssetUrl(baseUrl .. "/Asset/?id=%d")
    game:GetService("InsertService"):SetAssetVersionUrl(baseUrl .. "/Asset/?assetversionid=%d")
    game:GetService("SocialService"):SetFriendUrl(baseUrl .. "/Game/LuaWebService/HandleSocialRequest.ashx?method=IsFriendsWith&playerid=%d&userid=%d")
    
    game:GetService("FriendService"):SetMakeFriendUrl(baseUrl .. "/Game/CreateFriend?firstUserId=%d&secondUserId=%d")
    game:GetService("FriendService"):SetBreakFriendUrl(baseUrl .. "/Game/BreakFriend?firstUserId=%d&secondUserId=%d")
    game:GetService("FriendService"):SetGetFriendsUrl(baseUrl .. "/Game/AreFriends?userId=%d")
        
    pcall(function() game:GetService("SocialService"):SetBestFriendUrl("http://kiseki.local/Game/LuaWebService/HandleSocialRequest.ashx?method=IsBestFriendsWith&playerid=%d&userid=%d") end)
    pcall(function() game:GetService("SocialService"):SetGroupUrl("http://kiseki.local/Game/LuaWebService/HandleSocialRequest.ashx?method=IsInGroup&playerid=%d&groupid=%d") end)
    pcall(function() game:GetService("SocialService"):SetGroupRankUrl("http://kiseki.local/Game/LuaWebService/HandleSocialRequest.ashx?method=GetGroupRank&playerid=%d&groupid=%d") end)
    pcall(function() game:GetService("SocialService"):SetGroupRoleUrl("http://kiseki.local/Game/LuaWebService/HandleSocialRequest.ashx?method=GetGroupRole&playerid=%d&groupid=%d") end)

    -- print(baseUrl .. "/Game/LoadPlaceInfo.ashx?PlaceId=" .. placeId .. "&Token=" .. token)
    -- this crashes?
    -- pcall(function() loadfile(baseUrl .. "/Game/LoadPlaceInfo.ashx?PlaceId=" .. placeId .. "&Token=" .. token)() end)
    
    -- pcall(function() game:GetService("NetworkServer"):SetIsPlayerAuthenticationRequired(true) end)
end

settings().Diagnostics.LuaRamLimit = 0

game:GetService("Players").PlayerAdded:connect(function(player)
    print("Player " .. player.userId .. " added")
end)

game:GetService("Players").PlayerRemoving:connect(function(player)
    print("Player " .. player.userId .. " leaving")
end)

if placeId ~= nil and baseUrl ~= nil then
    wait()
    game:Load(baseUrl .. "/asset/?id=" .. placeId .. "&token=" .. token)

end

ns:Start(port)

scriptContext:SetTimeout(10)
scriptContext.ScriptsDisabled = false

------------------------------END START GAME SHARED SCRIPT--------------------------

game:GetService("RunService"):Run()

local Players = game:GetService("Players")
local httpService = game:GetService("HttpService")
local userIdList = "" -- the data
local timeIntervalBetweenChecks = 20
local attemptsBeforeShutdown = 6 -- every 20 seconds gets checked, so if 6 attempts w/o players, then bye job (2 minutes)
local attempts = 0

local function updateUserListString(LeavingUserId)
    local userIds = {} -- make table for turning into string
    for _, player in ipairs(Players:GetPlayers()) do
        if not LeavingUserId or player.UserId ~= LeavingUserId then -- if player is not currently leaving
            table.insert(userIds, player.UserId)
        end
    end
    userIdList = table.concat(userIds, ",") -- concatenate all userIds with comma
end

local function postKeepAlive()
    httpService.HttpEnabled = true

    local serverToken = token
    local data = {
        ["ServerIP"] = serverToken,
        ["PlaceId"] = game.PlaceId,
        ["PlayerCount"] = #Players:GetChildren(),
        ["PlayerList"] = userIdList
    }

    local jsonData = httpService:JSONEncode(data)
    local url = baseUrl .. "/v1.1/keep_alive"

    print('[Kiseki] keepalive users: ' .. userIdList)
    
    local success, errorMessage = pcall(function()
        httpService:PostAsync(url, jsonData)
    end)
    
    if not success then
        warn("[Kiseki] Failed to send keepalive: " .. errorMessage)
    end
end

local function sendShutdownRequest()
    httpService.HttpEnabled = true

    local serverToken = token
    local data = {
        ["ServerIP"] = serverToken,
        ["PlaceId"] = game.PlaceId,
        ["PlayerCount"] = #Players:GetChildren(),
        ["PlayerList"] = userIdList
    }

    local jsonData = httpService:JSONEncode(data)
    local url = baseUrl .. "/api/arbiter/shutdown"

    print('[Kiseki] shutting down job due to lack of players')
    
    local success, errorMessage = pcall(function()
        httpService:PostAsync(url, jsonData)
    end)
    
    if not success then
        warn("[Kiseki] Failed to send close job request: " .. errorMessage)
    end
end


local function scheduleKeepAlive()
    while wait(timeIntervalBetweenChecks) do
        if #Players:GetChildren() == 0 then
            warn("[Kiseki] attempt #" .. attempts .. ", after #" .. attemptsBeforeShutdown .. ", job will shut down because of no players!")

            attempts = attempts + 1

            if attempts >= attemptsBeforeShutdown then
                sendShutdownRequest()
            end
        else
            attempts = 0
        end

        postKeepAlive()
    end
end

spawn(scheduleKeepAlive)

Players.PlayerAdded:connect(function(player)
    updateUserListString()
    print("Player " .. player.UserId .. " added")
    postKeepAlive()
end)

Players.PlayerRemoving:connect(function(player)
    updateUserListString(player.UserId)
    print("Player " .. player.UserId .. " leaving")
    postKeepAlive()
end)