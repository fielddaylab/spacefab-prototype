using BeauRoutine;
using BeauUtil;
using BeauUtil.Debugger;
using BeauUtil.Tags;
using FieldDay;
using FieldDay.Scripting;
using FieldDay.UI;
using Leaf.Runtime;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceFab {
    public sealed class BasicDialogPanel : SharedRoutinePanel, IDialoguePrinter, IRegistrationCallbacks {
        [Header("Dialog Panel")]
        [SerializeField] private SerializedHash32 m_PanelId;
        [SerializeField] private TMP_Text m_Text;
        [SerializeField] private LayoutSizeGroup m_Size;
        [SerializeField] private Button m_ContinueButton;

        #region Registration

        void IRegistrationCallbacks.OnRegister() {
            ScriptUtility.RegisterDialoguePrinter(m_PanelId, this);
        }

        void IRegistrationCallbacks.OnDeregister() {
            ScriptUtility.DeregisterDialoguePrinter(m_PanelId, this);
        }

        #endregion // Registration

        #region IDialoguePrinter

        public TagStringEventHandler PrepareLine(TagString text, DialogueCharacterState character, TagStringEventHandler parentHandler) {
            m_Text.SetText(text.RichText);
            m_Text.maxVisibleCharacters = 0;
            Vector2 size = m_Text.GetPreferredValues();
            Log.Msg("text size: {0}x{1}", size.x, size.y);
            m_Size.SetSize(size);
            GuiCommands.SetActive(m_ContinueButton.gameObject, false);
            return parentHandler;
        }

        public void UpdateCharacter(DialogueCharacterState character) {
        }

        public IEnumerator TypeLine(TagString text, TagTextData textData) {
            if (m_Text.maxVisibleCharacters == 0) {
                Show();
            }

            int charsToType = textData.VisibleCharacterCount;
            int charIndex = textData.VisibleCharacterOffset;
            while (charsToType-- > 0) {
                char characterAtIndex = text.VisibleText[charIndex++];
                m_Text.maxVisibleCharacters++;
                yield return null;
            }
        }

        public IEnumerator CompleteLine() {
            if (IsShowing() && m_Text.maxVisibleCharacters > 0) {
                GuiCommands.SetActive(m_ContinueButton.gameObject, true);
                yield return m_ContinueButton.onClick.WaitForInvoke();
                GuiCommands.SetActive(m_ContinueButton.gameObject, false);
            }
        }

        public void FastForwardLine(int visibleCount, int richText) {
            m_Text.maxVisibleCharacters = visibleCount;
        }

        public void StartSkip() {
        }

        public void CancelSkip() {
        }

        #endregion // IDialoguePrinter

        #region Panel

        protected override void OnHideComplete(bool inbInstant) {
            m_Text.SetText(string.Empty);
        }

        #endregion // Panel

        #region Thread Owner

        [NonSerialized] private LeafThreadHandle m_Owner;

        public LeafThreadHandle GetThreadOwner() {
            return m_Owner;
        }

        public void SetThreadOwner(LeafThreadHandle handle) {
            Assert.True(m_Owner == default, "Multiple ScriptThreads contenting for ownership");
            m_Owner = handle;
        }

        public bool TryClearThreadOwner(LeafThreadHandle handle, ScriptThreadOwnershipClearReason cancelType) {
            if (handle != m_Owner) {
                return false;
            }

            m_Owner = default;
            Hide();
            return true;
        }

        public void ClearThreadOwner() {
            if (m_Owner != default) {
                m_Owner = default;
                Hide();
            }
        }

        #endregion // Thread Owner
    }
}