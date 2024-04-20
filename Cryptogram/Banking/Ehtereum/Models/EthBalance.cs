using MvvmHelpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Banking.Ehtereum
{
    public class EthBalance : ObservableObject
    {
        private double confirmed;
        public double Confirmed
        {
            get => confirmed;
            set => SetProperty(ref confirmed, value);
        }
        private double total;
        public double Total
        {
            get => total;
            set => SetProperty(ref total, value);
        }
    }
}
