using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceFab.ChipFab {
    public class Wiki : MonoBehaviour
    {
        [Header("Toggle")]
        public Button ToggleButton;
        public GameObject WikiContainer;

        [Header("Tabs")]
        public Button ConceptsTab;
        public Button StationsTab;

        [Header("Subcategories")]
        public Button FurnaceSubCategoryBtn;
        public Button ResistSubCategoryBtn;
        public Button PhotoSubCategoryBtn;
        public Button EtchSubCategoryBtn;
        public Button SpraySubCategoryBtn;

        [Header("Concepts Pages")]
        public WikiPage ConceptDevelopingPage;

        [Header("Stations Pages")]
        public WikiPage StationPhotoPage;

        private List<WikiPage> AllPages;
        private WikiPageCategoryIndex m_currCategory;
        private WikiPageSubCategoryIndex m_currSubCategory;

        private void Awake()
        {
            ToggleButton.onClick.AddListener(HandleToggleClicked);

            ConceptsTab.onClick.AddListener(HandleConceptsTabClicked);
            StationsTab.onClick.AddListener(HandleStationsTabClicked);

            FurnaceSubCategoryBtn.onClick.AddListener(HandleFurnaceClicked);
            ResistSubCategoryBtn.onClick.AddListener(HandleResistClicked);
            PhotoSubCategoryBtn.onClick.AddListener(HandlePhotoClicked);
            EtchSubCategoryBtn.onClick.AddListener(HandleEtchClicked);
            SpraySubCategoryBtn.onClick.AddListener(HandleSprayClicked);

            AllPages = new List<WikiPage>() {
                ConceptDevelopingPage,
                StationPhotoPage,
            };
        }

        private void UpdateCurrPage()
        {
            foreach (var page in AllPages)
            {
                if (page.CategoryId == m_currCategory && page.SubCategoryId == m_currSubCategory)
                {
                    page.gameObject.SetActive(true);
                }
                else
                {
                    page.gameObject.SetActive(false);
                }
            }
        }

        #region Handlers

        private void HandleToggleClicked()
        {
            WikiContainer.SetActive(!WikiContainer.activeSelf);

            if (WikiContainer.activeSelf)
            {
                UpdateCurrPage();
            }
        }

        private void HandleConceptsTabClicked()
        {
            m_currCategory = WikiPageCategoryIndex.Concept;
            UpdateCurrPage();
        }

        private void HandleStationsTabClicked()
        {
            m_currCategory = WikiPageCategoryIndex.Station;
            UpdateCurrPage();
        }

        private void HandleFurnaceClicked()
        {
            m_currSubCategory = WikiPageSubCategoryIndex.Furnace;
            UpdateCurrPage();
        }

        private void HandleResistClicked()
        {
            m_currSubCategory = WikiPageSubCategoryIndex.Resist;
            UpdateCurrPage();
        }

        private void HandlePhotoClicked()
        {
            m_currSubCategory = WikiPageSubCategoryIndex.Photo;
            UpdateCurrPage();
        }

        private void HandleEtchClicked()
        {
            m_currSubCategory = WikiPageSubCategoryIndex.Etch;
            UpdateCurrPage();
        }

        private void HandleSprayClicked()
        {
            m_currSubCategory = WikiPageSubCategoryIndex.Spray;
            UpdateCurrPage();
        }

        #endregion // Handlers
    }
}