using TMPro;
using TowerDefense.Game;
using UnityEngine;
using UnityEngine.UI;

public class LocalizedText : MonoBehaviour
{
    Text text;

    [Multiline(3)]
    public string[] texts;

    void OnEnable()
    {
        TryGetComponent(out text);
        GameManager.OnLanguageChange += UpdateLanguageText; 
        UpdateLanguageText((int)GameManager.CurrLang);
    }
    void OnDisable()
    {
        GameManager.OnLanguageChange -= UpdateLanguageText; 
    }

    public void UpdateLanguageText(int lang)
    {
        text.text = texts[lang];
    }
}
