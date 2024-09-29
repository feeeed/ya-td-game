using UnityEngine;
using UnityEngine.Events;
using YG;

public class RewardAd : MonoBehaviour
{
    public int rewardId;
    public UnityEvent onRewardEvent;

    public System.Action onReward;

    void OnEnable()
    {
        YandexGame.RewardVideoEvent += OnRewardVideoEvent;
    } 
    void OnDisable()
    {
        YandexGame.RewardVideoEvent -= OnRewardVideoEvent;
    }

    void OnRewardVideoEvent(int id)
    {
        if (id == rewardId)
        {
            onReward?.Invoke();
            onRewardEvent.Invoke();
        }
    }

    public void TryGetReward()
    {
        YandexGame.RewVideoShow(rewardId);
    }
}
