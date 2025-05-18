using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelBase : MonoBehaviour
{
    protected EscapeAction escapeAction;
    /// <summary>
    /// 返回按钮的行为
    /// </summary>
    public EscapeAction EscapeAction {  get { return escapeAction; }}

    /// <summary>
    /// 自定义返回按钮触发事件
    /// </summary>
    public virtual void EscapeCustomAction()
    {

    }

    /// <summary>
    /// 返回事件为Goback时调用
    /// </summary>
    public virtual void Goback()
    {
        this.gameObject.SetActive(false);
    }

    /// <summary>
    /// 展示这个界面
    /// </summary>
    public virtual void Show()
    {
        this.transform.SetAsLastSibling();
        this.gameObject.SetActive(true);
    }

    /// <summary>
    /// 隐藏这个界面
    /// </summary>
    public virtual void Hide()
    {
        this.gameObject.SetActive(false);
    }
}
