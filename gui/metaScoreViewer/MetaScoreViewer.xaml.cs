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
    /// Interaction logic for MetaScoreViewer.xaml
    /// </summary>
    public partial class MetaScoreViewer : UserControl, IEnumerable<TrackViewer>
    {
        #region Events
        public event EventHandler OnClickTrackCheckBox;

        public event EventHandler OnMetaRiffPackNameChanged;
        #endregion

        #region Fields
        private List<TrackViewer> trackViewerList = new List<TrackViewer>();
        #endregion

        #region Constructors
        public MetaScoreViewer()
        {
            InitializeComponent();
        }
        #endregion

        #region Public Methods
        public void RefreshControls(PredefinedGenerator generator)
        {    
            for (int index = 0; index < generator.Count; index++)
            {
                PredefinedGeneratorTrack track = generator[index];
                this[index].RefreshControls(track,generator.BarCount);
                this[index].OnClickCheckBox += ClickTrackCheckBoxHandler;
                this[index].OnMetaRiffPackNameChanged += MetaRiffPackNameChangedHandler;
            }
        }
        #endregion

        #region Private Methods
        private int GetTrackId(TrackViewer trackViewer)
        {
            int id = 0;
            foreach (TrackViewer currentTrackViewer in this)
            {
                if (trackViewer == currentTrackViewer)
                    return id;
                id++;
            }
            return -1;
        }
        #endregion

        #region Properties
        public TrackViewer this[int index]
        {
            get
            {
                MetaRiffPackLoader metaRiffPackLoader = new MetaRiffPackLoader();
                IEnumerable<string> trackList = from name in ((IEnumerable<string>)metaRiffPackLoader) orderby name select name.Substring(12);

                while (trackViewerList.Count -1 < index)
                {
                    TrackViewer trackViewer = new TrackViewer(trackList);
                    trackViewerList.Add(trackViewer);
                    this.stackTrackList.Children.Add(trackViewer);
                }

                return trackViewerList[index];
            }

            set
            {
                MetaRiffPackLoader metaRiffPackLoader = new MetaRiffPackLoader();
                IEnumerable<string> trackList = from name in ((IEnumerable<string>)metaRiffPackLoader) orderby name select name.Substring(12);

                while (trackViewerList.Count -1 < index)
                {
                    TrackViewer trackViewer = new TrackViewer(trackList);
                    trackViewerList.Add(trackViewer);
                    this.stackTrackList.Children.Add(trackViewer);
                }

                trackViewerList[index] = value;
            }
        }
        #endregion

        #region IEnumerable<TrackViewer> Members
        public IEnumerator<TrackViewer> GetEnumerator()
        {
            return trackViewerList.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return trackViewerList.GetEnumerator();
        }

        public void Add(TrackViewer item)
        {
            trackViewerList.Add(item);
            stackTrackList.Children.Add(item);
            stackTrackList.Children.Add(new Separator());
        }

        public int Count
        {
            get { return trackViewerList.Count; }
        }
        #endregion

        #region Handlers
        private void ClickTrackCheckBoxHandler(object source, EventArgs e)
        {
            TrackViewer trackViewer = (TrackViewer)source;
            CheckBox checkBox = (CheckBox)((RoutedEventArgs)e).Source;
            int checkBoxId = (int)checkBox.Tag;
            int trackId = GetTrackId(trackViewer);


            List<int> info = new List<int>();
            info.Add(trackId);
            info.Add(checkBoxId);

            if ((bool)checkBox.IsChecked)
                info.Add(1);
            else
                info.Add(0);


            if (OnClickTrackCheckBox != null)
                OnClickTrackCheckBox(info, e);
        }

        private void MetaRiffPackNameChangedHandler(object source, EventArgs e)
        {
            TrackViewer trackViewer = (TrackViewer)source;
            ComboBox comboBox = (ComboBox)((RoutedEventArgs)e).Source;

            int trackId = GetTrackId(trackViewer);
            string metaRiffPackName = (string)comboBox.SelectedItem;

            List<object> info = new List<object>();
            info.Add(trackId);
            info.Add(metaRiffPackName);

            if (OnMetaRiffPackNameChanged != null)
                OnMetaRiffPackNameChanged(info, e);
        }
        #endregion
    }
}
