using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VTFversionChanger.Models.Abstract;

namespace VTFversionChanger.Models
{
    /// <summary>
    /// Databinding for the progress bar
    /// </summary>
    internal class ProgressBar : DoubleWrapper
    {

        public double CurrentProgress
        {
            get => val;
            set
            {
                val = value;
                NotifyPropertyChanged();
            }
        }

        #region Constructor

        public ProgressBar() : base() { }

        #endregion

        #region INotifyPropertyChanged

        public override event PropertyChangedEventHandler PropertyChanged;

        public override void NotifyPropertyChanged()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CurrentProgress"));
        }

        #endregion

    }

}
