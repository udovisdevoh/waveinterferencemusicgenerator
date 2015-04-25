using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Threading;
using System.Windows.Controls;
using System.IO;
using Microsoft.Win32;
using ArtificialArt.Audio.Midi.Generator;
using ArtificialArt.Audio.Midi;
using ArtificialArt.Lyrics;

namespace WaveBuilder
{
    /// <summary>
    /// Program's entry point
    /// </summary>
    class Program
    {
        #region Constants
        /// <summary>
        /// Lyrics source file name
        /// </summary>
        private static readonly string lyricSourcePath = Path.GetDirectoryName(System.Reflection.Assembly.GetAssembly(typeof(Program)).Location) + @"\";
        #endregion

        #region Parts
        /// <summary>
        /// Wave viewer
        /// </summary>
        private MainWindow mainWindow;

        /// <summary>
        /// Random music generator
        /// </summary>
        private MusicGenerator musicGenerator;

        /// <summary>
        /// Represents a generator that can be saved, loaded and used to build MetaSongs and songs
        /// </summary>
        private PredefinedGenerator predefinedGenerator;

        /// <summary>
        /// Collection of lyric song generators
        /// </summary>
        private LyricSongFactoryCollection lyricSongFactoryCollection = new LyricSongFactoryCollection(lyricSourcePath + "lyrics.en.txt", lyricSourcePath, "en");

        /// <summary>
        /// Current song
        /// </summary>
        private RiffPack currentSong = null;

        /// <summary>
        /// Music player
        /// </summary>
        private Player player;

        /// <summary>
        /// Playing thread
        /// </summary>
        private Thread playingThread = null;

        /// <summary>
        /// WPF Application
        /// </summary>
        private Application application;

        /// <summary>
        /// Manages selections of themes
        /// </summary>
        private ThemeSelectionManager themeSelectionManager = new ThemeSelectionManager();

        /// <summary>
        /// Current song lyrics
        /// </summary>
        private List<string> songLyrics;
        #endregion

        #region Constructors
        /// <summary>
        /// Create program instance (mostly for testing)
        /// </summary>
        public Program()
        {
            try
            {
                player = new Player();
            }
            catch (OutputDeviceException)
            {
                Console.WriteLine("Warning, midi device not found");
            }

            musicGenerator = new MusicGenerator(new Random());
            mainWindow = new MainWindow(lyricSongFactoryCollection);
            mainWindow.OnGeneratorNew += GeneratorNewHandler;
            mainWindow.OnEditControl += EditControlHandler;
            mainWindow.OnClickTrackCheckBox += ClickTrackCheckBoxHandler;
            mainWindow.OnMetaRiffPackNameChanged += MetaRiffPackNameChangedHandler;
            mainWindow.OnSongBuild += SongBuildHandler;
            mainWindow.OnSongPlay += SongPlayHandler;
            mainWindow.OnSongStop += SongStopHandler;
            mainWindow.OnExit += ExitHandler;
            mainWindow.OnThemeSelectionChanged += ThemeSelectionChangedHandler;
            mainWindow.OnSliderChange += SliderChangeHandler;
            mainWindow.OnLyricsSaveAs += LyricsSaveAsHandler;
            mainWindow.OnLyricsOpen += LyricsOpenHandler;
            mainWindow.OnMusicSaveAs += MusicSaveAsHandler;
            mainWindow.OnMusicOpen += MusicOpenHandler;
            mainWindow.OnSongSaveAs += SongSaveAsHandler;
            mainWindow.OnSongOpen += SongOpenHandler;
            mainWindow.OnLyricsGeneratorSaveAs += LyricsGeneratorSaveAsHandler;
            mainWindow.OnLyricsGeneratorOpen += LyricsGeneratorOpenHandler;
            mainWindow.OnLyricsGeneratorClear += LyricsGeneratorClearHandler;
            mainWindow.OnMusicGeneratorSaveAs += MusicGeneratorSaveAsHandler;
            mainWindow.OnMusicGeneratorOpen += MusicGeneratorOpenHandler;
            mainWindow.OnGeneratorOpen += GeneratorOpenHandler;
            mainWindow.OnGeneratorSaveAs += GeneratorSaveAsHandler;
            mainWindow.OnFileOpen += FileOpenHandler;
            mainWindow.OnFileSaveAs += FileSaveAsHandler;
            mainWindow.OnSongPlayAsKaraoke += SongPlayAsKaraokeHandler;
            mainWindow.OnSelectLanguage += SelectLanguageHandler;

            GeneratorNewHandler(null, null);
        }
        #endregion

        #region Handlers
        private void GeneratorNewHandler(object source, EventArgs e)
        {
            predefinedGenerator = new PredefinedGenerator();
            currentSong = null;
            mainWindow.RefreshControls(predefinedGenerator,lyricSongFactoryCollection);
        }

        private void EditControlHandler(object source, EventArgs e)
        {
            predefinedGenerator.IsOverrideScale = (bool)mainWindow.checkBoxOverrideScale.IsChecked;
            predefinedGenerator.IsOverrideTempo = (bool)mainWindow.checkBoxOverrideTempo.IsChecked;
            predefinedGenerator.Modulation = mainWindow.sliderModulation.Value / 100;
            predefinedGenerator.ScaleName = (string)mainWindow.comboBoxScale.SelectedItem;
            predefinedGenerator.Tempo = (int)Math.Round(mainWindow.sliderTempo.Value);
            predefinedGenerator.IsOverrideKey = (bool)mainWindow.checkBoxOverrideKey.IsChecked;
            predefinedGenerator.ForcedModulationOffset = (int)mainWindow.comboBoxDefaultKey.SelectedIndex;

            if (currentSong != null && (bool)mainWindow.checkBoxOverrideTempo.IsChecked)
                currentSong.Tempo = predefinedGenerator.Tempo;

            predefinedGenerator.BarCount = lyricSongFactoryCollection.BarCount = (int)Math.Round(mainWindow.sliderBarCount.Value);
            predefinedGenerator.LyricsToMusicPhase = mainWindow.sliderLyricsToMusicPhase.Value;

            mainWindow.RefreshControls(predefinedGenerator,lyricSongFactoryCollection);
        }

        private void ClickTrackCheckBoxHandler(object source, EventArgs e)
        {
            List<int> info = (List<int>)source;

            int trackId = info[0];
            int barId = info[1];
            bool status = false;
            if (info[2] == 1)
                status = true;

            predefinedGenerator[trackId][barId] = status;
        }

        private void MetaRiffPackNameChangedHandler(object source, EventArgs e)
        {
            List<object> info = (List<object>)source;
            int trackId = (int)info[0];
            string metaRiffPackName = (string)info[1];

            predefinedGenerator[trackId].MetaRiffPackName = metaRiffPackName;
        }

        private void SongBuildHandler(object source, EventArgs e)
        {
            currentSong = musicGenerator.BuildSong(predefinedGenerator);
            songLyrics = lyricSongFactoryCollection.Build();
            mainWindow.LyricsText = songLyrics;
        }

        private void SongPlayHandler(object source, EventArgs e)
        {
            if (player.IsPlaying)
                return;

            if (currentSong == null)
                SongBuildHandler(source, e);

            if (currentSong != null && (bool)mainWindow.checkBoxOverrideTempo.IsChecked)
                currentSong.Tempo = predefinedGenerator.Tempo;

            player.IRiff = currentSong;

            while (playingThread != null && playingThread.IsAlive)
                Thread.Sleep(10);

            player.ClearEventHandlers();

            playingThread = new Thread(player.Play);
            playingThread.IsBackground = true;
            playingThread.Priority = ThreadPriority.Highest;

            playingThread.Start();

            //ThreadPool.QueueUserWorkItem(new WaitCallback(player.Play));
        }

        private void SongPlayAsKaraokeHandler(object source, EventArgs e)
        {

            if (player.IsPlaying)
                return;

            if (currentSong == null)
                SongBuildHandler(source, e);

            if (currentSong != null && (bool)mainWindow.checkBoxOverrideTempo.IsChecked)
                currentSong.Tempo = predefinedGenerator.Tempo;

            player.IRiff = currentSong;

            while (playingThread != null && playingThread.IsAlive)
                Thread.Sleep(10);

            player.ClearEventHandlers();
            mainWindow.KaraokeViewer.AddEventHandlers(player);
            mainWindow.KaraokeViewer.Song = currentSong;
            mainWindow.KaraokeViewer.Lyrics = songLyrics;
            mainWindow.KaraokeViewer.Reset();

            mainWindow.tabControl.SelectedIndex = 2;
            playingThread = new Thread(player.Play);
            playingThread.IsBackground = true;
            playingThread.Priority = ThreadPriority.Highest;

            playingThread.Start();
        }

        private void SongStopHandler(object source, EventArgs e)
        {
            if (player.IsPlaying)
                player.Stop();
        }

        private void ExitHandler(object source, EventArgs e)
        {
            application.Shutdown();
        }

        /// <summary>
        /// When theme selection changed
        /// </summary>
        /// <param name="source">source lyric track viewer</param>
        /// <param name="e">arguments</param>
        private void ThemeSelectionChangedHandler(object source, EventArgs e)
        {
            if (themeSelectionManager.IsLocked)
                return;
            themeSelectionManager.IsLocked = true;

            LyricTrackViewer lyricTrackViewerSource = (LyricTrackViewer)source;
            int trackId = (int)lyricTrackViewerSource.Tag;

            LyricSongFactory lyricSongFactory = lyricSongFactoryCollection[trackId];

            lyricTrackViewerSource.RefreshThemeListFromView(lyricSongFactory, themeSelectionManager, lyricSongFactoryCollection, mainWindow);

            themeSelectionManager.IsLocked = false;
        }

        private void SliderChangeHandler(object source, EventArgs e)
        {
            mainWindow.RefreshControls(predefinedGenerator, lyricSongFactoryCollection);
        }

        private void LyricsSaveAsHandler(object sender, EventArgs e)
        {
            if (songLyrics == null)
                return;

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Lyrics xml|*.xml";
            saveFileDialog.OverwritePrompt = true;

            bool result = (bool)saveFileDialog.ShowDialog();

            if (result)
            {
                songLyrics.Save(saveFileDialog.FileName);
            }
        }

        private void LyricsGeneratorSaveAsHandler(object sender, EventArgs e)
        {
            if (lyricSongFactoryCollection == null)
                return;

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Lyrics generator xml|*.xml";
            saveFileDialog.OverwritePrompt = true;

            bool result = (bool)saveFileDialog.ShowDialog();

            if (result)
            {
                lyricSongFactoryCollection.Save(saveFileDialog.FileName);
            }
        }

        private void GeneratorOpenHandler(object sender, EventArgs e)
        {
            this.LyricsGeneratorClearHandler(sender, e);

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Song generator xml|*.xml";

            bool result = (bool)openFileDialog.ShowDialog();

            if (result)
            {
                predefinedGenerator = MidiGeneratorIO.Load(openFileDialog.FileName);
                lyricSongFactoryCollection = LyricSongFactoryIO.Load(openFileDialog.FileName, lyricSourcePath);
                mainWindow.ClearLyricGeneratorTracks();
                mainWindow.RefreshLyricGeneratorViewer(lyricSongFactoryCollection);
                mainWindow.RefreshControls(predefinedGenerator, lyricSongFactoryCollection);
                mainWindow.LanguageCode = lyricSongFactoryCollection.LanguageCode;
            }
        }

        private void GeneratorSaveAsHandler(object sender, EventArgs e)
        {
            if (predefinedGenerator == null || lyricSongFactoryCollection == null)
                return;

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Song generator xml|*.xml";
            saveFileDialog.OverwritePrompt = true;

            bool result = (bool)saveFileDialog.ShowDialog();

            if (result)
            {
                lyricSongFactoryCollection.Save(saveFileDialog.FileName);
                predefinedGenerator.Save(saveFileDialog.FileName);
            }
        }

        private void MusicSaveAsHandler(object sender, EventArgs e)
        {
            if (currentSong == null)
                return;

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Music xml|*.xml";
            saveFileDialog.OverwritePrompt = true;

            bool result = (bool)saveFileDialog.ShowDialog();

            if (result)
            {
                currentSong.Save(saveFileDialog.FileName);
            }
        }

        private void MusicGeneratorSaveAsHandler(object sender, EventArgs e)
        {
            if (predefinedGenerator == null)
                return;

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Music generator xml|*.xml";
            saveFileDialog.OverwritePrompt = true;

            bool result = (bool)saveFileDialog.ShowDialog();

            if (result)
            {
                predefinedGenerator.Save(saveFileDialog.FileName);
            }
        }

        private void SongSaveAsHandler(object sender, EventArgs e)
        {
            if (currentSong == null || songLyrics == null)
                return;

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Song xml|*.xml";
            saveFileDialog.OverwritePrompt = true;

            bool result = (bool)saveFileDialog.ShowDialog();

            if (result)
            {
                currentSong.Save(saveFileDialog.FileName);
                songLyrics.Save(saveFileDialog.FileName);
            }
        }

        private void LyricsOpenHandler(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Lyrics xml|*.xml";

            bool result = (bool)openFileDialog.ShowDialog();

            if (result)
            {
                songLyrics = LyricIO.Load(openFileDialog.FileName);
                mainWindow.LyricsText = songLyrics;
            }
        }

        private void LyricsGeneratorClearHandler(object sender, EventArgs e)
        {
            mainWindow.Clear();
        }

        private void LyricsGeneratorOpenHandler(object sender, EventArgs e)
        {
            this.LyricsGeneratorClearHandler(sender, e);

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Lyrics generator xml|*.xml";

            bool result = (bool)openFileDialog.ShowDialog();

            if (result)
            {
                lyricSongFactoryCollection = LyricSongFactoryIO.Load(openFileDialog.FileName, lyricSourcePath);
                mainWindow.ClearLyricGeneratorTracks();
                mainWindow.RefreshLyricGeneratorViewer(lyricSongFactoryCollection);
                mainWindow.RefreshControls(predefinedGenerator, lyricSongFactoryCollection);
                mainWindow.LanguageCode = lyricSongFactoryCollection.LanguageCode;
            }
        }

        private void MusicGeneratorOpenHandler(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Music generator xml|*.xml";

            bool result = (bool)openFileDialog.ShowDialog();

            if (result)
            {
                predefinedGenerator = MidiGeneratorIO.Load(openFileDialog.FileName);
                mainWindow.RefreshControls(predefinedGenerator, lyricSongFactoryCollection);
            }
        }

        private void MusicOpenHandler(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Music xml|*.xml";

            bool result = (bool)openFileDialog.ShowDialog();

            if (result)
            {
                currentSong = RiffIO.Load(openFileDialog.FileName);
            }
        }

        private void SongOpenHandler(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Song xml|*.xml";

            bool result = (bool)openFileDialog.ShowDialog();

            if (result)
            {
                currentSong = RiffIO.Load(openFileDialog.FileName);
                songLyrics = LyricIO.Load(openFileDialog.FileName);
                mainWindow.LyricsText = songLyrics;
            }
        }

        private void FileOpenHandler(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "XML file|*.xml";

            bool result = (bool)openFileDialog.ShowDialog();

            if (result)
            {
                currentSong = RiffIO.Load(openFileDialog.FileName);
                songLyrics = LyricIO.Load(openFileDialog.FileName);
                predefinedGenerator = MidiGeneratorIO.Load(openFileDialog.FileName);
                lyricSongFactoryCollection = LyricSongFactoryIO.Load(openFileDialog.FileName, lyricSourcePath);
                mainWindow.ClearLyricGeneratorTracks();
                mainWindow.RefreshLyricGeneratorViewer(lyricSongFactoryCollection);
                mainWindow.RefreshControls(predefinedGenerator, lyricSongFactoryCollection);
                mainWindow.LanguageCode = lyricSongFactoryCollection.LanguageCode;
                mainWindow.AddEmptyLyricsFieldsIfNeeded(lyricSongFactoryCollection,themeSelectionManager);
                mainWindow.LyricsText = songLyrics;
            }
        }

        private void FileSaveAsHandler(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "XML file|*.xml";
            saveFileDialog.OverwritePrompt = true;

            bool result = (bool)saveFileDialog.ShowDialog();

            if (result)
            {
                currentSong.Save(saveFileDialog.FileName);
                songLyrics.Save(saveFileDialog.FileName);
                lyricSongFactoryCollection.Save(saveFileDialog.FileName);
                predefinedGenerator.Save(saveFileDialog.FileName);
            }
        }

        private void SelectLanguageHandler(object sender, EventArgs e)
        {
            string languageCode = (string)sender;
            lyricSongFactoryCollection.LanguageCode = languageCode;
        }
        #endregion

        #region Main
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Program program = new Program();
            program.Start();
        }

        /// <summary>
        /// Start the application
        /// </summary>
        private void Start()
        {
            #warning Must be able to know where lyrics file are when changing languages
            #warning Add unit test for single riff
            #warning Add unit test for riff pack
            #warning add quasi straight kick metaRiffPacks
            #warning Time Splitter must allow shuffle
            #warning Must exit properly and ask save etc...
            #warning Must remove start method
            #warning Implement file IO
            #warning Implement use override song values (from toolbar)
            #warning Must allow to disable tritones and semitones
            #warning Add cello and contrabass
            #warning Add a final note at the very end (play first note of next non-existant riff)
            #warning Add big slider for all char counts and all intensity

            application = new Application();
            application.Run(mainWindow);
        }
        #endregion
    }
}