using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * 泥土，没有任何属性
 * 石头，没有任何属性
 * 熔炉，如果周围8格物品可以熔炼，则熔炼
 * 圆石，在熔炉周围时，被烧制为石头
 * 草方块，没有任何属性
 * 红石块，向四周提供红石信号
 * 红石粉，传递红石信号
 * TNT，四周有红石信号时爆炸，爆炸范围暂定为2
 * 箱子，周围有方块落下时将被存储到箱子里，箱子被破坏时将所有东西随机填充
 * 潜影盒，周围有方块落下时将被存储到潜影盒里，潜影盒被破坏时将所有东西销毁
 * 铁砧，落下后如果这一列的下方有空格，则这一列往下掉落
 * 黑曜石，无法被活塞推动，无法往下掉落。俄罗斯方块的消除规则和下落规则可以销毁黑曜石
 * 沙子，固定位置后如果下方有空格，则往下掉落
 * 沙砾，固定位置后如果下方有空格，则往下掉落
 * 玻璃，易碎，如果顶部有方块落下，则被破坏
 * 仙人掌，如果头顶有方块落下则直接破坏方块，如果两侧有方块则仙人掌被破坏
 * 末影箱，只要头顶有方块落下则随机将方块填入缝隙（不会随机到末影箱头顶），如果没有可用位置则不填充。优先填充缝隙
 * 铁块，可以召唤铁傀儡
 * 雪块，可以召唤雪傀儡
 * 雕刻南瓜，用于召唤铁傀儡和雪傀儡
 * 漏斗，将一侧的方块移动到另外一侧
 * 侦测器，朝向的面发生变化时，触发红石信号
 * 粘液块，可以黏住方块随活塞一起被推动
 * 蜂蜜块，可以黏住方块随活塞一起被推动，无法黏住粘液块
 * 活塞，可以推动朝向的方块，整体移动一格。存在bud状态（红石信号处于四个角时，此时活塞四周有更新则推动方块）
 * 粘性活塞，同活塞，但收回时可以将一个方块粘回来
 * 
 * 铁傀儡：不属于材质，向方块多的一侧移动，随机踩碎沿途方块
 * 雪傀儡：不属于材质，向屏幕投掷雪球，遮挡玩家一部分视线，让一部分方块变成雪块，造成一定干扰，随后消失
 */

/// <summary>
/// 格子材料类型
/// </summary>
public enum GridMaterialType
{
    /// <summary>
    /// 泥土
    /// </summary>
    Dirt,
    /// <summary>
    /// 圆石
    /// </summary>
    Cobblestone,
    /// <summary>
    /// 石头
    /// </summary>
    Stone,
    /// <summary>
    /// 草方块
    /// </summary>
    GrassBlock,
    /// <summary>
    /// 熔炉
    /// </summary>
    Furnace,
    /// <summary>
    /// 红石块
    /// </summary>
    RedStoneBlock,
    /// <summary>
    /// 红石粉，未激活
    /// </summary>
    RedStonePweder_disable,
    /// <summary>
    /// 红石粉
    /// </summary>
    RedStonePweder,
    /// <summary>
    /// TNT
    /// </summary>
    TNTBlock,
    /// <summary>
    /// 箱子
    /// </summary>
    Chest,
    /// <summary>
    /// 潜影盒
    /// </summary>
    ShulkerBox,
    /// <summary>
    /// 铁砧
    /// </summary>
    Anvil,
    /// <summary>
    /// 黑曜石
    /// </summary>
    Obsidian,
    /// <summary>
    /// 沙子
    /// </summary>
    Sand,
    /// <summary>
    /// 沙砾
    /// </summary>
    Gravel,
    /// <summary>
    /// 玻璃
    /// </summary>
    Glass,
    /// <summary>
    /// 仙人掌
    /// </summary>
    Cactus,
    /// <summary>
    /// 末影箱
    /// </summary>
    EnderChest,
    /// <summary>
    /// 铁块
    /// </summary>
    IronBlock,
    /// <summary>
    /// 雪块
    /// </summary>
    SnowBlock,
    /// <summary>
    /// 雕刻南瓜
    /// </summary>
    CarvedPumpkin,
    /// <summary>
    /// 漏斗
    /// </summary>
    Hopper,
}
