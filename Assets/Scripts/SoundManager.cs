using System;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private Transform m_PlaceSFXPrefab;
    [SerializeField] private Transform m_WinSFXPrefab;
    [SerializeField] private Transform m_LoseSFXPrefab;

    void Start()
    {
        GameManager.Instance.OnPlacedObject += GameManager_OnPlacedObject;
        GameManager.Instance.OnGameWin += GameManager_OnGameWin;
    }

    void OnDestroy()
    {
        GameManager.Instance.OnPlacedObject -= GameManager_OnPlacedObject;
        GameManager.Instance.OnGameWin -= GameManager_OnGameWin;
    }

    private void GameManager_OnGameWin(object sender, GameManager.OnGameWinEventArgs e)
    {
        if (GameManager.Instance.GetLocalPlayerType() == e.winPlayerType)
        {
            Transform sfxTransform = Instantiate(m_WinSFXPrefab);
            Destroy(sfxTransform.gameObject, 5f);
        }
        else
        {
            Transform sfxTransform = Instantiate(m_LoseSFXPrefab);
            Destroy(sfxTransform.gameObject, 5f);
        }
    }

    private void GameManager_OnPlacedObject(object sender, EventArgs e)
    {
        Transform sfxTransform = Instantiate(m_PlaceSFXPrefab);
        Destroy(sfxTransform.gameObject, 5f);
    }
}
