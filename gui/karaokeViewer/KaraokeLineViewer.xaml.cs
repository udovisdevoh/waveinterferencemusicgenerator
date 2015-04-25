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
using System.Windows.Threading;

namespace WaveBuilder
{
    /// <summary>
    /// Interaction logic for KaraokeLineViewer.xaml
    /// </summary>
    public partial class KaraokeLineViewer : UserControl
    {
        #region Private Fields
        /// <summary>
        /// Source text
        /// </summary>
        private string sourceLine;
        #endregion

        #region Constructor
        /// <summary>
        /// Build karaoke line viewer from text line
        /// </summary>
        /// <param name="sourceLine">text source line</param>
        public KaraokeLineViewer(string sourceLine)
        {
            InitializeComponent();
            this.sourceLine = sourceLine;
            textBlock.Text = sourceLine;
        }
        #endregion

        #region Protected Method
        /// <summary>
        /// Adjust blue bar's width
        /// </summary>
        /// <param name="noteCounter">note counter</param>
        internal void AdjustBlueBar(int noteCounter)
        {
            noteCounter++;
            while (noteCounter > 8)
                noteCounter -= 8;

            int desiredLength = (sourceLine.Length * noteCounter) / 8;

            string blueLine = sourceLine.Substring(0,desiredLength);

            Action action = new Action(delegate() {textBlockBlue.Text = blueLine; });

            Dispatcher.Invoke(DispatcherPriority.Normal, action);
        }
        #endregion
    }
}
