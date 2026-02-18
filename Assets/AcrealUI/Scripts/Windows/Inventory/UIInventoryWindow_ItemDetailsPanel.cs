/*
Copyright (c) 2025-2026 Acreal (https://github.com/acreal)

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
documentation files (the "Software"), to deal in the Software without restriction, including without 
limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of 
the Software, and to permit persons to whom the Software is furnished to do so, subject to the following 
conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions 
of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
DEALINGS IN THE SOFTWARE.
*/

using DaggerfallWorkshop.Game.Items;
using TMPro;
using UnityEngine;

[System.Obsolete]
public class UIInventoryWindow_ItemDetailsPanel : MonoBehaviour
{
    [SerializeField] private GameObject displayParent_playerInfo = null;
    [SerializeField] private GameObject displayParent_itemInfo = null;

    [SerializeField] private GameObject itemDamageParent = null;
    [SerializeField] private GameObject itemArmorParent = null;
    [SerializeField] private GameObject itemConditionParent = null;
    [SerializeField] private GameObject itemWeightParent = null;
    [SerializeField] private GameObject itemValueParent = null;

    [SerializeField] private TextMeshProUGUI text_itemName = null;
    [SerializeField] private TextMeshProUGUI text_itemDamage = null;
    [SerializeField] private TextMeshProUGUI text_itemArmor = null;
    [SerializeField] private TextMeshProUGUI text_itemCondition = null;
    [SerializeField] private TextMeshProUGUI text_itemWeight = null;
    [SerializeField] private TextMeshProUGUI text_itemValue = null;

    public DaggerfallUnityItem displayedItem { get; private set; }

    private void Awake()
    {
        if (displayParent_itemInfo != null)
        {
            displayParent_itemInfo.SetActive(displayedItem != null);
        }

        if (displayParent_playerInfo != null)
        {
            displayParent_playerInfo.SetActive(displayedItem == null);
        }
    }

    public void SetItem(DaggerfallUnityItem item)
    {
        displayedItem = item;

        if (displayedItem != null)
        {
            bool showDmg = false;
            bool showArmor = false;

            SetTextValue(text_itemName, item.LongName);

            if (item.ItemGroup == ItemGroups.Weapons)
            {
                SetTextValue(text_itemDamage, item.GetBaseDamageMin().ToString("N0") + "-" + item.GetBaseDamageMax().ToString("N0"));
                showDmg = true;
            }
            else if (item.IsShield)
            {
                SetTextValue(text_itemArmor, item.GetShieldArmorValue().ToString("N0"));
                showArmor = true;
            }
            else if (item.ItemGroup == ItemGroups.Armor)
            {
                SetTextValue(text_itemArmor, item.GetMaterialArmorValue().ToString("N0"));
                showArmor = true;
            }

            if (itemArmorParent != null)
            {
                itemArmorParent.SetActive(showArmor);
            }

            if (itemDamageParent != null)
            {
                itemDamageParent.SetActive(showDmg);
            }

            if (itemWeightParent != null)
            {
                bool showWeight = item.weightInKg > 0f;
                if (showWeight)
                {
                    SetTextValue(text_itemWeight, item.weightInKg.ToString("N2"));
                }
                itemWeightParent.SetActive(showWeight);
            }

            if (itemConditionParent != null)
            {
                bool showCondition = item.maxCondition > 0;
                if (showCondition)
                {
                    SetTextValue(text_itemCondition, item.ConditionPercentage.ToString("N0") + "%");
                }
                itemConditionParent.SetActive(showCondition);
            }

            if (itemValueParent != null)
            {
                bool showValue = item.value > 0;
                if (showValue)
                {
                    SetTextValue(text_itemValue, item.value.ToString("N0"));
                }
                itemValueParent.SetActive(showValue);
            }
        }

        if (displayParent_playerInfo != null)
        {
            displayParent_playerInfo.SetActive(displayedItem == null);
        }

        if (displayParent_itemInfo != null)
        {
            displayParent_itemInfo.SetActive(displayedItem != null);
        }
    }

    private void SetTextValue(TextMeshProUGUI textComponent, string value)
    {
        if(textComponent != null)
        {
            textComponent.text = value;
        }
    }
}
