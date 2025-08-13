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

using BadEcho.Extensions;

namespace BadEcho.Presentation.ViewModels;

/// <summary>
/// Provides a base view abstraction that automates communication between a view and bound
/// <typeparamref name="T"/>-typed data.
/// </summary>
/// <typeparam name="T">The type of data bound to the view model for display on a view.</typeparam>
public abstract class ViewModel<T> : ViewModel, IViewModel<T>
{
    private readonly List<T> _boundData = [];

    /// <inheritdoc/>
    public T? ActiveModel
    { get; private set; }

    /// <inheritdoc/>
    public override void Disconnect()
    {
        List<T> boundData = [.._boundData];

        foreach (T boundDatum in boundData)
        {
            Unbind(boundDatum);

            if (boundDatum is IViewModel viewModel) 
                viewModel.Disconnect();
        }
    }

    /// <inheritdoc/>
    public bool IsBound(T model)
        => _boundData.Contains(model);

    /// <inheritdoc/>
    public void Bind(T model)
    {
        Require.NotNull(model, nameof(model));

        if (UnbindOnBind && !model.Equals<T>(ActiveModel))
            Unbind();

        OnBinding(model);

        if (!_boundData.Contains(model)) 
            _boundData.Add(model);

        ActiveModel = model;
    }
    
    /// <inheritdoc/>
    public void Bind(IEnumerable<T> models)
    {
        Require.NotNull(models, nameof(models));

        models = models.ToList();
        
        OnBatchBinding(models);

        foreach (T model in models)
        {
            if (!_boundData.Contains(model))
                _boundData.Add(model);
        }
    }

    /// <inheritdoc/>
    public async Task BindAsync(IEnumerable<T> models)
    {
        Require.NotNull(models, nameof(models));

        models = models.ToList();

        await OnBatchBindingAsync(models).ConfigureAwait(false);

        foreach (T model in models)
        {
            if (!_boundData.Contains(model))
                _boundData.Add(model);
        }
    }

    /// <inheritdoc/>
    public bool Unbind(T? model)
    {
        model ??= ActiveModel;

        if (model == null || !_boundData.Contains(model))
            return false;

        if (model.Equals<T>(ActiveModel))
            ActiveModel = default;

        _boundData.Remove(model);

        OnUnbound(model);

        return true;
    }

    /// <inheritdoc/>
    public void Unbind(IEnumerable<T> models)
    {
        Require.NotNull(models, nameof(models));

        models = models.ToList();

        OnBatchUnbound(models);

        foreach (T model in models)
        {
            _boundData.Remove(model);
        }
    }

    /// <inheritdoc/>
    public bool Unbind()
        => Unbind(ActiveModel);

    /// <summary>
    /// Gets a value indicating if this type of <see cref="ViewModel{T}"/> requires existing data to be explicitly unbound before
    /// new data is bound.
    /// </summary>
    protected virtual bool UnbindOnBind
        => false;

    /// <summary>
    /// Called when a sequence of new data is being bound to the view model so that any work required for the data
    /// to be fully represented in a view can be performed.
    /// </summary>
    /// <param name="models">The sequence of new data being bound to the view model.</param>
    protected virtual void OnBatchBinding(IEnumerable<T> models)
    {
        Require.NotNull(models, nameof(models));

        foreach (T model in models)
        {
            Bind(model);
        }
    }

    /// <summary>
    /// Called when a sequence of new data is being bound asynchronously to the view model so that any work required for the data
    /// to be fully represented in a view can be performed.
    /// </summary>
    /// <param name="models">The sequence of new data being bound to the view model.</param>
    /// <returns>The task object representing the asynchronous operation.</returns>
    protected virtual Task OnBatchBindingAsync(IEnumerable<T> models)
    {
        OnBatchBinding(models);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Called when a sequence of data is being unbound from the view model so that any work required for the data
    /// to no longer be represented in a view can be performed.
    /// </summary>
    /// <param name="models">The sequence of data unbound from the view model.</param>
    protected virtual void OnBatchUnbound(IEnumerable<T> models)
    {
        Require.NotNull(models, nameof(models));

        foreach (T model in models)
        {
            Unbind(model);
        }
    }

    /// <summary>
    /// Called when new data is being bound to the view model so that any work required for the data to be
    /// fully represented in a view can be performed.
    /// </summary>
    /// <param name="model">The new data being bound to the view model.</param>
    protected abstract void OnBinding(T model);

    /// <summary>
    /// Called when data has been unbound from the view model so that any work required for the data to no longer
    /// be represented in a view can be performed.
    /// </summary>
    /// <param name="model">The data unbound from the view model.</param>
    protected abstract void OnUnbound(T model);
}