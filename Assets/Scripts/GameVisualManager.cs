using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameVisualManager : NetworkBehaviour
{
    private readonly float GRID_SIZE = 3.1f;

    [Header("Prefabs")]
    [SerializeField] Transform m_CrossPrefab;
    [SerializeField] Transform m_CirclePrefab;
    [SerializeField] Transform m_LineCompletePrefab;

    private List<GameObject> m_VisualGOList;
    void Start()
    {
        m_VisualGOList = new List<GameObject>();
        GameManager.Instance.OnClickedOnGridPosition += OnClickedOnGridPosition;
        GameManager.Instance.OnGameWin += GameManager_OnGameWin;
        GameManager.Instance.OnGameRematch += GameManager_OnGameRematch;
    }

    private void GameManager_OnGameRematch(object sender, EventArgs e)
    {
         if (!NetworkManager.Singleton.IsServer)
        {
            return;
        }

        foreach (var visual in m_VisualGOList)
        {
            Destroy(visual);
        }

        m_VisualGOList.Clear();
    }

    // public override void OnDestroy()
    // {
    //     base.OnDestroy();
    //     GameManager.Instance.OnClickedOnGridPosition -= OnClickedOnGridPosition;
    //     GameManager.Instance.OnGameWin -= GameManager_OnGameWin;
    // }

    private void GameManager_OnGameWin(object sender, GameManager.OnGameWinEventArgs e)
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            return;
        }

        float eulerZ;
        switch (e.line.orientation)
        {
            default:
            case GameManager.Orientation.Horizontal: eulerZ = 0; break;
            case GameManager.Orientation.Vertical: eulerZ = 90; break;
            case GameManager.Orientation.DiagonalA: eulerZ = 45; break;
            case GameManager.Orientation.DiagonalB: eulerZ = -45; break;
        }

        Transform lineCompleteTransform = Instantiate(
            m_LineCompletePrefab,
            GetGridWorldPosition(e.line.centerGridPosition.x, e.line.centerGridPosition.y),
            Quaternion.Euler(0, 0, eulerZ));
        lineCompleteTransform.GetComponent<NetworkObject>().Spawn();

        m_VisualGOList.Add(lineCompleteTransform.gameObject);
    }

    private void OnClickedOnGridPosition(object sender, GameManager.OnClickedOnPositionEventArgs e)
    {
        SpawnObjectRpc(e.x,e.y,e.playerType);
    }

    [Rpc(SendTo.Server)]
    private void SpawnObjectRpc(int x, int y, GameManager.PlayerType playerType)
    {
        Transform spawnedPrefab;
        switch (playerType)
        {
            default:
            case GameManager.PlayerType.Cross:
                spawnedPrefab = m_CrossPrefab;
                break;

            case GameManager.PlayerType.Circle:
                spawnedPrefab = m_CirclePrefab;
                break;
        }

        Transform spawnedCrossTransform = Instantiate(spawnedPrefab, GetGridWorldPosition(x, y), Quaternion.identity);
        spawnedCrossTransform.GetComponent<NetworkObject>().Spawn(true);

        m_VisualGOList.Add(spawnedCrossTransform.gameObject);
    }

    private Vector2 GetGridWorldPosition(int x, int y)
    {
        return new Vector2(-GRID_SIZE + x * GRID_SIZE, -GRID_SIZE + y * GRID_SIZE);
    }
}
