using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 格子材质
/// </summary>
public abstract class GridMaterial : ICloneable
{
    /// <summary>
    /// 材质
    /// </summary>
    public abstract GridMaterialType MaterialType { get; }

    /// <summary>
    /// 深拷贝
    /// </summary>
    /// <returns></returns>
    public abstract object Clone();

    /// <summary>
    /// 需要更新状态的位置偏移量
    /// </summary>
    protected Vector2Int[] gridUpdateArray;

    protected Direction direction;
    /// <summary>
    /// 当前方块朝向
    /// </summary>
    public Direction Direction {  get { return direction; } }

    public GridMaterial()
    {
        gridUpdateArray = new Vector2Int[]
        {
            new Vector2Int(0, 1),
            new Vector2Int(1, 0),
            new Vector2Int(0, -1),
            new Vector2Int(-1, 0)
        };
    }

    /// <summary>
    /// 结束下落
    /// </summary>
    /// <param name="trans"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public virtual void FallDown(Transform trans, int x, int y)
    {

    }

    /// <summary>
    /// 更新周围格子的方法
    /// </summary>
    /// <param name="trans"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="updateSource"></param>
    public virtual void GridUpdate(Transform trans, int x, int y, bool updateSource)
    {
        if (updateSource)
        {
            Vector2Int pos = new Vector2Int(x, y);
            for (int i = 0; i < gridUpdateArray.Length; i++)
            {
                Vector2Int updateGridPos = pos + gridUpdateArray[i];
                GridData gridData = GameManager.Instance.GridsController.GetGridData(updateGridPos);
                gridData?.GridMaterial.GridUpdate(gridData.GameObject.transform, updateGridPos.x, updateGridPos.y, false);
            }
        }
    }

    /// <summary>
    /// 格子被消除
    /// </summary>
    /// <param name="go"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public virtual void Eliminate(GameObject go, int x, int y)
    {
        GridUpdate(go.transform, x, y, true);
    }

    /// <summary>
    /// 格子发生了旋转
    /// </summary>
    /// <param name="gridsParent"></param>
    public virtual void Rotate(GameObject gridsParent)
    {
        int value = (int)direction;
        ++value;
        if (value > (int)Direction.Left)
            value = (int)Direction.Up;
        direction = (Direction)value;
    }
}
