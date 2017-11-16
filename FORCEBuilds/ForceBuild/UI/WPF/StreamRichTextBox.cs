using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;

namespace FORCEBuild.UI.WPF
{
    public class StreamRichTextBox:RichTextBox
    {

        public Stream ContentStream {
            get { return (Stream) GetValue(ContentStreamProperty); }
            set { SetValue(ContentStreamProperty, value); }
        }

        public static readonly DependencyProperty ContentStreamProperty =
            DependencyProperty.Register(nameof(ContentStream), typeof(Stream),
                typeof(StreamRichTextBox), 
                new FrameworkPropertyMetadata(ContenStreamPropertyChanged) {
                    BindsTwoWayByDefault = true,
                    DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                });

        private static void ContenStreamPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var textbox = dependencyObject as RichTextBox;
            if (textbox != null)
            {
                if (dependencyPropertyChangedEventArgs.NewValue == null)
                {
                    textbox.Document.Blocks.Clear();
                }
                else
                {
                    var input= (Stream)dependencyPropertyChangedEventArgs.NewValue;
                    if (input.Length == 0)
                        return;
                    input.Seek(0, SeekOrigin.Begin);
                    new TextRange(textbox.Document.ContentStart, textbox.Document.ContentEnd).Load(input, DataFormats.XamlPackage);
                }
                
            }
        }
    }
}