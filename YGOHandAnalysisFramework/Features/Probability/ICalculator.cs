﻿namespace YGOHandAnalysisFramework.Features.Probability;

public interface ICalculator<T>
{
    double Calculate(Func<T, double> selector);
    double Calculate<TArgs>(TArgs args, Func<T, TArgs, double> selector);
}
