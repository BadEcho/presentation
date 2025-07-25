﻿// -----------------------------------------------------------------------
// <copyright>
//		Created by Matt Weber <matt@badecho.com>
//		Copyright @ 2024 Bad Echo LLC. All rights reserved.
//
//		Bad Echo Technologies are licensed under the
//		GNU Affero General Public License v3.0.
//
//		See accompanying file LICENSE.md or a copy at:
//		https://www.gnu.org/licenses/agpl-3.0.html
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using BadEcho.Presentation.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace BadEcho.Presentation.Tests;

public class SteppedBinderTests
{
    // A time drift of 125 ms is deemed acceptable.
    private const double DRIFT_TOLERANCE = 125;
        
    private readonly SourceObject _sourceObject;
    private readonly IBinding _binding;
    private readonly ITestOutputHelper _output;

    public SteppedBinderTests(ITestOutputHelper output)
    {
        _sourceObject = new SourceObject();
        _binding = new FakeBinding(_sourceObject);
        _output = output;
    }

    [Theory]
    [InlineData(111, 11, 3000)]
    [InlineData(11, 111, 3000)]
    [InlineData(50, 60, 3000)]
    [InlineData(60, 50, 3000)]
    [InlineData(111, 11, 2500)]
    [InlineData(50, 65, 2500)]
    [InlineData(111, 11, 8000)]
    [InlineData(50, 60, 8000)]
    [InlineData(111, 11, 1000)]
    [InlineData(50, 60, 1000)]
    public void OnSourceChanged_UpdatesInTime(int initialTargetValue, int newSourceValue, double durationMilliseconds)
    {
        TimeSpan steppingDuration = TimeSpan.FromMilliseconds(durationMilliseconds);
        TimeSpan elapsed = default;
        int retries = 3;

        RunTest();
        
        double drift = Math.Abs(elapsed.Subtract(steppingDuration).TotalMilliseconds);

        while (drift >= DRIFT_TOLERANCE && retries > 0)
        {
            _output.WriteLine(
                $"Drift of {drift} exceeds DRIFT_TOLERANCE. Trying {retries} additional time(s) to account for system load.");

            RunTest();

            drift = Math.Abs(elapsed.Subtract(steppingDuration).TotalMilliseconds);
            retries--;
        }

        _output.WriteLine($"Drift: {drift}");

        Assert.True(drift < DRIFT_TOLERANCE,
                    $"Expected Duration: {steppingDuration} Actual Duration: {elapsed}");
        return;

        void RunTest()
        {
            UserInterface.RunUIFunction(
                () => elapsed = UpdateSource(initialTargetValue.ToString(), newSourceValue, steppingDuration),
                true,
                false);
        }
    }

    [Fact]
    public void OnSourceChanged_UnsetTarget_TargetUpdatesToSource()
    {
        TimeSpan steppingDuration = TimeSpan.FromSeconds(1);
        TimeSpan elapsed = default;

        UserInterface.RunUIFunction(() => elapsed = UpdateSource(string.Empty, 10000, steppingDuration), true, false);

        Assert.True(elapsed < steppingDuration,
                    $"Actual duration of {elapsed} greater than expected duration of {steppingDuration} when no stepping should have occurred.");
    }

    [Fact]
    public void OnSourceChanged_IdenticalSourceTarget_NoUpdate()
    {
        TimeSpan steppingDuration = TimeSpan.FromSeconds(1);
        TimeSpan elapsed = default;

        UserInterface.RunUIFunction(() => elapsed = UpdateSource("10000", 10000, steppingDuration), true, false);

        Assert.True(elapsed < steppingDuration,
                    $"Actual duration of {elapsed} greater than expected duration of {steppingDuration} when no stepping should have occurred.");
    }

    [Fact]
    public void OnSourceChanged_ZeroDuration_TargetUpdatesToSource()
    {
        TimeSpan steppingDuration = TimeSpan.Zero;
        TimeSpan elapsed = default;

        UserInterface.RunUIFunction(() => elapsed = UpdateSource("10", 15, steppingDuration), true, false);

        Assert.True(
            Math.Abs(elapsed.Subtract(steppingDuration).TotalMilliseconds) < DRIFT_TOLERANCE,
            $"Expected Duration: {steppingDuration} Actual Duration: {elapsed}");
    }

    [Theory]
    [InlineData(111, 11, 3000)]
    [InlineData(11, 111, 3000)]
    [InlineData(50, 60, 3000)]
    [InlineData(60, 50, 3000)]
    [InlineData(111, 11, 2500)]
    [InlineData(50, 65, 2500)]
    [InlineData(111, 11, 8000)]
    [InlineData(50, 60, 8000)]
    [InlineData(111, 11, 1000)]
    [InlineData(50, 60, 1000)]
    public void OnTargetChanged_UpdatesInTime(int initialSourceValue, int newTargetValue, double durationMilliseconds)
    {
        TimeSpan steppingDuration = TimeSpan.FromMilliseconds(durationMilliseconds);
        TimeSpan elapsed = default;
        int retries = 3;

        RunTest();

        double drift = Math.Abs(elapsed.Subtract(steppingDuration).TotalMilliseconds);

        while (drift >= DRIFT_TOLERANCE && retries > 0)
        {
            _output.WriteLine(
                $"Drift of {drift} exceeds DRIFT_TOLERANCE. Trying {retries} additional time(s) to account for system load.");

            RunTest();

            drift = Math.Abs(elapsed.Subtract(steppingDuration).TotalMilliseconds);
            retries--;
        }

        _output.WriteLine($"Drift: {drift}");

        Assert.True(drift < DRIFT_TOLERANCE,
                    $"Expected Duration: {steppingDuration} Actual Duration: {elapsed}");
        return;

        void RunTest()
        {
            UserInterface.RunUIFunction(() => elapsed =
                                            UpdateTarget(initialSourceValue,
                                                         initialSourceValue.ToString(),
                                                         newTargetValue.ToString(),
                                                         steppingDuration),
                                        true,
                                        false);
        }
    }

    [Fact]
    public void OnTargetChanged_UnsetTarget_SourceUpdatesToTarget()
    {
        TimeSpan steppingDuration = TimeSpan.FromSeconds(1);
        TimeSpan elapsed = default;

        UserInterface.RunUIFunction(() => elapsed = UpdateTarget(10, string.Empty, "1000", steppingDuration), true, false);

        Assert.True(
            Math.Abs(elapsed.Subtract(steppingDuration).TotalMilliseconds) < DRIFT_TOLERANCE,
            $"Expected Duration: {steppingDuration} Actual Duration: {elapsed}");
    }

    [Fact]
    public void OnTargetChanged_IdenticalSourceTarget_NoUpdate()
    {
        TimeSpan steppingDuration = TimeSpan.FromSeconds(1);
        TimeSpan elapsed = default;

        UserInterface.RunUIFunction(() => elapsed = UpdateTarget(10, string.Empty, "10", steppingDuration), true, false);

        Assert.True(elapsed < steppingDuration,
                    $"Actual duration of {elapsed} greater than expected duration of {steppingDuration} when no stepping should have occurred.");
    }

    [Fact]
    public void OnTargetChanged_ZeroDuration_TargetUpdatesToSource()
    {
        TimeSpan steppingDuration = TimeSpan.Zero;
        TimeSpan elapsed = default;

        UserInterface.RunUIFunction(() => elapsed = UpdateTarget(10, "10", "15", steppingDuration), true, false);

        Assert.True(    // Drift tolerance needs to be widened for zero-duration tests to account for WPF binding system spin-up time.
            Math.Abs(elapsed.Subtract(steppingDuration).TotalMilliseconds) < DRIFT_TOLERANCE * 2,
            $"Expected Duration: {steppingDuration} Actual Duration: {elapsed}");
    }

    [Fact]
    public void Initialize_NegativeDuration_ThrowsException()
    {
        UserInterface.RunUIFunction(
            () =>
            {
                Assert.Throws<ArgumentException>(() => InitializeBinder(TimeSpan.FromSeconds(-2.0)));
                Dispatcher.CurrentDispatcher.InvokeShutdown();
            },
            true,
            false);
    }

    private SteppedBinder InitializeBinder(TimeSpan steppingDuration)
    {
        var textBox = new TextBox {Text = string.Empty};
        return new SteppedBinder(textBox,
                                 TextBox.TextProperty,
                                 new SteppingOptions(_binding)
                                 {
                                     SteppingDuration = steppingDuration,
                                     MinimumSteps = 0,
                                     StepAmount = 1.0,
                                     IsInteger = true
                                 });
    }

    private TimeSpan UpdateSource(string initialTargetValue, int newSourceValue, TimeSpan steppingDuration)
    {
        bool updatedToFinalValue = false;

        var textBox = new TextBox { Text = initialTargetValue };

        var binder = new SteppedBinder(textBox,
                                       TextBox.TextProperty,
                                       new SteppingOptions(_binding)
                                       {
                                           SteppingDuration = steppingDuration,
                                           MinimumSteps = 0,
                                           StepAmount = 1.0,
                                           IsInteger = true
                                       });

        var stopwatch = new Stopwatch();

        textBox.TextChanged += (_, _) =>
                               {
                                   if (Convert.ToInt32(textBox.Text) != newSourceValue)
                                       return;

                                   stopwatch.Stop();
                                   updatedToFinalValue = true;
                               };

        // This is only needed for tests where the text box's text never actually ends up changing.
        binder.Changed += (_, _) =>
                          {
                              if (textBox.Text != newSourceValue.ToString())
                                  return;

                              stopwatch.Stop();
                              updatedToFinalValue = true;
                          };

        stopwatch.Start();

        _sourceObject.Value = newSourceValue;

        while (!updatedToFinalValue)
            textBox.ProcessMessages();

        Dispatcher.CurrentDispatcher.InvokeShutdown();

        return stopwatch.Elapsed;
    }

    private TimeSpan UpdateTarget(int initialSourceValue, string initialTargetValue, string newTargetValue, TimeSpan steppingDuration)
    {
        bool updatedToFinalValue = false;

        _sourceObject.Value = initialSourceValue;
        var textBox = new TextBox { Text = initialTargetValue };

        var binder = new SteppedBinder(textBox,
                                       TextBox.TextProperty,
                                       new SteppingOptions(_binding)
                                       {
                                           SteppingDuration = steppingDuration,
                                           MinimumSteps = 0,
                                           StepAmount = 1.0,
                                           IsInteger = true
                                       });

        var stopwatch = new Stopwatch();
            
        binder.Changed += (_, _) =>
                          {
                              if (_sourceObject.Value.ToString() != newTargetValue)
                                  return;

                              stopwatch.Stop();
                              updatedToFinalValue = true;
                          };

        stopwatch.Start();

        binder.SetValue(TransientBinder.TargetProperty, newTargetValue);

        while (!updatedToFinalValue)
            textBox.ProcessMessages();

        Dispatcher.CurrentDispatcher.InvokeShutdown();

        return stopwatch.Elapsed;
    }
        
    private sealed class SourceObject : INotifyPropertyChanged
    {
        private int _value;

        public event PropertyChangedEventHandler? PropertyChanged;

        public int Value
        {
            get => _value;
            set 
            {
                if (_value == value)
                    return;

                _value = value;

                OnPropertyChanged();
            }
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null) 
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private sealed class FakeBinding : IBinding
    {
        private readonly Binding _binding;

        public FakeBinding(SourceObject sourceObject) 
            => _binding = new Binding(nameof(SourceObject.Value))
                          {
                              Source = sourceObject,
                              Mode = BindingMode.TwoWay
                          };

        public string BindingGroupName
        { get; set; } = string.Empty;

        public int Delay
        { get; set; }

        public object FallbackValue
        { get; set; } = new();

        public object TargetNullValue
        { get; set; } = new();

        public string? StringFormat
        {
            get;
            set;
        } = string.Empty;

        public CultureInfo? ConverterCulture
        { get; set; } = CultureInfo.InvariantCulture;

        public object? ConverterParameter
        { get; set; }

        public BindingMode Mode
        { get; set; }

        public bool NotifyOnSourceUpdated
        { get; set; }

        public bool NotifyOnTargetUpdated
        { get; set; }

        public bool NotifyOnValidationError
        { get; set; }

        public UpdateSourceExceptionFilterCallback? UpdateSourceExceptionFilter
        { get; set; }

        public UpdateSourceTrigger UpdateSourceTrigger
        { get; set; }

        public bool ValidatesOnDataErrors
        { get; set; }

        public bool ValidatesOnExceptions
        { get; set; }

        public bool ValidatesOnNotifyDataErrors
        { get; set; }

        public Collection<ValidationRule> ValidationRules
            => [];

        public void ClearConverter()
        { }

        public bool DoBindingAction(Func<bool> bindingAction) 
            => bindingAction();

        public void DoBindingAction(Action bindingAction)
            => bindingAction();

        public BindingExpressionBase SetBinding(DependencyObject targetObject, DependencyProperty targetProperty) 
            => BindingOperations.SetBinding(targetObject, targetProperty, _binding);
    }
}