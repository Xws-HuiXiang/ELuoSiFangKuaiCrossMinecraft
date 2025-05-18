using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnterPanel : PanelBase
{
    [SerializeField]
    private Button startGameBtn;
    [SerializeField]
    private Button handbookBtn;
    [SerializeField]
    private Button rankBtn;
    [SerializeField]
    private Button aboutBtn;

    private void Awake()
    {
        escapeAction = EscapeAction.Custom;

        startGameBtn.onClick.AddListener(OnStartGameBtnClick);
        handbookBtn.onClick.AddListener(OnHandbookBtnClick);
        rankBtn.onClick.AddListener(OnRankBtnClick);
        aboutBtn.onClick.AddListener(OnAboutBtnClick);
    }

    /// <summary>
    /// 关于按钮点击事件
    /// </summary>
    private void OnAboutBtnClick()
    {
        GameManager.Instance.UIController.AboutPanel.Show();
        Hide();
    }

    /// <summary>
    /// 排行榜按钮点击事件
    /// </summary>
    private void OnRankBtnClick()
    {

    }

    /// <summary>
    /// 图鉴按钮点击事件
    /// </summary>
    private void OnHandbookBtnClick()
    {
        GameManager.Instance.UIController.HandbookPanel.Show();
        Hide();
    }

    /// <summary>
    /// 开始游戏按钮点击事件
    /// </summary>
    private void OnStartGameBtnClick()
    {
        GameManager.Instance.ClearGame();
        GameManager.Instance.StartGame();
        GameManager.Instance.UIController.GamePanel.Show();
        Hide();
    }

    public override void EscapeCustomAction()
    {
        base.EscapeCustomAction();

        //TODO:弹窗提示退出游戏

    }
}
