using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class GridData
{
    private GameObject gameObject;
    public GameObject GameObject { get { return gameObject; } }
    private SpriteRenderer spriteRenderer;
    public SpriteRenderer SpriteRenderer { get { return spriteRenderer; } }
    private GridMaterial gridMaterial;
    public GridMaterial GridMaterial
    {
        get { return gridMaterial; }
        set
        {
            if (value != gridMaterial)
            {
                gridMaterial = value;
                SetMaterial(value);
            }
        }
    }

    public GridData(GameObject go, GridMaterial gridMaterial)
    {
        this.gameObject = go;
        this.gridMaterial = gridMaterial;
        spriteRenderer = go.GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// 修改材质
    /// </summary>
    /// <param name="gridMaterial"></param>
    protected virtual void SetMaterial(GridMaterial gridMaterial)
    {
        //修改材质图片
        spriteRenderer.sprite = GameManager.Instance.GridMaterialController.GetGridMaterialSprite(gridMaterial.MaterialType);
    }

    /// <summary>
    /// 格子被消除
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public async void Eliminate(int x, int y)
    {
        await this.gridMaterial.Eliminate(this.gameObject, x, y);
    }
}
