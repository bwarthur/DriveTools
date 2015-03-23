using System.IO;
using System.Text;
using System.Windows.Controls;

namespace DriveTools
{
    public class TextBoxStreamWriter : TextWriter
    {
        private TextBox _output = null;

        public TextBoxStreamWriter(TextBox output)
        {
            _output = output;
        }

        public override void Write(char value)
        {
            base.Write(value);
            _output.AppendText(value.ToString());

            if (_output.Parent.GetType() == typeof (ScrollViewer))
            {
                var scrollViewer = (ScrollViewer)_output.Parent;
                scrollViewer.ScrollToBottom();
            }
        }

        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }
    }
}
