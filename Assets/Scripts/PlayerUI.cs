using System;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private GameObject m_CrossArrowGO;
    [SerializeField] private GameObject m_CircleArrowGO;
    [SerializeField] private GameObject m_CrossYouTextGO;
    [SerializeField] private GameObject m_CircleYouTextGO;

    void Awake()
    {
        m_CrossArrowGO.SetActive(false);
        m_CircleArrowGO.SetActive(false);
        m_CrossYouTextGO.SetActive(false);
        m_CircleYouTextGO.SetActive(false);
    }

    void Start()
    {
        GameManager.Instance.OnGameStarted += GameManager_OnGameStarted;
        GameManager.Instance.OnCurrentPlayablePlayerTypeChanged += GameManager_OnCurrentPlayablePlayerType;
    }


    void OnDestroy()
    {
        GameManager.Instance.OnGameStarted -= GameManager_OnGameStarted;
        GameManager.Instance.OnCurrentPlayablePlayerTypeChanged -= GameManager_OnCurrentPlayablePlayerType;
    }

    private void GameManager_OnGameStarted(object sender, EventArgs e)
    {
        if (GameManager.Instance.GetLocalPlayerType() == GameManager.PlayerType.Cross)
        {
            m_CrossYouTextGO.SetActive(true);
        }
        else if (GameManager.Instance.GetLocalPlayerType() == GameManager.PlayerType.Circle)
        {
            m_CircleYouTextGO.SetActive(true);
        }

        UpdateCurrentArrow();
    }

     private void GameManager_OnCurrentPlayablePlayerType(object sender, EventArgs e)
    {
        UpdateCurrentArrow();
    }

    private void UpdateCurrentArrow()
    {
        if (GameManager.Instance.GetCurrentPlayablePlayerType() == GameManager.PlayerType.Circle)
        {
            m_CircleArrowGO.SetActive(true);
            m_CrossArrowGO.SetActive(false);
        }
        else
        {
            m_CircleArrowGO.SetActive(false);
            m_CrossArrowGO.SetActive(true);
        }
    }
}
