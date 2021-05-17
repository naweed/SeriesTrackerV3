using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Shapes;
using System.Numerics;

namespace XGENO.UWP.Controls.Composition
{
    [ContentProperty(Name = nameof(CastingElement))]
    public sealed partial class CompositionShadow : UserControl
    {
        private FrameworkElement _castingElement;
        private readonly DropShadow _dropShadow;
        private readonly SpriteVisual _shadowVisual;

        public static readonly DependencyProperty BlurRadiusProperty = DependencyProperty.Register(nameof(BlurRadius), typeof(double), typeof(CompositionShadow), new PropertyMetadata(9.0, OnBlurRadiusChanged));

        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register(nameof(Color), typeof(Color), typeof(CompositionShadow), new PropertyMetadata(Colors.Black, OnColorChanged));

        public static readonly DependencyProperty OffsetXProperty = DependencyProperty.Register(nameof(OffsetX), typeof(double), typeof(CompositionShadow), new PropertyMetadata(0.0, OnOffsetXChanged));

        public static readonly DependencyProperty OffsetYProperty = DependencyProperty.Register(nameof(OffsetY), typeof(double), typeof(CompositionShadow), new PropertyMetadata(0.0, OnOffsetYChanged));

        public static readonly DependencyProperty OffsetZProperty = DependencyProperty.Register(nameof(OffsetZ), typeof(double), typeof(CompositionShadow), new PropertyMetadata(0.0, OnOffsetZChanged));

        public static readonly DependencyProperty ShadowOpacityProperty = DependencyProperty.Register(nameof(ShadowOpacity), typeof(double), typeof(CompositionShadow), new PropertyMetadata(1.0, OnShadowOpacityChanged));

        public CompositionShadow()
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

            // SetElementChildVisual on the control itself ("this") would result in the shadow
            // rendering on top of the content. To avoid this, CompositionShadow contains a Border
            // (to host the shadow) and a ContentPresenter (to hose the actual content, "CastingElement").
            ElementCompositionPreview.SetElementChildVisual(ShadowElement, _shadowVisual);
        }

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

         public FrameworkElement CastingElement
        {
            get
            {
                return _castingElement;
            }

            set
            {
                if (_castingElement != null)
                {
                    _castingElement.SizeChanged -= CompositionShadow_SizeChanged;
                }

                _castingElement = value;
                _castingElement.SizeChanged += CompositionShadow_SizeChanged;

                ConfigureShadowVisualForCastingElement();
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

        private static void OnBlurRadiusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CompositionShadow)d).OnBlurRadiusChanged((double)e.NewValue);
        }

        private static void OnColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CompositionShadow)d).OnColorChanged((Color)e.NewValue);
        }

        private static void OnOffsetXChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CompositionShadow)d).OnOffsetXChanged((double)e.NewValue);
        }

        private static void OnOffsetYChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CompositionShadow)d).OnOffsetYChanged((double)e.NewValue);
        }

        private static void OnOffsetZChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CompositionShadow)d).OnOffsetZChanged((double)e.NewValue);
        }

        private static void OnShadowOpacityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CompositionShadow)d).OnShadowOpacityChanged((double)e.NewValue);
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

        private void UpdateShadowMask()
        {
            if (_castingElement != null)
            {
                CompositionBrush mask = null;
                if (_castingElement is Image)
                {
                    mask = ((Image)_castingElement).GetAlphaMask();
                }
                else if (_castingElement is Shape)
                {
                    mask = ((Shape)_castingElement).GetAlphaMask();
                }
                else if (_castingElement is TextBlock)
                {
                    mask = ((TextBlock)_castingElement).GetAlphaMask();
                }

                _dropShadow.Mask = mask;
            }
            else
            {
                _dropShadow.Mask = null;
            }
        }

        private void UpdateShadowOffset(float x, float y, float z)
        {
            _dropShadow.Offset = new Vector3(x, y, z);
        }

        private void UpdateShadowSize()
        {
            Vector2 newSize = new Vector2(0, 0);
            if (_castingElement != null)
            {
                newSize = new Vector2((float)_castingElement.ActualWidth, (float)_castingElement.ActualHeight);
            }

            _shadowVisual.Size = newSize;
        }


    }

}


