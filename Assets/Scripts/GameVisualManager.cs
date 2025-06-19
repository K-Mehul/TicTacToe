using Unity.Netcode;
using UnityEngine;

public class GameVisualManager : NetworkBehaviour
{
    private readonly float GRID_SIZE = 3.1f;

    [Header("Prefabs")]
    [SerializeField] Transform m_CrossPrefab;
    [SerializeField] Transform m_CirclePrefab;

    void Start()
    {
        GameManager.Instance.OnClickedOnGridPosition += OnClickedOnGridPosition;
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

        Transform spawnedCrossTransform = Instantiate(spawnedPrefab, GetGridWorldPosition(x, y),Quaternion.identity);
        spawnedCrossTransform.GetComponent<NetworkObject>().Spawn(true);
    }

    private Vector2 GetGridWorldPosition(int x, int y)
    {
        return new Vector2(-GRID_SIZE + x * GRID_SIZE, -GRID_SIZE + y * GRID_SIZE);
    }
}
