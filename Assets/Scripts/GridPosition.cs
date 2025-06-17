using UnityEngine;

public class GridPosition : MonoBehaviour
{
    [Header("GridSettings")]
    [SerializeField] private int m_X;
    [SerializeField] private int m_Y;

    void OnMouseDown()
    {
        GameManager.Instance.ClickedOnGridPosition(m_X, m_Y);
    }
}
