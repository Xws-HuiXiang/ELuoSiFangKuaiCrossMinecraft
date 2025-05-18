using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameMenuPanel : PanelBase
{
    [SerializeField]
    private Button continueBtn;
    [SerializeField]
    private Button uploadScoreBtn;
    [SerializeField]
    private Button gobackBtn;

    private void Awake()
    {
        continueBtn.onClick.AddListener(OnContinueBtnClick);
        uploadScoreBtn.onClick.AddListener(OnUploadScoreBtnClick);
        gobackBtn.onClick.AddListener(OnGobackBtnClick);
    }

    /// <summary>
    /// 返回主界面按钮点击事件
    /// </summary>
    private void OnGobackBtnClick()
    {
        GameManager.Instance.ClearGame();
        GameManager.Instance.UIController.EnterPanel.Show();
        Hide();
    }

    /// <summary>
    /// 上传分数按钮点击事件
    /// </summary>
    private void OnUploadScoreBtnClick()
    {

    }

    /// <summary>
    /// 继续按钮点击事件
    /// </summary>
    private void OnContinueBtnClick()
    {
        Hide();
    }

    private void OnEnable()
    {
        GameManager.Instance.GamePause();
    }

    private void OnDisable()
    {
        GameManager.Instance.GamePlay();
    }
}
