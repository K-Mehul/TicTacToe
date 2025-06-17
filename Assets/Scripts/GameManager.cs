using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public event EventHandler<OnClickedOnPositionEventArgs> OnClickedOnGridPosition;
    public class OnClickedOnPositionEventArgs : EventArgs
    {
        public int x;
        public int y;
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    public void ClickedOnGridPosition(int x, int y)
    {
        Debug.Log($"Clicked on grid position : {x},{y}");

        OnClickedOnGridPosition?.Invoke(this, new OnClickedOnPositionEventArgs
        {
            x = x,
            y = y
        });
    }
}
