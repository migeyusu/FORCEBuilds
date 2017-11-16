using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interactivity;
using System.Windows.Media;

namespace FORCEBuild.UI.WPF
{
    public class WaterMarkBehavior:Behavior<TextBox>
    {
        public string Mark
        {
            get { return (string)GetValue(MarkProperty); }
            set { SetValue(MarkProperty, value); }
        }

        public static readonly DependencyProperty MarkProperty =
            DependencyProperty.Register("Mark", typeof(string), typeof(WaterMarkBehavior), 
                new FrameworkPropertyMetadata("", MarkChangedCallback) {
                    BindsTwoWayByDefault = true,
                    DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                });

        private static void MarkChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var item = dependencyPropertyChangedEventArgs.NewValue.ToString();
            var textbox = dependencyObject as TextBox;
            if (textbox!=null)
            {
                CreateMark(textbox,item);
            }
        }


        public WaterMarkBehavior()
        {
            
        }

        private static void CreateMark(TextBox attachTextBox,string mark)
        {
            if (string.IsNullOrEmpty(attachTextBox.Text))
            {
                var visualBrush = new VisualBrush
                {
                    Visual = new TextBlock {
                        FontSize = attachTextBox.FontSize,
                        Text = mark,
                        FontStyle = FontStyles.Italic
                    },
                    TileMode = TileMode.None,
                    Stretch = Stretch.None,
                    AlignmentX = AlignmentX.Left,
                    Opacity = 0.6,
                };
                attachTextBox.Background = visualBrush;
            }
        }

        private Brush _backBrush;

        protected override void OnAttached()
        {
            base.OnAttached();
            _backBrush = AssociatedObject.Background;
            CreateMark(this.AssociatedObject,Mark);
            this.AssociatedObject.TextChanged += AssociatedObject_TextChanged;
        }

        private void AssociatedObject_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textbox = sender as TextBox;
            if (string.IsNullOrEmpty(textbox.Text))
                CreateMark(textbox, Mark);
            else
                AssociatedObject.Background = _backBrush;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            if (this.AssociatedObject!=null)
            {
                this.AssociatedObject.TextChanged -= AssociatedObject_TextChanged;
            }
        }


    }
}