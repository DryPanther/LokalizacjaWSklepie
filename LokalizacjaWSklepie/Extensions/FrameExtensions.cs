using LokalizacjaWSklepie.Models;

namespace LokalizacjaWSklepie.Extensions
{
    public class FrameExtensions : Frame
    {
        public static readonly BindableProperty IdProperty =
            BindableProperty.CreateAttached("Id", typeof(int), typeof(FrameExtensions), 0);

        public static readonly BindableProperty ProductListProperty =
            BindableProperty.CreateAttached("ProductList", typeof(List<Product>), typeof(FrameExtensions), null);

        public static int GetId(BindableObject view)
        {
            return (int)view.GetValue(IdProperty);
        }

        public static void SetId(BindableObject view, int value)
        {
            view.SetValue(IdProperty, value);
        }

        public static List<Product> GetProductList(BindableObject view)
        {
            return (List<Product>)view.GetValue(ProductListProperty);
        }

        public static void SetProductList(BindableObject view, List<Product> value)
        {
            view.SetValue(ProductListProperty, value);
        }
    }
}