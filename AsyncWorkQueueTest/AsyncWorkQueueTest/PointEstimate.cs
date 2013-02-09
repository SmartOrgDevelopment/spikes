using System;
using System.Collections.Generic;
using System.Text;

namespace AsyncWorkQueueTest
{
    public class PointEstimate
    {
        private double value;
        private double k;
        private double sum;
        private double v;

        public PointEstimate()
        {
            k = 0;
            sum = 0;
            v = 0;
        }

        public void nextValueIs(double value)
        {
            this.value = value;
            calculate();
        }

        public void calculate()
        {
            k++;
            if (k > 1)
            {
                double diff = (sum - (k - 1) * value);
                v = ((k - 2) * v + (diff * diff) / (k * (k - 1))) / (k - 1);
            }
            sum += value;
        }

        public double variance()
        {
            return v;
        }

        public double halfIntervalFor(double z)
        {
            return z * Math.Sqrt(v / k);
        }

        public SummaryStatistics computeConfidenceIntervalForPercent(int percent) {
	        double hi = halfIntervalFor(zFor(percent));
	        double pointEstimate = mean();
		    double cLower = pointEstimate - hi;
		    double cUpper = pointEstimate + hi;
            SummaryStatistics summary = new SummaryStatistics();
		    summary.cLower = cLower;
            summary.cUpper = cUpper;
            summary.pointEstimate = pointEstimate;
            return summary;
	    }

        public double zFor(int percent)
        {
            if (percent == 98)
            {
                return 2.33;
            }
            else if (percent == 99)
            {
                return 2.576;
            }
            else return -1;
        }

        public double mean()
        {
            return sum / k;
        }

        public long numberOfTrialsNeeded(double z, double epsilon)
        {
            double muHat = mean();
            return (long)((v * z * z) / (epsilon * muHat * epsilon * muHat));
        }

        public double sumValue()
        {
            return sum;
        }
    }
    public class SummaryStatistics
    {
        public double cLower {get; set;}
        public double cUpper {get; set;}
        public double pointEstimate { get; set; }
    }
}
