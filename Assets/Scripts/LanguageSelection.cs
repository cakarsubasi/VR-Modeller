using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class LanguageSelection : MonoBehaviour
{
    public void SetLanguage(int val)
    {
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[val];
    }
}
