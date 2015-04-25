using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = System.Random;

public class NeuralNetwork
{
    private const float alpha = 1;
    private double[,] trainingWeights;
    private double[,] weights;
    private bool train;


    public NeuralNetwork()
    {
        trainingWeights = null;
        weights = null;
        train = true;
    }

    public bool IsNeedTraining()
    {
        return train;
    }

    public double[] Training (double[] input)
    {
        if (!train) return null;
		var desired = GetDesired (input);
        if (desired == null){return null;}
        var e = new double[4];
        if (trainingWeights == null)
        {
            var r = new Random();
            trainingWeights = new double[input.Length,4];
            for (var i = 0; i < input.Length; i++)
            {
                for (var j = 0; j < 4; j++)
                {
                    trainingWeights[i,j] = Math.Round(r.NextDouble(),3);    
                }
            }
        }
        if (weights != null)
        {
            if (trainingWeights.Equals(weights))
            {
                train = false;
                return null;
            }
            trainingWeights = weights;
        }
        weights = new double[input.Length,4];
        var y = new double[4];
        //Single-Layer
        for (var j = 0; j < 4; j++)
        {
            //Value of v in every neuron
            var vj = (float) Sigma(input, trainingWeights, j);
            //Value of v through the activationfunction.
            var yj= ActivationFunction(vj);
            y[j] = vj;
            //Getting the error
            e[j] = desired[j] - yj;
            Debug.Log("Error: "+e[j]);
            //Getting delta of wkj
            var deltaw = new double[input.Length];
            for (var i = 0; i < input.Length; i++)
            {                
                deltaw[i] = 1.5*e[j]*input[i];
                weights[i, j] = trainingWeights[i, j] + deltaw[i];
            }
        }
        return y;
	}


    public double[] Ejecution(double[] input)
    {
        var y = new double[4];
        //Single-Layer
        for (var j = 0; j < 4; j++)
        {
            //Value of v in every neuron
            var vj = (float)Sigma(input, weights, j);
            //Value of v through the activationfunction.
            var yj = ActivationFunction(vj);
            //Getting the error
            y[j] = yj;
        }
        return y;
    }


	double[] GetDesired (double[] input)
	{
		double[] value = null;
		if (input [0].Equals (1)) {
			value = new double[]{1,1,0,0,0};
		}
        else if (input[1].Equals(1))
		{
		    value = new double[] {1,0, 1, 0, 0};
		}
        else if (input[2].Equals(1))
        {
            value = new double[]{1,0,0,1,0};
        }
        else if (input[3].Equals(1))
        {
            value = new double[] {1, 0, 0, 0, 1};
        }
        else value = new double[] {1, 0, 0, 0, 0};
	    return value;
	}

	double Sigma (double[] inputs, double[,] w, int j)
	{
		double sum = 0;
	    for (int i = 0; i < inputs.Length; i++)
	    {
            sum += inputs[i] * w[i, j];
	    }
	    return sum;
	}

	double ActivationFunction (float v)
	{
	    var exp = alpha*v;
	    var result = 1/(1 + Math.Exp(-exp));
		return Math.Round(result,3);
	}
}