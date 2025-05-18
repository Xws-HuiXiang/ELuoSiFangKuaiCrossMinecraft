using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 熔炉接口
/// </summary>
public interface IFurnace
{
    /// <summary>
    /// 熔炉烧制
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    void Smelt(int x, int y);
}
