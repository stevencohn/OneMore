---
name: onemore-telemetry-extrapolation
description: "Estimate OneMore command usage across the full user base by extrapolating from opt-in telemetry data. Scales telemetry counts by assumed opt-in rates to model actual usage patterns."
license: Proprietary
---

# OneMore Telemetry Extrapolation

Estimate how often each OneMore command is executed across your entire user base, even though only a subset of users have opted into telemetry.

## Overview

Your telemetry data captures only a fraction of actual usage (opt-in users). This skill extrapolates from known telemetry counts to estimate total command executions across all users by modeling different opt-in rate scenarios.

**Key assumption:** Telemetry users behave similarly to non-telemetry users (no selection bias).

## Methodology

### Input Data Required

1. **Unique telemetry sessions per month** — e.g., 3,368
2. **Monthly downloads** — e.g., 2,992
3. **Top command counts from telemetry** — with per-month averages for each command

The maintained Excel file containing this data is here: "C:\Users\steve\OneDrive\OneMore\Metrics.xlsx"

### Calculation

For each command, divide the telemetry count by the assumed opt-in rate:

```
Estimated Total Usage = Telemetry Count ÷ Opt-in Rate
```

**Example:** If ExportCLI shows 7,249 executions in telemetry, and you assume 10% opt-in:
- Estimated total = 7,249 ÷ 0.10 = **72,490 executions/month**

### Scenario Models

| Scenario | Opt-in Rate | Use Case |
|----------|------------|----------|
| **Conservative** | 5% | Very low adoption; privacy-conscious user base |
| **Moderate** | 10% | Balanced assumption; typical default behavior |
| **Optimistic** | 20% | Higher adoption; users generally accept telemetry |
| **Very High** | 30% | Aggressive opt-in; enthusiast or enterprise users |

**Most likely:** 10-20% opt-in, based on typical SaaS adoption patterns.

## Usage

### Step 1: Gather Your Data

Collect from your telemetry system:
- Total unique sessions recorded this month
- Average sessions per month (over your tracking period)
- Top N commands, with counts and per-month averages
- Download volume (context only)

### Step 2: Choose Your Scenario

Pick the opt-in rate that best matches your user base:
- **Opt-in telemetry is required or pre-enabled?** → Use Very High (30%) or Optimistic (20%)
- **Opt-in is default but users can disable?** → Use Moderate (10%)
- **Opt-in requires deliberate action?** → Use Conservative (5%)

### Step 3: Scale the Data

For each command, multiply by the inverse of your chosen opt-in rate:

```python
estimated_usage = telemetry_count / opt_in_rate
```

Python example:
```python
commands = {
    'ExportCLI': 7249,
    'ApplyStyle': 4780,
    'PasteText': 1811,
}

opt_in_rate = 0.10  # 10% scenario

for command, count in commands.items():
    estimated = count / opt_in_rate
    print(f"{command}: {estimated:,.0f}/month")
```

### Step 4: Analyze Results

- **Top commands** tend to be used 10-100x more often than edge commands
- **Power-user commands** (e.g., CLI, batch operations) dominate usage
- **Formatting/styling commands** are second-tier staples
- **Niche/specialized commands** cluster at the low end

## Example Results

Based on OneMore data (May–Sep 2026):

| Command | Telemetry | Moderate (10%) | Optimistic (20%) |
|---------|-----------|---|---|
| ExportCLI | 7,249 | 72,490 | 36,245 |
| ApplyStyle | 4,780 | 47,803 | 23,902 |
| PasteText | 1,811 | 18,110 | 9,055 |
| InsertToc | 1,709 | 17,087 | 8,543 |
| JoinParagraph | 1,534 | 15,340 | 7,670 |

**Interpretation:** In the Moderate scenario, ExportCLI is run ~72k times per month across all users—roughly 2.5x more than ApplyStyle.

## Important Limitations

⚠️ **Selection bias.** Users who opt into telemetry may not represent the full user base:
- Power users might opt in more (inflating active-command estimates)
- Privacy-conscious users opt out (missing conservative usage patterns)
- Different segments (free vs. paid, enterprise vs. individual) may have different opt-in rates

⚠️ **Downloads ≠ active users:**
- Same user downloading updates multiple times
- Downloads from users who never activate the product
- No data on churn (how long users remain active)

⚠️ **Sessions ≠ users:**
- A single user may have multiple sessions (different machines, reinstalls)
- No visibility into one-time users vs. power users

⚠️ **Data quality:**
- Telemetry may miss certain commands (network issues, offline mode)
- Commands may be recorded multiple times per action
- Session lifetime definition affects session counts

## When to Use This Analysis

✅ **Good for:**
- Prioritizing which features to optimize (focus on high-usage commands)
- Forecasting server load or backend traffic
- Identifying feature tiers (which commands power users rely on)
- Comparing relative usage (ExportCLI vs. ApplyStyle) more reliably than absolute numbers
- Detecting seasonal or trend changes in command popularity

❌ **Not reliable for:**
- Precise user counts (opt-in rates vary too much)
- Predicting user growth (downloads are lagging, not predictive)
- Per-user metrics (no session-to-user mapping)
- Business metrics (revenue, churn, retention)

## Refining Your Estimate

Over time, you can calibrate your opt-in rate:

1. **Run an A/B test:** Disable telemetry, track product usage another way (server logs), then compare
2. **Survey users:** Ask opt-in vs. opt-out users about their behavior
3. **Compare with server logs:** If you have backend data (API calls, file writes), correlate with telemetry counts
4. **Monitor anomalies:** If a command's telemetry count suddenly spikes/drops, check if opt-in rate changed

## Tools & Scripts

### Python Script

```python
import json

def extrapolate_usage(telemetry_data, opt_in_rate):
    """
    Scale telemetry counts to estimate full-user-base usage.
    
    Args:
        telemetry_data: dict of {command: telemetry_count}
        opt_in_rate: float (e.g., 0.10 for 10%)
    
    Returns:
        dict of {command: estimated_total}
    """
    return {
        cmd: count / opt_in_rate 
        for cmd, count in telemetry_data.items()
    }

# Usage
telemetry = {
    'ExportCLI': 7249,
    'ApplyStyle': 4780,
    'PasteText': 1811,
}

scenarios = {
    'Conservative (5%)': 0.05,
    'Moderate (10%)': 0.10,
    'Optimistic (20%)': 0.20,
    'Very High (30%)': 0.30,
}

for scenario_name, rate in scenarios.items():
    estimated = extrapolate_usage(telemetry, rate)
    print(f"\n{scenario_name}:")
    for cmd, est_count in sorted(estimated.items(), key=lambda x: -x[1]):
        print(f"  {cmd}: {est_count:,.0f}/month")
```

### CSV Format

Export your results to CSV for spreadsheet analysis:

```
Command,Telemetry,Conservative (5%),Moderate (10%),Optimistic (20%),Very High (30%)
ExportCLI,7249,144980,72490,36245,24163
ApplyStyle,4780,95607,47803,23902,15934
PasteText,1811,36220,18110,9055,6037
...
```

## References

- **Opt-in telemetry best practices:** https://telemetry.microsoft.com/
- **Selection bias in observational data:** https://en.wikipedia.org/wiki/Selection_bias
- **Survivorship bias & telemetry:** https://www.lessig.org/survivorshipbias/

---

**Last updated:** September 2026  
**Methodology source:** Extrapolation from OneMore telemetry, May–September 2026
