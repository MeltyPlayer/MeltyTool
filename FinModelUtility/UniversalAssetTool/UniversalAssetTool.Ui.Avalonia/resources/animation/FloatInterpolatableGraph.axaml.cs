using System;

using Avalonia.Controls;

using fin.animation.interpolation;
using fin.animation.keyframes;
using fin.animation.types.single;
using fin.ui.avalonia;

using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

using ReactiveUI;

namespace uni.ui.avalonia.resources.animation;

public class FloatInterpolatableGraphViewModelForDesigner
    : FloatInterpolatableGraphViewModel {
  public FloatInterpolatableGraphViewModelForDesigner() {
    var sharedConfig = new SharedInterpolationConfig {
        AnimationLength = 30,
    };

    var keyframes
        = new InterpolatedKeyframes<KeyframeWithTangents<float>, float>(
            sharedConfig,
            FloatKeyframeWithTangentsInterpolator.Instance);

    keyframes.Add(new KeyframeWithTangents<float>(5, 1, 2, -2, 2));
    keyframes.Add(new KeyframeWithTangents<float>(10, 4, 5, 2, 2));
    keyframes.Add(new KeyframeWithTangents<float>(20, 8));
    keyframes.Add(new KeyframeWithTangents<float>(25, -5));

    this.Keyframes = keyframes;
  }
}

public class FloatInterpolatableGraphViewModel : ViewModelBase {
  public IConfiguredInterpolatable<float> Keyframes {
    get;
    set {
      this.RaiseAndSetIfChanged(ref field, value);

      Func<double, double> graphFunction = frame => {
        if (field.TryGetAtFrameOrDefault(
                (float) frame,
                out var value)) {
          return value;
        }

        return Double.NaN;
      };

      var plotModel = new PlotModel();
      plotModel.Series.Add(new FunctionSeries(
                               graphFunction,
                               0,
                               value.SharedConfig.AnimationLength,
                               0.0001));

      plotModel.Axes.Add(new LinearAxis {
          Position = AxisPosition.Bottom,
          Title = "Frame",
      });
      plotModel.Axes.Add(new LinearAxis {
          Position = AxisPosition.Left,
          Title = "Value",
      });

      this.PlotModel = plotModel;
    }
  }

  public IPlotModel PlotModel {
    get;
    private set => this.RaiseAndSetIfChanged(ref field, value);
  }
}

public partial class FloatInterpolatableGraph : UserControl {
  public FloatInterpolatableGraph() {
    this.InitializeComponent();
  }
}