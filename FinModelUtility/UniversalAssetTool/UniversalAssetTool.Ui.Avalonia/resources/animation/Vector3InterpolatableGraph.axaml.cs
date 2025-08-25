using System;
using System.Numerics;

using Avalonia.Controls;

using fin.animation.interpolation;
using fin.animation.keyframes;
using fin.animation.types;
using fin.animation.types.vector3;
using fin.ui.avalonia;

using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

using ReactiveUI;

namespace uni.ui.avalonia.resources.animation;

public class Vector3InterpolatableGraphViewModelForDesigner
    : Vector3InterpolatableGraphViewModel {
  public Vector3InterpolatableGraphViewModelForDesigner() {
    var sharedConfig = new SharedInterpolationConfig {
        AnimationLength = 30,
    };

    var keyframes = new CombinedVector3Keyframes<KeyframeWithTangents<Vector3>>(
        sharedConfig,
        Vector3KeyframeWithTangentsInterpolator.Instance);

    keyframes.Add(
        new KeyframeWithTangents<Vector3>(5,
                                          new Vector3(1, 2, 3),
                                          new Vector3(2, 3, 4),
                                          -2,
                                          2));
    keyframes.Add(new KeyframeWithTangents<Vector3>(10,
                                                    new Vector3(1, 2, 3),
                                                    new Vector3(2, 3, 4),
                                                    2,
                                                    2));
    keyframes.Add(new KeyframeWithTangents<Vector3>(20,
                                                    new Vector3(8, 16, 24)));
    keyframes.Add(
        new KeyframeWithTangents<Vector3>(25, new Vector3(-5, -6, -10)));

    this.Keyframes = keyframes;
  }
}

public class Vector3InterpolatableGraphViewModel : ViewModelBase {
  public IAxesConfiguredInterpolatable<Vector3> Keyframes {
    get;
    set {
      this.RaiseAndSetIfChanged(ref field, value);

      Func<double, Vector3> graphFunction = frame => {
        if (field.TryGetAtFrameOrDefault(
                (float) frame,
                out var value)) {
          return value;
        }

        return new Vector3(float.NaN);
      };

      Func<double, double> xGraphFunction = frame => graphFunction(frame).X;
      Func<double, double> yGraphFunction = frame => graphFunction(frame).Y;
      Func<double, double> zGraphFunction = frame => graphFunction(frame).Z;

      var plotModel = new PlotModel();
      plotModel.Series.Add(new FunctionSeries(
                               xGraphFunction,
                               0,
                               value.SharedConfig.AnimationLength,
                               0.0001,
                               "X"));
      plotModel.Series.Add(new FunctionSeries(
                               yGraphFunction,
                               0,
                               value.SharedConfig.AnimationLength,
                               0.0001,
                               "Y"));
      plotModel.Series.Add(new FunctionSeries(
                               zGraphFunction,
                               0,
                               value.SharedConfig.AnimationLength,
                               0.0001,
                               "Z"));

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

public partial class Vector3InterpolatableGraph : UserControl {
  public Vector3InterpolatableGraph() {
    this.InitializeComponent();
  }
}