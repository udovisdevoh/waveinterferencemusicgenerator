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
using ArtificialArt.Audio.Midi.Generator;

namespace WaveBuilder
{
    /// <summary>
    /// Interaction logic for LyricGeneratorViewer.xaml
    /// </summary>
    public partial class LyricGeneratorViewer : UserControl, IEnumerable<LyricTrackViewer>
    {
        #region Fields
        /// <summary>
        /// Internal list of track viewer
        /// </summary>
        private List<LyricTrackViewer> trackViewerList;
        #endregion

        #region Events
        /// <summary>
        /// When theme selection changes
        /// </summary>
        public event EventHandler OnThemeSelectionChanged;

        /// <summary>
        /// When slider changes
        /// </summary>
        public event EventHandler OnSliderChange;
        #endregion

        #region Constructors
        /// <summary>
        /// Create lyric generator viewer
        /// </summary>
        /// <param name="lyricSongFactoryCollection">lyric song factory collection</param>
        public LyricGeneratorViewer(LyricSongFactoryCollection lyricSongFactoryCollection)
        {
            InitializeComponent();
            trackViewerList = new List<LyricTrackViewer>();
            RefreshTrackList(lyricSongFactoryCollection);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Refresh the list of track
        /// </summary>
        /// <param name="lyricSongFactoryCollection">lyric song factory collection</param>
        public void RefreshTrackList(LyricSongFactoryCollection lyricSongFactoryCollection)
        {
            int trackCounter = 0;
            foreach (LyricSongFactory lyricSongFactory in lyricSongFactoryCollection)
            {
                RefreshTrack(trackCounter, lyricSongFactory);
                trackCounter++;
            }
        }

        public void AddLyricTrackViewer(LyricSongFactory lyricSongFactory)
        {
            LyricTrackViewer lyricTrackViewer = new LyricTrackViewer(lyricSongFactory);
            lyricTrackViewer.Tag = trackViewerList.Count;
            lyricTrackViewer.OnSelectTheme += ThemeSelectionChangedHandler;
            lyricTrackViewer.OnSliderChange += SliderChangeHandler;
            trackViewerList.Add(lyricTrackViewer);
            stackTrackList.Children.Add(lyricTrackViewer);
        }

        public bool IsNeedNewLyricTrackViewer(LyricSongFactoryCollection lyricSongFactoryCollection)
        {
            LyricSongFactory last = lyricSongFactoryCollection.Last();
            return last.ThemeList.Count > 0 || last.ThemeBlackList.Count > 0;
        }

        internal void Clear()
        {
            foreach (LyricTrackViewer lyricTrackViewer in new List<LyricTrackViewer>(trackViewerList))
                lyricTrackViewer.Clear();

            while (trackViewerList.Count > 1)
            {
                LyricTrackViewer last = trackViewerList.Last();
                trackViewerList.Remove(last);
                stackTrackList.Children.Remove(last);
            }
        }

        internal void ClearTracks()
        {
            trackViewerList.Clear();
            stackTrackList.Children.Clear();
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Refresh track
        /// </summary>
        /// <param name="trackId">track ID</param>
        /// <param name="lyricSongFactory">lyric song factory</param>
        private void RefreshTrack(int trackId, LyricSongFactory lyricSongFactory)
        {
            if (trackViewerList.Count <= trackId)
            {
                AddLyricTrackViewer(lyricSongFactory);
            }
            
            trackViewerList[trackId].Refresh(lyricSongFactory);
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// When theme selection changed
        /// </summary>
        /// <param name="source">source lyric track viewer</param>
        /// <param name="e">arguments</param>
        private void ThemeSelectionChangedHandler(object source, EventArgs e)
        {
            if (OnThemeSelectionChanged != null) OnThemeSelectionChanged(source,e);
        }

        private void SliderChangeHandler(object source, EventArgs e)
        {
            if (OnSliderChange != null) OnSliderChange(source, e);
        }
        #endregion

        #region Properties
        public LyricTrackViewer this[int index]
        {
            get { return trackViewerList[index]; }
        }
        #endregion

        #region IEnumerable<LyricTrackViewer> Members
        /// <summary>
        /// Track viewer enumerator
        /// </summary>
        /// <returns>Track viewer enumerator</returns>
        public IEnumerator<LyricTrackViewer> GetEnumerator()
        {
            return trackViewerList.GetEnumerator();
        }

        /// <summary>
        /// Track viewer enumerator
        /// </summary>
        /// <returns>Track viewer enumerator</returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return trackViewerList.GetEnumerator();
        }
        #endregion
    }
}