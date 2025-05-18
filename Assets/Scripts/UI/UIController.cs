using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour
{
    [SerializeField]
    private Canvas canvas;

    [SerializeField]
    private EnterPanel enterPanel;
    public EnterPanel EnterPanel {  get { return enterPanel; } }
    [SerializeField]
    private AboutPanel aboutPanel;
    public AboutPanel AboutPanel { get { return aboutPanel; } }
    [SerializeField]
    private GamePanel gamePanel;
    public GamePanel GamePanel { get { return gamePanel; } }
    [SerializeField]
    private GameMenuPanel gameMenuPanel;
    public GameMenuPanel GameMenuPanel { get { return gameMenuPanel; } }
    [SerializeField]
    private HandbookPanel handbookPanel;
    public HandbookPanel HandbookPanel { get { return handbookPanel; } }

    private void Awake()
    {
        //初始只展示 enterPanel
        enterPanel.gameObject.SetActive(true);
        aboutPanel.gameObject.SetActive(false);
        gamePanel.gameObject.SetActive(false);
        gameMenuPanel.gameObject.SetActive(false);
        handbookPanel.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PanelBase panel = GetTopPanel();
            if (panel != null)
            {
                switch (panel.EscapeAction)
                {
                    case EscapeAction.Goback:
                        panel.Goback();
                        break;
                    case EscapeAction.ExitGame:
                        Application.Quit();
                        break;
                    case EscapeAction.Custom:
                        panel.EscapeCustomAction();
                        break;
                    default:
                        Debug.LogWarning($"未处理的返回按钮行为类型：{panel.EscapeAction}");
                        break;
                }
            }
        }
    }

    /// <summary>
    /// 获取最上层显示的UI
    /// </summary>
    /// <returns></returns>
    private PanelBase GetTopPanel()
    {
        int childCount = canvas.transform.childCount;
        for (int i = childCount - 1; i >= 0; i--)
        {
            Transform child = canvas.transform.GetChild(i);
            if (child.gameObject.activeSelf)
            {
                return child.GetComponent<PanelBase>();
            }
        }

        return null;
    }
}
