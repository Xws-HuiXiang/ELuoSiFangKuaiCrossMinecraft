using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 泥土材质
/// </summary>
public class DirtGridMaterial : GridMaterial
{
    public override GridMaterialType MaterialType => GridMaterialType.Dirt;

    public override object Clone()
    {
        return new DirtGridMaterial();
    }
}

/// <summary>
/// 圆石材质
/// </summary>
public class CobblestoneGridMaterial : GridMaterial, IFurnace
{
    public override GridMaterialType MaterialType => GridMaterialType.Cobblestone;

    public override object Clone()
    {
        return new CobblestoneGridMaterial();
    }

    public void Smelt(int x, int y)
    {
        GridData gridData = GameManager.Instance.GridsController.GetGridData(x, y);
        if (gridData != null)
        {
            gridData.GridMaterial = new StoneGridMaterial();
        }
    }
}

/// <summary>
/// 石头材质
/// </summary>
public class StoneGridMaterial : GridMaterial
{
    public override GridMaterialType MaterialType => GridMaterialType.Stone;

    public override object Clone()
    {
        return new StoneGridMaterial();
    }
}

/// <summary>
/// 草方块材质
/// </summary>
public class GrassBlockGridMaterial : GridMaterial
{
    public override GridMaterialType MaterialType => GridMaterialType.GrassBlock;

    public override object Clone()
    {
        return new GrassBlockGridMaterial();
    }
}

/// <summary>
/// 熔炉
/// </summary>
public class FurnaceGridMaterial : GridMaterial
{
    public override GridMaterialType MaterialType => GridMaterialType.Furnace;

    public FurnaceGridMaterial()
    {
        //监听周围八个格子的更新
        gridUpdateArray = new Vector2Int[]
        {
            new Vector2Int(0, 1),
            new Vector2Int(1, 0),
            new Vector2Int(0, -1),
            new Vector2Int(-1, 0),
            new Vector2Int(1, 1),
            new Vector2Int(-1, -1),
            new Vector2Int(1, -1),
            new Vector2Int(-1, 1)
        };
    }

    public override object Clone()
    {
        return new FurnaceGridMaterial();
    }

    /// <summary>
    /// 尝试烧制周围的格子
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    protected virtual void TrySmelt(int x, int y)
    {
        //调用周围八格可烧制接口
        for (int px = x - 1; px <= x + 1; px++)
        {
            for (int py = y - 1; py <= y + 1; py++)
            {
                //忽略自身
                if (px == x && py == y) continue;
                GridData gridData = GameManager.Instance.GridsController.GetGridData(px, py);
                if (gridData != null)
                {
                    if (gridData.GridMaterial is IFurnace furnace)
                    {
                        furnace.Smelt(px, py);
                        Effects.Instance.PlaySmeltEffect(px, py);
                    }
                }
            }
        }
    }

    public override void FallDown(Transform trans, int x, int y)
    {
        base.FallDown(trans, x, y);

        TrySmelt(x, y);
    }

    public override async UniTask GridUpdate(Transform trans, int x, int y, bool updateSource)
    {
        await base.GridUpdate(trans, x, y, updateSource);

        TrySmelt(x, y);
    }
}

/// <summary>
/// 红石块材质
/// </summary>
public class RedStoneBlockGridMaterial : GridMaterial
{
    public override GridMaterialType MaterialType => GridMaterialType.RedStoneBlock;

    public override object Clone()
    {
        return new RedStoneBlockGridMaterial();
    }

    public override async UniTask GridUpdate(Transform trans, int x, int y, bool updateSource)
    {
        await base.GridUpdate(trans, x, y, updateSource);

        //向四周提供红石能量
        for (int i = 0; i < gridUpdateArray.Length; i++)
        {
            Vector2Int pos = new Vector2Int(x + gridUpdateArray[i].x, y + gridUpdateArray[i].y);
            GridData gridData = GameManager.Instance.GridsController.GetGridData(pos);
            if (gridData != null)
            {
                if (gridData.GridMaterial is IRedStoneEnergy redStoneEnergy)
                    redStoneEnergy.RedStoneEnergy(pos.x, pos.y, true);
            }
        }
    }
}

/// <summary>
/// 红石粉材质
/// </summary>
public class RedStonePwederGridMaterial : GridMaterial, IRedStoneEnergy
{
    protected bool energy = false;
    /// <summary>
    /// 是否含有红石能量
    /// </summary>
    public bool Energy { get { return energy; } }

    public override GridMaterialType MaterialType => GridMaterialType.RedStonePweder_disable;

    public override object Clone()
    {
        return new RedStonePwederGridMaterial() { energy = this.energy };
    }

    public void RedStoneEnergy(int x, int y, bool energy)
    {
        this.energy = energy;

        //获取相连的所有红石粉格子
        List<GridData> redStonePwederList = new List<GridData>();
        List<(int x, int y)> redStonePwederPosList = new List<(int x, int y)>();

        List<(GridData data, int x, int y)> checkList = new List<(GridData, int, int)>();
        List<GridData> closeList = new List<GridData>
        {
            GameManager.Instance.GridsController.GetGridData(x, y)
        };
        var gd = GameManager.Instance.GridsController.GetGridData(x, y + 1);
        if (gd != null) checkList.Add((gd, x, y + 1));
        gd = GameManager.Instance.GridsController.GetGridData(x + 1, y);
        if (gd != null) checkList.Add((gd, x + 1, y));
        gd = GameManager.Instance.GridsController.GetGridData(x, y - 1);
        if (gd != null) checkList.Add((gd, x, y - 1));
        gd = GameManager.Instance.GridsController.GetGridData(x - 1, y);
        if (gd != null) checkList.Add((gd, x - 1, y));
        while (checkList.Count > 0)
        {
            var gridInfo = checkList.First();

            GridData gridData = gridInfo.data;
            if (gridData == null) break;
            closeList.Add(gridData);
            checkList.RemoveAt(0);

            if (gridData.GridMaterial is RedStonePwederGridMaterial)
            {
                //这个格子是红石粉
                redStonePwederList.Add(gridData);
                redStonePwederPosList.Add((gridInfo.x, gridInfo.y));

                //将相邻的格子加入待检查列表
                GridData up = GameManager.Instance.GridsController.GetGridData(gridInfo.x, gridInfo.y + 1);
                GridData right = GameManager.Instance.GridsController.GetGridData(gridInfo.x + 1, gridInfo.y);
                GridData down = GameManager.Instance.GridsController.GetGridData(gridInfo.x, gridInfo.y - 1);
                GridData left = GameManager.Instance.GridsController.GetGridData(gridInfo.x - 1, gridInfo.y);
                if (up != null && !closeList.Contains(up)) checkList.Add((up, gridInfo.x, gridInfo.y + 1));
                if (right != null && !closeList.Contains(right)) checkList.Add((right, gridInfo.x + 1, gridInfo.y));
                if (down != null && !closeList.Contains(down)) checkList.Add((down, gridInfo.x, gridInfo.y - 1));
                if (left != null && !closeList.Contains(left)) checkList.Add((left, gridInfo.x - 1, gridInfo.y));
            }
        }
        //修改材质
        if (energy)
        {
            GridData gridData = GameManager.Instance.GridsController.GetGridData(x, y);
            if (gridData != null)
            {
                gridData.GameObject.GetComponent<SpriteRenderer>().sprite = GameManager.Instance.GridMaterialController.GetGridMaterialSprite(GridMaterialType.RedStonePweder);
            }
            for (int i = 0; i < redStonePwederList.Count; i++)
            {
                GridData linkedGridData = redStonePwederList[i];
                linkedGridData.GameObject.GetComponent<SpriteRenderer>().sprite = GameManager.Instance.GridMaterialController.GetGridMaterialSprite(GridMaterialType.RedStonePweder);
            }
        }
        else
        {
            GridData gridData = GameManager.Instance.GridsController.GetGridData(x, y);
            if (gridData != null)
            {
                gridData.GameObject.GetComponent<SpriteRenderer>().sprite = GameManager.Instance.GridMaterialController.GetGridMaterialSprite(GridMaterialType.RedStonePweder_disable);
            }
            for (int i = 0; i < redStonePwederList.Count; i++)
            {
                GridData linkedGridData = redStonePwederList[i];
                linkedGridData.GameObject.GetComponent<SpriteRenderer>().sprite = GameManager.Instance.GridMaterialController.GetGridMaterialSprite(GridMaterialType.RedStonePweder_disable);
            }
        }

        //相连的所有红石粉进行一次更新
        for (int i = 0; i < redStonePwederList.Count; i++)
        {
            for (int j = 0; j < gridUpdateArray.Length; j++)
            {
                int posX =  redStonePwederPosList[i].x + gridUpdateArray[j].x;
                int posY = redStonePwederPosList[i].y + gridUpdateArray[j].y;

                GridData gridData = GameManager.Instance.GridsController.GetGridData(posX, posY);
                gridData?.GridMaterial.GridUpdate(gridData.GameObject.transform, posX, posY, false);

                //向非红石粉方块传递红石信号
                if (gridData?.GridMaterial is not RedStonePwederGridMaterial && gridData?.GridMaterial is IRedStoneEnergy redStoneEnergy)
                    redStoneEnergy.RedStoneEnergy(posX, posY, this.energy);
            }
        }
    }
}

/// <summary>
/// TNT方块材质
/// </summary>
public class TNTBlockGridMaterial : GridMaterial, IRedStoneEnergy
{
    /// <summary>
    /// 爆炸半径
    /// </summary>
    protected int burstingRadius = 2;

    public override GridMaterialType MaterialType => GridMaterialType.TNTBlock;

    public override object Clone()
    {
        return new TNTBlockGridMaterial() { burstingRadius = this.burstingRadius };
    }

    public void RedStoneEnergy(int x, int y, bool energy)
    {
        if (energy)
        {
            Debug.Log($"({x},{y}) 的tnt 爆炸");

            //爆炸
            for (int px = x - burstingRadius; px <= x + burstingRadius; px++)
            {
                for (int py = y - burstingRadius; py <= y + burstingRadius; py++)
                {
                    if ((Mathf.Abs(x - px) + Mathf.Abs(y - py)) > burstingRadius) continue;

                    //获取格子数据
                    GridData gridData = GameManager.Instance.GridsController.GetGridData(px, py);
                    if (gridData != null)
                    {
                        //销毁格子
                        UnityEngine.Object.Destroy(gridData.GameObject);
                        GameManager.Instance.GridsController.SetGridData(px, py, null);
                    }
                }
            }
        }
    }
}

/// <summary>
/// 箱子材质
/// </summary>
public class ChestGridMaterial : GridMaterial
{
    /// <summary>
    /// 箱子最大容量
    /// </summary>
    public int chestTotalCapacity = 8;
    private List<GridData> chestContentList;

    public ChestGridMaterial()
    {
        chestContentList = new List<GridData>();
    }

    public override GridMaterialType MaterialType => GridMaterialType.Chest;

    public override object Clone()
    {
        return new ChestGridMaterial()
        {
            chestTotalCapacity = this.chestTotalCapacity,
            chestContentList = this.chestContentList
        };
    }

    public override async UniTask GridUpdate(Transform trans, int x, int y, bool updateSource)
    {
        await base.GridUpdate(trans, x, y, updateSource);

        //如果箱子没满则尝试存储周围的方块
        if (chestContentList.Count < chestTotalCapacity)
        {
            for (int px = x - 1; px <= x + 1; px++)
            {
                for (int py = y - 1; py <= y + 1; py++)
                {
                    //忽略自身
                    if (px == x && py == y) continue;
                    if (chestContentList.Count >= chestTotalCapacity) break;

                    //获取格子数据
                    GridData gridData = GameManager.Instance.GridsController.GetGridData(px, py);
                    if (gridData != null)
                    {
                        //不能存储箱子
                        if (gridData.GridMaterial is ChestGridMaterial) continue;
                        //不能存储潜影盒
                        if (gridData.GridMaterial is ShulkerBoxGridMaterial) continue;
                        //不能存储末影箱
                        if (gridData.GridMaterial is EnderChestGridMaterial) continue;

                        //存储格子数据
                        chestContentList.Add(gridData);
                        GameManager.Instance.GridsController.SetGridData(px, py, null);
                        gridData.GameObject.SetActive(false);
                    }
                }
            }
        }
    }

    public override async UniTask Eliminate(GameObject go, int x, int y)
    {
        await base.Eliminate(go, x, y);

        List<GridData> resetList = new List<GridData>();
        //箱子里的物品随机填充到某一列的顶部
        for (int i = 0; i < chestContentList.Count; i++)
        {
            GridData gridData = chestContentList[i];

            int colIndex = UnityEngine.Random.Range(0, GameManager.Instance.GridsController.GridWidthCount);
            //从上到下找到第一个空格子
            int posY = GameManager.Instance.GridsController.GridHeightCount;
            for (int py = posY - 1; py >= 0; py--)
            {
                GridData gd = GameManager.Instance.GridsController.GetGridData(colIndex, py);
                if (gd != null)
                    break;
                else
                    --posY;
            }
            if (posY != GameManager.Instance.GridsController.GridHeightCount)
            {
                GameManager.Instance.GridsController.SetGridData(colIndex, posY, gridData);
                Vector2 pos = GameManager.Instance.GridsController.GetGridPosition(colIndex, posY);
                gridData.GameObject.transform.localPosition = pos;
                gridData.GameObject.SetActive(true);

                Debug.Log($"填充位置：{colIndex},{posY}");
            }
            else
                resetList.Add(gridData);
        }
        if (resetList.Count > 0)
        {
            List<(int x, int y)> emptyGridList = new List<(int x, int y)>();
            //填充到任意的空格
            for (int py = 0; py < GameManager.Instance.GridsController.GridHeightCount; py++)
            {
                for (int px = 0; px < GameManager.Instance.GridsController.GridWidthCount; px++)
                {
                    GridData gridData = GameManager.Instance.GridsController.GetGridData(px, py);
                    if (gridData == null)
                        emptyGridList.Add((px, py));
                }
            }
            //只有这么多空格子了，还有剩余的物品不管了，直接销毁
            for (int i = 0; i < emptyGridList.Count; i++)
            {
                var pos = emptyGridList[i];

                GridData gridData = resetList[i];
                GameManager.Instance.GridsController.SetGridData(pos.x, pos.y, gridData);
                Vector2 p = GameManager.Instance.GridsController.GetGridPosition(pos.x, pos.y);
                gridData.GameObject.transform.localPosition = p;
                gridData.GameObject.SetActive(true);
            }
            //如果还有剩余的物体，直接销毁
            if (resetList.Count > emptyGridList.Count)
            {
                int count = resetList.Count - emptyGridList.Count;
                for (int i = 0; i < count; i++)
                {
                    GridData gridData = resetList[i + emptyGridList.Count];
                    UnityEngine.Object.Destroy(gridData.GameObject);
                }
            }
        }

        resetList.Clear();
        chestContentList.Clear();
    }
}

/// <summary>
/// 潜影盒材质
/// </summary>
public class ShulkerBoxGridMaterial : GridMaterial
{
    /// <summary>
    /// 潜影盒最大容量
    /// </summary>
    public int chestTotalCapacity = 8;
    private List<GridData> chestContentList;

    public ShulkerBoxGridMaterial()
    {
        chestContentList = new List<GridData>();
    }

    public override GridMaterialType MaterialType => GridMaterialType.ShulkerBox;

    public override object Clone()
    {
        return new ShulkerBoxGridMaterial()
        {
            chestTotalCapacity = this.chestTotalCapacity,
            chestContentList = this.chestContentList
        };
    }

    public override async UniTask GridUpdate(Transform trans, int x, int y, bool updateSource)
    {
        await base.GridUpdate(trans, x, y, updateSource);

        //如果潜影盒没满则尝试存储周围的方块
        if (chestContentList.Count < chestTotalCapacity)
        {
            for (int px = x - 1; px <= x + 1; px++)
            {
                for (int py = y - 1; py <= y + 1; py++)
                {
                    //忽略自身
                    if (px == x && py == y) continue;
                    if (chestContentList.Count >= chestTotalCapacity) break;

                    //获取格子数据
                    GridData gridData = GameManager.Instance.GridsController.GetGridData(px, py);
                    if (gridData != null)
                    {
                        //不能存储箱子
                        if (gridData.GridMaterial is ChestGridMaterial) continue;
                        //不能存储潜影盒
                        if (gridData.GridMaterial is ShulkerBoxGridMaterial) continue;
                        //不能存储末影箱
                        if (gridData.GridMaterial is EnderChestGridMaterial) continue;

                        //存储格子数据
                        chestContentList.Add(gridData);
                        GameManager.Instance.GridsController.SetGridData(px, py, null);
                        gridData.GameObject.SetActive(false);
                    }
                }
            }
        }
    }

    public override async UniTask Eliminate(GameObject go, int x, int y)
    {
        await base.Eliminate(go, x, y);

        //同时清除已经存储的方块
        for (int i = 0; i < chestContentList.Count; i++)
        {
            GridData gridData = chestContentList[i];
            UnityEngine.Object.Destroy(gridData.GameObject);
        }
    }
}

/// <summary>
/// 铁砧材质
/// </summary>
public class AnvilGridMaterial : GridMaterial
{
    public override GridMaterialType MaterialType => GridMaterialType.Anvil;
    public override object Clone()
    {
        return new AnvilGridMaterial();
    }

    public override void FallDown(Transform trans, int x, int y)
    {
        base.FallDown(trans, x, y);

        //如果这一列下方还有空格，则将整列的格子下落
        var res = GameManager.Instance.GridsController.SearchDownwardsForTheFirstEmptyGrid(x, y);
        if (res.Item1 != -1)
        {
            int ty = res.Item1;
            //下方有空格，可以下落
            for (int i = res.Item2.Count - 1; i >= 0; i--)
            {
                GridData gridData = res.Item2[i];
                GameManager.Instance.GridsController.MoveGrid(gridData, x, ty);
                ++ty;
            }
        }
    }
}

/// <summary>
/// 黑曜石材质
/// </summary>
public class ObsidianGridMaterial : GridMaterial
{
    public override GridMaterialType MaterialType => GridMaterialType.Obsidian;

    public override object Clone()
    {
        return new ObsidianGridMaterial();
    }
}

/// <summary>
/// 沙子材质
/// </summary>
public class SandGridMaterial : GridMaterial
{
    public override GridMaterialType MaterialType => GridMaterialType.Sand;

    public override object Clone()
    {
        return new SandGridMaterial();
    }

    public override async UniTask GridUpdate(Transform trans, int x, int y, bool updateSource)
    {
        await base.GridUpdate(trans, x, y, updateSource);

        if (updateSource)
        {
            var emptyPosY = GameManager.Instance.GridsController.SearchDownwardsForTheLastEmptyGrid(x, y);
            if (emptyPosY != -1)
            {
                //下方有空格，可以下落
                GameManager.Instance.GridsController.MoveGrid(x, y, x, emptyPosY);

                GridData gridData = GameManager.Instance.GridsController.GetGridData(x, emptyPosY);
                //触发一次更新事件
                await GridUpdate(gridData.GameObject.transform, x, emptyPosY, true);
            }
        }
    }
}

/// <summary>
/// 沙砾材质
/// </summary>
public class GravelGridMaterial : GridMaterial
{
    public override GridMaterialType MaterialType => GridMaterialType.Gravel;

    public override object Clone()
    {
        return new GravelGridMaterial();
    }

    public override async UniTask GridUpdate(Transform trans, int x, int y, bool updateSource)
    {
        await base.GridUpdate(trans, x, y, updateSource);

        if (updateSource)
        {
            var emptyPosY = GameManager.Instance.GridsController.SearchDownwardsForTheLastEmptyGrid(x, y);
            if (emptyPosY != -1)
            {
                //下方有空格，可以下落
                GameManager.Instance.GridsController.MoveGrid(x, y, x, emptyPosY);

                GridData gridData = GameManager.Instance.GridsController.GetGridData(x, emptyPosY);
                //触发一次更新事件
                await GridUpdate(gridData.GameObject.transform, x, emptyPosY, true);
            }
        }
    }
}

/// <summary>
/// 玻璃材质
/// </summary>
public class GlassGridMaterial : GridMaterial
{
    public override GridMaterialType MaterialType => GridMaterialType.Glass;

    public override object Clone()
    {
        return new GlassGridMaterial();
    }

    public override async UniTask GridUpdate(Transform trans, int x, int y, bool updateSource)
    {
        await base.GridUpdate(trans, x, y, updateSource);

        //如果头顶有方块，则玻璃碎裂
        var gridData = GameManager.Instance.GridsController.GetGridData(x, y + 1);
        if (gridData != null)
        {
            if (gridData.GridMaterial is GlassGridMaterial) return;

            //头顶有方块且非玻璃，玻璃自身被销毁
            GameManager.Instance.GridsController.DestroyGridData(x, y);
        }
    }
}

/// <summary>
/// 仙人掌材质
/// </summary>
public class CactusGridMaterial : GridMaterial
{
    public override GridMaterialType MaterialType => GridMaterialType.Cactus;

    public override object Clone()
    {
        return new CactusGridMaterial();
    }

    public override async UniTask GridUpdate(Transform trans, int x, int y, bool updateSource)
    {
        await base.GridUpdate(trans, x, y, updateSource);

        //如果头顶有方块，则方块被摧毁
        var upGridData = GameManager.Instance.GridsController.GetGridData(x, y + 1);
        if (upGridData != null)
            GameManager.Instance.GridsController.DestroyGridData(x, y + 1);
        //如果仙人掌两侧有方块，则仙人掌被摧毁
        var leftGridData = GameManager.Instance.GridsController.GetGridData(x - 1, y);
        var rightGridData = GameManager.Instance.GridsController.GetGridData(x + 1, y);
        if (leftGridData != null || rightGridData != null)
            GameManager.Instance.GridsController.DestroyGridData(x, y);
    }
}

/// <summary>
/// 末影箱材质
/// </summary>
public class EnderChestGridMaterial : GridMaterial
{
    public override GridMaterialType MaterialType => GridMaterialType.EnderChest;

    public override object Clone()
    {
        return new EnderChestGridMaterial();
    }

    public override async UniTask GridUpdate(Transform trans, int x, int y, bool updateSource)
    {
        await base.GridUpdate(trans, x, y, updateSource);

        //如果头顶有方块，则方块将被随机移动到其他位置
        var upGridData = GameManager.Instance.GridsController.GetGridData(x, y + 1);
        if (upGridData != null)
        {
            List<int> colArray = new List<int>();
            for (int i = 0; i < GameManager.Instance.GridsController.GridWidthCount; i++)
                colArray.Add(i);
            List<(int x, int y)> cacheList = new List<(int x, int y)>();
            while (colArray.Count > 0)
            {
                int colIndex = UnityEngine.Random.Range(0, colArray.Count);
                //判断这一列有没有空格子，不会移动到最上面三行
                for (int py = 0; py < GameManager.Instance.GridsController.GridHeightCount - 3; py++)
                {
                    GridData gridData = GameManager.Instance.GridsController.GetGridData(x, py);
                    if (gridData == null)
                        cacheList.Add((colIndex, py));
                }
                if (cacheList.Count <= 0)
                {
                    //这一列没有空格子，删除这一列并再次随机
                    colArray.RemoveAt(colIndex);
                }
                else
                    break;
            }
            //如果没有空格子则不处理移动
            if (cacheList.Count > 0)
            {
                int index = UnityEngine.Random.Range(0, cacheList.Count);
                var pos = cacheList[index];

                GameManager.Instance.GridsController.SetGridData(x, y + 1, null);
                GameManager.Instance.GridsController.SetGridData(pos.x, pos.y, upGridData);
                upGridData.GameObject.transform.localPosition = GameManager.Instance.GridsController.GetGridPosition(pos.x, pos.y);
            }
        }
    }
}

/// <summary>
/// 铁块材质
/// </summary>
public class IronBlockGridMaterial : GridMaterial
{
    public override GridMaterialType MaterialType => GridMaterialType.IronBlock;

    public override object Clone()
    {
        return new IronBlockGridMaterial();
    }
}

/// <summary>
/// 雪块材质
/// </summary>
public class SnowBlockGridMaterial : GridMaterial
{
    public override GridMaterialType MaterialType => GridMaterialType.SnowBlock;

    public override object Clone()
    {
        return new SnowBlockGridMaterial();
    }
}

/// <summary>
/// 雕刻南瓜材质
/// </summary>
public class CarvedPumpkinGridMaterial : GridMaterial
{
    public override GridMaterialType MaterialType => GridMaterialType.CarvedPumpkin;

    public override object Clone()
    {
        return new CarvedPumpkinGridMaterial();
    }
}

/// <summary>
/// 漏斗材质
/// </summary>
public class HopperGridMaterial : GridMaterial
{
    public override GridMaterialType MaterialType => GridMaterialType.Hopper;

    public override object Clone()
    {
        return new HopperGridMaterial() { direction = base.direction };
    }

    public override void Rotate(GameObject gridsParent)
    {
        base.Rotate(gridsParent);

        int childCount = gridsParent.transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            Transform child = gridsParent.transform.GetChild(i);
            child.localEulerAngles = new Vector3(0, 0, -90 * (int)direction);
        }
    }

    public override async UniTask GridUpdate(Transform trans, int x, int y, bool updateSource)
    {
        await base.GridUpdate(trans, x, y, updateSource);

        //如果漏斗前方有方块且后方没有方块，则将前方的方块移动到后方
        GridData fromGridData = null;
        GridData toGridData = null;
        Vector2Int fromPos = Vector2Int.zero;
        Vector2Int toPos = Vector2Int.zero;
        switch (Direction)
        {
            case Direction.Up:
                fromGridData = GameManager.Instance.GridsController.GetGridData(x, y + 1);
                toGridData = GameManager.Instance.GridsController.GetGridData(x, y - 1);
                fromPos = new Vector2Int(x, y + 1);
                toPos = new Vector2Int(x, y - 1);
                break;
            case Direction.Right:
                fromGridData = GameManager.Instance.GridsController.GetGridData(x + 1, y);
                toGridData = GameManager.Instance.GridsController.GetGridData(x - 1, y);
                fromPos = new Vector2Int(x + 1, y);
                toPos = new Vector2Int(x - 1, y);
                break;
            case Direction.Down:
                fromGridData = GameManager.Instance.GridsController.GetGridData(x, y - 1);
                toGridData = GameManager.Instance.GridsController.GetGridData(x, y + 1);
                fromPos = new Vector2Int(x, y - 1);
                toPos = new Vector2Int(x, y + 1);
                break;
            case Direction.Left:
                fromGridData = GameManager.Instance.GridsController.GetGridData(x - 1, y);
                toGridData = GameManager.Instance.GridsController.GetGridData(x + 1, y);
                fromPos = new Vector2Int(x - 1, y);
                toPos = new Vector2Int(x + 1, y);
                break;
            default:
                Debug.LogError($"未处理的朝向：{Direction}");
                break;
        }
        if (fromGridData != null && toGridData == null)
        {
            //无法移动漏斗方块
            if (fromGridData.GridMaterial is HopperGridMaterial) return;
            //无法移动箱子
            if (fromGridData.GridMaterial is ChestGridMaterial) return;
            //无法移动末影箱
            if (fromGridData.GridMaterial is EnderChestGridMaterial) return;
            //无法移动潜影盒
            if (fromGridData.GridMaterial is ShulkerBoxGridMaterial) return;

            //检查坐标是否越界
            if (toPos.x < 0 || toPos.x >= GameManager.Instance.GridsController.GridWidthCount ||
                toPos.y < 0 || toPos.y >= GameManager.Instance.GridsController.GridHeightCount)
            {
                return;
            }

            //移动方块
            GameManager.Instance.GridsController.SetGridData(fromPos.x, fromPos.y, null);
            GameManager.Instance.GridsController.SetGridData(toPos.x, toPos.y, fromGridData);
            fromGridData.GameObject.transform.localPosition = GameManager.Instance.GridsController.GetGridPosition(toPos.x, toPos.y);

            //触发被移动方块更新
            await fromGridData.GridMaterial.GridUpdate(fromGridData.GameObject.transform, toPos.x, toPos.y, true);
        }
    }
}
