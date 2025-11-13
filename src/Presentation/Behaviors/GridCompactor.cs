// -----------------------------------------------------------------------
// <copyright>
//      Created by Matt Weber <matt@badecho.com>
//      Copyright @ 2025 Bad Echo LLC. All rights reserved.
//
//      Bad Echo Technologies are licensed under the
//      GNU Affero General Public License v3.0.
//
//      See accompanying file LICENSE.md or a copy at:
//      https://www.gnu.org/licenses/agpl-3.0.html
// </copyright>
// -----------------------------------------------------------------------

using System.Windows;
using System.Windows.Controls;

namespace BadEcho.Presentation.Behaviors;

/// <summary>
/// Provides an attachable grid layout compactor, which will make the layout of a grid responsive to its
/// parent container.
/// </summary>
public sealed class GridCompactor : AttachableComponent<Grid>
{
    /// <summary>
    /// Identifies the <see cref="SizeToCompact"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty SizeToCompactProperty =
        DependencyProperty.Register(nameof(SizeToCompact),
                                    typeof(double),
                                    typeof(GridCompactor),
                                    new PropertyMetadata(OnSettingsChanged));
    /// <summary>
    /// Identifies the <see cref="Range"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty RangeProperty =
        DependencyProperty.Register(nameof(Range),
                                    typeof(int),
                                    typeof(GridCompactor),
                                    new PropertyMetadata(OnSettingsChanged));
    private bool _initialized;
    private int _childrenCount;
    
    /// <summary>
    /// Gets or sets the size at which compaction occurs.
    /// </summary>
    /// <remarks>
    /// The specific dimension which this size pertains to depends on the configured <see cref="Orientation"/>.
    /// </remarks>
    public double SizeToCompact
    {
        get => (double) GetValue(SizeToCompactProperty);
        set => SetValue(SizeToCompactProperty, value);
    }

    /// <summary>
    /// Gets the number of rows and columns items can be compacted into.
    /// </summary>
    public int Range
    {
        get => (int) GetValue(RangeProperty);
        set => SetValue(RangeProperty, value);
    }

    /// <inheritdoc/>
    protected override void OnAttached()
    {
        base.OnAttached();

        if (TargetObject == null)
            return;
        
        if (!TargetObject.IsLoaded)
            TargetObject.Loaded += HandleTargetLoaded;

        TargetObject.LayoutUpdated += HandleTargetLayoutUpdated;
        TargetObject.SizeChanged += HandleTargetSizeChanged;
    }

    /// <inheritdoc/>
    protected override void OnDetaching()
    {
        base.OnDetaching();

        _childrenCount = 0;
        _initialized = false;

        if (TargetObject == null)
            return;

        TargetObject.LayoutUpdated -= HandleTargetLayoutUpdated;
        TargetObject.Loaded -= HandleTargetLoaded;
        TargetObject.SizeChanged -= HandleTargetSizeChanged;
    }

    /// <inheritdoc/>
    protected override Freezable CreateInstanceCore()
        => new GridCompactor();

    private static void OnSettingsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var compactor = (GridCompactor)d;

        compactor.EvaluateSize();
    }

    private void ChangeCompaction(bool compact)
    {
        if (TargetObject == null)
            return;

        var sizes = new SizeDefinitionCollection();

        SizeDefinitionCollection? rowSizes = null;
        SizeDefinitionCollection? columnSizes = null;

        if (compact)
            rowSizes = sizes;
        else
            columnSizes = sizes;

        for (int i = 0; i < Range; i++)
        {
            sizes.Add(new SizeDefinition
                      {
                          Size = new GridLength(1.0, i + 1 < Range ? GridUnitType.Auto : GridUnitType.Star)
                      });
        }

        GridBehaviors.SetRowDefinitions(TargetObject, rowSizes);
        GridBehaviors.SetColumnDefinitions(TargetObject, columnSizes);

        ElementLayout.SetIsCompact(TargetObject, compact);
        _initialized = true;
    }

    private void EvaluateSize()
    {
        if (TargetObject is not { IsLoaded: true })
            return;

        if (ReadLocalValue(RangeProperty) == DependencyProperty.UnsetValue)
            return;

        double actualSize = TargetObject.ActualWidth;

        bool isCompacted = ElementLayout.GetIsCompact(TargetObject);

        if (actualSize <= SizeToCompact * Range && (!isCompacted || !_initialized))
            ChangeCompaction(true);
        else if (actualSize > SizeToCompact * Range && (isCompacted || !_initialized))
            ChangeCompaction(false);
    }

    private void HandleTargetLoaded(object sender, RoutedEventArgs e) 
        => EvaluateSize();

    private void HandleTargetSizeChanged(object sender, SizeChangedEventArgs e) 
        => EvaluateSize();

    private void HandleTargetLayoutUpdated(object? sender, EventArgs e)
    {
        if (TargetObject == null)
            return;

        // This is the closest thing we have to an event notifying us there was a change in the grid's collection of children.
        // This will fire for many other reasons, however, so we will set the following attached properties only when necessary.
        if (_childrenCount == TargetObject.Children.Count)
            return;

        _childrenCount = TargetObject.Children.Count;

        for (int i = 0; i < TargetObject.Children.Count; i++)
        {
            Grid.SetRow(TargetObject.Children[i], i);
            Grid.SetColumn(TargetObject.Children[i], i);
        }
    }
}
