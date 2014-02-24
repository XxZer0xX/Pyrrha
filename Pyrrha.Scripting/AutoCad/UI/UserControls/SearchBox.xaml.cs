using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Pyrrha.Scripting.AutoCad.UI.UserControls
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class SearchBox
    {
        public string Text
        {
            get { return TextBox.Text; }
            set { TextBox.Text = value; }
        }

        public ICommand Command
        {
            get { return Button.Command; }
            set { Button.Command = value; }
        }

        public object CommandParameter
        {
            get { return Button.CommandParameter; }
            set { Button.CommandParameter = value; }
        }

        public Brush BorderBrush
        {
            set
            {
                Button.BorderBrush = value;
                TextBox.BorderBrush = value;
            }
        }

        public SearchBox()
        {
            InitializeComponent();
        }
    }
}
