using System;

using Avalonia.Controls;

using fin.animation.interpolation;
using fin.animation.keyframes;
using fin.animation.types.single;

using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

using ReactiveUI;

using uni.ui.avalonia.ViewModels;

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
  private IInterpolatable<float> keyframes_;
  private IPlotModel plotModel_;

  public IInterpolatable<float> Keyframes {
    get => this.keyframes_;
    set {
      this.RaiseAndSetIfChanged(ref this.keyframes_, value);

      Func<double, double> graphFunction = frame => {
        if (this.keyframes_.TryGetAtFrame((float) frame, out var value)) {
          return value;
        }

        return Double.NaN;
      };

      var plotModel = new PlotModel();
      // TODO: Where to get the animation length?
      plotModel.Series.Add(new FunctionSeries(graphFunction, 0, 30, 0.0001));

      plotModel.Axes.Add(new LinearAxis {
          Position = AxisPosition.Bottom,
          Title = "Frame",
      });

      this.PlotModel = plotModel;
    }
  }

  public IPlotModel PlotModel {
    get => this.plotModel_;
    private set => this.RaiseAndSetIfChanged(ref this.plotModel_, value);
  }
}

public partial class FloatInterpolatableGraph : UserControl {
  public FloatInterpolatableGraph() {
    InitializeComponent();
  }
}