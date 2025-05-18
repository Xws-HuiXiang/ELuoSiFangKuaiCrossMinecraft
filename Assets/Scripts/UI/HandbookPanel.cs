using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HandbookPanel : PanelBase
{
    [SerializeField]
    private Button gobackBtn;
    [SerializeField]
    private RectTransform blockListContentRectTrans;
    [SerializeField]
    private GameObject handbookItem;

    /// <summary>
    /// 图鉴配置信息
    /// </summary>
    private HandbookConfigData[] configs;

    private void Awake()
    {
        gobackBtn.onClick.AddListener(OnGobackBtnClick);

        TextAsset textAsset = Resources.Load<TextAsset>("Config/HandbookConfig");
        configs = JsonMapper.ToObject<HandbookConfigData[]>(textAsset.text);

        CreateAllHandbookItems();
    }

    /// <summary>
    /// 返回按钮点击事件
    /// </summary>
    private void OnGobackBtnClick()
    {
        GameManager.Instance.UIController.EnterPanel.Show();
        Hide();
    }

    /// <summary>
    /// 创建全部图鉴项
    /// </summary>
    private void CreateAllHandbookItems()
    {
        for (int i = 0; i < configs.Length; i++)
            CreateHandbookItem(configs[i]);
    }

    /// <summary>
    /// 根据配置信息创建一个图鉴项
    /// </summary>
    /// <param name="data"></param>
    private void CreateHandbookItem(HandbookConfigData data)
    {
        GameObject handbookGO = Instantiate(handbookItem, blockListContentRectTrans);
        handbookGO.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        handbookGO.transform.localScale = Vector3.one;
        handbookGO.transform.Find("Icon").GetComponent<Image>().sprite = Resources.Load<Sprite>(data.Icon);
        handbookGO.transform.Find("BlockName").GetComponent<TextMeshProUGUI>().text = data.BlockName;
        handbookGO.transform.Find("Description").GetComponent<TextMeshProUGUI>().text = data.Description;
        handbookGO.SetActive(true);
    }
}

/// <summary>
/// 图鉴项配置对象
/// </summary>
public class HandbookConfigData
{
    public string BlockName { get; set; }
    public string Description { get; set; }
    public string Icon { get; set; }
}