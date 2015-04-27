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
using Assets.My_Assets;

public class NeuralNetwork
{
    private const float alpha = 1;
    private double[,] trainingWeights;
    private double[,] weights;
    private bool train;
	private Agent agent;
	private int counter;

    public NeuralNetwork(Agent a)
    {
        trainingWeights = null;
        weights = null;
        train = true;
		agent = a;
		if (a is Prey) {
			counter = 4;
		} else {
			counter = 3;
		}
    }

    public bool IsNeedTraining()
    {
        return train;
    }

	bool Equal (double[,] A, double[,] B)
	{
		for (int i=0; i<A.GetLength(0); i++) {
			for(int j = 0; j<A.GetLength(1);j++){
				if(A[i,j]!=B[i,j])return false;
			}
		}
		return true;
	}

    public double[] Training (double[] input)
    {
        if (!train) return null;
		var desired = GetDesired (input);
        if (desired == null){return null;}
        var e = new double[counter];
        if (trainingWeights == null)
        {
			//initializing in case is the first time
            var r = new Random();
			trainingWeights = new double[counter,input.Length];
            for (var i = 0; i < counter; i++)
            {
                for (var j = 0; j < input.Length; j++)
                {
                    trainingWeights[i,j] = Math.Round(r.NextDouble(),3);    
                }
            }
        }
        if (weights != null)
        {
			if(Equal(trainingWeights,weights)){
				train = false;
				return null;
			}
            trainingWeights = weights;
        }
		weights = new double[counter,input.Length];
		var y = yVector(trainingWeights,input);
        //Single-Layer
		for (var j = 0; j < counter; j++)
        {
            //Getting the error
            e[j] = desired[j] - y[j];
            //Getting delta of wkj
            var deltaw = new double[input.Length];
            for (var i = 0; i < input.Length; i++)
            {                
                deltaw[i] = e[j]*input[i];
                weights[j, i] = trainingWeights[j, i] + deltaw[i];
            }
        }
        return y;
	}


    public double[] Ejecution(double[] input)
    {
        return yVector(weights,input);
    }


	double[] GetDesired (double[] input)
	{
		if (agent is Prey) {
			if (input [1].Equals (1)) {
				return new double[]{1,0,0,0};
			} else if (input [2].Equals (1)) {
				return new double[] {0, 1, 0, 0};
			} else if (input [3].Equals (1)) {
				return new double[]{0,0,1,0};
			} else if (input [4].Equals (1)) {
				return new double[] {0, 0, 0, 1};
			} else
				return new double[] {0, 0, 0, 0};
		} else {
			if (input [1].Equals (1)) {
				return new double[]{1,0,0};
			} else if (input [2].Equals (1)) {
				return new double[] {0, 1, 0};
			} else if (input [3].Equals (1)) {
				return new double[]{0,0,1};
			} else
				return new double[] {0, 0, 0};
		}
	    return null;
	}

	double[] yVector(double[,] weight,double[] input){
		double[] yk = new double[weight.GetLength(0)];
		for (int i = 0; i< weight.GetLength(0); i++) {
			double sum = 0;
			for(int j = 0; j < weight.GetLength(1);j++){
				sum+=weight[i,j]*input[j];
			}
			yk[i] = ActivationFunction(sum);
		}
		return yk;
	}

	double ActivationFunction (double v)
	{
	    var exp = alpha*v;
	    var result = 1/(1 + Math.Exp(-exp));
		return Math.Round(result,3);
	}
}