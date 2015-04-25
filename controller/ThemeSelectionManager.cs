using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using ArtificialArt.Lyrics;

namespace WaveBuilder
{
    /// <summary>
    /// Manages selections of themes
    /// </summary>
    internal class ThemeSelectionManager
    {
        #region Fields
        /// <summary>
        /// Whether theme selection is currently locked
        /// </summary>
        private bool isLocked = false;
        #endregion

        #region Internal Methods
        /// <summary>
        /// Add themes from combo box (from view to model)
        /// </summary>
        /// <param name="lyricSongFactory">lyric song factory (model)</param>
        /// <param name="lyricTrackViewerSource">lyric track viewer (view)</param>
        internal void AddThemesFromComboBox(LyricSongFactory lyricSongFactory, LyricTrackViewer lyricTrackViewerSource)
        {
            AddThemeFromComboBox(lyricSongFactory, lyricTrackViewerSource, true);
            AddThemeFromComboBox(lyricSongFactory, lyricTrackViewerSource, false);
        }

        /// <summary>
        /// Clear themes from view
        /// </summary>
        /// <param name="comboBoxList1">comboBox List 1</param>
        /// <param name="comboBoxList2">comboBox List 2</param>
        internal void ClearThemes(List<ComboBox> comboBoxList1, List<ComboBox> comboBoxList2)
        {
            foreach (ComboBox comboBox in comboBoxList1)
            {
                comboBox.IsEnabled = false;
                comboBox.SelectedItem = null;
                comboBox.IsEnabled = true;
            }

            foreach (ComboBox comboBox in comboBoxList2)
            {
                comboBox.IsEnabled = false;
                comboBox.SelectedItem = null;
                comboBox.IsEnabled = true;
            }
        }

        /// <summary>
        /// Add themes from model to view
        /// </summary>
        /// <param name="lyricSongFactory">model</param>
        /// <param name="lyricTrackViewerSource">view</param>
        internal void AddThemesToComboBox(LyricSongFactory lyricSongFactory, LyricTrackViewer lyricTrackViewerSource)
        {
            AddThemeToComboBox(lyricSongFactory, lyricTrackViewerSource, true);
            AddThemeToComboBox(lyricSongFactory, lyricTrackViewerSource, false);
        }

        /// <summary>
        /// Add combo boxes if all of them are full
        /// </summary>
        /// <param name="lyricSongFactory">lyric song factory (model)</param>
        /// <param name="lyricTrackViewerSource">lyric track viewer sources</param>
        internal void AddNewComboBoxesIfNeeded(LyricSongFactory lyricSongFactory, LyricTrackViewer lyricTrackViewerSource)
        {
            AddNewComboBoxesIfNeeded(lyricSongFactory, lyricTrackViewerSource, true);
            AddNewComboBoxesIfNeeded(lyricSongFactory, lyricTrackViewerSource, false);
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Add themes from combo box (from view to model)
        /// </summary>
        /// <param name="lyricSongFactory">lyric song factory (model)</param>
        /// <param name="lyricTrackViewerSource">lyric track viewer (view)</param>
        /// <param name="isThemeDesired">whether we manage desired or undesired themes</param>
        private void AddThemeFromComboBox(LyricSongFactory lyricSongFactory, LyricTrackViewer lyricTrackViewer, bool isThemeDesired)
        {
            IEnumerable<ComboBox> comboBoxList;
            if (isThemeDesired)
                comboBoxList = lyricTrackViewer.ComboBoxDesiredThemesList;
            else
                comboBoxList = lyricTrackViewer.ComboBoxUndesiredThemesList;

            foreach (ComboBox comboBox in comboBoxList)
            {
                if (comboBox.SelectedItem != null)
                {
                    if (isThemeDesired)
                    {
                        lyricSongFactory.AddTheme((string)comboBox.SelectedItem);
                    }
                    else
                    {
                        lyricSongFactory.CensorTheme((string)comboBox.SelectedItem);
                    }
                }
            }
        }

        /// <summary>
        /// Add themes from model to view
        /// </summary>
        /// <param name="lyricSongFactory">model</param>
        /// <param name="lyricTrackViewerSource">view</param>
        /// <param name="isThemeDesired">whether we manage desired or undesired themes</param>
        private void AddThemeToComboBox(LyricSongFactory lyricSongFactory, LyricTrackViewer lyricTrackViewerSource, bool isThemeDesired)
        {
            IEnumerable<ComboBox> comboBoxList;
            if (isThemeDesired)
                comboBoxList = lyricTrackViewerSource.ComboBoxDesiredThemesList;
            else
                comboBoxList = lyricTrackViewerSource.ComboBoxUndesiredThemesList;

            List<string> themeNameList;
            if (isThemeDesired)
                themeNameList = lyricSongFactory.ThemeList;
            else
                themeNameList = lyricSongFactory.ThemeBlackList;

            foreach (string themeName in themeNameList)
                if (!isContainTheme(themeName, comboBoxList))
                    AddThemeToNextEmptyComboBox(themeName, comboBoxList);
        }

        private bool isContainTheme(string themeName, IEnumerable<ComboBox> comboBoxList)
        {
            foreach (ComboBox comboBox in comboBoxList)
                if (comboBox.SelectedItem != null && ((string)comboBox.SelectedItem) == themeName)
                    return true;
            return false;
        }

        /// <summary>
        /// Add theme to next empyt combo box
        /// </summary>
        /// <param name="themeName">theme name</param>
        /// <param name="comboBoxList">list of combo box</param>
        private void AddThemeToNextEmptyComboBox(string themeName, IEnumerable<ComboBox> comboBoxList)
        {
            foreach (ComboBox comboBox in comboBoxList)
                if (comboBox.SelectedItem == null || ((string)comboBox.SelectedItem) == "")
                {
                    comboBox.IsEnabled = false;
                    comboBox.SelectedItem = themeName;
                    comboBox.IsEnabled = true;
                    return;
                }
        }

        /// <summary>
        /// Add combo boxes if all of them are full
        /// </summary>
        /// <param name="lyricTrackViewerSource">lyric track viewer sources</param>
        /// <param name="lyricTrackViewerSource">lyric Track Viewer Source (view)</param>
        /// <param name="isThemeDesired">whether we manage desired themes or undesired themes</param>
        private void AddNewComboBoxesIfNeeded(LyricSongFactory lyricSongFactory, LyricTrackViewer lyricTrackViewerSource, bool isThemeDesired)
        {
            IEnumerable<ComboBox> comboBoxList;
            if (isThemeDesired)
                comboBoxList = lyricTrackViewerSource.ComboBoxDesiredThemesList;
            else
                comboBoxList = lyricTrackViewerSource.ComboBoxUndesiredThemesList;

            foreach (ComboBox comboBox in comboBoxList)
                if (comboBox.SelectedItem == null || ((string)comboBox.SelectedItem).Trim() == "")
                    return;

            lyricTrackViewerSource.AddComboBox(lyricSongFactory, isThemeDesired);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Whether theme selection is currently locked
        /// </summary>
        public bool IsLocked
        {
            get { return isLocked; }
            set { isLocked = value; }
        }
        #endregion
    }
}
