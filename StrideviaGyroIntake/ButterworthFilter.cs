using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StrideviaGyroIntake
{
    public class ButterworthFilter
    {
        private readonly double[] b;
        private readonly double[] a;
        private readonly double[] x;
        private readonly double[] y;

        public ButterworthFilter(double[] bCoeffs, double[] aCoeffs)
        {
            this.b = bCoeffs;
            this.a = aCoeffs;
            x = new double[b.Length];
            y = new double[a.Length];
        }

        public double FilterSample(double input)
        {
            // Shift old samples
            for (int i = x.Length - 1; i > 0; i--) x[i] = x[i - 1];
            for (int i = y.Length - 1; i > 0; i--) y[i] = y[i - 1];

            x[0] = input;
            y[0] = 0;

            // Apply difference equation
            for (int i = 0; i < b.Length; i++) y[0] += b[i] * x[i];
            for (int i = 1; i < a.Length; i++) y[0] -= a[i] * y[i];

            return y[0];
        }

        public double[] Filter(double[] signal)
        {
            var result = new double[signal.Length];
            for (int i = 0; i < signal.Length; i++)
                result[i] = FilterSample(signal[i]);
            return result;
        }
    }
}
