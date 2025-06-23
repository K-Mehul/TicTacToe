using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] TMP_Text m_ResultTextMesh;
    [SerializeField] private Color m_WinColor;
    [SerializeField] private Color m_LoseColor;
    [SerializeField] private Color m_TiedColor;
    [SerializeField] Button m_RematchButton;

    void Start()
    {
        GameManager.Instance.OnGameWin += GameManager_OnGameWin;
        GameManager.Instance.OnGameRematch += GameManager_OnGameRematch;
        GameManager.Instance.OnGameTied += GameManager_OnGameTied;

        m_RematchButton.onClick.AddListener(() =>
        {
            GameManager.Instance.RematchRpc();
        });

        Hide();
    }

    private void GameManager_OnGameTied(object sender, EventArgs e)
    {
        m_ResultTextMesh.text = "TIE";
        m_ResultTextMesh.color = m_TiedColor;

        Show();
    }

    private void GameManager_OnGameRematch(object sender, EventArgs e)
    {
        Hide();
    }

    private void GameManager_OnGameWin(object sender, GameManager.OnGameWinEventArgs e)
    {
        if (e.winPlayerType == GameManager.Instance.GetLocalPlayerType())
        {
            m_ResultTextMesh.text = "YOU WIN!";
            m_ResultTextMesh.color = m_WinColor;
        }
        else
        {
            m_ResultTextMesh.text = "YOU LOSE!";
            m_ResultTextMesh.color = m_LoseColor;
        }

        Show();
    }

    private void Hide()
    {
        this.gameObject.SetActive(false);
    }

    private void Show()
    {
        this.gameObject.SetActive(true);
    }
}
