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

using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Threading;
using BadEcho.Extensions;

namespace BadEcho.Presentation.ViewModels;

/// <summary>
/// Provides a base view model that automates communication between a view and a collection of bound data.
/// </summary>
/// <typeparam name="TModel">
/// The type of data bound to the view model as part of a collection for display on a view.
/// </typeparam>
/// <typeparam name="TChildViewModel">
/// The type of view model that this collection view model generates for its children.
/// </typeparam>
public abstract class CollectionViewModel<TModel, TChildViewModel> : ViewModel<TModel>, ICollectionViewModel<TModel,TChildViewModel>
    where TChildViewModel : class, IViewModel, IModelProvider<TModel>
{
    private readonly CollectionViewModelEngine<TModel, TChildViewModel> _engine;

    /// <summary>
    /// Initializes a new instance of the <see cref="CollectionViewModel{TModel,TChildViewModel}"/> class.
    /// </summary>
    /// <param name="options">
    /// A <see cref="CollectionViewModelOptions"/> instance that configures the behavior of this engine.
    /// </param>
    protected CollectionViewModel(CollectionViewModelOptions options)
        : this(options, new UnsortedAddStrategy<TChildViewModel>())
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="CollectionViewModel{TModel,TChildViewModel}"/> class.
    /// </summary>
    /// <param name="options">
    /// A <see cref="CollectionViewModelOptions"/> instance that configures the behavior of this view model's internal engine.
    /// </param>
    /// <param name="changeStrategy">
    /// The strategy behind how view models will be added to and removed from the view model's collection.
    /// </param>
    protected CollectionViewModel(CollectionViewModelOptions options, ICollectionChangeStrategy<TChildViewModel> changeStrategy)
    {
        Require.NotNull(options, nameof(options));

        options.CollectionChangedHandler = HandleCollectionChanged;
        options.ItemChangedHandler = HandleItemChanged;
        
        _engine = new CollectionViewModelEngine<TModel, TChildViewModel>(this, changeStrategy, options);
    }

    /// <inheritdoc/>
    public AtomicObservableCollection<TChildViewModel> Items
        => _engine.Items;

    /// <inheritdoc/>
    public bool HasItems
    {
        get;
        set => NotifyIfChanged(ref field, value);
    }

    /// <inheritdoc/>
    public void Bind(TChildViewModel viewModel) 
        => _engine.Bind(viewModel);

    /// <inheritdoc/>
    public bool Unbind(TChildViewModel viewModel) 
        => _engine.Unbind(viewModel);

    /// <inheritdoc/>
    public void ChangeDispatcher(Dispatcher dispatcher)
        => _engine.ChangeDispatcher(dispatcher);

    /// <inheritdoc/>
    public TChildViewModelImpl? FindItem<TChildViewModelImpl>(TModel model) 
        where TChildViewModelImpl : TChildViewModel
    {
        return _engine.FindItem<TChildViewModelImpl>(model);
    }

    /// <inheritdoc/>
    public abstract TChildViewModel CreateItem(TModel model);

    /// <inheritdoc/>
    public abstract void UpdateItem(TModel model);

    /// <inheritdoc/>
    protected override void OnBinding(TModel model) 
        => _engine.Bind(model);

    /// <inheritdoc/>
    protected override void OnBatchBinding(IEnumerable<TModel> models) 
        => _engine.Bind(models);

    /// <inheritdoc/>
    protected override async Task OnBatchBindingAsync(IEnumerable<TModel> models)
        => await _engine.BindAsync(models).ConfigureAwait(false);

    /// <inheritdoc/>
    protected override void OnUnbound(TModel model)
        => _engine.Unbind(model);

    /// <inheritdoc/>
    protected override void OnBatchUnbound(IEnumerable<TModel> models) 
        => _engine.Unbind(models);

    /// <summary>
    /// Called when there is a change to the collection's composition.
    /// </summary>
    /// <param name="e">The <see cref="NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
    protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e) 
        => HasItems = !Items.IsEmpty();

    /// <summary>
    /// Called when there is a change to a property value of one of this view model's children.
    /// </summary>
    /// <param name="child">The child view model that had a change in a property value.</param>
    /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
    protected virtual void OnItemChanged(TChildViewModel? child, PropertyChangedEventArgs e)
    { }

    private void HandleCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        => OnCollectionChanged(e);

    private void HandleItemChanged(object? sender, PropertyChangedEventArgs e) 
        => OnItemChanged((TChildViewModel?) sender, e);
}