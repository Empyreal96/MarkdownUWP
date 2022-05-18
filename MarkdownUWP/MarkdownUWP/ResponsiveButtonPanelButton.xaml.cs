using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation.Peers;
using Windows.UI.Xaml.Automation.Provider;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace ResponsiveButtonPanel
{
    public sealed partial class ResponsiveButtonPanelButton : UserControl, IResponsiveButtonPanelElement
    {
        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register("Command",
                    typeof(ICommand),
                    typeof(ResponsiveButtonPanelButton),
                    new PropertyMetadata(null));

        public static readonly DependencyProperty GlyphProperty = DependencyProperty.Register("Glyph",
            typeof(string),
            typeof(ResponsiveButtonPanelButton),
            new PropertyMetadata(null));

        public static readonly DependencyProperty GlyphFontSizeProperty = DependencyProperty.Register("GlyphFontSize",
            typeof(Double),
            typeof(ResponsiveButtonPanelButton),
            new PropertyMetadata(18.0));

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text",
            typeof(string),
            typeof(ResponsiveButtonPanelButton),
            new PropertyMetadata(null));

        public static readonly DependencyProperty TextFontSizeProperty = DependencyProperty.Register("TextFontSize",
            typeof(Double),
            typeof(ResponsiveButtonPanelButton),
            new PropertyMetadata(14.0));

        public static readonly DependencyProperty PinnedProperty = DependencyProperty.Register("Pinned",
            typeof(Boolean),
            typeof(ResponsiveButtonPanelButton),
            new PropertyMetadata(false));

        public event RoutedEventHandler Click;

        public ResponsiveButtonPanelButton()
        {
            this.InitializeComponent();

            ThisButton.Click += Button_Click;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var eventHandler = this.Click;

            if (eventHandler != null)
                eventHandler(this, e);
        }

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public string Glyph
        {
            get { return (string)GetValue(GlyphProperty); }
            set { SetValue(GlyphProperty, value); }
        }

        public Double GlyphFontSize
        {
            get { return (Double)GetValue(GlyphFontSizeProperty); }
            set { SetValue(GlyphFontSizeProperty, value); }
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public Double TextFontSize
        {
            get { return (Double)GetValue(TextFontSizeProperty); }
            set { SetValue(TextFontSizeProperty, value); }
        }

        public bool Pinned
        {
            get { return (bool)GetValue(PinnedProperty); }
            set { SetValue(PinnedProperty, value); }
        }

        private bool isCompact = true;
        public bool IsCompact
        {
            get
            {
                return isCompact;
            }

            set
            {
                isCompact = value;

                Caption.Visibility = isCompact ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public void RaiseClick()
        {
            if(Command != null)
            {
                Command.Execute(null);
            }
            else
            {
                ButtonAutomationPeer peer = new ButtonAutomationPeer(ThisButton);

                IInvokeProvider invokeProv =
                  peer.GetPattern(PatternInterface.Invoke)
                  as IInvokeProvider;

                invokeProv.Invoke();
            }

        }
    }
}
