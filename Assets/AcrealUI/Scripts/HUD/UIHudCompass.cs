
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

using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Utility.ModSupport;

namespace AcrealUI
{
    [ImportedComponent]
    public class UIHudCompass : MonoBehaviour
    {
        #region Definitions
        private class IconContainer
        {
            public GameObject uiObject = null;
            public GameObject gameWorldObject = null;
        }
        #endregion


        #region Variables
        [SerializeField] private float minX = -150f;
        [SerializeField] private float maxX = 150f;
        [SerializeField] private string _gameObjName_text_north = null;
        [SerializeField] private string _gameObjName_text_east = null;
        [SerializeField] private string _gameObjName_text_south = null;
        [SerializeField] private string _gameObjName_text_west = null;


        private TextMeshProUGUI _text_north = null;
        private TextMeshProUGUI _text_east = null;
        private TextMeshProUGUI _text_south = null;
        private TextMeshProUGUI _text_west = null;
        private List<IconContainer> iconsList = null;
        private Dictionary<GameObject, IconContainer> gameWorldObjToIconContainerDict = null;
        #endregion


        #region Monobehaviour
        private void Awake()
        {
            iconsList = new List<IconContainer>();
            gameWorldObjToIconContainerDict = new Dictionary<GameObject, IconContainer>();

            if (!string.IsNullOrEmpty(_gameObjName_text_north))
            {
                Transform northTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_text_north);
                _text_north = northTform != null ? northTform.GetComponent<TextMeshProUGUI>() : null;
            }

            if (!string.IsNullOrEmpty(_gameObjName_text_east))
            {
                Transform eastTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_text_east);
                _text_east = eastTform != null ? eastTform.GetComponent<TextMeshProUGUI>() : null;
            }

            if (!string.IsNullOrEmpty(_gameObjName_text_south))
            {
                Transform southTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_text_south);
                _text_south = southTform != null ? southTform.GetComponent<TextMeshProUGUI>() : null;
            }

            if (!string.IsNullOrEmpty(_gameObjName_text_west))
            {
                Transform westTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_text_west);
                _text_west = westTform != null ? westTform.GetComponent<TextMeshProUGUI>() : null;
            }
        }

        private void LateUpdate()
        {
            // TODO(Acreal): move this to UIUtilityFunctions so there's no need
            // to reference DaggerfallUnity scripts in here
            GameObject playerGameObject = null;
            PlayerEntity playerEntity = null;
            if (DaggerfallAction.GetPlayer(out playerGameObject, out playerEntity))
            {
                UpdateCardinalPositions(playerGameObject.transform.rotation);
            }
        }
        #endregion


        #region Compass Icons
        public void AddIcon(GameObject gameWorldObjToTrack, Sprite iconToUse)
        {
            GameObject iconObj = null;
            IconContainer container = new IconContainer() { uiObject = iconObj, gameWorldObject = gameWorldObjToTrack };
            gameWorldObjToIconContainerDict[gameWorldObjToTrack] = container;
        }

        public void RemoveIcon(GameObject gameWorldObjectToRemove)
        {

        }

        public void UpdateCardinalPositions(Quaternion playerRotation)
        {
            Vector3 playerForwardDir = (playerRotation * Vector3.forward).normalized;
            Vector3 playerRightDir = (Quaternion.Euler(0f, 90f, 0f) * playerForwardDir).normalized;

            UpdateCardinalDirection(_text_north, Vector3.forward, playerForwardDir, playerRightDir);
            UpdateCardinalDirection(_text_east, Vector3.right, playerForwardDir, playerRightDir);
            UpdateCardinalDirection(_text_south, -Vector3.forward, playerForwardDir, playerRightDir);
            UpdateCardinalDirection(_text_west, -Vector3.right, playerForwardDir, playerRightDir);
        }

        private void UpdateCardinalDirection(TextMeshProUGUI dirText, Vector3 cardinalDir, Vector3 playerForwardDir, Vector3 playerRightDir)
        {
            if (dirText != null)
            {
                float angle = Vector3.Angle(cardinalDir, playerForwardDir);
                float dot = Vector3.Dot(playerForwardDir, cardinalDir);
                bool isRight = Vector3.Dot(playerRightDir, cardinalDir) > 0.0f;

                if (angle <= 90.0f)
                {
                    Vector3 pos = dirText.transform.localPosition;
                    float t = Mathf.InverseLerp(0f, 90f, angle);
                    pos.x = isRight ? Mathf.Lerp(0f, maxX, t) : Mathf.Lerp(minX, 0f, 1f - t);
                    dirText.transform.localPosition = pos;

                    if (!dirText.gameObject.activeSelf)
                    {
                        dirText.gameObject.SetActive(true);
                    }
                }
                else
                {
                    dirText.gameObject.SetActive(false);
                }
            }
        }
        #endregion
    }
}