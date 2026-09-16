using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EchoShopItemData
{
    public string id;
    public string name;
    public string description;
    public string price;
    public string iapId;
    public string category;
    public string icon;
    public bool isCollection;
    public List<string> includes;

    public static readonly EchoShopItemData[] Items = new EchoShopItemData[]
    {
        new EchoShopItemData {
            id = "class_blood",
            name = "Blood 血族",
            description = "血液即武器。每一次受伤都让你更强，但失控的代价是彻底腐化。",
            price = "$19.99",
            iapId = "com.parasitetower.class.blood",
            category = "职业",
            icon = "icon_blood",
            isCollection = false,
            includes = null
        },
        new EchoShopItemData {
            id = "class_mech",
            name = "Mech 机械",
            description = "钢铁与血肉的错误融合。用装置改写战场，直到装置开始改写你。",
            price = "$19.99",
            iapId = "com.parasitetower.class.mech",
            category = "职业",
            icon = "icon_mech",
            isCollection = false,
            includes = null
        },
        new EchoShopItemData {
            id = "class_titan",
            name = "Titan 泰坦",
            description = "钢铁巨兽降临。用绝对力量碾压一切敌人。",
            price = "$29.99",
            iapId = "com.parasitetower.class.titan",
            category = "职业",
            icon = "icon_class_titan",
            isCollection = false,
            includes = null
        },
        new EchoShopItemData {
            id = "class_ghost",
            name = "Ghost 幽灵",
            description = "穿梭于虚实之间。在阴影中给予致命一击。",
            price = "$29.99",
            iapId = "com.parasitetower.class.ghost",
            category = "职业",
            icon = "icon_class_ghost",
            isCollection = false,
            includes = null
        },
        new EchoShopItemData {
            id = "class_swarm",
            name = "Swarm 虫群",
            description = "亿万虫群的意志。数量即是力量。",
            price = "$39.99",
            iapId = "com.parasitetower.class.swarm",
            category = "职业",
            icon = "icon_class_swarm",
            isCollection = false,
            includes = null
        },
        new EchoShopItemData {
            id = "skin_neon",
            name = "霓虹污染",
            description = "荧光在血管里流动。5款形态专属配色。",
            price = "$9.99",
            iapId = "com.parasitetower.skin.neon",
            category = "皮肤",
            icon = "icon_skin_neon",
            isCollection = false,
            includes = null
        },
        new EchoShopItemData {
            id = "skin_rust",
            name = "锈蚀机械",
            description = "废土朋克的终焉美学。齿轮与骨刺。",
            price = "$9.99",
            iapId = "com.parasitetower.skin.rust",
            category = "皮肤",
            icon = "icon_skin_rust",
            isCollection = false,
            includes = null
        },
        new EchoShopItemData {
            id = "skin_abyss",
            name = "深渊原生",
            description = "克苏鲁不会敲门，它从内部生长。更深，更暗。",
            price = "$19.99",
            iapId = "com.parasitetower.skin.abyss",
            category = "皮肤",
            icon = "icon_skin_abyss",
            isCollection = false,
            includes = null
        },
        new EchoShopItemData {
            id = "skin_cyber",
            name = "赛博变异",
            description = "数字与血肉的融合。数据流在皮肤下流动。",
            price = "$19.99",
            iapId = "com.parasitetower.skin.cyber",
            category = "皮肤",
            icon = "icon_skin_cyber",
            isCollection = false,
            includes = null
        },
        new EchoShopItemData {
            id = "skin_void",
            name = "虚空侵蚀",
            description = "来自虚空的力量。你的形态正在被虚空吞噬。",
            price = "$29.99",
            iapId = "com.parasitetower.skin.void",
            category = "皮肤",
            icon = "icon_skin_void",
            isCollection = false,
            includes = null
        },
        new EchoShopItemData {
            id = "func_seed",
            name = "种子工具包",
            description = "输入指定种子，挑战同一局。与朋友分享你的挑战。",
            price = "$4.99",
            iapId = "com.parasitetower.func.seed",
            category = "功能",
            icon = "icon_func_seed",
            isCollection = false,
            includes = null
        },
        new EchoShopItemData {
            id = "func_rewind",
            name = "死亡回溯包",
            description = "本局死亡？回溯3层，重新选择你的进化之路。",
            price = "$9.99",
            iapId = "com.parasitetower.func.rewind",
            category = "功能",
            icon = "icon_func_rewind",
            isCollection = false,
            includes = null
        },
        new EchoShopItemData {
            id = "func_bestiary",
            name = "图鉴补全",
            description = "未收集的形态也能预览名称与图标。提前规划你的收集路线。",
            price = "$14.99",
            iapId = "com.parasitetower.func.bestiary",
            category = "功能",
            icon = "icon_func_bestiary",
            isCollection = false,
            includes = null
        },
        new EchoShopItemData {
            id = "func_customize",
            name = "自定义形态",
            description = "自由调整形态的颜色和特效。打造专属外观。",
            price = "$19.99",
            iapId = "com.parasitetower.func.customize",
            category = "功能",
            icon = "icon_func_customize",
            isCollection = false,
            includes = null
        },
        new EchoShopItemData {
            id = "pack_starter",
            name = "新手礼包",
            description = "500残响 + 职业解锁券 + 皮肤体验卡。新手必备。",
            price = "$9.99",
            iapId = "com.parasitetower.pack.starter",
            category = "礼包",
            icon = "icon_pack_starter",
            isCollection = true,
            includes = new List<string> { "500残响", "职业解锁券x1", "皮肤体验卡x3" }
        },
        new EchoShopItemData {
            id = "pack_gold",
            name = "黄金礼包",
            description = "2000残响 + 形态扩展槽 + 进化加速剂。快速成长。",
            price = "$29.99",
            iapId = "com.parasitetower.pack.gold",
            category = "礼包",
            icon = "icon_pack_gold",
            isCollection = true,
            includes = new List<string> { "2000残响", "形态槽+1", "进化加速剂x5" }
        },
        new EchoShopItemData {
            id = "pack_epic",
            name = "史诗礼包",
            description = "5000残响 + 限定皮肤 + 专属称号。超值特惠。",
            price = "$69.99",
            iapId = "com.parasitetower.pack.epic",
            category = "礼包",
            icon = "icon_pack_epic",
            isCollection = true,
            includes = new List<string> { "5000残响", "限定史诗皮肤", "史诗称号" }
        },
        new EchoShopItemData {
            id = "pack_legendary",
            name = "传说礼包",
            description = "10000残响 + 传说形态 + 限定称号 + 专属头像框。",
            price = "$99.99",
            iapId = "com.parasitetower.pack.legendary",
            category = "礼包",
            icon = "icon_pack_legendary",
            isCollection = true,
            includes = new List<string> { "10000残响", "传说形态", "限定称号", "专属头像框" }
        },
        new EchoShopItemData {
            id = "daily_pack_small",
            name = "每日特惠-小额",
            description = "每日限量特惠。100残响 + 进化点x50。",
            price = "$0.99",
            iapId = "com.parasitetower.daily.small",
            category = "每日特惠",
            icon = "icon_daily_small",
            isCollection = true,
            includes = new List<string> { "100残响", "进化点x50" }
        },
        new EchoShopItemData {
            id = "daily_pack_medium",
            name = "每日特惠-中额",
            description = "每日限量特惠。300残响 + 进化点x200 + 形态锁定卡。",
            price = "$4.99",
            iapId = "com.parasitetower.daily.medium",
            category = "每日特惠",
            icon = "icon_daily_medium",
            isCollection = true,
            includes = new List<string> { "300残响", "进化点x200", "形态锁定卡x1" }
        },
        new EchoShopItemData {
            id = "daily_pack_large",
            name = "每日特惠-大额",
            description = "每日限量特惠。800残响 + 进化点x500 + 皮肤体验卡x5。",
            price = "$19.99",
            iapId = "com.parasitetower.daily.large",
            category = "每日特惠",
            icon = "icon_daily_large",
            isCollection = true,
            includes = new List<string> { "800残响", "进化点x500", "皮肤体验卡x5" }
        },
        new EchoShopItemData {
            id = "monthly_card",
            name = "月卡",
            description = "每日领取300残响 + 进化点x100。持续30天。",
            price = "$19.99",
            iapId = "com.parasitetower.monthly.card",
            category = "月卡",
            icon = "icon_monthly_card",
            isCollection = false,
            includes = null
        },
        new EchoShopItemData {
            id = "monthly_premium",
            name = "尊享月卡",
            description = "每日领取500残响 + 进化点x200 + 额外抽奖次数。持续30天。",
            price = "$39.99",
            iapId = "com.parasitetower.monthly.premium",
            category = "月卡",
            icon = "icon_monthly_premium",
            isCollection = false,
            includes = null
        },
        new EchoShopItemData {
            id = "limit_pack_blood",
            name = "限购-血族觉醒",
            description = "限时限购。血族职业永久解锁 + 专属皮肤。",
            price = "$29.99",
            iapId = "com.parasitetower.limit.blood",
            category = "限购礼包",
            icon = "icon_limit_blood",
            isCollection = true,
            includes = new List<string> { "血族职业", "血族专属皮肤", "1000残响" }
        },
        new EchoShopItemData {
            id = "limit_pack_mech",
            name = "限购-机械核心",
            description = "限时限购。机械职业永久解锁 + 专属皮肤。",
            price = "$29.99",
            iapId = "com.parasitetower.limit.mech",
            category = "限购礼包",
            icon = "icon_limit_mech",
            isCollection = true,
            includes = new List<string> { "机械职业", "机械专属皮肤", "1000残响" }
        },
        new EchoShopItemData {
            id = "limit_pack_legendary",
            name = "限购-传说形态",
            description = "限时限购。传说级形态解锁 + 专属称号。",
            price = "$69.99",
            iapId = "com.parasitetower.limit.legendary",
            category = "限购礼包",
            icon = "icon_limit_legendary",
            isCollection = true,
            includes = new List<string> { "传说形态", "传说称号", "2000残响" }
        },
        new EchoShopItemData {
            id = "level_pack_10",
            name = "闯关礼包-10层",
            description = "到达10层即可购买。加速你的进化之路。",
            price = "$4.99",
            iapId = "com.parasitetower.level.10",
            category = "闯关礼包",
            icon = "icon_level_10",
            isCollection = true,
            includes = new List<string> { "500残响", "进化点x300", "形态扩展槽" }
        },
        new EchoShopItemData {
            id = "level_pack_25",
            name = "闯关礼包-25层",
            description = "到达25层即可购买。解锁高级进化能力。",
            price = "$19.99",
            iapId = "com.parasitetower.level.25",
            category = "闯关礼包",
            icon = "icon_level_25",
            isCollection = true,
            includes = new List<string> { "1000残响", "进化点x800", "职业解锁券x2" }
        },
        new EchoShopItemData {
            id = "level_pack_50",
            name = "闯关礼包-50层",
            description = "到达50层即可购买。终极进化奖励。",
            price = "$39.99",
            iapId = "com.parasitetower.level.50",
            category = "闯关礼包",
            icon = "icon_level_50",
            isCollection = true,
            includes = new List<string> { "3000残响", "进化点x2000", "限定称号", "传说皮肤" }
        },
        new EchoShopItemData {
            id = "bundle_class_all",
            name = "全职业解锁包",
            description = "解锁全部5种职业，体验完整的进化之路。",
            price = "$129.99",
            iapId = "com.parasitetower.bundle.class_all",
            category = "合集",
            icon = "icon_bundle_class_all",
            isCollection = true,
            includes = new List<string> { "血族", "机械", "泰坦", "幽灵", "虫群" }
        },
        new EchoShopItemData {
            id = "bundle_skin_all",
            name = "皮肤大全",
            description = "全部5款皮肤。打造独一无二的形态。",
            price = "$69.99",
            iapId = "com.parasitetower.bundle.skin_all",
            category = "合集",
            icon = "icon_bundle_skin_all",
            isCollection = true,
            includes = new List<string> { "霓虹污染", "锈蚀机械", "深渊原生", "赛博变异", "虚空侵蚀" }
        },
        new EchoShopItemData {
            id = "bundle_func_all",
            name = "功能尊享包",
            description = "全部功能道具一次性解锁。畅玩无限制。",
            price = "$49.99",
            iapId = "com.parasitetower.bundle.func_all",
            category = "合集",
            icon = "icon_bundle_func_all",
            isCollection = true,
            includes = new List<string> { "种子工具", "死亡回溯", "图鉴补全", "自定义形态" }
        },
        new EchoShopItemData {
            id = "bundle_all",
            name = "终极典藏版",
            description = "解锁游戏全部内容。包含所有职业、皮肤、功能和装饰。",
            price = "$199.99",
            iapId = "com.parasitetower.bundle.all",
            category = "合集",
            icon = "icon_bundle_all",
            isCollection = true,
            includes = new List<string> { "全职业", "全皮肤", "全功能", "限定内容" }
        }
    };

    public static List<EchoShopItemData> GetItemsByCategory(string category)
    {
        List<EchoShopItemData> result = new List<EchoShopItemData>();
        foreach (var item in Items)
        {
            if (item.category == category)
            {
                result.Add(item);
            }
        }
        return result;
    }

    public static string[] GetCategories()
    {
        HashSet<string> categories = new HashSet<string>();
        foreach (var item in Items)
        {
            categories.Add(item.category);
        }
        return categories.ToArray();
    }

    public static EchoShopItemData GetItemById(string id)
    {
        foreach (var item in Items)
        {
            if (item.id == id)
            {
                return item;
            }
        }
        return null;
    }

    public bool IsPurchased()
    {
        return PlayerPrefs.GetInt($"shop_purchased_{id}", 0) == 1;
    }

    public void MarkPurchased()
    {
        PlayerPrefs.SetInt($"shop_purchased_{id}", 1);
        PlayerPrefs.Save();
    }
}
