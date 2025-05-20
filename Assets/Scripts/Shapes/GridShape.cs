using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public abstract class GridShape
{
    private Vector2Int[] points;
    public abstract GridShapeType GridShapeType { get; }

    /// <summary>
    /// 当前图形的位置信息
    /// </summary>
    public Vector2Int Position { get; set; }
    /// <summary>
    /// 当前形状的材质
    /// </summary>
    public GridMaterial GridMaterial { get; set; }

    private GameObject gridsParent;
    /// <summary>
    /// 当前形状的所有格子的父物体
    /// </summary>
    public GameObject GridsParent { get { return gridsParent; } }

    public GridShape(Vector2Int[] points)
    {
        this.points = points;

        if (!points.Contains(Vector2Int.zero))
            Debug.LogWarning("GridShape的points必须包含(0,0)，这个点将作为旋转中心");
    }

    /// <summary>
    /// 创建这个形状，材质根据材质权重随机生成
    /// </summary>
    /// <param name="pos"></param>
    public virtual void Create(Vector2Int pos)
    {
        GridMaterialType gridMaterialType = GameManager.Instance.GridMaterialController.RandomGenerateGridMaterialTypeByWeighted();
        GridMaterial gridMaterial = GameManager.Instance.GridMaterialController.GetGridMaterial(gridMaterialType);
        Create(pos, gridMaterial);
    }

    /// <summary>
    /// 创建这个形状
    /// </summary>
    /// <param name="pos">在哪个位置创建</param>
    /// <param name="gridMaterial"></param>
    public virtual void Create(Vector2Int pos, GridMaterial gridMaterial)
    {
        Position = pos;
        this.GridMaterial = gridMaterial;

        gridsParent = new GameObject("GridsParent");
        gridsParent.transform.SetParent(GameManager.Instance.GridsController.GridsTrans);
        Vector2 localPosition = GameManager.Instance.GridsController.GetGridPosition(pos.x, pos.y);
        gridsParent.transform.localPosition = localPosition;
        for (int i = 0; i < points.Length; i++)
        {
            Vector2Int p = points[i];
            GameObject gridGO = new GameObject("Grid");
            gridGO.transform.SetParent(gridsParent.transform);
            gridGO.transform.localPosition = GameManager.Instance.GridsController.GetGridPosition(p.x, p.y);
            SpriteRenderer spriteRenderer = gridGO.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = GameManager.Instance.GridMaterialController.GetGridMaterialSprite(gridMaterial.MaterialType);
        }
    }

    /// <summary>
    /// 判断是否可以向指定方向移动
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    public virtual bool CanMovement(Vector2Int direction)
    {
        bool result = true;
        for (int i = 0; i < points.Length; i++)
        {
            Vector2Int p = points[i] + direction;
            Vector2Int p1 = new Vector2Int(Position.x + p.x, Position.y + p.y);
            //判断是否越界。顶部超过的部分不判断
            if (p1.x < 0 || p1.x >= GameManager.Instance.GridsController.GridWidthCount || p1.y < 0)
            {
                result = false;
                break;
            }
            GridData gridData = GameManager.Instance.GridsController.GetGridData(p1.x, p1.y);
            if (gridData != null)
            {
                result = false;
                break;
            }
        }

        return result;
    }

    /// <summary>
    /// 判断是否可以向下降一格
    /// </summary>
    /// <returns></returns>
    public virtual bool CanFallDown()
    {
        return CanMovement(Vector2Int.down);
    }

    /// <summary>
    /// 向指定方向移动一格
    /// </summary>
    /// <param name="direction"></param>
    public virtual void Movement(Vector2Int direction)
    {
        Position += direction;
        
        Vector2 pos = GameManager.Instance.GridsController.GetGridPosition(Position.x, Position.y);
        gridsParent.transform.localPosition = pos;
    }

    /// <summary>
    /// 向下降一格
    /// </summary>
    public virtual void FallDown()
    {
        Movement(Vector2Int.down);
    }

    /// <summary>
    /// 结束下落
    /// </summary>
    public virtual async UniTask FallDownEnd()
    {
        //将这个形状添加到网格数据中
        int childCount = gridsParent.transform.childCount;
        for (int i = childCount - 1; i >= 0; i--)
        {
            Transform child = gridsParent.transform.GetChild(i);
            int x = Mathf.RoundToInt(child.localPosition.x + Position.x);
            int y = Mathf.RoundToInt(child.localPosition.y + Position.y);
            //修改层级结构
            child.SetParent(GameManager.Instance.GridsController.GridsListTrans);
            if (x < 0 || x >= GameManager.Instance.GridsController.GridWidthCount ||
                y < 0 || y >= GameManager.Instance.GridsController.GridHeightCount)
                continue;
            GridMaterial gridMaterial = (GridMaterial)this.GridMaterial.Clone();
            GameManager.Instance.GridsController.SetGridData(x, y, new GridData(child.gameObject, gridMaterial));

            //触发材质落下事件
            gridMaterial.FallDown(child, x, y);
            //触发材质更新事件
            await gridMaterial.GridUpdate(child, x, y, true);
        }
        //删除形状组物体
        UnityEngine.Object.Destroy(gridsParent);

    }

    /// <summary>
    /// 判断是否可以旋转
    /// </summary>
    /// <returns></returns>
    public bool CanRotate()
    {
        //将所有的点旋转
        Vector2Int[] rotatePoints = CalcRotateResult();
        //判断是否可以旋转
        bool result = true;
        for (int i = 0; i < rotatePoints.Length; i++)
        {
            Vector2Int p = rotatePoints[i];
            Vector2Int p1 = new Vector2Int(Position.x + p.x, Position.y + p.y);

            //判断是否越界。顶部超过的部分不判断
            if (p1.x < 0 || p1.x >= GameManager.Instance.GridsController.GridWidthCount || p1.y < 0)
            {
                result = false;
                break;
            }
            GridData gridData = GameManager.Instance.GridsController.GetGridData(p1.x, p1.y);
            if (gridData != null)
            {
                result = false;
                break;
            }
        }

        return result;
    }

    /// <summary>
    /// 计算当前图形的旋转结果
    /// </summary>
    /// <returns></returns>
    private Vector2Int[] CalcRotateResult()
    {
        Vector2Int[] rotatePoints = new Vector2Int[points.Length];
        for (int i = 0; i < points.Length; i++)
        {
            Vector2Int point = points[i];
            if (point == Vector2Int.zero)
            {
                rotatePoints[i] = point;
                continue;
            }

            //构建从原点到目标点的向量
            Vector2Int op = point - Vector2Int.zero;
            //将向量旋转指定度数。起点坐标为（0,0），所以这个值即为旋转后的坐标
            Vector3 target = GameManager.Instance.GridsController.SingleRotateAngle * (Vector2)op;

            rotatePoints[i] = new Vector2Int(Mathf.RoundToInt(target.x), Mathf.RoundToInt(target.y));
        }

        return rotatePoints;
    }

    /// <summary>
    /// 应用方块旋转
    /// </summary>
    public void Rotate()
    {
        Vector2Int[] rotatePoints = CalcRotateResult();
        int childCount = gridsParent.transform.childCount;
        for (int i = childCount - 1; i >= 0; i--)
        {
            Vector2Int p = rotatePoints[i];
            Transform child = gridsParent.transform.GetChild(i);

            Vector2 pos = GameManager.Instance.GridsController.GetGridPosition(p.x, p.y);
            child.localPosition = pos;
        }
        this.GridMaterial.Rotate(gridsParent);

        this.points = rotatePoints;
    }
}
