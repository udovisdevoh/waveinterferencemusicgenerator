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

namespace WaveBuilder
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class KaraokeViewer : UserControl
    {
        #region Fields and parts
        /// <summary>
        /// Note counter
        /// </summary>
        private int noteCounter = 0;

        /// <summary>
        /// Previous karaoke line
        /// </summary>
        private KaraokeLineViewer previousKaraokeLine = null;

        /// <summary>
        /// List of karaoke lines
        /// </summary>
        private List<KaraokeLineViewer> karaokeLineList = new List<KaraokeLineViewer>();
        #endregion

        #region Constructor
        /// <summary>
        /// Build karaoke viewer
        /// </summary>
        public KaraokeViewer()
        {
            InitializeComponent();
        }
        #endregion

        #region Internal Methods
        /// <summary>
        /// Add event handlers for player
        /// </summary>
        /// <param name="player">music player</param>
        internal void AddEventHandlers(Player player)
        {
            //player.OnNoteOn += NoteOnHandler;
            //player.OnNoteOff += NoteOffHandler;
            player.OnBlackNoteTimeElapsed += BlackNoteTimeElapsedHandler;
        }

        /// <summary>
        /// Reset note counter
        /// </summary>
        internal void Reset()
        {
            noteCounter = 0;
            previousKaraokeLine = null;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Get karaoke line from note counter
        /// </summary>
        /// <param name="noteCounter">note counter</param>
        /// <returns>karaoke line from note counter or null if out of range</returns>
        private KaraokeLineViewer GetKaraokeLine(int noteCounter)
        {
            noteCounter /= 8;
            if (karaokeLineList.Count <= noteCounter)
                return null;
            return karaokeLineList[noteCounter];
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// After black note played
        /// </summary>
        /// <param name="source">source</param>
        /// <param name="e">event arguments</param>
        private void BlackNoteTimeElapsedHandler(object source, EventArgs e)
        {
            noteCounter++;
            KaraokeLineViewer currentKaraokeLine = GetKaraokeLine(noteCounter);

            if (currentKaraokeLine == null)
                return;

            #warning Remove previous line by using dispatcher
            /*if (previousKaraokeLine != null && previousKaraokeLine != currentKaraokeLine)
            lyricStackPanel.Children.Remove(previousKaraokeLine);*/

            previousKaraokeLine = currentKaraokeLine;

            currentKaraokeLine.AdjustBlueBar(noteCounter);          
        }
        #endregion

        #region Properties
        /// <summary>
        /// Song
        /// </summary>
        public RiffPack Song
        {
            get;
            set;
        }

        /// <summary>
        /// Lyrics
        /// </summary>
        public List<string> Lyrics
        {
            set
            {
                karaokeLineList.Clear();
                lyricStackPanel.Children.Clear();
                foreach (string line in value)
                {
                    KaraokeLineViewer karaokeLineViewer = new KaraokeLineViewer(line);
                    karaokeLineList.Add(karaokeLineViewer);
                    lyricStackPanel.Children.Add(karaokeLineViewer);
                }
            }
        }
        #endregion
    }
}
