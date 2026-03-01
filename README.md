# DoyStratOptimizer

**DoyStratOptimizer** is a project within the [**DoyVestment ecosystem**](https://github.com/DO-YAC/DoyVestment.public) and is designed for the automated optimization of trading strategies based on **technical analysis**.

The goal of the project is to identify **optimal parameter values and thresholds** for rule-based trading strategies by systematically backtesting them on historical market data and evaluating their performance.

---

## Overview

DoyStratOptimizer uses an **evolutionary optimization approach** to efficiently evaluate and iteratively improve large numbers of strategy variants.

Core concept:

- Multiple variants of a strategy are backtested in parallel
- Each variant receives a score based on predefined metrics
- The best strategies of a generation are selected
- New generations are created through controlled mutations of these top strategies

---

## How It Works

### 1. Generation-Based Backtesting

- A fixed number of strategies with different parameter sets are backtested simultaneously per generation
- All strategies share the same core logic but differ in their parameter values

---

### 2. Evaluation Metrics (Key Metrics)

Each strategy is evaluated using several key performance indicators, including:

- **PnL (Profit and Loss)**
- **Maximum Profit**
- **Positive-to-Negative Trade Ratio**
- **Time in Drawdown to Time in Profit Ratio**
- **Maximum Drawdown**

These metrics are combined into a **total score**, representing the overall quality of the strategy.

---

### 3. Selection & Evolution

- After a generation completes, all strategies are ranked by their score
- The **top 10 strategies** are automatically transferred to the next generation
- All other strategies are discarded

---

### 4. Parameter Mutation

- The next generation is created by **mutating the parameters** of the top 10 strategies
- Mutation strength is:
    - determined randomly
    - constrained within a **range defined at program startup**

Benefits of this approach:

- **Fine-tuning** through small mutations
- **Exploration of new parameter spaces** through larger mutations
- Prevents strategies from getting stuck in local optima when the global optimum lies outside narrow mutation bounds

---

### 5. Termination & Target Definition

- An optional **target score** can be defined; once reached, the optimization stops automatically
- Alternatively, during computation the user can:
    - save the currently best candidate via a **keyboard shortcut**
    - manually abort the evaluation

---

## Technical Details

- **Programming Language:** C#
- **Framework:** .NET 10
- **Current State:** Console application
- **Platform:** Cross-platform (depending on .NET runtime support)

---

## Roadmap

Planned enhancements and improvements:

-  **Unit & integration testing**
-  **Stronger abstraction via interfaces**
-  **MAUI-based GUI application**
    - Alternative for the console application
    - Visualization of scores, generations, and metrics
    - Interactive control of the optimization process

---

## Target Audience

DoyStratOptimizer is intended for:
- Quantitative traders
- Algorithmic traders
- Developers of rule-based trading strategies
- Users of the DoyVestment ecosystem seeking data-driven strategy optimization

---

## Status

The project is under active development. Feedback, ideas, and improvement suggestions are welcome and will be considered for the ongoing roadmap.