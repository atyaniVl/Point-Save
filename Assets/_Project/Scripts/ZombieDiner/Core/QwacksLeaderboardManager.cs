using System;
using System.Threading.Tasks;
using UnityEngine;
using Flock;
using Flock.Config;
using Flock.Exceptions;
using Flock.Models;

namespace ZombieDiner.Core
{
    public static class QwacksLeaderboardManager
    {
        private static bool _isAuthenticating = false;

        public static async Task EnsureAuthenticatedAsync()
        {
            if (_isAuthenticating) return;

            try
            {
                _isAuthenticating = true;

                // 1. Initialize SDK if not initialized yet
                if (!FlockClient.IsInitialized)
                {
                    FlockConfigAsset configAsset = Resources.Load<FlockConfigAsset>("FlockConfig");
                    if (configAsset != null)
                    {
                        FlockClient.Create(configAsset.ToInitConfig());
                        Debug.Log("<color=cyan>[Qwacks] FlockClient initialized from FlockConfig asset.</color>");
                    }
                    else
                    {
                        Debug.LogError("[Qwacks] Failed to load FlockConfig asset from Resources.");
                        return;
                    }
                }

                // 2. Check Authentication
                if (FlockClient.IsInitialized && !FlockClient.Instance.IsAuthenticated)
                {
                    // Try to restore an existing session
                    bool restored = await FlockClient.Instance.Authentication.TryRestoreSessionAsync();
                    if (restored)
                    {
                        Debug.Log($"<color=green>[Qwacks] Session restored for player: {FlockClient.Instance.CurrentPlayerId}</color>");
                        return;
                    }

                    // Perform Device Authentication using deviceId
                    string deviceId = SystemInfo.deviceUniqueIdentifier;
                    Debug.Log($"<color=yellow>[Qwacks] Authenticating via Device ID: {deviceId}...</color>");

                    try
                    {
                        await FlockClient.Instance.Authentication.LoginWithDeviceAsync(deviceId);
                        Debug.Log($"<color=green>[Qwacks] Device login successful! PlayerId: {FlockClient.Instance.CurrentPlayerId}</color>");
                    }
                    catch (FlockException)
                    {
                        // Fallback to Device Registration if not registered yet
                        await FlockClient.Instance.Authentication.RegisterWithDeviceAsync(deviceId);
                        Debug.Log($"<color=green>[Qwacks] Device registration successful! PlayerId: {FlockClient.Instance.CurrentPlayerId}</color>");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Qwacks] Device authentication failed: {ex.Message}");
            }
            finally
            {
                _isAuthenticating = false;
            }
        }

        public static async Task SubmitWaveScoreAsync(int waveReached, string leaderboardName = "Zombie Serving Leaderboard")
        {
            try
            {
                await EnsureAuthenticatedAsync();

                if (!FlockClient.IsInitialized || !FlockClient.Instance.IsAuthenticated)
                {
                    Debug.LogWarning("[Qwacks] Cannot submit score: Qwacks SDK unauthenticated.");
                    return;
                }

                string myPlayerId = FlockClient.Instance.CurrentPlayerId;
                Debug.Log($"<color=cyan>[Qwacks] Submitting Wave={waveReached} for Leaderboard '{leaderboardName}' (PlayerId: {myPlayerId})...</color>");

                // 1. Fetch player data rows for the authenticated player
                var allData = await FlockClient.Instance.Player.GetAllDataAsync(myPlayerId);
                string playerDataId = null;

                if (allData != null && allData.Items != null && allData.Items.Length > 0)
                {
                    playerDataId = allData.Items[0].Id;
                }

                // 2. If row exists, update the Wave field
                if (!string.IsNullOrEmpty(playerDataId))
                {
                    await FlockClient.Instance.Commands.UpdatePlayerDataFieldAsync(playerDataId, "Wave", waveReached);
                    Debug.Log($"<color=green>[Qwacks] Leaderboard updated! Wave={waveReached} for PlayerDataId: {playerDataId}</color>");
                    return;
                }

                // 3. If no row exists yet for this player, auto-create a PlayerData row using the active Player Template
                var templates = await FlockClient.Instance.Player.GetTemplatesAsync();
                if (templates != null && templates.Count > 0)
                {
                    string templateId = templates[0].Id;
                    var initialFields = new System.Collections.Generic.Dictionary<string, object>
                    {
                        { "Wave", waveReached }
                    };

                    PlayerData newRow = await FlockClient.Instance.Player.CreatePlayerDataAsync(templateId, initialFields);
                    if (newRow != null && !string.IsNullOrEmpty(newRow.Id))
                    {
                        Debug.Log($"<color=green>[Qwacks] Created new PlayerData row ({newRow.Id}) and updated Leaderboard Wave={waveReached}!</color>");
                        return;
                    }
                }

                Debug.LogWarning($"<color=orange>[Qwacks] Could not create PlayerData row for Player ({myPlayerId}). Ensure a Player Template is created in Qwacks Dashboard.</color>");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Qwacks] Error submitting wave score to Leaderboard: {ex.Message}");
            }
        }
    }
}
