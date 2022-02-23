using System.Windows;
using System.Windows.Media;

namespace PD.Models
{  
    public class UI_DependencyProperty : DependencyObject
    {
        #region Image dependency property

        /// <summary>
        /// An attached dependency property which provides an
        /// <see cref="ImageSource" /> for arbitrary WPF elements.
        /// </summary>
        public static readonly DependencyProperty MouseOverImageProperty;
        public static readonly DependencyProperty OriginalImageProperty;
        public static readonly DependencyProperty ImageProperty;

        /// <summary>
        /// Gets the <see cref="ImageProperty"/> for a given
        /// <see cref="DependencyObject"/>, which provides an
        /// <see cref="ImageSource" /> for arbitrary WPF elements.
        /// </summary>
        public static ImageSource GetImage(DependencyObject obj)
        {
            return (ImageSource)obj.GetValue(ImageProperty);
        }

        /// <summary>
        /// Gets the attached <see cref="ImageProperty"/> for a given
        /// <see cref="DependencyObject"/>, which provides an
        /// <see cref="ImageSource" /> for arbitrary WPF elements.
        /// </summary>
        public static void SetImage(DependencyObject obj, ImageSource value)
        {
            obj.SetValue(ImageProperty, value);
        }

        #endregion

        static UI_DependencyProperty()
        {            
            
            //register attached dependency property 註冊一個新的相依屬依以供UI聯結
            var metadata = new FrameworkPropertyMetadata((ImageSource)null);
            ImageProperty = DependencyProperty.RegisterAttached("Image",
                                                                typeof(ImageSource),
                                                                typeof(UI_DependencyProperty), metadata);

            MouseOverImageProperty = DependencyProperty.Register("MouseOverImage", typeof(ImageSource), typeof(UI_DependencyProperty), new UIPropertyMetadata(null));
            OriginalImageProperty = DependencyProperty.Register("OriginalImage", typeof(ImageSource), typeof(UI_DependencyProperty), new UIPropertyMetadata(null));

        }

        public ImageSource OriginalImage
        {
            get { return (ImageSource)GetValue(OriginalImageProperty); }
            set { SetValue(OriginalImageProperty, value); }
        }
    }
}
