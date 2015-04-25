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
    /// Interaction logic for TrackViewer.xaml
    /// </summary>
    public partial class TrackViewer : UserControl, IEnumerable<CheckBox>
    {
        #region Constants
        private const int maxBarCount = 64;
        #endregion

        #region Events
        public event EventHandler OnClickCheckBox;

        public event EventHandler OnMetaRiffPackNameChanged;
        #endregion

        #region Fields
        private List<CheckBox> internalList = new List<CheckBox>();
        #endregion

        #region Constructors
        public TrackViewer(IEnumerable<string> listValues) : this(listValues,0)
        {
        }

        public TrackViewer(IEnumerable<string> listValues, int barCountAtStart)
        {
            InitializeComponent();
            metaRiffPackNameListBox.Items.Add("");
            foreach (string value in listValues)
                metaRiffPackNameListBox.Items.Add(value);

            metaRiffPackNameListBox.SelectedItem = "";
            for (int index = 0; index < barCountAtStart; index++)
                this[index].IsChecked = false;
        }
        #endregion

        #region Public Methods
        public void RefreshControls(PredefinedGeneratorTrack track, int barCount)
        {
            metaRiffPackNameListBox.SelectedItem = track.MetaRiffPackName;
            for (int index = 0; index<barCount;index++)
            {
                this[index].IsChecked = track[index];
            }

            if (internalList.Count > barCount)
            {
                int currentIndex = 0;
                foreach (CheckBox checkBox in new List<CheckBox>(this))
                {
                    if (currentIndex >= barCount)
                        Remove(checkBox);
                    currentIndex++;
                }
            }
        }
        #endregion

        #region Properties
        public CheckBox this[int index]
        {
            get
            {
                while (internalList.Count - 1 < index)
                {
                    CheckBox checkBox = new CheckBox();
                    checkBox.Width = 48;
                    checkBox.Tag = internalList.Count;

                    checkBox.Click += new RoutedEventHandler(CheckBoxClickHandler);

                    internalList.Add(checkBox);
                    stackPanelTrack.Children.Add(checkBox);
                }
                return internalList[index];
            }
        }
        #endregion

        #region Handlers
        private void CheckBoxClickHandler(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;

            e.Source = checkBox;

            if (OnClickCheckBox != null)
                OnClickCheckBox(this, e);
        }

        private void MetaRiffPackNameChangedHandler(object sender, RoutedEventArgs e)
        {
            if (OnMetaRiffPackNameChanged != null)
            {
                ComboBox comboBox = (ComboBox)sender;
                e.Source = comboBox;
                OnMetaRiffPackNameChanged(this, e);
            }
        }
        #endregion

        #region Collection Members
        private void Remove(CheckBox checkBox)
        {
            internalList.Remove(checkBox);
            stackPanelTrack.Children.Remove(checkBox);
        }

        public IEnumerator<CheckBox> GetEnumerator()
        {
            return internalList.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return internalList.GetEnumerator();
        }
        #endregion
    }
}
