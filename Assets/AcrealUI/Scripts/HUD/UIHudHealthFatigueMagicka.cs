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

using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using UnityEngine;

namespace AcrealUI
{
    [ImportedComponent]
    public class UIHudHealthFatigueMagicka : MonoBehaviour
    {
        [SerializeField] private string _gameObjName_healthSlider = null;
        [SerializeField] private string _gameObjName_fatigueSlider = null;
        [SerializeField] private string _gameObjName_magickaSlider = null;

        private UISlider healthSlider = null;
        private UISlider fatigueSlider = null;
        private UISlider magickaSlider = null;

        private float prevHealthPercent = -1.0f;
        private float prevFatiguePercent = -1.0f;
        private float prevMagickaPercent = -1.0f;

        private void Awake()
        {
            if(!string.IsNullOrEmpty(_gameObjName_healthSlider))
            {
                Transform healthTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_healthSlider);
                healthSlider = healthTform != null ? healthTform.GetComponent<UISlider>() : null;
                if (healthSlider != null)
                {
                    healthSlider.Initialize();
                    healthSlider.SetSliderMinMax(0f, 1f, false);
                }
            }

            if (!string.IsNullOrEmpty(_gameObjName_fatigueSlider))
            {
                Transform fatigueTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_fatigueSlider);
                fatigueSlider = fatigueTform != null ? fatigueTform.GetComponent<UISlider>() : null;
                if (fatigueSlider != null)
                {
                    fatigueSlider.Initialize();
                    fatigueSlider.SetSliderMinMax(0f, 1f, false);
                }
            }

            if (!string.IsNullOrEmpty(_gameObjName_magickaSlider))
            {
                Transform magickaTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_magickaSlider);
                magickaSlider = magickaTform != null ? magickaTform.GetComponent<UISlider>() : null;
                if (magickaSlider != null)
                {
                    magickaSlider.Initialize();
                    magickaSlider.SetSliderMinMax(0f, 1f, false);
                }
            }
        }

        private void Update()
        {
            // TODO(Acreal): move this to UIUtilityFunctions so there's no need
            // to reference DaggerfallUnity scripts in here
            // make it event-based
            PlayerEntity player = GameManager.Instance.PlayerEntity;

            float healthPercent = player.CurrentHealthPercent;
            if(Mathf.Abs(healthPercent - prevHealthPercent) >= 0.01f)
            {
                prevHealthPercent = healthPercent;
                healthSlider.SetSliderValue(healthPercent, false);
            }

            float fatiguePercent = player.CurrentFatigue / (float)player.MaxFatigue;
            if (Mathf.Abs(fatiguePercent - prevFatiguePercent) >= 0.01f)
            {
                prevFatiguePercent = fatiguePercent;
                fatigueSlider.SetSliderValue(fatiguePercent, false);
            }

            float magickaPercent = player.CurrentMagicka / (float)player.MaxMagicka;
            if (Mathf.Abs(magickaPercent - prevMagickaPercent) >= 0.01f)
            {
                prevMagickaPercent = magickaPercent;
                magickaSlider.SetSliderValue(magickaPercent, false);
            }
        }
    }
}