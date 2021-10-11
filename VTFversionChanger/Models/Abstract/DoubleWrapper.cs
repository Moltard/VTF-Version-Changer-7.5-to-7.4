using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VTFversionChanger.Models.Abstract
{
    /// <summary>
    /// Wrapper for Double that update the GUI
    /// </summary>
    public abstract class DoubleWrapper : INotifyPropertyChanged
    {
        #region Attributes

        protected double val;

        #endregion

        #region Constructor

        public DoubleWrapper()
        {
            Reset();
        }

        #endregion

        #region Methods

        public void Reset()
        {
            val = 0;
            NotifyPropertyChanged();
        }

        public void Increment()
        {
            val += 1;
            NotifyPropertyChanged();
        }

        public void Decrement()
        {
            val -= 1;
            NotifyPropertyChanged();
        }

        public void Add(int i)
        {
            val += i;
            NotifyPropertyChanged();
        }

        public void Subtract(int i)
        {
            val -= i;
            NotifyPropertyChanged();
        }

        #endregion

        #region INotifyPropertyChanged

        public abstract event PropertyChangedEventHandler PropertyChanged;

        public abstract void NotifyPropertyChanged();

        #endregion

    }

}
