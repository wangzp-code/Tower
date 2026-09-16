using UnityEngine;
using System.Collections.Generic;

public class ShopManager : SingletonBase<ShopManager>
{
    private List<OriginalShopData.ShopItem> _currentItems = new List<OriginalShopData.ShopItem>();
    private Dictionary<string, int> _purchaseCounts = new Dictionary<string, int>();

    private void Awake()
    {
        base.Awake();
    }

    public void GenerateShopItems(int floor)
    {
        _currentItems.Clear();
        int zone = Mathf.Clamp((floor - 1) / 10, 0, 5);

        foreach (var item in OriginalShopData.Items)
        {
            if (item.minZone <= zone)
            {
                _currentItems.Add(item);
            }
        }
    }

    public List<OriginalShopData.ShopItem> GetCurrentItems()
    {
        return _currentItems;
    }

    public bool CanBuy(OriginalShopData.ShopItem item)
    {
        var player = GameManager.Instance?.Player;
        if (player == null) return false;

        int price = GetPrice(item);
        if (player.evolutionPoints < price) return false;

        int bought = _purchaseCounts.ContainsKey(item.id) ? _purchaseCounts[item.id] : 0;
        if (item.maxBuy > 0 && bought >= item.maxBuy) return false;

        return true;
    }

    public int GetPrice(OriginalShopData.ShopItem item)
    {
        int bought = _purchaseCounts.ContainsKey(item.id) ? _purchaseCounts[item.id] : 0;
        return Mathf.RoundToInt(item.cost * Mathf.Pow(item.priceScale, bought));
    }

    public bool Purchase(OriginalShopData.ShopItem item)
    {
        if (!CanBuy(item)) return false;

        var player = GameManager.Instance.Player;
        int price = GetPrice(item);
        player.evolutionPoints -= price;

        if (!_purchaseCounts.ContainsKey(item.id))
            _purchaseCounts[item.id] = 0;
        _purchaseCounts[item.id]++;

        OriginalShopData.ApplyItem(item.type, player);
        GameManager.Instance.NotifyPlayerStatsChanged();
        EventBus.Emit(EventTypes.PlayerStatsChanged);

        AudioManager.Instance?.PlaySFX("shop");

        return true;
    }

    public void ResetPurchaseCounts()
    {
        _purchaseCounts.Clear();
    }

    protected override void OnDestroy()
    {
        _currentItems.Clear();
        _purchaseCounts.Clear();
        base.OnDestroy();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
    }
}
