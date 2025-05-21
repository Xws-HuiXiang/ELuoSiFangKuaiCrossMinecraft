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

    [SerializeField]
    private Button leftArrowBtn;
    [SerializeField]
    private Button rightArrowBtn;
    [SerializeField]
    private Button downArrowBtn;
    [SerializeField]
    private Button rotateArrowBtn;

    private void Awake()
    {
        menuBtn.onClick.AddListener(OnMenuBtnClick);

        leftArrowBtn.onClick.AddListener(() => GameManager.Instance.GameInput(GameManager.GameInputType.Left));
        rightArrowBtn.onClick.AddListener(() => GameManager.Instance.GameInput(GameManager.GameInputType.Right));
        downArrowBtn.onClick.AddListener(() => GameManager.Instance.GameInput(GameManager.GameInputType.Down));
        rotateArrowBtn.onClick.AddListener(() => GameManager.Instance.GameInput(GameManager.GameInputType.Rotate));
    }

    /// <summary>
    /// 菜单按钮点击事件
    /// </summary>
    private void OnMenuBtnClick()
    {
        GameManager.Instance.UIController.GameMenuPanel.Show();
    }
}
