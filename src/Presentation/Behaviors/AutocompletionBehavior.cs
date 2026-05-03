// -----------------------------------------------------------------------
// <copyright>
//      Created by Matt Weber <matt@badecho.com>
//      Copyright @ 2026 Bad Echo LLC. All rights reserved.
//
//      Bad Echo Technologies are licensed under the
//      GNU Affero General Public License v3.0.
//
//      See accompanying file LICENSE.md or a copy at:
//      https://www.gnu.org/licenses/agpl-3.0.html
// </copyright>
// -----------------------------------------------------------------------

using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BadEcho.Extensions;
using BadEcho.Presentation.Properties;

namespace BadEcho.Presentation.Behaviors;

/// <summary>
/// Provides a behavior that, when attached to a target text box, offers autocompletion functionality. 
/// </summary>
internal sealed class AutocompletionBehavior : Behavior<TextBox, IAutocompletionSource>, IHandlerBypassable
{
    /// <summary>
    /// Gets a mapping between target dependency objects and their autocompletion configuration.
    /// </summary>
    private ConditionalWeakTable<TextBox, AutocompletionState> StateMap
    {
        get
        {
            ReadPreamble();

            return field;
        }
    } = [];

    /// <inheritdoc/>
    protected override void OnValueAssociated(TextBox targetObject, IAutocompletionSource newValue)
    {
        var state = new AutocompletionState(this);

        WritePreamble();
        StateMap.Add(targetObject, state);
        WritePostscript();

        targetObject.Unloaded += HandleTargetUnloaded;
        targetObject.TextChanged += HandleTargetTextChanged;
        targetObject.PreviewKeyDown += HandleTargetPreviewKeyDown;
    }

    /// <inheritdoc/>
    protected override void OnValueDisassociated(TextBox targetObject, IAutocompletionSource oldValue)
    {
        targetObject.Unloaded -= HandleTargetUnloaded;
        targetObject.TextChanged -= HandleTargetTextChanged;
        targetObject.PreviewKeyDown -= HandleTargetPreviewKeyDown;

        WritePreamble();
        StateMap.Remove(targetObject);
        WritePostscript();
    }

    /// <inheritdoc/>
    protected override Freezable CreateInstanceCore()
        => new AutocompletionBehavior();

    private AutocompletionState GetState(TextBox targetObject)
    {
        return !StateMap.TryGetValue(targetObject, out AutocompletionState? state)
            ? throw new InvalidOperationException(Strings.NoAutocompletionStateForTarget)
            : state;
    }

    private void InsertSuggestion(TextBox textBox, string? suggestion)
    {
        if (string.IsNullOrEmpty(suggestion))
            return;

        var state = GetState(textBox);
        int selectionLength = suggestion.Length - state.SelectionStart;

        this.BypassHandlers(() => textBox.Text = suggestion);
        
        textBox.Select(state.SelectionStart, selectionLength);
    }

    private void HandleTargetTextChanged(object sender, TextChangedEventArgs e)
    {
        if (this.IsHandlingBypassed())
            return;

        TextBox textBox = (TextBox) sender;
        IAutocompletionSource source = GetAssociatedValue(textBox);
        AutocompletionState state = GetState(textBox);

        state.LoadSuggestions(source.SuggestCompletions(textBox.Text));

        // When text changes, the start of any selected text changes with it, so that selections begin at the end of the original input.
        state.SelectionStart = textBox.Text.Length;

        // Only insert completion suggestions when text is being added to the text box...otherwise it might be impossible to delete text.
        if (!e.Changes.Any(c => c.AddedLength > 0))
            return;

        InsertSuggestion(textBox, state.NextSuggestion());
    }

    private void HandleTargetPreviewKeyDown(object sender, KeyEventArgs e)
    {
        TextBox textBox = (TextBox) sender;
        IAutocompletionSource source = GetAssociatedValue(textBox);
        AutocompletionState state = GetState(textBox);

        switch (e.Key)
        {   // We handle PreviewKeyDown since KeyDown is not raised for navigational keys.
            case Key.Right:
                // Pressing the Right key will accept any previously suggested text, inserting a new suggestion (if one exists) to the end of the current text.
                state.LoadSuggestions(source.SuggestCompletions(textBox.Text));
                string? suggestion = state.NextSuggestion();
            
                // If there are no valid suggestions, we let the normal event handling occur, which will advance the caret to the end.
                if (string.IsNullOrEmpty(suggestion))
                    return;
                                
                state.SelectionStart = textBox.Text.Length;

                InsertSuggestion(textBox, suggestion);
                
                // We prevent the event from being handled further, otherwise any suggested text we just selected will become deselected.
                e.Handled = true;
                break;

            case Key.Tab:
                // Pressing tab cycles through the current set of suggestions. The selection start is not changed so we can discern between original and
                // suggested text.
                InsertSuggestion(textBox, state.NextSuggestion());

                break;
        }
    }

    private void HandleTargetUnloaded(object sender, RoutedEventArgs e)
    {
        TextBox textBox = (TextBox) sender;

        Detach(textBox);
    }

    /// <summary>
    /// Provides state data for a target object a <see cref="AutocompletionBehavior"/> instance is attached to.
    /// </summary>
    private sealed class AutocompletionState
    {
        private readonly AutocompletionBehavior _behavior;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutocompletionState"/> class.
        /// </summary>
        /// <param name="behavior">The behavior this provides state data for.</param>
        public AutocompletionState(AutocompletionBehavior behavior)
        {
            _behavior = behavior;
        }

        /// <summary>
        /// Gets or sets the character index for the beginning of any suggested text selections.
        /// </summary>
        /// <remarks>
        /// <para>
        /// When suggested text is appended to the text box, it is then selected so we can discern it from the original text inputted
        /// by the user.
        /// </para>
        /// <para>
        /// The selection start is tracked independently from <see cref="TextBox.SelectionStart"/>, since that property will change whenever
        /// the caret index changes (which could occur if the user presses a navigational key or clicks somewhere on the text box, causing a
        /// deselection), potentially causing us to lose information regarding where the original text ends and the suggested text begins.
        /// </para>
        /// </remarks>
        public int SelectionStart
        {
            get
            {
                _behavior.ReadPreamble();
                return field;
            }
            set
            {
                _behavior.WritePreamble();
                field = value;
                _behavior.WritePostscript();
            }
        }

        /// <summary>
        /// Gets the list of currently applicable suggestions.
        /// </summary>
        private List<string> Suggestions
        {
            get
            {
                _behavior.ReadPreamble();
                return field;
            }
        } = [];

        /// <summary>
        /// Gets or sets the index of the currently proffered suggestion within <see cref="Suggestions"/>.
        /// </summary>
        private int SuggestionIndex
        {
            get
            {
                _behavior.ReadPreamble();
                return field;
            }
            set
            {
                _behavior.WritePreamble();
                field = value;
                _behavior.WritePostscript();
            }
        }

        /// <summary>
        /// Loads a set of suggestions into this state data.
        /// </summary>
        /// <param name="suggestions">The suggestions to load.</param>
        public void LoadSuggestions(IEnumerable<string> suggestions)
        {
            _behavior.WritePreamble();
            Suggestions.Clear();
            Suggestions.AddRange(suggestions);
            _behavior.WritePostscript();
            
            SuggestionIndex = -1;
        }

        /// <summary>
        /// Retrieves the next applicable suggestion.
        /// </summary>
        /// <returns>The next applicable suggestion; or null, if no suggestions exist.</returns>
        public string? NextSuggestion()
        {
            if (Suggestions.IsEmpty())
                return null;

            SuggestionIndex = SuggestionIndex == Suggestions.Count - 1 ? 0 : SuggestionIndex + 1;

            return Suggestions[SuggestionIndex];
        }
    }
}
