namespace LokalizacjaWSklepie.Extensions
{
    public class BoxViewExtensions : Frame
    {
        public static readonly BindableProperty IdProperty =
            BindableProperty.CreateAttached("Id", typeof(int), typeof(BoxViewExtensions), 0);

        public static int GetId(BindableObject view)
        {
            return (int)view.GetValue(IdProperty);
        }

        public static void SetId(BindableObject view, int value)
        {
            view.SetValue(IdProperty, value);
        }
    }
}