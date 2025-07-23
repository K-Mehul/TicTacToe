using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.Rendering;

public class GameLobby : MonoBehaviour
{
    private Lobby hostLobby;
    private float heartBeatTimer;
    [SerializeField] private string playerName = "Mehul";

    private async void Start()
    {
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log($"Signed in {AuthenticationService.Instance.PlayerId}");
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    private void Update()
    {
        HandleLobbyHeartbeat();
    }

    private async Task HandleLobbyHeartbeat()
    {
        if(hostLobby != null)
        {
            heartBeatTimer -= Time.deltaTime;
            if(heartBeatTimer < 0)
            {
                float heartBeatTimerMax = 15;
                heartBeatTimer = heartBeatTimerMax;

                await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
            }
        }
    }

    [ContextMenu("CreateLobby")]
    private async void CreateLobby()
    {
        try
        {
            string lobbyName = "MyLobby";
            int maxPlayers = 2;

            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
            {
                IsPrivate = true,
                Player = new Player
                {
                    Data = new Dictionary<string, PlayerDataObject>
                    {
                        {"PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName) },
                    }
                }
            };
             
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);

            hostLobby = lobby;

            Debug.Log($"Created Lobby! NAME: {lobby.Name}, MP: {lobby.MaxPlayers}, ID: {lobby.Id}, LobbyCode: {lobby.LobbyCode}");
            PrintPlayers(hostLobby);
        }
        catch (LobbyServiceException ex)
        {
            Debug.Log(ex);
        }
    }


    [ContextMenu("ListLobbies")]
    private async void ListLobbies()
    {
        try
        {
            QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions
            {
                Count = 25,
                Filters = new List<QueryFilter>
                {
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots,"0",QueryFilter.OpOptions.GT)
                },
                Order = new List<QueryOrder>
                {
                    new QueryOrder(false,QueryOrder.FieldOptions.Created)
                }
            };

            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync(queryLobbiesOptions);

            Debug.Log($"Lobbies found : {queryResponse.Results.Count}");

            foreach(Lobby lobby in queryResponse.Results)
            {
                Debug.Log($"{lobby.Name} : {lobby.MaxPlayers}");
            }
        }
        catch(LobbyServiceException ex)
        {
            Debug.LogError(ex);
        }
    }

    private async void JoinLobbyById()
    {
        try
        {
            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync();

            await LobbyService.Instance.JoinLobbyByIdAsync(queryResponse.Results[0].Id);
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogError(ex);
        }
    }

    private async void JoinLobbyById(string joinCode)
    {
        try
        {
            await LobbyService.Instance.JoinLobbyByCodeAsync(joinCode);

            Debug.Log($"Joined lobby with code : {joinCode}");
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogError(ex);
        }
    }

    private async void QuickJoinLobby()
    {
        try
        {
            await LobbyService.Instance.QuickJoinLobbyAsync();
        }
        catch(LobbyServiceException ex)
        {
            Debug.LogError($"{ex.Message}");
        }
    }

    private void PrintPlayers(Lobby lobby)
    {
        Debug.Log($"Players in lobby: {lobby.Name}");
        foreach (Player player in lobby.Players)
        {
            Debug.Log(player.Id + " " + player.Data["PlayerName"].Value);
        }
    }
}
