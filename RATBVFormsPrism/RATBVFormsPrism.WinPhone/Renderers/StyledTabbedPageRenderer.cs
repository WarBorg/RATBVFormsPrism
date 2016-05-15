//using RATBVFormsPrism.WinPhone.Renderers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Markup;
using Xamarin.Forms;
using Xamarin.Forms.Platform.WinRT;

//[assembly: ExportRenderer(typeof(TabbedPage), typeof(StyledTabbedPageRenderer))]
//namespace RATBVFormsPrism.WinPhone.Renderers
//{
//    public class StyledTabbedPageRenderer : TabbedPageRenderer
//    {
//        protected override void OnElementChanged(VisualElementChangedEventArgs e)
//        {
//            base.OnElementChanged(e);

//            Element.
//            this.HeaderTemplate = GetStyledHeaderTemplate();
//        }

//        private System.Windows.DataTemplate GetStyledHeaderTemplate()
//        {
//            string dataTemplateXaml =
//                @"<DataTemplate
//            xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
//            xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
//                <TextBlock 
//                    Text=""{Binding Title}"" 
//                    FontSize=""60"" />
//            </DataTemplate>";

//            return (System.Windows.DataTemplate)XamlReader.Load(dataTemplateXaml);
//        }
//    }
//}
