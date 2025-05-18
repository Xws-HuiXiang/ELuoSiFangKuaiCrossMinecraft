using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 形状类型
/// </summary>
public enum GridShapeType
{
    /// <summary>
    /// x x
    /// x x
    /// </summary>
    O,
    /// <summary>
    /// x
    /// x
    /// x
    /// x
    /// </summary>
    I,
    /// <summary>
    ///   x x
    /// x x
    /// </summary>
    S,
    /// <summary>
    /// x x
    ///   x x
    /// </summary>
    Z,
    /// <summary>
    /// x
    /// x
    /// x
    /// x x
    /// </summary>
    L,
    /// <summary>
    ///   x
    ///   x
    /// x x
    /// </summary>
    J,
    /// <summary>
    /// x x x
    ///   x
    /// </summary>
    T,
    /// <summary>
    /// 单个方块
    /// </summary>
    Single
}
