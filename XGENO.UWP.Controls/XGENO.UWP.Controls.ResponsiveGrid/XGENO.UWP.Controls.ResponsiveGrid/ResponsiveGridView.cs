using System;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


namespace XGENO.UWP.Controls.ResponsiveGrid
{
    public sealed class ResponsiveGridView : GridView
    {
        public double DesiredHeight
        {
            get
            {
                return (double)GetValue(ResponsiveGridView.DesiredHeightProperty);
            }
            set
            {
                SetValue(ResponsiveGridView.DesiredHeightProperty, value);
            }
        }

        public static readonly DependencyProperty DesiredHeightProperty = DependencyProperty.Register("DesiredHeight", typeof(double), typeof(ResponsiveGridView),
                                        new PropertyMetadata(1.0, (s, a) =>
                                        {
                                            if (!double.IsNaN((double)a.NewValue))
                                            {
                                                ((ResponsiveGridView)s).InvalidateMeasure();
                                            }
                                        })
                                    );

        public double DesiredWidth
        {
            get
            {
                return (double)GetValue(ResponsiveGridView.DesiredWidthProperty);
            }
            set
            {
                SetValue(ResponsiveGridView.DesiredWidthProperty, value);
            }
        }

        public static readonly DependencyProperty DesiredWidthProperty = DependencyProperty.Register("DesiredWidth", typeof(double), typeof(ResponsiveGridView),
                                        new PropertyMetadata(1.0, (s, a) =>
                                        {
                                            if (!Double.IsNaN((double)a.NewValue))
                                            {
                                                ((ResponsiveGridView)s).InvalidateMeasure();
                                            }
                                        })
                                    );

        public double FixedBottomBarHeight
        {
            get
            {
                return (double)GetValue(ResponsiveGridView.FixedBottomBarHeightProperty);
            }
            set
            {
                SetValue(ResponsiveGridView.FixedBottomBarHeightProperty, value);
            }
        }

        public static readonly DependencyProperty FixedBottomBarHeightProperty = DependencyProperty.Register("FixedBottomBarHeight", typeof(double), typeof(ResponsiveGridView),
                                        new PropertyMetadata(0.0, (s, a) =>
                                        {
                                            if (!Double.IsNaN((double)a.NewValue))
                                            {
                                                ((ResponsiveGridView)s).InvalidateMeasure();
                                            }
                                        })
                                    );


        public ResponsiveGridView()
        {
            if (this.ItemContainerStyle == null)
            {
                this.ItemContainerStyle = new Style(typeof(GridViewItem));
            }

            this.ItemContainerStyle.Setters.Add(new Setter(GridViewItem.HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch));

            this.Loaded += (s, a) =>
            {
                if (this.ItemsPanelRoot != null)
                {
                    this.InvalidateMeasure();
                }
            };
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var panel = this.ItemsPanelRoot as ItemsWrapGrid;

            if (panel != null)
            {
                if (DesiredWidth == 0 || DesiredHeight == 0)
                {
                    throw new ArgumentException("You need to set MinItemHeight and MinItemWidth to a value greater than 0");
                }

                var availableWidth = finalSize.Width - (Padding.Right + Padding.Left) - 0.1;

                var numColumns = Math.Floor(availableWidth / DesiredWidth);
                numColumns = numColumns == 0 ? 1 : numColumns;

                //Not used yet (for horizontal scrolling scenarios)
                //var numRows = Math.Ceiling(this.Items.Count / numColumns);

                var itemWidth = availableWidth / numColumns;
                var aspectRatio = DesiredHeight / DesiredWidth;
                var itemHeight = itemWidth * aspectRatio;

                panel.ItemWidth = itemWidth;
                panel.ItemHeight = itemHeight + FixedBottomBarHeight;
            }

            return base.ArrangeOverride(finalSize);
        }

    }
}
