using Xamarin.Forms;

namespace RATBVFormsPrism.Views
{
    public partial class BusTimetables : TabbedPage
    {
        public BusTimetables()
        {
            InitializeComponent();
        }

        private void BusTime_OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null)
            {
                return;
            }

            // De-select the row
            ((ListView)sender).SelectedItem = null;
        }
    }
}
