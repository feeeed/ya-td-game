using TowerDefense.Level;
using UnityEngine;
using YG;

public class AddCurrencyReward : MonoBehaviour
{
    public RewardAd rewardAd;
    public int addCurrencyReward;

    void Start()
    {
        rewardAd.onReward += OnReward;
    }

    void OnReward()
    {
        LevelManager.instance.currency.AddCurrency(addCurrencyReward);
    }
}
