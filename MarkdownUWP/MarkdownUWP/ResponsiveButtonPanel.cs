using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation.Peers;
using Windows.UI.Xaml.Automation.Provider;
using Windows.UI.Xaml.Controls;

namespace ResponsiveButtonPanel
{
    public interface IResponsiveButtonPanelElement
    {
        string Text { get; }
        bool Pinned { get; }
        void RaiseClick();
    }

    public sealed class ResponsiveButtonPanel : Control
    {
        public ResponsiveButtonPanel()
        {
            this.DefaultStyleKey = typeof(ResponsiveButtonPanel);

            Elements = new ObservableCollection<IResponsiveButtonPanelElement>();
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            moreButton = GetTemplateChild("MoreButton") as Button;
            moreButton.Click += MoreButton_Click;

            commandsPanel = GetTemplateChild("CommandsPanel") as ItemsControl;
            commandsPanel.ItemsSource = Elements;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            var size = base.MeasureOverride(availableSize);

            // Determine desired width of each element

            var desiredWidthList = new List<double>();

            foreach (var element in Elements)
            {
                var uiElement = element as UIElement;
                var oldVisibility = uiElement.Visibility;
                uiElement.Visibility = Visibility.Visible;
                uiElement.Measure(new Size(9999, 9999));
                desiredWidthList.Add(uiElement.DesiredSize.Width);
            }

            // Calculate total desired space

            var totalRequestedWidth = 0.0;
            foreach (var desiredWidth in desiredWidthList)
                totalRequestedWidth += desiredWidth;

            // If there is not enough space, make the more button visible

            if (moreButton != null)
            {
                if (totalRequestedWidth > availableSize.Width)
                {
                    moreButton.Visibility = Visibility.Visible;
                    moreButton.Measure(new Size(9999, 9999));
                    totalRequestedWidth += moreButton.DesiredSize.Width;
                }
                else
                {
                    moreButton.Visibility = Visibility.Collapsed;
                }
            }

            // Make items that are not marked pinned invisible
            // until the remaining items fit.
            // Make items that fit or which are pinned visible

            for (int i = Elements.Count - 1; i >= 0; i--)
            {
                var element = Elements[i];
                var uiElement = element as UIElement;

                if(totalRequestedWidth > availableSize.Width && !element.Pinned)
                {
                    uiElement.Visibility = Visibility.Collapsed;

                    totalRequestedWidth -= desiredWidthList[i];
                }
                else
                {
                    uiElement.Visibility = Visibility.Visible;
                }
            }

            size.Width = totalRequestedWidth;

            return size;
        }
        
        private void MoreButton_Click(object sender, RoutedEventArgs e)
        {
            var flyout = new MenuFlyout();

            foreach(var element in Elements)
            {
                var uiElement = element as UIElement;
                var responsiveElement = element as IResponsiveButtonPanelElement;

                if(uiElement.Visibility == Visibility.Collapsed)
                {
                    MenuFlyoutItem item = new MenuFlyoutItem();
                    item.Text = responsiveElement.Text;
                    item.Tag = responsiveElement;
                    item.Click += (s, a) =>
                    {
                        var panelElement = item.Tag as IResponsiveButtonPanelElement;

                        if (panelElement != null)
                            panelElement.RaiseClick();
                    };

                    flyout.Items.Add(item);
                }
            }

            if (flyout.Items.Count > 0)
                flyout.ShowAt(moreButton);
        }

        public ObservableCollection<IResponsiveButtonPanelElement> Elements
        {
            get { return (ObservableCollection<IResponsiveButtonPanelElement>)GetValue(ElementsProperty); }
            set { SetValue(ElementsProperty, value); }
        }

        public static readonly DependencyProperty ElementsProperty =
            DependencyProperty.Register("Elements",
                typeof(ObservableCollection<IResponsiveButtonPanelElement>),
                typeof(ResponsiveButtonPanel),
                new PropertyMetadata(null));

        public Style MoreButtonStyle
        {
            get { return (Style)GetValue(MoreButtonStyleProperty); }
            set { SetValue(MoreButtonStyleProperty, value); }
        }

        public static readonly DependencyProperty MoreButtonStyleProperty =
            DependencyProperty.Register("MoreButtonStyle", typeof(Style), typeof(ResponsiveButtonPanel), new PropertyMetadata(null));

        private Button moreButton = null;
        private ItemsControl commandsPanel = null;
    }
}
