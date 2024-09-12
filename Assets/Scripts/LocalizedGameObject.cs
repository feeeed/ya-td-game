using TowerDefense.Game;
using UnityEngine;

public class LocalizedGameObject : MonoBehaviour
{
    public GameObject ruObj;
    public GameObject enObj;

    void OnEnable()
    {
        GameManager.OnLanguageChange += UpdateLanguage; 
        UpdateLanguage((int)GameManager.CurrLang);
    }
    void OnDisable()
    {
        GameManager.OnLanguageChange -= UpdateLanguage; 
    }

    public void UpdateLanguage(int lang)
    {
        ruObj.SetActive(lang == 0);
        enObj.SetActive(lang == 1);
    }
}
