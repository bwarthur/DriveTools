using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace DriveTools.Commands
{
    public static class OpenCommands
    {

        public static byte[] OpenJSONCert()
        {
            return OpenCert("JSON (*.json)|*.json");
        }

        public static byte[] OpenP12Cert()
        {
            return OpenCert("P12 (*.p12)|*.p12");
        }

        private static byte[] OpenCert(string fileExtensionFilter)
        {
            var stream = new MemoryStream();
            var openFileDialog = new OpenFileDialog
            {
                InitialDirectory = @"C:\",
                Filter = fileExtensionFilter + @"|All Files (*.*)|*.*",
                FilterIndex = 1,
                RestoreDirectory = true
            };

            if (openFileDialog.ShowDialog() != DialogResult.OK) return null;
            try
            {
                openFileDialog.OpenFile().CopyTo(stream);
                return stream.ToArray();
            }
            catch (Exception ex)
            {
                EnhancedLogging.Log.Error(ex);
                return null;
            }
        }
    }
}
