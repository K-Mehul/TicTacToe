using System;
using TMPro;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private GameObject m_CrossArrowGO;
    [SerializeField] private GameObject m_CircleArrowGO;
    [SerializeField] private GameObject m_CrossYouTextGO;
    [SerializeField] private GameObject m_CircleYouTextGO;
    [SerializeField] private TMP_Text m_CrossScoreTextMesh;
    [SerializeField] private TMP_Text m_CircleScoreTextMesh;

    void Awake()
    {
        m_CrossArrowGO.SetActive(false);
        m_CircleArrowGO.SetActive(false);
        m_CrossYouTextGO.SetActive(false);
        m_CircleYouTextGO.SetActive(false);
        m_CrossScoreTextMesh.text = string.Empty;
        m_CircleScoreTextMesh.text = string.Empty;
    }

    void Start()
    {
        GameManager.Instance.OnGameStarted += GameManager_OnGameStarted;
        GameManager.Instance.OnCurrentPlayablePlayerTypeChanged += GameManager_OnCurrentPlayablePlayerType;
        GameManager.Instance.OnScoreChanged += GameManager_OnScoreChanged;
    }

    private void GameManager_OnScoreChanged(object sender, EventArgs e)
    {
        GameManager.Instance.GetScore(out int crossScore, out int circleScore);

        m_CircleScoreTextMesh.text = circleScore.ToString();
        m_CrossScoreTextMesh.text = crossScore.ToString();
    }

    void OnDestroy()
    {
        GameManager.Instance.OnGameStarted -= GameManager_OnGameStarted;
        GameManager.Instance.OnCurrentPlayablePlayerTypeChanged -= GameManager_OnCurrentPlayablePlayerType;
        GameManager.Instance.OnScoreChanged -= GameManager_OnScoreChanged;
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

        m_CircleScoreTextMesh.text = "0";
        m_CrossScoreTextMesh.text = "0";

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
        else if(GameManager.Instance.GetCurrentPlayablePlayerType() == GameManager.PlayerType.Cross)
        {
            m_CircleArrowGO.SetActive(false);
            m_CrossArrowGO.SetActive(true);
        }
    }
}
