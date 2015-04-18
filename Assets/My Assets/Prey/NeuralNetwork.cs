using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using UnityEditor;

class NeuralNetwork
{
    public static float alpha = 3;

    double training(double[] input)
    {
        double[] desired;
        if (input[0].Equals(1))
        {
            desired = new double[]{1,0,0,0};
        }
        else if (input[1].Equals(1))
        {
            desired = new double[]{0,1,0,0};
        }
    }

    double sigma(double[] inputs, double[][] weights, int i)
    {
        double sum = 0;
        int j = 0;
        foreach (var input in inputs)
        {
            sum += input*weights[j][i];
            j++;
        }
        return sum;
    }

    double ActivationFunction(float v)
    {
        return Math.Round(1/(1 + Math.Exp(-(alpha*v))),4);
    }
}