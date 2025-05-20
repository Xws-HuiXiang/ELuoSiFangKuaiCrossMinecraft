using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridMaterialController
{
    /// <summary>
    /// 形状材质字典
    /// </summary>
    private Dictionary<GridMaterialType, GridMaterial> gridMaterialsDict = new Dictionary<GridMaterialType, GridMaterial>();
    /// <summary>
    /// 形状材质权重列表
    /// </summary>
    private List<(GridMaterialType value, int weight)> gridMaterialsWeightList = new List<(GridMaterialType, int)>();

    private Dictionary<GridMaterialType, Sprite> gridMaterialSpriteDict = new Dictionary<GridMaterialType, Sprite>();

    public GridMaterialController()
    {
        RegisterAllGridMaterials();
    }

    /// <summary>
    /// 获取格子材质图片
    /// </summary>
    /// <param name="materialType"></param>
    /// <returns></returns>
    public Sprite GetGridMaterialSprite(GridMaterialType materialType)
    {
        if (!gridMaterialSpriteDict.TryGetValue(materialType, out var sprite))
        {
            sprite = Resources.Load<Sprite>($"GridMaterials/{materialType.ToString()}");
            if (sprite == null)
                Debug.LogError($"未找到材质：GridMaterials/{materialType}");
            else
                gridMaterialSpriteDict.Add(materialType, sprite);
        }

        return sprite;
    }

    /// <summary>
    /// 注册所有格子材质
    /// </summary>
    private void RegisterAllGridMaterials()
    {
        RegisterGridMaterial(new DirtGridMaterial(), 2);
        RegisterGridMaterial(new CobblestoneGridMaterial(), 2);
        RegisterGridMaterial(new StoneGridMaterial(), 2);
        RegisterGridMaterial(new GrassBlockGridMaterial(), 2);
        RegisterGridMaterial(new FurnaceGridMaterial(), 2);
        RegisterGridMaterial(new RedStoneBlockGridMaterial(), 2);
        RegisterGridMaterial(new RedStonePwederGridMaterial(), 2);
        RegisterGridMaterial(new TNTBlockGridMaterial(), 1);
        RegisterGridMaterial(new ChestGridMaterial(), 1);
        RegisterGridMaterial(new ShulkerBoxGridMaterial(), 1);
        RegisterGridMaterial(new AnvilGridMaterial(), 1);
        RegisterGridMaterial(new ObsidianGridMaterial(), 2);
        RegisterGridMaterial(new SandGridMaterial(), 2);
        RegisterGridMaterial(new GlassGridMaterial(), 2);
        RegisterGridMaterial(new CactusGridMaterial(), 2);
        RegisterGridMaterial(new IronBlockGridMaterial(), 2);
        RegisterGridMaterial(new SnowBlockGridMaterial(), 2);
        RegisterGridMaterial(new CarvedPumpkinGridMaterial(), 2);
        RegisterGridMaterial(new HopperGridMaterial(), 2);
    }

    /// <summary>
    /// 注册格子材质
    /// </summary>
    /// <param name="gridMaterial"></param>
    /// <param name="weight"></param>
    public void RegisterGridMaterial(GridMaterial gridMaterial, int weight)
    {
        if (gridMaterialsDict.ContainsKey(gridMaterial.MaterialType))
        {
            Debug.LogError($"材质已经注册过了, tag = {gridMaterial.MaterialType}");
            return;
        }

        gridMaterialsDict.Add(gridMaterial.MaterialType, gridMaterial);
        gridMaterialsWeightList.Add((gridMaterial.MaterialType, weight));
    }

    /// <summary>
    /// 根据权重随机生成一个格子材质
    /// </summary>
    /// <returns></returns>
    public GridMaterialType RandomGenerateGridMaterialTypeByWeighted()
    {
        int totalWeight = gridMaterialsWeightList.Sum(item => item.weight);
        int randomValue = UnityEngine.Random.Range(0, totalWeight);
        int currentSum = 0;
        foreach (var item in gridMaterialsWeightList)
        {
            currentSum += item.weight;
            if (randomValue < currentSum)
                return item.value;
        }

        Debug.LogWarning($"随机生成材质溢出：{currentSum}");
        return GridMaterialType.Stone;
    }

    /// <summary>
    /// 获取格子材质
    /// </summary>
    /// <param name="gridMaterialType"></param>
    /// <returns></returns>
    public GridMaterial GetGridMaterial(GridMaterialType gridMaterialType)
    {
        if (gridMaterialsDict.TryGetValue(gridMaterialType, out GridMaterial gridMaterial))
            return gridMaterial;

        Debug.LogError($"未注册的材质：{gridMaterialType}");
        return null;
    }
}
