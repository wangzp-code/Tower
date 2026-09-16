using UnityEngine;
using System.Collections.Generic;

public class RewardManager : SingletonBase<RewardManager>
{
    [Header("Reward Settings")]
    public int baseGoldReward = 20;
    public float goldPerFloor = 5;
    public int baseExpReward = 10;
    public float expPerFloor = 3;
    public float fragmentDropChance = 0.3f;
    public float formDropChance = 0.05f;

    private void Awake()
    {
        base.Awake();
    }

    public RewardData GenerateReward(MonsterBase monster, bool isBoss = false)
    {
        RewardData reward = new RewardData();
        
        int floor = GameManager.Instance.CurrentFloor;
        float bossMultiplier = isBoss ? 3f : 1f;
        
        reward.gold = Mathf.RoundToInt((baseGoldReward + floor * goldPerFloor) * bossMultiplier);
        reward.exp = Mathf.RoundToInt((baseExpReward + floor * expPerFloor) * bossMultiplier);
        
        reward.fragment = Random.value < fragmentDropChance;
        
        if (monster.data.dropFormId != null && monster.data.dropFormId != "")
        {
            float adjustedChance = formDropChance * (isBoss ? 2f : 1f);
            if (Random.value < adjustedChance)
            {
                reward.formId = monster.data.dropFormId;
            }
        }

        reward.monsterName = monster.data.monsterName;
        
        return reward;
    }

    public void ClaimReward(RewardData reward)
    {
        var player = GameManager.Instance.Player;

        player.gold += reward.gold;
        player.evolutionPoints += reward.exp;
        if (reward.fragment)
            player.fragments++;

        player.pollution = Mathf.Min(100f, player.pollution + 0.02f);

        if (reward.formId != null && reward.formId != "")
        {
            FormManager.Instance.TryAddForm(reward.formId);
            EventBus.Emit(EventTypes.FormUnlocked, reward.formId);
        }

        EventBus.Emit(EventTypes.RewardClaimed, reward);
    }

    public void GenerateRandomReward()
    {
        RewardData reward = new RewardData();
        
        int floor = GameManager.Instance.CurrentFloor;
        reward.gold = Mathf.RoundToInt(baseGoldReward * 0.5f + floor * goldPerFloor * 0.5f);
        reward.exp = 0;
        reward.fragment = Random.value < fragmentDropChance * 0.5f;
        reward.formId = null;
        reward.monsterName = "Random";

        ClaimReward(reward);
    }

    [System.Serializable]
    public class RewardData
    {
        public int gold;
        public int exp;
        public bool fragment;
        public string formId;
        public string monsterName;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
    }
}