using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TourPlanner.UI.Views
{
    public partial class TourListView : UserControl
    {
        public TourListView()
        {
            InitializeComponent();

            // Simple focus helper for Ctrl+F
            Loaded += (_, __) =>
            {
                if (DataContext is not null)
                {
                    CommandBindings.Add(new CommandBinding(
                        ApplicationCommands.Find,
                        (_, __) => SearchBox.Focus(),
                        (_, __) => { } // always can execute
                    ));
                }
            };
        }
    }
}