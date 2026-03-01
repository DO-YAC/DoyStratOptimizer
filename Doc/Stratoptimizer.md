![[StrategyOptimizer.jpg]]

The user fills in the form, where he adds the path to the strategy-assembly, the path to the testingdata, the desired score, the default paramaters, the amount of strategys per generation and the mutationfactor. This is saved into a configurationclass.

Then the viewmodel instantiates the Genetic optimizer with the Configuration and starts the algorithm.

The optimizer instantiates and holds a class of the evaluationcalculator with the chartdata. The optimizer starts the first generation with the default parameters and however many mutated copies. In a parallel for loop multiple strategies are evaluated through the evaluationcalculator.

The parameters are stored within a dictionary with the parameters as key and the evaluationresult as value.

After a Generation is tested, the score of the parameters is checked and if it reaches the desired score the optimization stops and is reported to the user which is prompted to save it to a file.

If the score is not reached, the top ten parameters will be added to the next generation and the generation will be filled with mutations of each paramaters of the previous top ten. Then the cycle continues until the desired score is reached, or stopped manually.

---
