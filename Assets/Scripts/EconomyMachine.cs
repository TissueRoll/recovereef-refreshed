using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using Assets.Scripts.EntityClasses;

public class EconomyMachine
{

	private float actualHF;
	private float actualCF;
	private float tolerance;
	private int coralNumberTolerance;
	private int cycle;
	private int cycleMax;
	private List<float> vals; // multiplier history
	private bool initialCycleComplete;
	private List<float> desired;
	float tolerancePercentage;

	// __ENHANCE: some values are hardcoded, maybe move data to external file
	public EconomyMachine(float aHF, float aCF, float tol, int cNT)
	{
		actualHF = aHF;
		actualCF = aCF;
		tolerance = tol;
		coralNumberTolerance = cNT;
		cycle = 0;
		cycleMax = 5;
		vals = new List<float>() { 1.0f, 1.0f, 1.0f, 1.0f, 1.0f };
		initialCycleComplete = false;
		desired = new List<float>() { 0.60f, 0.30f, 0.10f }; 
		tolerancePercentage = 0.05f;
	}

	public bool AlgaeWillSurvive(AlgaeCellData algaeCellData, int groundViability, int additiveFactors)
	{
		int computedSurvivability = groundViability
									+ algaeCellData.maturity
									+ algaeCellData.algaeData.survivability
									+ additiveFactors;
		bool result = Random.Range(0, 1001) <= computedSurvivability;
		return result;
	}

	public bool CoralWillSurvive(CoralCellData coralCellData, int groundViability, int additiveFactors, string groundName)
	{
		int computedSurvivability = groundViability
									+ coralCellData.maturity
									+ coralCellData.coralData.survivability
									+ additiveFactors
									+ (Regex.IsMatch(groundName, "." + coralCellData.coralData.prefTerrain + ".") ? 5 : -10);
		bool result = Random.Range(0, 1001) <= computedSurvivability;
		return result;
	}

	public bool CoralWillPropagate(CoralCellData coralCellData, int additiveFactors, string groundName)
	{
		int computedPropagatability = UnityEngine.Random.Range(30, 41) // base
									+ coralCellData.coralData.propagatability
									+ additiveFactors
									+ (Regex.IsMatch(groundName, "." + coralCellData.coralData.prefTerrain + ".") ? 5 : -10);
		bool result = Random.Range(0, 101) <= computedPropagatability;
		return result;
	}

	public bool AlgaeWillPropagate(AlgaeCellData algaeCellData, int additiveFactors, string groundName)
	{
		int computedPropagatability = UnityEngine.Random.Range(20, 31) // base
									+ algaeCellData.algaeData.propagatability
									+ additiveFactors;
		bool result = Random.Range(0, 101) <= computedPropagatability;
		return result;
	}

	public float DiversityMultiplier(List<int> coralNumbers)
	{
		float multiplier = 1.0f;
		float sum = 0;
		foreach (int x in coralNumbers)
		{
			sum += x;
		}

		for (int i = 0; i < coralNumbers.Count; i++)
		{
			float computedPercentage = ((float)coralNumbers[i]) / sum;
			if (desired[i] - tolerancePercentage <= computedPercentage && computedPercentage <= desired[i] + tolerancePercentage)
			{
				multiplier += 0.5f / ((float)coralNumbers.Count);
			}
		}

		vals[cycle] = multiplier;
		if (cycle == 4 && !initialCycleComplete)
		{
			initialCycleComplete = true;
		}
		cycle = (cycle + 1) % cycleMax;

		float averageMultiplier = 0.0f;
		foreach (float x in vals)
		{
			averageMultiplier += x;
		}
		averageMultiplier /= ((float)cycleMax);

		// Debug.Log("---");
		// foreach(float x in vals)
		//     Debug.Log(x);
		// Debug.Log("---");

		return averageMultiplier;
	}

	public void UpdateHFCF(float hf, float cf)
	{
		float diff = (Mathf.Max(cf, hf) - Mathf.Min(cf, hf));
		if (Mathf.Abs(diff) <= tolerance)
		{ // within threshold
			actualHF = hf;
			actualCF = (cf > hf ? cf - diff : cf);
		}
		else if (cf > hf)
		{ // more carnivorous
			actualCF = hf - 1.5f * diff;
			actualHF = actualCF + tolerance;
		}
		else
		{ // more herbivorous
			actualCF = Mathf.Max(hf - tolerance, cf);
			actualHF = hf - 0.5f * tolerance;
		}
		actualHF = Mathf.Max(0f, actualHF);
		actualCF = Mathf.Max(0f, actualCF);
	}

	public float GetActualHF()
	{
		return actualHF;
	}

	public float GetActualCF()
	{
		return actualCF;
	}

	public float GetTotalFish(List<int> coralNumbers)
	{
		return (0.2f * actualHF + 0.3f * actualCF) * DiversityMultiplier(coralNumbers);
	}

	public bool IsAverageGood()
	{
		float averageMultiplier = 0.0f;
		foreach (float x in vals)
		{
			averageMultiplier += x;
		}
		averageMultiplier /= ((float)cycleMax);
		return (averageMultiplier >= 1.35f);
	}

}
