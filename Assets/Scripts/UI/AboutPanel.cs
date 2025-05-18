using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AboutPanel : PanelBase
{
    [SerializeField]
    private Button gobackBtn;

    private void Awake()
    {
        gobackBtn.onClick.AddListener(OnGobackBtnClick);
    }

    /// <summary>
    /// 返回按钮点击事件
    /// </summary>
    private void OnGobackBtnClick()
    {
        GameManager.Instance.UIController.EnterPanel.Show();
        Hide();
    }
}
