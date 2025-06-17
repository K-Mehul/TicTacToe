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
        SpawnedObjectRpc(e.x,e.y);
    }

    [Rpc(SendTo.Server)]
    private void SpawnedObjectRpc(int x, int y)
    {
        Transform spawnedCrossTransform = Instantiate(m_CrossPrefab, GetGridWorldPosition(x, y),Quaternion.identity);
        spawnedCrossTransform.GetComponent<NetworkObject>().Spawn(true);
    }

    private Vector2 GetGridWorldPosition(int x, int y)
    {
        return new Vector2(-GRID_SIZE + x * GRID_SIZE, -GRID_SIZE + y * GRID_SIZE);
    }
}
