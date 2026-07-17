# AutoSkippy - control your measurement equipment sequentially

## Case study
The following case study was formed:

Certain ESD (Electrostatic discharge) PPE (personal protection equipment) needs to be tested for certification purposes.  
The measurement is performed by placing the object under test in a climate chamber, on a conducting surface, and connecting the probe.  
Both conducting surface and the probe are connected to the meter.  
The measurement procedure are a number of consecutive measurements, where the device needs to charge before each measurement and discharge after.

### The problem
The problem is that the measurement equipment, *IET Labs 1865+ Megaohmmeter* in this case, needs babysitting before and during the measurement.  

All the measurement parameters need to be set beforehand. Often several pieces of the PPE need to be tested, and a limitted number of pieces fits into the climate chamber. It takes about ohe hour for the climate chamber to achieve the required parameters.  
This creates failure point no. 1 (different startup configuration between the measurement sessions).

During the measurement session (a series of consecutive measurements) the operator needs to initiate the measurement, wait for the charging to complete, note down the measurement result, file it in the report form and initiate subsequent measurement.  
Here are the failure point no. 2 (noting the results down and transferring to the report form is prone to human errors) and no. 3 (initiating the measurements at specified time intervals).

### System analysis and user analysis
*IET Labs 1865+ Megaohmmeter* allows interaction o.a. by means of SCPI (Standard Commands for Programmable Instruments). This means that all the manual steps can be automated.

The operator is either not capable or does not have the intention of maintaining advanced scripting system (e.g. Python). They don't want to be involved in the low-level communication aspect of the system.  
They are however tech savvy, can read and understand the SCPI commandset, and they can prepare a set of commands, provided they have a mean of sending them to the equipment in the controlled manner and reading the measurement results back.

### Solution
A GUI program (C#/AvaloniaUI) was perpared that allows:
  - bidirectional communication with the *IET Labs 1865+ Megaohmmeter* through COM port (fixed initialisation parameters, suitable for this device only)
  - defining a script with set of SCPI commands as _Setup_, _Loop_, and _Teardown_ segments, where the number of _Loop_ execution is settable
  - storing and reading back the prepared commands sets
  - running the command set
  - reading back the data sent from the measurement equipment and transferring it to external destination by means of system clipboard

## Instructions for use
User manual (Instructions for use) can be found in the [Manual](MANUAL.md) file.
