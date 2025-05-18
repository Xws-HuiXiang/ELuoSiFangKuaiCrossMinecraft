using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GamePanel : PanelBase
{
    [SerializeField]
    private Button menuBtn;
    [SerializeField]
    private TextMeshProUGUI scoreTMPro;

    private void Awake()
    {
        menuBtn.onClick.AddListener(OnMenuBtnClick);
    }

    /// <summary>
    /// 菜单按钮点击事件
    /// </summary>
    private void OnMenuBtnClick()
    {
        GameManager.Instance.UIController.GameMenuPanel.Show();
    }
}
