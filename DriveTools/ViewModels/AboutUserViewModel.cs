using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using DriveTools.Model;
using FirstFloor.ModernUI.Presentation;

namespace DriveTools.ViewModels
{
    public class AboutUserViewModel
    {
        private RelayCommand _refreshCommand;

        public AboutUserViewModel()
        {
            Refresh(null);
        }

        public string Username { get; set; }
        public long? TotalQuota { get; set; }
        public long? QuotaBytesUsed { get; set; }

        public string TotalQuotaString
        {
            get { return String.Format("{0:n0}", TotalQuota); }
        }

        public string QuotaBytesUsedString
        {
            get { return String.Format("{0:n0}", QuotaBytesUsed); }
        }

        public string RemainingQuota
        {
            get
            {
                var remaining = TotalQuota - QuotaBytesUsed;
                var percentage = 100 * (1 - ((float) QuotaBytesUsed.Value/(float) TotalQuota.Value));

                return String.Format("{0:n0}", remaining) + @" Bytes - ( " + percentage + "% )";
            }
        }

        public ICommand RefreshCommand
        {
            get { return _refreshCommand ?? (_refreshCommand = new RelayCommand(Refresh)); }
        }

        private void Refresh(object args)
        {
            var model = new GoogleDriveModel(MainWindow.Service);
            var about = model.GetAboutUser();

            Username = about.Name;
            TotalQuota = about.QuotaBytesTotal;
            QuotaBytesUsed = about.QuotaBytesUsed;
        }

    }
}
