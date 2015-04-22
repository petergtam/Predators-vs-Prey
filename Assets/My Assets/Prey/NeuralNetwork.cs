using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using Random = System.Random;

public class NeuralNetwork
{
    private const float alpha = 3;
    private double[,] trainingWeights;
    public double[,] weights;

    public NeuralNetwork()
    {
        trainingWeights = null;
        weights = null;
        
    }

    public void training (double[] input)
	{
		var desired = GetDesired (input);
        if (desired == null){return;}
        var e = new double[4];
        if (trainingWeights == null)
        {
            var r = new Random();
            trainingWeights = new double[input.Length,4];
            for (var i = 0; i < input.Length; i++)
            {
                for (var j = 0; j < 4; j++)
                {
                    trainingWeights[i,j] = Math.Round(r.NextDouble(), 4);    
                }
            }
        }
        if (weights != null)
        {
            trainingWeights = weights;
        }
        weights = new double[input.Length,4];
        //Single-Layer
        for (var i = 0; i < 4; i++)
        {
            //Value of v in every neuron
            var vj = (float) Sigma(input, trainingWeights, i);
            //Value of v through the activationfunction.
            var yj= ActivationFunction(vj);
            //Getting the error
            e[i] = desired[i] - yj;
            //Getting delta of wkj
            var deltaw = new double[input.Length];
            for (var j = 0; j < input.Length; j++)
            {
                //Obtaining delta_{ij}
                deltaw[j] = e[i]*input[j];
                weights[i, j] = trainingWeights[i, j] + deltaw[j];
            }
        }
	}

	double[] GetDesired (double[] input)
	{
		double[] value = null;
		if (input [0].Equals (1)) {
			value = new double[]{1,0,0,0};
		} else if (input[1].Equals(1))
		{
		    value = new double[] {0, 1, 0, 0};
		}
        else if (input[2]>0)
        {
            value = new double[]{0,0,1,0};
        }
	    return value;
	}

	double Sigma (double[] inputs, double[,] w, int i)
	{
		double sum = 0;
		var j = 0;
		foreach (var input in inputs) {
			sum += input * w [j,i];
			j++;
		}
		return sum;
	}

	double ActivationFunction (float v)
	{
		return Math.Round( (1 / (1 + Math.Exp (-(alpha * v)))),4);
	}
}