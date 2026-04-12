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
using System.Collections.Generic;
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
        [Header("Topics")]
        [SerializeField] private string _gameObjName_topicViewportLayoutElement = null;
        [SerializeField] private string _gameObjName_topicEntryParent = null;
        [SerializeField] private string _gameObjName_prevTopicButton = null;
        [SerializeField] private string _gameObjName_topicDivider = null;

        [Header("Player/NPC Dialogue")]
        [SerializeField] private string _gameObjName_dialogueScrollViewCanvasGroup = null;
        [SerializeField] private string _gameObjName_recentDialogueEntryParent = null;
        [SerializeField] private string _gameObjName_oldDialogueEntryParent = null;
        [SerializeField] private string _gameObjName_pendingDialogueText = null;
        [SerializeField] private string _gameObjName_dialoguePanelInteractiveElement = null;
        [SerializeField] private string _gameObjName_dialogueLayoutElement = null;
        [SerializeField] private string _gameObjName_pendingDialogueLayoutElement = null;
        [SerializeField] private string _gameObjName_okayButton = null;

        [Header("Speaking Style/Talk Mode")]
        [SerializeField] private string _gameObjName_speakingStyleToggleGroup = null;
        [SerializeField] private string _gameObjName_normalSpeakingStyleToggle = null;
        [SerializeField] private string _gameObjName_politeSpeakingStyleToggle = null;
        [SerializeField] private string _gameObjName_bluntSpeakingStyleToggle = null;


        private const float _TOPIC_PADDING = 12f;
        private const float _TOPIC_HEADER_PADDING = 38f;
        private const float _TOPIC_FOOTER_PADDING = 64f;
        private const float _EMPTY_SPACE_PADDING = 24f;

        private LayoutElement _topicViewportLayoutElement = null;
        private RectTransform _topicEntryParent = null;
        private UIButton _previousTopicButton = null;
        private GameObject _topicDivider;
        private CanvasGroup _dialogueScrollViewCanvasGroup = null;
        private RectTransform _recentDialogueEntryParent = null;
        private RectTransform _oldDialogueEntryParent = null;
        private UIInteractiveElement _dialoguePanelInteractiveElement = null;
        private LayoutElement _dialogueLayoutElement = null;
        private LayoutElement _pendingDialogueLayoutElement = null;
        private UIButton _okayButton = null;
        private TextMeshProUGUI _pendingDialogueText = null;
        private UIToggleGroup _speakingStyleToggleGroup = null;

        private List<UIDialogueEntry> _allDialogueEntries = null;
        private Queue<UIDialogueEntry> _recentDialogueEntries = null;
        private Queue<UIDialogueEntry> _oldDialogueEntries = null;
        private List<UIButton> _topicEntries = null;
        #endregion


        #region Properties
        public RectTransform topicEntryParent { get { return _topicEntryParent; } }
        public RectTransform dialogueEntryParent { get { return _recentDialogueEntryParent; } }
        public UIButton previousTopicButton { get { return _previousTopicButton; } }
        public UIToggle normalSpeakingStyleToggle { get; private set; }
        public UIToggle politeSpeakingStyleToggle { get; private set; }
        public UIToggle bluntSpeakingStyleToggle { get; private set; }
        public int numDialogueEntries { get { return _allDialogueEntries.Count; } }
        #endregion


        #region Events
        public event System.Action Event_ButtonClicked_OnSubmitDialogueEntry = null;
        public event System.Action<UIDialogueEntry, int> Event_OnCopyDialogueToNotebook = null;
        #endregion


        #region Monobehaviour
        protected override void OnDisable()
        {
            base.OnDisable();
            StopAllCoroutines();
        }
        #endregion


        #region Initialization/Cleanup
        public override void Initialize()
        {
            base.Initialize();

            _allDialogueEntries = new List<UIDialogueEntry>();
            _recentDialogueEntries = new Queue<UIDialogueEntry>(UIConstants.MAX_RECENT_DIALOGUE_ENTRIES);
            _oldDialogueEntries = new Queue<UIDialogueEntry>(UIConstants.MAX_OLD_DIALOGUE_ENTRIES);
            _topicEntries = new List<UIButton>();

            _topicEntryParent = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_topicEntryParent) as RectTransform;
            _recentDialogueEntryParent = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_recentDialogueEntryParent) as RectTransform;
            _oldDialogueEntryParent = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_oldDialogueEntryParent) as RectTransform;

            Transform scrollGroupTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_dialogueScrollViewCanvasGroup);
            if (scrollGroupTform != null)
            {
                _dialogueScrollViewCanvasGroup = scrollGroupTform.GetComponent<CanvasGroup>();
            }

            Transform viewportTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_topicViewportLayoutElement);
            if(viewportTform != null)
            {
                _topicViewportLayoutElement = viewportTform.GetComponent<LayoutElement>();
            }

            Transform dialogueTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_dialogueLayoutElement);
            if (dialogueTform != null)
            {
                _dialogueLayoutElement = dialogueTform.GetComponent<LayoutElement>();
            }

            Transform dialogueInteractElem = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_dialoguePanelInteractiveElement);
            if (dialogueInteractElem != null)
            {
                _dialoguePanelInteractiveElement = dialogueInteractElem.GetComponent<UIInteractiveElement>();
                _dialoguePanelInteractiveElement?.Initialize();
            }

            Transform pendingTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_pendingDialogueLayoutElement);
            if (pendingTform != null)
            {
                _pendingDialogueLayoutElement = pendingTform.GetComponent<LayoutElement>();
            }

            Transform dividerTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_topicDivider);
            if (dividerTform != null)
            {
                _topicDivider = dividerTform.gameObject;
            }

            Transform okayTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_okayButton);
            if (okayTform != null)
            {
                _okayButton = okayTform.GetComponent<UIButton>();
                if (_okayButton != null)
                {
                    _okayButton.Initialize();
                    _okayButton.Event_OnAnyClick += (UIButton btn, PointerEventData data) =>
                    {
                        if (data.button == PointerEventData.InputButton.Left && data.clickCount == 1)
                        {
                            Event_ButtonClicked_OnSubmitDialogueEntry?.Invoke();
                        }
                    };
                }
            }

            Transform prevTopicTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_prevTopicButton);
            if (prevTopicTform != null)
            {
                _previousTopicButton = prevTopicTform.GetComponent<UIButton>();
                _previousTopicButton?.Initialize();
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
                _speakingStyleToggleGroup?.Initialize();
            }

            Transform normalTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_normalSpeakingStyleToggle);
            if (normalTform != null)
            {
                normalSpeakingStyleToggle = normalTform.GetComponent<UIToggle>();
                normalSpeakingStyleToggle?.Initialize();
            }

            Transform politeTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_politeSpeakingStyleToggle);
            if (politeTform != null)
            {
                politeSpeakingStyleToggle = politeTform.GetComponent<UIToggle>();
                politeSpeakingStyleToggle?.Initialize();
            }

            Transform bluntTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_bluntSpeakingStyleToggle);
            if (bluntTform != null)
            {
                bluntSpeakingStyleToggle = bluntTform.GetComponent<UIToggle>();
                bluntSpeakingStyleToggle?.Initialize();
            }
        }

        public override void Cleanup()
        {
            Event_ButtonClicked_OnSubmitDialogueEntry = null;
            Event_OnCopyDialogueToNotebook = null;

            ClearTopics();
            ClearDialogue();
            SetPendingDialogue(null);

            _allDialogueEntries = null;
            _recentDialogueEntries = null;
            _oldDialogueEntries = null;
            _topicEntries = null;

            _previousTopicButton?.Cleanup();
            _previousTopicButton = null;

            _dialoguePanelInteractiveElement?.Cleanup();
            _dialoguePanelInteractiveElement = null;

            _okayButton?.Cleanup();
            _okayButton = null;

            _speakingStyleToggleGroup?.Cleanup();
            _speakingStyleToggleGroup = null;

            base.Cleanup();
        }

        public override void ResetWindow()
        {
            ClearTopics();
            ClearDialogue();
            SetPendingDialogue(null);
            base.ResetWindow();
        }
        #endregion


        #region UIWindow Overrides
        protected override void ShowInternal()
        {
            if(_dialogueLayoutElement != null)
            {
                float padding = _EMPTY_SPACE_PADDING;
                if(_pendingDialogueLayoutElement != null)
                {
                    padding += Mathf.Max(_pendingDialogueLayoutElement.minHeight, _pendingDialogueLayoutElement.preferredHeight);
                }

                RectTransform rt = transform as RectTransform;
                _dialogueLayoutElement.minHeight = rt.sizeDelta.y - padding;
            }

            if(_dialogueScrollViewCanvasGroup != null)
            {
                _dialogueScrollViewCanvasGroup.blocksRaycasts = false;
            }

            base.ShowInternal();
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

        public UIDialogueEntry GetDialogueEntryByIndex(int idx)
        {
            return idx >= 0 && idx < _allDialogueEntries.Count ? _allDialogueEntries[idx] : null;
        }

        public UIDialogueEntry AddDialogueEntry(DialogueInfo dialogue)
        {
            if(dialogue.entryPrefab == null) { return null; }

            while(_oldDialogueEntries.Count > UIConstants.MAX_OLD_DIALOGUE_ENTRIES - 1)
            {
                UIDialogueEntry entry = _oldDialogueEntries.Dequeue();
                _allDialogueEntries.Remove(entry);
                Destroy(entry.gameObject);
            }

            while (_recentDialogueEntries.Count > UIConstants.MAX_RECENT_DIALOGUE_ENTRIES - 1)
            {
                UIDialogueEntry removedEntry = _recentDialogueEntries.Dequeue();
                removedEntry.transform.SetParent(_oldDialogueEntryParent);
                _oldDialogueEntries.Enqueue(removedEntry);
            }

            UIDialogueEntry dialogueEntry = Instantiate(dialogue.entryPrefab, _recentDialogueEntryParent);
            if (dialogueEntry != null)
            {
                _allDialogueEntries.Add(dialogueEntry);
                _recentDialogueEntries.Enqueue(dialogueEntry);

                dialogueEntry.Initialize();
                dialogueEntry.SetPortraitTexture(dialogue.speakerPortrait);
                dialogueEntry.SetTitle(dialogue.speakerName);
                dialogueEntry.SetDisplayValue(dialogue.dialogueText);
                dialogueEntry.PlaySlideInAnim();

                dialogueEntry.Event_OnButtonClicked_CopyToNotebook += (UIDialogueEntry entry) =>
                {
                    int entryIndex = _allDialogueEntries.IndexOf(entry);
                    Event_OnCopyDialogueToNotebook?.Invoke(entry, entryIndex);
                };

                if (_dialogueScrollViewCanvasGroup != null)
                {
                    _dialogueScrollViewCanvasGroup.blocksRaycasts = _allDialogueEntries.Count > 1;
                }
            }
            return dialogueEntry;
        }

        public void ClearDialogue()
        {
            if (_allDialogueEntries != null)
            {
                for (int i = _allDialogueEntries.Count - 1; i >= 0; i--)
                {
                    if (_allDialogueEntries[i] != null)
                    {
                        Destroy(_allDialogueEntries[i].gameObject);
                    }
                }
                _allDialogueEntries.Clear();
            }

            _recentDialogueEntries?.Clear();
            _oldDialogueEntries?.Clear();
        }

        public UIButton AddTopicEntry(string buttonLabel, UIButton prefab)
        {
            if (prefab == null) { return null; }

            UIButton topicTypeBtn = Instantiate(prefab, _topicEntryParent);
            if (topicTypeBtn != null)
            {
                _topicEntries.Add(topicTypeBtn);

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

        public void ClearTopics()
        {
            if (_topicEntries != null)
            {
                for (int i = _topicEntries.Count - 1; i > -1; i--)
                {
                    Destroy(_topicEntries[i].gameObject);
                }
                _topicEntries.Clear();
            }
        }

        public void UpdateTopicPanelSize()
        {
            StartCoroutine(UpdateSizeRoutine());
        }

        public void ActivateTopicDividerWithSiblingIndex(int siblingIndex)
        {
            StartCoroutine(ActivateDividerWithSiblingIndexRoutine(siblingIndex));
        }

        public void DeactivateTopicDivider()
        {
            if (_topicDivider != null)
            {
                _topicDivider.SetActive(false);
            }
        }

        public void SetTopicDividerSiblingIndex(int index)
        {
            if(_topicDivider != null)
            {
                _topicDivider.transform.SetSiblingIndex(index);
            }
        }

        public void SetOkayButtonEnabled(bool enabled)
        {
            if (_okayButton != null)
            {
                _okayButton.isDisabled = !enabled;
            }
        }
        #endregion


        #region Coroutines
        private IEnumerator UpdateSizeRoutine()
        {
            yield return 0f;
            yield return 0f;

            if (_topicViewportLayoutElement != null)
            {
                float height = _topicEntryParent != null ? _topicEntryParent.sizeDelta.y : 0f;
                float maxTopicPanelSize = height;

                if (_canvasComponent != null)
                {
                    RectTransform rt = transform as RectTransform;
                    maxTopicPanelSize = rt.sizeDelta.y - _TOPIC_HEADER_PADDING - _TOPIC_FOOTER_PADDING - _EMPTY_SPACE_PADDING;
                }

                _topicViewportLayoutElement.minHeight = Mathf.Min(height + _TOPIC_PADDING, maxTopicPanelSize);
            }
        }

        private IEnumerator<float> ActivateDividerWithSiblingIndexRoutine(int siblingIndex)
        {
            yield return 0f;

            if (_topicDivider != null)
            {
                _topicDivider.SetActive(true);
                _topicDivider.transform.SetSiblingIndex(siblingIndex);
            }
        }
        #endregion
    }
}
