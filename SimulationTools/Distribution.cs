﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimulationTools
{
    static class Distribution
    {
        static private Random rand;

        static Distribution()
        {
            rand = new Random();
        }

        static public double Uniform01()
        {
            return rand.NextDouble();
        }

        static public int UniformInt(int upper)
        {
            return rand.Next(upper);
        }

        static private double SampleStandardNormal()
        {
            double u1 = 1.0 - rand.NextDouble();
            double u2 = 1.0 - rand.NextDouble();
            return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);       
        }

        /// <summary>
        /// Uses the Box-Mueller transform to generate a N(0,1) random number, then addapts it to make it N(mu,S2)
        /// </summary>
        /// <returns></returns>
        static public double SampleNormal(double mean, double StdDev)
        {            
            double ans = mean + StdDev * SampleStandardNormal();

            if (ans < 0)
            {
               // Console.Error.WriteLine("WARNING: Normal distribution N({0},{1}) sampled with value < 0, returning 0 instead.",mean,StdDev);
                return 0;
            }
            else { return ans; }
        }



        /// <summary>
        /// Returns the probability that a sample from N(mu,StdDev) is leq upto
        /// </summary>
        /// <param name="mean">mean</param>
        /// <param name="StdDev">Standard Deviation</param>
        /// <param name="upto">Upper limit</param>
        /// <returns></returns>
        static public double NormalCDF(double mean, double StdDev, double upto)
        {
            double z = (upto - mean) / StdDev;
            return Phi(z);
        }

        /// <summary>
        /// Approximation of the error function. The probability than a standard Normal variable is leq x.
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        static double Phi(double x)
        {
            // constants
            double a1 = 0.254829592;
            double a2 = -0.284496736;
            double a3 = 1.421413741;
            double a4 = -1.453152027;
            double a5 = 1.061405429;
            double p = 0.3275911;

            // Save the sign of x
            int sign = 1;
            if (x < 0)
                sign = -1;
            x = Math.Abs(x) / Math.Sqrt(2.0);

            // A&S formula 7.1.26
            double t = 1.0 / (1.0 + p * x);
            double y = 1.0 - (((((a5 * t + a4) * t) + a3) * t + a2) * t + a1) * t * Math.Exp(-x * x);

            return 0.5 * (1.0 + sign * y);
        }

        static public double SampleExponential(double mean) // always positive
        {
            double u = Uniform01(); // u in 0,1
            return -mean * Math.Log(1 - u); //Inverse of CDF. Use: P(X<= x) = P(Finv(u) <= x) = P(u<= F(x)) = F(x) (u is uniform).
        }

        static public double SampleLogNormal(double LNmean, double LNStdDev) //always positive
        {
            double LNvar = LNStdDev * LNStdDev;
            double LNm2 = LNmean * LNmean;
            double mu = Math.Log(LNm2 / Math.Sqrt(LNvar + LNm2));
            double sigma = Math.Sqrt(Math.Log(1.0 + LNvar / LNm2));
            double Z = SampleStandardNormal();
            return Math.Exp(mu + sigma * Z);
        }

    }
}
