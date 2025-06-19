using System;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    public event EventHandler<OnClickedOnPositionEventArgs> OnClickedOnGridPosition;
    public event EventHandler OnGameStarted;
    public event EventHandler OnCurrentPlayablePlayerTypeChanged;
    public class OnClickedOnPositionEventArgs : EventArgs
    {
        public int x;
        public int y;
        public PlayerType playerType;
    }

    public enum PlayerType
    {
        None,
        Cross,
        Circle
    }

    private PlayerType m_LocalPlayerType;
    private NetworkVariable<PlayerType> m_CurrentPlayablePlayerType = new NetworkVariable<PlayerType>();
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log($"{NetworkManager.Singleton.LocalClientId}");

        m_LocalPlayerType = NetworkManager.Singleton.LocalClientId == 0 ? PlayerType.Cross : PlayerType.Circle;

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
        }

        m_CurrentPlayablePlayerType.OnValueChanged += (PlayerType oldPlayerType, PlayerType newPlayerType) =>
        {
            OnCurrentPlayablePlayerTypeChanged?.Invoke(this, EventArgs.Empty);
        };
    }

    private void NetworkManager_OnClientConnectedCallback(ulong obj)
    {
        if (NetworkManager.Singleton.ConnectedClientsList.Count == 2)
        {
            //Start Game
            m_CurrentPlayablePlayerType.Value = PlayerType.Circle;
            TriggerOnGameStartedRpc();
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnGameStartedRpc()
    {
        OnGameStarted?.Invoke(this, EventArgs.Empty);
    }

    [Rpc(SendTo.Server)]
    public void ClickedOnGridPositionRpc(int x, int y, PlayerType playerType)
    {
        if (playerType != m_CurrentPlayablePlayerType.Value) return;

        OnClickedOnGridPosition?.Invoke(this, new OnClickedOnPositionEventArgs
        {
            x = x,
            y = y,
            playerType = playerType
        });

        switch (m_CurrentPlayablePlayerType.Value)
        {
            default:
            case PlayerType.Cross:
                m_CurrentPlayablePlayerType.Value = PlayerType.Circle;
                break;

            case PlayerType.Circle:
                m_CurrentPlayablePlayerType.Value = PlayerType.Cross;
                break;
        }
    }

    public PlayerType GetLocalPlayerType() => m_LocalPlayerType;

    public PlayerType GetCurrentPlayablePlayerType() => m_CurrentPlayablePlayerType.Value;
}
