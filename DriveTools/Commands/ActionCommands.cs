using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using DriveTools.ViewModels;
using DriveTools.Views;

namespace DriveTools.Commands
{
    public static class ActionCommands
    {
        public static void OpenRemoveDuplicateWindow()
        {
            var viewModel = new RemoveDuplicateWindowViewModel();
            var window = new RemoveDuplicateWindow
            {
                DataContext = viewModel,
            };

            window.Show();
        }

        public static void OpenAboutUser(ContentControl parentElement)
        {
            var viewModel = new AboutUserViewModel();
            var control = new AboutUserView {DataContext = viewModel};

            parentElement.Content = control;
        }
    }
}
