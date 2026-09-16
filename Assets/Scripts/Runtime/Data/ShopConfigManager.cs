using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class ShopConfigManager : MonoBehaviour
{
    public static ShopConfigManager Instance;

    private ShopConfigData _configData;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadConfig();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void LoadConfig()
    {
        _configData = LoadConfigFromJson();
        
        if (_configData == null || _configData.items == null || _configData.items.Count == 0)
        {

            _configData = CreateEmbeddedConfig();
        }
    }

    ShopConfigData LoadConfigFromJson()
    {
        try
        {
            string shopConfigPath = Path.Combine(Application.streamingAssetsPath, "Config", "ShopItems.json");
            
            if (File.Exists(shopConfigPath))
            {
                string json = File.ReadAllText(shopConfigPath);
                json = "{\"data\":" + json + "}";
                Wrapper wrapper = JsonUtility.FromJson<Wrapper>(json);
                
                if (wrapper != null && wrapper.data != null && wrapper.data.items != null && wrapper.data.items.Count > 0)
                {

                    return wrapper.data;
                }
            }
        }
        catch (System.Exception e)
        {

        }
        
        return null;
    }
    
    [System.Serializable]
    class Wrapper
    {
        public ShopConfigData data;
    }

    ShopConfigData CreateEmbeddedConfig()
    {
        ShopConfigData data = new ShopConfigData();
        
        data.mainCategories = new string[] { "职业", "皮肤", "功能", "礼包" };
        data.bottomTabs = new string[] { "每日特惠", "月卡", "限购礼包", "闯关礼包" };
        
        data.items = new List<ShopItemConfig>();
        
        // 职业类
        data.items.Add(new ShopItemConfig("class_blood", "Blood 血族", "血液即武器。每一次受伤都让你更强，但失控的代价是彻底腐化。", "$19.99", "com.parasitetower.class.blood", "职业", "icon_blood", false, null));
        data.items.Add(new ShopItemConfig("class_mech", "Mech 机械", "钢铁与血肉的错误融合。用装置改写战场，直到装置开始改写你。", "$19.99", "com.parasitetower.class.mech", "职业", "icon_mech", false, null));
        data.items.Add(new ShopItemConfig("class_titan", "Titan 泰坦", "钢铁巨兽降临。用绝对力量碾压一切敌人。", "$29.99", "com.parasitetower.class.titan", "职业", "icon_class_titan", false, null));
        data.items.Add(new ShopItemConfig("class_ghost", "Ghost 幽灵", "穿梭于虚实之间。在阴影中给予致命一击。", "$29.99", "com.parasitetower.class.ghost", "职业", "icon_class_ghost", false, null));
        data.items.Add(new ShopItemConfig("class_swarm", "Swarm 虫群", "亿万虫群的意志。数量即是力量。", "$39.99", "com.parasitetower.class.swarm", "职业", "icon_class_swarm", false, null));
        
        // 皮肤类
        data.items.Add(new ShopItemConfig("skin_neon", "霓虹污染", "荧光在血管里流动。5款形态专属配色。", "$9.99", "com.parasitetower.skin.neon", "皮肤", "icon_skin_neon", false, null));
        data.items.Add(new ShopItemConfig("skin_rust", "锈蚀机械", "废土朋克的终焉美学。齿轮与骨刺。", "$9.99", "com.parasitetower.skin.rust", "皮肤", "icon_skin_rust", false, null));
        data.items.Add(new ShopItemConfig("skin_abyss", "深渊原生", "克苏鲁不会敲门，它从内部生长。更深，更暗。", "$19.99", "com.parasitetower.skin.abyss", "皮肤", "icon_skin_abyss", false, null));
        data.items.Add(new ShopItemConfig("skin_cyber", "赛博变异", "数字与血肉的融合。数据流在皮肤下流动。", "$19.99", "com.parasitetower.skin.cyber", "皮肤", "icon_skin_cyber", false, null));
        data.items.Add(new ShopItemConfig("skin_void", "虚空侵蚀", "来自虚空的力量。你的形态正在被虚空吞噬。", "$29.99", "com.parasitetower.skin.void", "皮肤", "icon_skin_void", false, null));
        
        // 功能类
        data.items.Add(new ShopItemConfig("func_seed", "种子工具包", "输入指定种子，挑战同一局。与朋友分享你的挑战。", "$4.99", "com.parasitetower.func.seed", "功能", "icon_func_seed", false, null));
        data.items.Add(new ShopItemConfig("func_rewind", "死亡回溯包", "本局死亡？回溯3层，重新选择你的进化之路。", "$9.99", "com.parasitetower.func.rewind", "功能", "icon_func_rewind", false, null));
        data.items.Add(new ShopItemConfig("func_bestiary", "图鉴补全", "未收集的形态也能预览名称与图标。提前规划你的收集路线。", "$14.99", "com.parasitetower.func.bestiary", "功能", "icon_func_bestiary", false, null));
        data.items.Add(new ShopItemConfig("func_customize", "自定义形态", "自由调整形态的颜色和特效。打造专属外观。", "$19.99", "com.parasitetower.func.customize", "功能", "icon_func_customize", false, null));
        
        // 礼包类
        data.items.Add(new ShopItemConfig("pack_starter", "新手礼包", "500残响 + 职业解锁券 + 皮肤体验卡。新手必备。", "$9.99", "com.parasitetower.pack.starter", "礼包", "icon_pack_starter", true, new List<string> { "500残响", "职业解锁券x1", "皮肤体验卡x3" }));
        data.items.Add(new ShopItemConfig("pack_gold", "黄金礼包", "2000残响 + 形态扩展槽 + 进化加速剂。快速成长。", "$29.99", "com.parasitetower.pack.gold", "礼包", "icon_pack_gold", true, new List<string> { "2000残响", "形态槽+1", "进化加速剂x5" }));
        data.items.Add(new ShopItemConfig("pack_epic", "史诗礼包", "5000残响 + 限定皮肤 + 专属称号。超值特惠。", "$69.99", "com.parasitetower.pack.epic", "礼包", "icon_pack_epic", true, new List<string> { "5000残响", "限定史诗皮肤", "史诗称号" }));
        data.items.Add(new ShopItemConfig("pack_legendary", "传说礼包", "10000残响 + 传说形态 + 限定称号 + 专属头像框。", "$99.99", "com.parasitetower.pack.legendary", "礼包", "icon_pack_legendary", true, new List<string> { "10000残响", "传说形态", "限定称号", "专属头像框" }));
        
        // 每日特惠
        data.items.Add(new ShopItemConfig("daily_small", "每日特惠-小额", "每日限量特惠。100残响 + 进化点x50。", "$0.99", "com.parasitetower.daily.small", "每日特惠", "icon_daily_small", true, new List<string> { "100残响", "进化点x50" }));
        data.items.Add(new ShopItemConfig("daily_medium", "每日特惠-中额", "每日限量特惠。300残响 + 进化点x200 + 形态锁定卡。", "$4.99", "com.parasitetower.daily.medium", "每日特惠", "icon_daily_medium", true, new List<string> { "300残响", "进化点x200", "形态锁定卡x1" }));
        data.items.Add(new ShopItemConfig("daily_large", "每日特惠-大额", "每日限量特惠。800残响 + 进化点x500 + 皮肤体验卡x5。", "$19.99", "com.parasitetower.daily.large", "每日特惠", "icon_daily_large", true, new List<string> { "800残响", "进化点x500", "皮肤体验卡x5" }));
        
        // 月卡
        data.items.Add(new ShopItemConfig("monthly_card", "月卡", "每日领取300残响 + 进化点x100。持续30天。", "$19.99", "com.parasitetower.monthly.card", "月卡", "icon_monthly_card", false, null));
        data.items.Add(new ShopItemConfig("monthly_premium", "尊享月卡", "每日领取500残响 + 进化点x200 + 额外抽奖次数。持续30天。", "$39.99", "com.parasitetower.monthly.premium", "月卡", "icon_monthly_premium", false, null));
        
        // 限购礼包
        data.items.Add(new ShopItemConfig("limit_blood", "限购-血族觉醒", "限时限购。血族职业永久解锁 + 专属皮肤。", "$29.99", "com.parasitetower.limit.blood", "限购礼包", "icon_limit_blood", true, new List<string> { "血族职业", "血族专属皮肤", "1000残响" }));
        data.items.Add(new ShopItemConfig("limit_mech", "限购-机械核心", "限时限购。机械职业永久解锁 + 专属皮肤。", "$29.99", "com.parasitetower.limit.mech", "限购礼包", "icon_limit_mech", true, new List<string> { "机械职业", "机械专属皮肤", "1000残响" }));
        data.items.Add(new ShopItemConfig("limit_legendary", "限购-传说形态", "限时限购。传说级形态解锁 + 专属称号。", "$69.99", "com.parasitetower.limit.legendary", "限购礼包", "icon_limit_legendary", true, new List<string> { "传说形态", "传说称号", "2000残响" }));
        
        // 闯关礼包
        data.items.Add(new ShopItemConfig("level_10", "闯关礼包-10层", "到达10层即可购买。加速你的进化之路。", "$4.99", "com.parasitetower.level.10", "闯关礼包", "icon_level_10", true, new List<string> { "500残响", "进化点x300", "形态扩展槽" }));
        data.items.Add(new ShopItemConfig("level_25", "闯关礼包-25层", "到达25层即可购买。解锁高级进化能力。", "$19.99", "com.parasitetower.level.25", "闯关礼包", "icon_level_25", true, new List<string> { "1000残响", "进化点x800", "职业解锁券x2" }));
        data.items.Add(new ShopItemConfig("level_50", "闯关礼包-50层", "到达50层即可购买。终极进化奖励。", "$39.99", "com.parasitetower.level.50", "闯关礼包", "icon_level_50", true, new List<string> { "3000残响", "进化点x2000", "限定称号", "传说皮肤" }));
        
        return data;
    }

    public string[] GetMainCategories()
    {
        return _configData?.mainCategories ?? new string[] { "职业", "皮肤", "功能", "礼包" };
    }

    public string[] GetBottomTabs()
    {
        return _configData?.bottomTabs ?? new string[] { "每日特惠", "月卡", "限购礼包", "闯关礼包" };
    }

    public List<ShopItemConfig> GetItemsByCategory(string category)
    {
        if (_configData == null || _configData.items == null)
            return new List<ShopItemConfig>();
        
        List<ShopItemConfig> result = new List<ShopItemConfig>();
        foreach (var item in _configData.items)
        {
            if (item.category == category)
            {
                result.Add(item);
            }
        }
        return result;
    }

    public ShopItemConfig GetItemById(string id)
    {
        if (_configData?.items == null) return null;
        foreach (var item in _configData.items)
        {
            if (item.id == id) return item;
        }
        return null;
    }

    public List<ShopItemConfig> GetAllItems()
    {
        return _configData?.items ?? new List<ShopItemConfig>();
    }

    public bool IsPurchased(string itemId)
    {
        return PlayerPrefs.GetInt($"shop_purchased_{itemId}", 0) == 1;
    }

    public void MarkPurchased(string itemId)
    {
        PlayerPrefs.SetInt($"shop_purchased_{itemId}", 1);
        PlayerPrefs.Save();
    }
}

[System.Serializable]
public class ShopConfigData
{
    public string[] mainCategories;
    public string[] bottomTabs;
    public List<ShopItemConfig> items;
}

[System.Serializable]
public class ShopItemConfig
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

    public ShopItemConfig() {}

    public ShopItemConfig(string id, string name, string description, string price, string iapId, string category, string icon, bool isCollection, List<string> includes)
    {
        this.id = id;
        this.name = name;
        this.description = description;
        this.price = price;
        this.iapId = iapId;
        this.category = category;
        this.icon = icon;
        this.isCollection = isCollection;
        this.includes = includes ?? new List<string>();
    }
}
