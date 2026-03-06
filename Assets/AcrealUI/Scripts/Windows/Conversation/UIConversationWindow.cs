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

using DaggerfallWorkshop.Game.Utility.ModSupport;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AcrealUI
{
    [ImportedComponent]
    public class UIConversationWindow : UIWindow
    {
        #region Variables
        [SerializeField] private string _gameObjName_okayButton = null;
        [SerializeField] private string _gameObjName_historyButton = null;
        [SerializeField] private string _gameObjName_topicDivider = null;
        [SerializeField] private string _gameObjName_topicCategoryParent = null;
        [SerializeField] private string _gameObjName_topicEntryParent = null;
        [SerializeField] private string _gameObjName_dialogueEntryParent = null;
        [SerializeField] private string _gameObjName_pendingDialogueText = null;
        [SerializeField] private string _gameObjName_topicViewportLayoutElement = null;
        [SerializeField] private string _gameObjName_speakingStyleToggleGroup = null;
        [SerializeField] private string _gameObjName_normalSpeakingStyleToggle = null;
        [SerializeField] private string _gameObjName_politeSpeakingStyleToggle = null;
        [SerializeField] private string _gameObjName_bluntSpeakingStyleToggle = null;

        private UIButton _okayButton = null;
        private UIButton _historyButton = null;
        private GameObject _topicDivider = null;
        private RectTransform _topicCategoryParent = null;
        private RectTransform _topicEntryParent = null;
        private RectTransform _dialogueEntryParent = null;
        private LayoutElement _topicViewportLayoutElement = null;
        private TextMeshProUGUI _pendingDialogueText = null;
        private UIToggleGroup _speakingStyleToggleGroup = null;
        #endregion


        #region Properties
        public RectTransform topicCategoryParent { get { return _topicCategoryParent; } }
        public RectTransform topicEntryParent { get { return _topicEntryParent; } }
        public RectTransform dialogueEntryParent { get { return _dialogueEntryParent; } }

        public UIToggle normalSpeakingStyleToggle { get; private set; }
        public UIToggle politeSpeakingStyleToggle { get; private set; }
        public UIToggle bluntSpeakingStyleToggle { get; private set; }
        #endregion


        #region Events
        public event System.Action OnSubmitDialogueEntry = null;
        public event System.Action OnOpenDialogueHistory = null;
        #endregion


        #region Monobehaviour
        protected override void OnDisable()
        {
            base.OnDisable();
            StopAllCoroutines();
        }
        #endregion


        #region Initialization
        public override void Initialize()
        {
            base.Initialize();

            _topicEntryParent = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_topicEntryParent) as RectTransform;
            _topicCategoryParent = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_topicCategoryParent) as RectTransform;
            _dialogueEntryParent = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_dialogueEntryParent) as RectTransform;

            Transform viewportTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_topicViewportLayoutElement);
            if(viewportTform != null)
            {
                _topicViewportLayoutElement = viewportTform.GetComponent<LayoutElement>();
            }

            Transform okayTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_okayButton);
            if (okayTform != null)
            {
                _okayButton = okayTform.GetComponent<UIButton>();
                if (_okayButton != null)
                {
                    _okayButton.Initialize();
                    _okayButton.Event_OnClicked += (UIButton btn, PointerEventData data) =>
                    {
                        if (data.button == PointerEventData.InputButton.Left && data.clickCount == 1)
                        {
                            OnSubmitDialogueEntry?.Invoke();
                        }
                    };
                }
            }

            Transform historyTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_historyButton);
            if (historyTform != null)
            {
                _historyButton = historyTform.GetComponent<UIButton>();
                if (_historyButton != null)
                {
                    _historyButton.Initialize();
                    _historyButton.Event_OnClicked += (UIButton btn, PointerEventData data) =>
                    {
                        if (data.button == PointerEventData.InputButton.Left && data.clickCount == 1)
                        {
                            OnOpenDialogueHistory?.Invoke();
                        }
                    };
                }
            }

            Transform divTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_topicDivider);
            if (divTform != null)
            {
                _topicDivider = divTform.gameObject;
            }

            Transform pendingDialogueTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_pendingDialogueText);
            if (pendingDialogueTform != null)
            {
                _pendingDialogueText = pendingDialogueTform.GetComponent<TextMeshProUGUI>();
            }

            Transform groupTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_speakingStyleToggleGroup);
            if (groupTform != null)
            {
                _speakingStyleToggleGroup = groupTform.GetComponent<UIToggleGroup>();
                if (_speakingStyleToggleGroup != null)
                {
                    _speakingStyleToggleGroup.Initialize();
                }
            }

            Transform normalTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_normalSpeakingStyleToggle);
            if (normalTform != null)
            {
                normalSpeakingStyleToggle = normalTform.GetComponent<UIToggle>();
                if (normalSpeakingStyleToggle != null)
                {
                    normalSpeakingStyleToggle.Initialize();
                }
            }

            Transform politeTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_politeSpeakingStyleToggle);
            if (politeTform != null)
            {
                politeSpeakingStyleToggle = politeTform.GetComponent<UIToggle>();
                if (politeSpeakingStyleToggle != null)
                {
                    politeSpeakingStyleToggle.Initialize();
                }
            }

            Transform bluntTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_bluntSpeakingStyleToggle);
            if (bluntTform != null)
            {
                bluntSpeakingStyleToggle = bluntTform.GetComponent<UIToggle>();
                if (bluntSpeakingStyleToggle != null)
                {
                    bluntSpeakingStyleToggle.Initialize();
                }
            }

        }
        #endregion


        #region Public API
        public void SetPendingDialogue(string dialogue)
        {
            if (_pendingDialogueText != null)
            {
                _pendingDialogueText.text = dialogue;
            }
        }

        public UIDialogueEntry AddDialogueEntry(DialogueInfo dialogue)
        {
            UIDialogueEntry dialogueEntry = Instantiate(UIManager.referenceManager.prefab_dialogueEntry, _dialogueEntryParent);
            if (dialogueEntry != null)
            {
                dialogueEntry.Initialize();
            }
            return dialogueEntry;
        }

        public void UpdateTopicPanelSize()
        {
            StartCoroutine(UpdateSizeRoutine());
        }

        public UIButton AddTopicCategoryEntry(string buttonLabel, UIButton prefab)
        {
            if (prefab == null) { return null; }

            UIButton topicTypeBtn = Instantiate(prefab, _topicCategoryParent);
            if (topicTypeBtn != null)
            {
                topicTypeBtn.Initialize();
                topicTypeBtn.SetDisplayValue(buttonLabel);
                topicTypeBtn.SetDisplayValueTextSize(28);

                LayoutElement layoutElem = topicTypeBtn.GetComponent<LayoutElement>();
                if (layoutElem != null)
                {
                    layoutElem.minWidth = 0;
                    layoutElem.minHeight = 42;
                    layoutElem.flexibleWidth = 1f;
                    layoutElem.flexibleHeight = 0f;
                }
            }
            return topicTypeBtn;
        }

        public UIButton AddTopicEntry()
        {
            UIButton topicEntry = Instantiate(UIManager.referenceManager.prefab_button_textOnly, _topicEntryParent);
            if (topicEntry != null)
            {
                topicEntry.Initialize();
                topicEntry.gameObject.SetActive(true);
            }
            return topicEntry;
        }

        public void SetTopicDividerActive(bool active)
        {
            _topicDivider?.SetActive(active);
        }

        private IEnumerator UpdateSizeRoutine()
        {
            yield return 0f;

            if (_topicViewportLayoutElement != null)
            {
                float height = _topicCategoryParent != null ? _topicCategoryParent.sizeDelta.y : 0f;
                height += _topicEntryParent != null ? _topicEntryParent.sizeDelta.y : 0f;
                float maxTopicPanelSize = Screen.height * 0.5f;
                _topicViewportLayoutElement.minHeight = Mathf.Min(height, maxTopicPanelSize);
            }
        }
        #endregion
    }
}
