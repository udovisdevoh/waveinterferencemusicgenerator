using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ArtificialArt.Lyrics;

namespace WaveBuilder
{
    /// <summary>
    /// Interaction logic for LyricTrackViewer.xaml
    /// </summary>
    public partial class LyricTrackViewer : UserControl
    {
        #region Constants
        /// <summary>
        /// How many theme to select or disable by default
        /// </summary>
        private const int selectedThemeCountDefault = 1;
        #endregion

        #region Fields
        /// <summary>
        /// List of Combo boxes for desired themes
        /// </summary>
        private List<ComboBox> comboBoxDesiredThemesList = new List<ComboBox>();

        /// <summary>
        /// List of Combo boxes for undesired themes
        /// </summary>
        private List<ComboBox> comboBoxUndesiredThemesList = new List<ComboBox>();

        /// <summary>
        /// List of sliders
        /// </summary>
        private List<Slider> sliderListIntensity = new List<Slider>();

        /// <summary>
        /// List of sliders
        /// </summary>
        private List<Slider> sliderListCharCount = new List<Slider>();
        #endregion

        #region Events
        /// <summary>
        /// When user selects theme
        /// </summary>
        public event EventHandler OnSelectTheme;

        /// <summary>
        /// When slider changes
        /// </summary>
        public event EventHandler OnSliderChange;
        #endregion

        #region Constructor
        /// <summary>
        /// Create lyric track viewer user control
        /// </summary>
        /// <param name="lyricSongFactory">lyric song factory</param>
        public LyricTrackViewer(LyricSongFactory lyricSongFactory)
        {
            InitializeComponent();
            for (int i = 0; i < Math.Max(selectedThemeCountDefault,lyricSongFactory.ThemeList.Count); i++)
            {
                ComboBox comboBox = new ComboBox();
                comboBox.SelectionChanged += SelectionChangedHandler;
                foreach (string themeName in lyricSongFactory.SelectableThemeNameList)
                {
                    comboBox.Items.Add(themeName);
                }

                if (lyricSongFactory.ThemeList.Count > i)
                    comboBox.SelectedItem = lyricSongFactory.ThemeList[i];

                comboBoxDesiredThemesList.Add(comboBox);
                stackPanelDesiredThemeSelectBoxes.Children.Add(comboBox);
            }

            for (int i = 0; i < Math.Max(selectedThemeCountDefault,lyricSongFactory.ThemeBlackList.Count); i++)
            {
                ComboBox comboBox = new ComboBox();
                comboBox.SelectionChanged += SelectionChangedHandler;
                foreach (string themeName in lyricSongFactory.SelectableThemeNameList)
                {
                    comboBox.Items.Add(themeName);
                }

                if (lyricSongFactory.ThemeBlackList.Count > i)
                    comboBox.SelectedItem = lyricSongFactory.ThemeBlackList[i];

                comboBoxUndesiredThemesList.Add(comboBox);
                stackPanelUndesiredThemeSelectBoxes.Children.Add(comboBox);
            }

            for (int i = 0; i < lyricSongFactory.BarCount; i++)
            {
                Slider slider = BuildSliderIntensity();
                sliderListIntensity.Add(slider);

                slider.Value = lyricSongFactory.GetBarIntensity(i);

                stackPanelIntensityFaderTrack.Children.Add(slider);
            }

            for (int i = 0; i < lyricSongFactory.BarCount; i++)
            {
                Slider slider = BuildSliderCharCount();
                sliderListCharCount.Add(slider);

                slider.Value = lyricSongFactory.GetBarLetterCount(i);

                stackPanelCharCountFaderTrack.Children.Add(slider);
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Refresh user control with provided lyric song factory
        /// </summary>
        /// <param name="lyricSongFactory">lyric song factory</param>
        public void Refresh(LyricSongFactory lyricSongFactory)
        {
            while (sliderListIntensity.Count < lyricSongFactory.BarCount)
            {
                Slider slider = BuildSliderIntensity();
                sliderListIntensity.Add(slider);
                stackPanelIntensityFaderTrack.Children.Add(slider);
            }

            while (sliderListCharCount.Count < lyricSongFactory.BarCount)
            {
                Slider slider = BuildSliderCharCount();
                sliderListCharCount.Add(slider);
                stackPanelCharCountFaderTrack.Children.Add(slider);
            }

            int barIndex = 0;
            foreach (Slider slider in sliderListIntensity)
            {
                lyricSongFactory.SetBarIntensity(barIndex, (float)slider.Value);
                barIndex++;
            }

            barIndex = 0;
            foreach (Slider slider in sliderListCharCount)
            {
                lyricSongFactory.SetBarLetterCount(barIndex, (short)slider.Value);
                barIndex++;
            }
        }

        /// <summary>
        /// Add combo box for desired theme list
        /// <param name="lyricSongFactory">lyric song factory reference for theme name list</param>
        /// <param name="isDesired">whether it's for desired themes or undesired themes</param>
        /// </summary>
        public void AddComboBox(LyricSongFactory lyricSongFactory, bool isDesired)
        {
            ComboBox comboBox = new ComboBox();
            comboBox.SelectionChanged += SelectionChangedHandler;

            foreach (string themeName in lyricSongFactory.SelectableThemeNameList)
            {
                comboBox.Items.Add(themeName);
            }

            if (isDesired)
            {
                comboBoxDesiredThemesList.Add(comboBox);
                stackPanelDesiredThemeSelectBoxes.Children.Add(comboBox);
            }
            else
            {
                comboBoxUndesiredThemesList.Add(comboBox);
                stackPanelUndesiredThemeSelectBoxes.Children.Add(comboBox);
            }
        }

        /// <summary>
        /// Refresh theme list
        /// </summary>
        internal void RefreshThemeListFromView(LyricSongFactory lyricSongFactory, ThemeSelectionManager themeSelectionManager, LyricSongFactoryCollection lyricSongFactoryCollection, MainWindow mainWindow)
        {
            //We clear themes in model
            lyricSongFactory.ClearThemes();

            //We add themes from view to model
            themeSelectionManager.AddThemesFromComboBox(lyricSongFactory, this);

            //We clear themes in view
            themeSelectionManager.ClearThemes(this.ComboBoxDesiredThemesList, this.ComboBoxUndesiredThemesList);

            //We add themes from model to view
            themeSelectionManager.IsLocked = true;
            themeSelectionManager.AddThemesToComboBox(lyricSongFactory, this);
            themeSelectionManager.IsLocked = false;

            //We add new combo boxes if existing ones are full
            themeSelectionManager.AddNewComboBoxesIfNeeded(lyricSongFactory, this);

            if (mainWindow.IsNeedNewLyricTrackViewer(lyricSongFactoryCollection))
            {
                LyricSongFactory newLyricSongFactory = lyricSongFactoryCollection.AddNew();
                mainWindow.AddLyricTrackViewer(newLyricSongFactory);
            }
        }

        internal void Clear()
        {
            foreach (ComboBox comboBox in comboBoxDesiredThemesList)
            {
                comboBox.IsEnabled = false;
                comboBox.SelectedItem = null;
                comboBox.IsEnabled = true;
            }

            while (comboBoxDesiredThemesList.Count > 1)
            {
                ComboBox last = comboBoxDesiredThemesList.Last();
                comboBoxDesiredThemesList.Remove(last);
                stackPanelDesiredThemeSelectBoxes.Children.Remove(last);
            }

            foreach (ComboBox comboBox in comboBoxUndesiredThemesList)
            {
                comboBox.IsEnabled = false;
                comboBox.SelectedItem = null;
                comboBox.IsEnabled = true;
            }

            while (comboBoxUndesiredThemesList.Count > 1)
            {
                ComboBox last = comboBoxUndesiredThemesList.Last();
                comboBoxUndesiredThemesList.Remove(last);
                stackPanelUndesiredThemeSelectBoxes.Children.Remove(last);
            }
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// When theme selection changed
        /// </summary>
        /// <param name="source">source</param>
        /// <param name="e">args</param>
        private void SelectionChangedHandler(object source, EventArgs e)
        {
            ComboBox sourceComboBox = (ComboBox)source;
            if (!sourceComboBox.IsEnabled)
                return;

            if (OnSelectTheme != null) OnSelectTheme(this, e);
        }

        /// <summary>
        /// When slider value changes
        /// </summary>
        /// <param name="source">source</param>
        /// <param name="e">event args</param>
        private void SliderChangeHandler(object source, EventArgs e)
        {
            if (OnSliderChange != null) OnSliderChange(this, e);
        }

        /// <summary>
        /// When char count master fader changes
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event args</param>
        private void SliderCharCountMasterDragCompletedHandler(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            foreach (Slider slider in sliderListCharCount)
                slider.Value = sliderCharCountMaster.Value;
        }

        /// <summary>
        /// When intensity master fader changes
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event args</param>
        private void SliderIntensityMasterDragCompletedHandler(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            foreach (Slider slider in sliderListIntensity)
                slider.Value = sliderIntensityMaster.Value;
        }
        #endregion

        #region Properties
        /// <summary>
        /// List of Combo boxes for desired themes
        /// </summary>
        public List<ComboBox> ComboBoxDesiredThemesList
        {
            get { return comboBoxDesiredThemesList; }
            set { comboBoxDesiredThemesList = value; }
        }

        /// <summary>
        /// List of Combo boxes for undesired themes
        /// </summary>
        public List<ComboBox> ComboBoxUndesiredThemesList
        {
            get { return comboBoxUndesiredThemesList; }
            set { comboBoxUndesiredThemesList = value; }
        }
        #endregion

        #region Private Methods
        private Slider BuildSliderIntensity()
        {
            Slider slider = new Slider();
            slider.DragOver += SliderChangeHandler;
            slider.Height = 96;
            slider.Width = 24;
            slider.Maximum = 1;
            slider.Minimum = 0;
            slider.Orientation = Orientation.Vertical;
            return slider;
        }

        private Slider BuildSliderCharCount()
        {
            Slider slider = new Slider();
            slider.DragOver += SliderChangeHandler;
            slider.Height = 96;
            slider.Width = 24;
            slider.Maximum = 128;
            slider.Minimum = 16;
            slider.Orientation = Orientation.Vertical;
            return slider;
        }
        #endregion
    }
}
