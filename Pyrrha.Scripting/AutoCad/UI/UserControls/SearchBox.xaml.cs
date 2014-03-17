#region Referencing

using System.Linq;
using System.Windows.Input;
using System.Windows.Media;

#endregion

namespace Pyrrha.Scripting.AutoCad.UI.UserControls
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class SearchBox
    {
        public string Text
        {
            get { return this.TextBox.Text; }
            set { this.TextBox.Text = value; }
        }

        public ICommand Command
        {
            get { return this.Button.Command; }
            set { this.Button.Command = value; }
        }

        public object CommandParameter
        {
            get { return this.Button.CommandParameter; }
            set { this.Button.CommandParameter = value; }
        }

        public Brush BorderBrush
        {
            set
            {
                this.Button.BorderBrush = value;
                this.TextBox.BorderBrush = value;
            }
        }

        public SearchBox()
        {
            this.InitializeComponent();
        }
    }
}
