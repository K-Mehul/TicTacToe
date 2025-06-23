using System;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEditor.Rendering.LookDev;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    public event EventHandler<OnClickedOnPositionEventArgs> OnClickedOnGridPosition;
    public event EventHandler OnGameStarted;
    public event EventHandler OnGameTied;
    public event EventHandler OnScoreChanged;
    public event EventHandler<OnGameWinEventArgs> OnGameWin;
    public event EventHandler OnGameRematch;
    public event EventHandler OnPlacedObject;
    public class OnGameWinEventArgs : EventArgs
    {
        public Line line;
        public PlayerType winPlayerType;
    }
    public event EventHandler OnCurrentPlayablePlayerTypeChanged;
    public class OnClickedOnPositionEventArgs : EventArgs
    {
        public int x;
        public int y;
        public PlayerType playerType;
    }

    public enum Orientation
    {
        Horizontal,
        Vertical,
        DiagonalA,
        DiagonalB
    }
    public enum PlayerType
    {
        None,
        Cross,
        Circle
    }

    public struct Line
    {
        public List<Vector2Int> gridVector2IntList;
        public Vector2Int centerGridPosition;
        public Orientation orientation;
    }

    private List<Line> m_LineList;
    private PlayerType m_LocalPlayerType;
    private NetworkVariable<PlayerType> m_CurrentPlayablePlayerType = new NetworkVariable<PlayerType>();
    private PlayerType[,] m_PlayerTypeArray;
    private NetworkVariable<int> m_PlayedCrossScore = new NetworkVariable<int>();
    private NetworkVariable<int> m_PlayedCircleScore = new NetworkVariable<int>();
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        m_PlayerTypeArray = new PlayerType[3, 3];

        m_LineList = new List<Line>
        {
            //Horizontal
            new Line{
                gridVector2IntList = new List<Vector2Int>{
                    new Vector2Int(0,0),new Vector2Int(1,0),new Vector2Int(2,0)
                },
                centerGridPosition = new Vector2Int(1,0),
                orientation = Orientation.Horizontal
            },
            new Line{
                gridVector2IntList = new List<Vector2Int>{
                    new Vector2Int(0,1),new Vector2Int(1,1),new Vector2Int(2,1)
                },
                centerGridPosition = new Vector2Int(1,1),
                orientation = Orientation.Horizontal
            },
            new Line{
                gridVector2IntList = new List<Vector2Int>{
                    new Vector2Int(0,2),new Vector2Int(1,2),new Vector2Int(2,2)
                },
                centerGridPosition = new Vector2Int(1,2),
                orientation = Orientation.Horizontal
            },
            //Vertical
            new Line{
                gridVector2IntList = new List<Vector2Int>{
                    new Vector2Int(0,0),new Vector2Int(0,1),new Vector2Int(0,2)
                },
                centerGridPosition = new Vector2Int(0,1),
                orientation = Orientation.Vertical
            },
            new Line{
                gridVector2IntList = new List<Vector2Int>{
                    new Vector2Int(1,0),new Vector2Int(1,1),new Vector2Int(1,2)
                },
                centerGridPosition = new Vector2Int(1,1),
                orientation = Orientation.Vertical
            },
            new Line{
                gridVector2IntList = new List<Vector2Int>{
                    new Vector2Int(2,0),new Vector2Int(2,1),new Vector2Int(2,2)
                },
                centerGridPosition = new Vector2Int(2,1),
                orientation = Orientation.Vertical
            },
            //Diagonal
            new Line{
                gridVector2IntList = new List<Vector2Int>{
                    new Vector2Int(0,0),new Vector2Int(1,1),new Vector2Int(2,2)
                },
                centerGridPosition = new Vector2Int(1,1),
                orientation = Orientation.DiagonalA
            },
            new Line{
                gridVector2IntList = new List<Vector2Int>{
                    new Vector2Int(0,2),new Vector2Int(1,1),new Vector2Int(2,0)
                },
                centerGridPosition = new Vector2Int(1,1),
                orientation = Orientation.DiagonalB
            },
        };
    }

    public override void OnNetworkSpawn()
    {
        m_LocalPlayerType = NetworkManager.Singleton.LocalClientId == 0 ? PlayerType.Cross : PlayerType.Circle;

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
        }

        m_CurrentPlayablePlayerType.OnValueChanged += (PlayerType oldPlayerType, PlayerType newPlayerType) =>
        {
            OnCurrentPlayablePlayerTypeChanged?.Invoke(this, EventArgs.Empty);
        };

        m_PlayedCrossScore.OnValueChanged += (int previousScore, int newScore) =>
        {
            OnScoreChanged?.Invoke(this, EventArgs.Empty);
        };

        m_PlayedCircleScore.OnValueChanged += (int previousScore, int newScore) =>
        {
            OnScoreChanged?.Invoke(this, EventArgs.Empty);
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

        if (m_PlayerTypeArray[x, y] != PlayerType.None)
        {
            //Already occupied
            return;
        }

        m_PlayerTypeArray[x, y] = playerType;

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

        TestWinner();
    }

    private void TestWinner()
    {
        for (int i = 0; i < m_LineList.Count; i++)
        {
            Line line = m_LineList[i];
            if (TestWinnerLine(line))
            {
                m_CurrentPlayablePlayerType.Value = PlayerType.None;
                PlayerType winType = m_PlayerTypeArray[line.centerGridPosition.x, line.centerGridPosition.y];

                switch (winType)
                {
                    case PlayerType.Circle:
                        m_PlayedCircleScore.Value++;
                        break;

                    case PlayerType.Cross:
                        m_PlayedCrossScore.Value++;
                        break;
                }

                TriggerOnGameWinRpc(i, winType);
                return;
            }
        }

        bool hasTie = true;
        for (int i = 0; i < m_PlayerTypeArray.GetLength(0); i++)
        {
            for (int j = 0; j < m_PlayerTypeArray.GetLength(1); j++)
            {
                if (m_PlayerTypeArray[i, j] == PlayerType.None)
                {
                    hasTie = false;
                    break;
                }
            }
        }

        if (hasTie)
        {
            m_CurrentPlayablePlayerType.Value = PlayerType.None;
            TriggerOnGameTiedRpc();
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnGameTiedRpc()
    {
        OnGameTied?.Invoke(this, EventArgs.Empty);
    }


    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnGameWinRpc(int lineIndex, PlayerType winPlayerType)
    {
        Line line = m_LineList[lineIndex];
        OnGameWin?.Invoke(this, new OnGameWinEventArgs
        {
            line = line,
            winPlayerType = winPlayerType
        });
    }

    private bool TestWinnerLine(PlayerType a, PlayerType b, PlayerType c)
    {
        return a != PlayerType.None && a == b && b == c;
    }

    private bool TestWinnerLine(Line line)
    {
        return TestWinnerLine(
            m_PlayerTypeArray[line.gridVector2IntList[0].x, line.gridVector2IntList[0].y],
            m_PlayerTypeArray[line.gridVector2IntList[1].x, line.gridVector2IntList[1].y],
            m_PlayerTypeArray[line.gridVector2IntList[2].x, line.gridVector2IntList[2].y]
        );
    }

    [Rpc(SendTo.Server)]
    public void RematchRpc()
    {
        for (int i = 0; i < m_PlayerTypeArray.GetLength(0); i++)
        {
            for (int j = 0; j < m_PlayerTypeArray.GetLength(1); j++)
            {
                m_PlayerTypeArray[i, j] = PlayerType.None;
            }
        }

        m_CurrentPlayablePlayerType.Value = PlayerType.Cross;

        TriggerOnRematchRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnRematchRpc()
    {
        OnGameRematch?.Invoke(this, EventArgs.Empty);
    }

    public PlayerType GetLocalPlayerType() => m_LocalPlayerType;

    public PlayerType GetCurrentPlayablePlayerType() => m_CurrentPlayablePlayerType.Value;

    public void GetScore(out int playerCrossScore, out int playerCircleScore)
    {
        playerCrossScore = m_PlayedCrossScore.Value;
        playerCircleScore = m_PlayedCircleScore.Value;
    }
}
