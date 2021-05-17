using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

namespace XGENO.UWP.Controls.DropShadowPanel
{
    [ContentProperty(Name = nameof(CastingElement))]
    public sealed partial class DropShadowPanel : UserControl
    {
        private readonly DropShadow _dropShadow;
        private readonly SpriteVisual _shadowVisual;
        private FrameworkElement _contentElement;


        public FrameworkElement CastingElement
        {
            get
            {
                return _contentElement;
            }

            set
            {
                if (_contentElement != null)
                {
                    _contentElement.SizeChanged -= CompositionShadow_SizeChanged;
                }

                _contentElement = value;
                _contentElement.SizeChanged += CompositionShadow_SizeChanged;

                ConfigureShadowVisualForCastingElement();
            }
        }

        public DropShadow DropShadow
        {
            get
            {
                return _dropShadow;
            }
        }

        public CompositionBrush Mask
        {
            get
            {
                return _dropShadow.Mask;
            }

            set
            {
                _dropShadow.Mask = value;
            }
        }



        public static readonly DependencyProperty BlurRadiusProperty = DependencyProperty.Register(nameof(BlurRadius), typeof(double), typeof(DropShadowPanel), new PropertyMetadata(9.0, OnBlurRadiusChanged));

        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register(nameof(Color), typeof(Color), typeof(DropShadowPanel), new PropertyMetadata(Colors.Black, OnColorChanged));

        public static readonly DependencyProperty OffsetXProperty = DependencyProperty.Register(nameof(OffsetX), typeof(double), typeof(DropShadowPanel), new PropertyMetadata(0.0, OnOffsetXChanged));

        public static readonly DependencyProperty OffsetYProperty = DependencyProperty.Register(nameof(OffsetY), typeof(double), typeof(DropShadowPanel), new PropertyMetadata(0.0, OnOffsetYChanged));

        public static readonly DependencyProperty OffsetZProperty = DependencyProperty.Register(nameof(OffsetZ), typeof(double), typeof(DropShadowPanel), new PropertyMetadata(0.0, OnOffsetZChanged));

        public static readonly DependencyProperty ShadowOpacityProperty = DependencyProperty.Register(nameof(ShadowOpacity), typeof(double), typeof(DropShadowPanel), new PropertyMetadata(1.0, OnShadowOpacityChanged));

        public double BlurRadius
        {
            get
            {
                return (double)GetValue(BlurRadiusProperty);
            }

            set
            {
                SetValue(BlurRadiusProperty, value);
            }
        }

        public Color Color
        {
            get
            {
                return (Color)GetValue(ColorProperty);
            }

            set
            {
                SetValue(ColorProperty, value);
            }
        }

        public double OffsetX
        {
            get
            {
                return (double)GetValue(OffsetXProperty);
            }

            set
            {
                SetValue(OffsetXProperty, value);
            }
        }

        public double OffsetY
        {
            get
            {
                return (double)GetValue(OffsetYProperty);
            }

            set
            {
                SetValue(OffsetYProperty, value);
            }
        }

        public double OffsetZ
        {
            get
            {
                return (double)GetValue(OffsetZProperty);
            }

            set
            {
                SetValue(OffsetZProperty, value);
            }
        }

        public double ShadowOpacity
        {
            get
            {
                return (double)GetValue(ShadowOpacityProperty);
            }

            set
            {
                SetValue(ShadowOpacityProperty, value);
            }
        }

        public DropShadowPanel()
        {
            InitializeComponent();
            DefaultStyleKey = typeof(CompositionShadow);

            SizeChanged += CompositionShadow_SizeChanged;
            Loaded += (object sender, RoutedEventArgs e) =>
            {
                ConfigureShadowVisualForCastingElement();
            };

            Compositor compositor = ElementCompositionPreview.GetElementVisual(this).Compositor;

            _shadowVisual = compositor.CreateSpriteVisual();

            _dropShadow = compositor.CreateDropShadow();
            _shadowVisual.Shadow = _dropShadow;

            ElementCompositionPreview.SetElementChildVisual(ShadowElement, _shadowVisual);
        }

        private void CompositionShadow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateShadowSize();
        }

        private void ConfigureShadowVisualForCastingElement()
        {
            UpdateShadowMask();

            UpdateShadowSize();
        }

        private void UpdateShadowSize()
        {
            Vector2 newSize = new Vector2(0, 0);
            if (_contentElement != null)
            {
                newSize = new Vector2((float)_contentElement.ActualWidth, (float)_contentElement.ActualHeight);
            }

            _shadowVisual.Size = newSize;
        }

        private void UpdateShadowMask()
        {
            if (_contentElement != null)
            {
                CompositionBrush mask = null;
                if (_contentElement is Image)
                {
                    mask = ((Image)_contentElement).GetAlphaMask();
                }
                else if (_contentElement is Shape)
                {
                    mask = ((Shape)_contentElement).GetAlphaMask();
                }
                else if (_contentElement is TextBlock)
                {
                    mask = ((TextBlock)_contentElement).GetAlphaMask();
                }

                _dropShadow.Mask = mask;
            }
            else
            {
                _dropShadow.Mask = null;
            }

        }

        private void OnBlurRadiusChanged(double newValue)
        {
            _dropShadow.BlurRadius = (float)newValue;
        }

        private void OnColorChanged(Color newValue)
        {
            _dropShadow.Color = newValue;
        }

        private void OnOffsetXChanged(double newValue)
        {
            UpdateShadowOffset((float)newValue, _dropShadow.Offset.Y, _dropShadow.Offset.Z);
        }

        private void OnOffsetYChanged(double newValue)
        {
            UpdateShadowOffset(_dropShadow.Offset.X, (float)newValue, _dropShadow.Offset.Z);
        }

        private void OnOffsetZChanged(double newValue)
        {
            UpdateShadowOffset(_dropShadow.Offset.X, _dropShadow.Offset.Y, (float)newValue);
        }

        private void OnShadowOpacityChanged(double newValue)
        {
            _dropShadow.Opacity = (float)newValue;
        }

        private void UpdateShadowOffset(float x, float y, float z)
        {
            _dropShadow.Offset = new Vector3(x, y, z);
        }

        private static void OnBlurRadiusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

            ((DropShadowPanel)d).OnBlurRadiusChanged((double)e.NewValue);

        }

        private static void OnColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

            ((DropShadowPanel)d).OnColorChanged((Color)e.NewValue);

        }

        private static void OnOffsetXChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

            ((DropShadowPanel)d).OnOffsetXChanged((double)e.NewValue);

        }

        private static void OnOffsetYChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

            ((DropShadowPanel)d).OnOffsetYChanged((double)e.NewValue);

        }

        private static void OnOffsetZChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

            ((DropShadowPanel)d).OnOffsetZChanged((double)e.NewValue);

        }

        private static void OnShadowOpacityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

            ((DropShadowPanel)d).OnShadowOpacityChanged((double)e.NewValue);

        }

    }
}
