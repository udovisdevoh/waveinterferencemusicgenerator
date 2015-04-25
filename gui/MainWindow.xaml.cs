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
using ArtificialArt.Audio.Midi.Generator;
using ArtificialArt.Lyrics;

namespace WaveBuilder
{
    /// <summary>
    /// Interaction logic for MainWindows.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Parts
        private MetaScoreViewer metaScoreViewer = new MetaScoreViewer();

        private LyricGeneratorViewer lyricGeneratorViewer;

        private KaraokeViewer karaokeViewer;
        #endregion

        #region Events
        /// <summary>
        /// When creating new generator
        /// </summary>
        public event EventHandler OnGeneratorNew;

        /// <summary>
        /// When edit control
        /// </summary>
        public event EventHandler OnEditControl;

        /// <summary>
        /// When click track checkbox
        /// </summary>
        public event EventHandler OnClickTrackCheckBox;

        /// <summary>
        /// When metariffpack name changes
        /// </summary>
        public event EventHandler OnMetaRiffPackNameChanged;

        /// <summary>
        /// When building song
        /// </summary>
        public event EventHandler OnSongBuild;

        /// <summary>
        /// When song plays
        /// </summary>
        public event EventHandler OnSongPlay;

        /// <summary>
        /// When song stops
        /// </summary>
        public event EventHandler OnSongStop;

        /// <summary>
        /// When exit
        /// </summary>
        public event EventHandler OnExit;

        /// <summary>
        /// When slider changes
        /// </summary>
        public event EventHandler OnSliderChange;
        
        /// <summary>
        /// When theme selection changes
        /// </summary>
        public event EventHandler OnThemeSelectionChanged;

        /// <summary>
        /// When saving lyrics
        /// </summary>
        public event EventHandler OnLyricsSaveAs;

        /// <summary>
        /// When saving music
        /// </summary>
        public event EventHandler OnMusicSaveAs;

        /// <summary>
        /// When loading lyrics
        /// </summary>
        public event EventHandler OnLyricsOpen;

        /// <summary>
        /// When loading music
        /// </summary>
        public event EventHandler OnMusicOpen;

        /// <summary>
        /// When saving song
        /// </summary>
        public event EventHandler OnSongSaveAs;

        /// <summary>
        /// When loading songs
        /// </summary>
        public event EventHandler OnSongOpen;

        /// <summary>
        /// When saving lyrics generator
        /// </summary>
        public event EventHandler OnLyricsGeneratorSaveAs;

        /// <summary>
        /// When loading lyrics generator
        /// </summary>
        public event EventHandler OnLyricsGeneratorOpen;

        /// <summary>
        /// Clear lyrics generator
        /// </summary>
        public event EventHandler OnLyricsGeneratorClear;

        /// <summary>
        /// When saving music generator
        /// </summary>
        public event EventHandler OnMusicGeneratorSaveAs;

        /// <summary>
        /// When loading music generator
        /// </summary>
        public event EventHandler OnMusicGeneratorOpen;

        /// <summary>
        /// When save as generator
        /// </summary>
        public event EventHandler OnGeneratorSaveAs;

        /// <summary>
        /// When open generator
        /// </summary>
        public event EventHandler OnGeneratorOpen;

        /// <summary>
        /// When loading file (everything that can be loaded)
        /// </summary>
        public event EventHandler OnFileOpen;

        /// <summary>
        /// When saving file (everything that can be saved)
        /// </summary>
        public event EventHandler OnFileSaveAs;

        /// <summary>
        /// Play song as karaoke
        /// </summary>
        public event EventHandler OnSongPlayAsKaraoke;

        /// <summary>
        /// When we select language
        /// </summary>
        public event EventHandler OnSelectLanguage;
        #endregion

        #region Constructor
        public MainWindow(LyricSongFactoryCollection lyricSongFactoryCollection)
        {
            InitializeComponent();

            metaScoreBorder.Child = metaScoreViewer;
            metaScoreViewer.OnClickTrackCheckBox += ClickTrackCheckBoxHandler;
            metaScoreViewer.OnMetaRiffPackNameChanged += MetaRiffPackNameChangedHandler;

            lyricGeneratorViewer = new LyricGeneratorViewer(lyricSongFactoryCollection);
            lyricGeneratorViewer.OnThemeSelectionChanged += ThemeSelectionChangedHandler;
            lyricGeneratorBorder.Child = lyricGeneratorViewer;
            lyricGeneratorViewer.OnSliderChange += SliderChangeHandler;

            karaokeViewer = new KaraokeViewer();
            karaokeTab.Content = karaokeViewer;
            
            foreach (string scaleName in Scales.GetNameList())
                comboBoxScale.Items.Add(scaleName);
        }
        #endregion

        #region Public Methods
        public void RefreshControls(PredefinedGenerator currentGenerator, LyricSongFactoryCollection lyricSongFactoryCollection)
        {
            checkBoxOverrideTempo.IsChecked = currentGenerator.IsOverrideTempo;
            checkBoxOverrideScale.IsChecked = currentGenerator.IsOverrideScale;

            sliderTempo.Value = currentGenerator.Tempo;
            textBoxTempo.Text = currentGenerator.Tempo.ToString();


            sliderModulation.Value = (int)(Math.Round(currentGenerator.Modulation * 100.0));
            textBoxModulation.Text = (int)(Math.Round(currentGenerator.Modulation * 100.0)) + "%";

            sliderLyricsToMusicPhase.Value = currentGenerator.LyricsToMusicPhase;

            textBoxBarCount.Text = currentGenerator.BarCount.ToString();
            sliderBarCount.Value = currentGenerator.BarCount;

            sliderTempo.IsEnabled = currentGenerator.IsOverrideTempo;
            textBoxTempo.IsEnabled = currentGenerator.IsOverrideTempo;
            comboBoxScale.IsEnabled = currentGenerator.IsOverrideScale;

            comboBoxScale.SelectedItem = currentGenerator.ScaleName;

            metaScoreViewer.RefreshControls(currentGenerator);
            lyricGeneratorViewer.RefreshTrackList(lyricSongFactoryCollection);
        }

        public void AddLyricTrackViewer(LyricSongFactory newLyricSongFactory)
        {
            lyricGeneratorViewer.AddLyricTrackViewer(newLyricSongFactory);
        }

        public bool IsNeedNewLyricTrackViewer(LyricSongFactoryCollection lyricSongFactoryCollection)
        {
            return lyricGeneratorViewer.IsNeedNewLyricTrackViewer(lyricSongFactoryCollection);
        }

        public void RefreshLyricGeneratorViewer(LyricSongFactoryCollection lyricSongFactoryCollection)
        {
            lyricGeneratorViewer.RefreshTrackList(lyricSongFactoryCollection);
        }

        internal void Clear()
        {
            lyricGeneratorViewer.Clear();
        }

        internal void ClearLyricGeneratorTracks()
        {
            lyricGeneratorViewer.ClearTracks();
        }

        internal void AddEmptyLyricsFieldsIfNeeded(LyricSongFactoryCollection lyricSongFactoryCollection, ThemeSelectionManager themeSelectionManager)
        {
            if (IsNeedNewLyricTrackViewer(lyricSongFactoryCollection))
            {
                LyricSongFactory newLyricSongFactory = lyricSongFactoryCollection.AddNew();
                AddLyricTrackViewer(newLyricSongFactory);
            }

            LyricTrackViewer lyricTrackViewer;
            int lyricTrackCounter = 0;
            foreach (LyricSongFactory lyricSongFactory in lyricSongFactoryCollection)
            {
                lyricTrackViewer = lyricGeneratorViewer[lyricTrackCounter];
                themeSelectionManager.AddNewComboBoxesIfNeeded(lyricSongFactory, lyricTrackViewer);
                lyricTrackCounter++;
            }
        }
        #endregion

        #region Overrides
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            if (OnExit != null)
                OnExit(this,e);
        }
        #endregion

        #region Event Handlers
        private void FileExitHandler(object sender, ExecutedRoutedEventArgs e)
        {
            if (OnExit != null)
                OnExit(this, e);
        }

        private void GeneratorNewHandler(object sender, ExecutedRoutedEventArgs e)
        {
            if (OnGeneratorNew != null)
                OnGeneratorNew(sender, e);
        }

        private void GeneratorSaveAsHandler(object sender, RoutedEventArgs e)
        {
            OnEditControl(sender, e);
            if (OnGeneratorSaveAs != null) OnGeneratorSaveAs(sender, e);
        }

        private void GeneratorOpenHandler(object sender, RoutedEventArgs e)
        {
            if (OnGeneratorOpen != null) OnGeneratorOpen(sender, e);
        }

        private void SongBuildHandler(object sender, RoutedEventArgs e)
        {
            OnEditControl(sender, e);
            if (OnSongBuild != null)
                OnSongBuild(sender, e);
        }

        private void SongStopHandler(object sender, RoutedEventArgs e)
        {
            if (OnSongStop != null)
                OnSongStop(sender, e);
        }

        private void EditControlHandler(object sender, RoutedEventArgs e)
        {
            if (OnEditControl != null)
                OnEditControl(sender,e);
        }

        private void ClickTrackCheckBoxHandler(object source, EventArgs e)
        {
            if (OnClickTrackCheckBox != null)
                OnClickTrackCheckBox(source, e);
        }

        private void MetaRiffPackNameChangedHandler(object source, EventArgs e)
        {
            if (OnMetaRiffPackNameChanged != null)
                OnMetaRiffPackNameChanged(source, e);
        }

        /// <summary>
        /// When theme selection changed
        /// </summary>
        /// <param name="source">source lyric track viewer</param>
        /// <param name="e">arguments</param>
        private void ThemeSelectionChangedHandler(object source, EventArgs e)
        {
            if (OnThemeSelectionChanged != null) OnThemeSelectionChanged(source, e);
        }

        private void SliderChangeHandler(object source, EventArgs e)
        {
            if (OnSliderChange != null) OnSliderChange(source, e);
        }

        private void MusicGeneratorOpenHandler(object sender, RoutedEventArgs e)
        {
            if (OnMusicGeneratorOpen != null) OnMusicGeneratorOpen(sender, e);
        }

        private void LyricsGeneratorOpenHandler(object sender, RoutedEventArgs e)
        {
            if (OnLyricsGeneratorOpen != null) OnLyricsGeneratorOpen(sender, e);
        }

        private void MusicSaveAsHandler(object sender, RoutedEventArgs e)
        {
            if (OnMusicSaveAs != null) OnMusicSaveAs(sender, e);
        }

        private void MusicGeneratorSaveAsHandler(object sender, RoutedEventArgs e)
        {
            if (OnMusicGeneratorSaveAs != null) OnMusicGeneratorSaveAs(sender, e);
        }

        private void LyricsSaveAsHandler(object sender, RoutedEventArgs e)
        {
            if (OnLyricsSaveAs != null) OnLyricsSaveAs(sender, e);
        }

        private void LyricsGeneratorSaveAsHandler(object sender, RoutedEventArgs e)
        {
            OnEditControl(sender, e);
            if (OnLyricsGeneratorSaveAs != null) OnLyricsGeneratorSaveAs(sender, e);
        }

        private void SongSaveAsHandler(object sender, RoutedEventArgs e)
        {
            if (OnSongSaveAs != null) OnSongSaveAs(sender, e);
        }

        private void LyricsOpenHandler(object sender, RoutedEventArgs e)
        {
            if (OnLyricsOpen != null) OnLyricsOpen(sender, e);
        }

        private void MusicOpenHandler(object sender, RoutedEventArgs e)
        {
            if (OnMusicOpen != null) OnMusicOpen(sender, e);
        }

        private void SongOpenHandler(object sender, RoutedEventArgs e)
        {
            if (OnSongOpen != null) OnSongOpen(sender, e);
        }

        private void LyricsGeneratorClearHandler(object sender, RoutedEventArgs e)
        {
            if (OnLyricsGeneratorClear != null) OnLyricsGeneratorClear(sender, e);
        }

        private void FileOpenHandler(object sender, ExecutedRoutedEventArgs e)
        {
            if (OnFileOpen != null) OnFileOpen(sender, e);
        }

        private void FileSaveAsHandler(object sender, ExecutedRoutedEventArgs e)
        {
            OnEditControl(sender, e);
            if (OnFileSaveAs != null) OnFileSaveAs(sender, e);
        }

        private void SongPlayAsSongHandler(object sender, RoutedEventArgs e)
        {
            if (OnSongPlay != null)
                OnSongPlay(sender, e);
        }

        private void SongPlayAsKaraokeHandler(object sender, RoutedEventArgs e)
        {
            if (OnSongPlayAsKaraoke != null) OnSongPlayAsKaraoke(sender, e);
        }

        private void ComboBoxLanguage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem item = (ComboBoxItem)((ComboBox)sender).SelectedValue;
            string languageCode = (string)item.Tag;
            if (OnSelectLanguage != null) OnSelectLanguage(languageCode, e);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Set the lyrics in lyrics tab
        /// </summary>
        public IEnumerable<string> LyricsText
        {
            set
            {
                string fullText = string.Empty;
                foreach (string line in value)
                    fullText += line + "\n";
                lyricsTextBox.Text = fullText;
            }
        }

        /// <summary>
        /// Karaoke viewer
        /// </summary>
        public KaraokeViewer KaraokeViewer
        {
            get { return karaokeViewer; }
        }

        /// <summary>
        /// Selected language code
        /// </summary>
        public string LanguageCode
        {
            set
            {
                if (value == "en")
                    comboBoxLanguage.SelectedIndex = 0;
                else if (value == "fr")
                    comboBoxLanguage.SelectedIndex = 1;
            }
        }
        #endregion
    }
}
