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
### Starting the software
Start the software by double clicking on the AutoSkippy icon or select AutoSkippy from the start menu.  
You should see the following window appear:
<img width="1032" height="685" alt="image" src="https://github.com/user-attachments/assets/14b9fe87-6ebb-4714-85ae-f73d6d98e213" />

### Connecting with your measurement equipment
Select the COM port, on which your device is connected. If the port is not listed in the drop-down box, make sure it's connected, powered on, has the communication enabled, refresh the ports (circled arrow button next to the drop-down box) and try again.  
Press on the "Connect" button.  
If the connection was established successfully, the button caption changes to "Disconnect", effectively allowing to end the connection eith the device.

### Loading existing command set
Load previously created payload (command set file) by pressing the "Browse..." button next to the text field in "Select SCPI payload" section. Select the respective payload form your files. If the payload is loaded successfully, you should see the commands appear in the fields below.

### Creating/editing the payload
The measurement session, defined by the commands in the payload, consists of the following secions:
  - _Setup_ where all the settings of the equipment are provided. Within the session this section is executed once.
  - _Loop_ - the actual measurement, often where **FETC?** command appears. The loop is executed the number of times given in "Repeat" field
  - _Teardown_ - cleaning up after the testing session

#### Command types
There are two types of commands:
  - sent to the device as "fire and forget", hoping for the best, that the device receives the command and executes it.  There is a timeout of 1 second. If send was not successful, it's not signalled, the commands execution continues.  One second delay is applied after sending a command.
  - request for response - this command must end with a question mark "?". After sending it, the software waits for half a second allowing the device to settle down, and then attempts to read the data from COM port. If any data is received (one second timeout), it is printed in the "Results" placeholder.

#### Pre-fetch delay
Certain commands require a delay before sending them. **FETC?** is an example. The delay is required for the measurement to effectively take place, before the result is read.  
*AutoSkippy* provides a mechanism to automate the calculation of the delay. It works in the following way:
  - value commands - this field specifies the commands placed in _Setup_ section, that carry a delay value for certain operations. Specify those commands, comma-separated, and/or add an integer value for fixed delay. All comma-separated values are added to create the total delay, presented in "Calculated delay" field (provided that _Setup_ section is filled in)
  - delayed commands - this field specifies which commands, placed in _Loop_ section, will be delayed before sending to the equipment. Again, comma-separated, the same delay will be applied to all specified commands

Pre-fetch configuration is stored and loaded together with the SCPI payload. I.e. that every payload may have different pre-fetch settings.

### Saving the payload
If a loaded payload was modified, it can be saved (overweritten) by pressing "Save" button. It can also be saved under a different name by pressing "Save as..." button.

### Running the payload
Press "Run" button to start sending the commands to the device. You should see the progress bar advancing. At any moment it is possible to abort the execution by pressing "Abort" button. Once aborted, it is not possible to resume, only to start again from the beginning.

### Transferring the results
Once the results appear in the _Results_ section, it is possible to copy the entire content to the clipboard by pressing "Copy" button.  
It is also possible to select (using mouse cursor) only a portion of the results and copy it to clipboads by pressing Ctrl+C keys.

Paste the copied content to the location of your choice.

### Running another measurement session
You can run another measurement session by simply running the payload again, or load another payload and run it.

The received results will be appended to the current results unless the _Results_ section is not cleared by pressing "Clear" button.

### Exiting the software
You can close the software by pressing the X sign in the corner of the window. Any unsaved changes in your payload will be lost.  
It's a good practice to disconnect the connected device.

The software retains the recently used COM port number and selects it upon the next start, if this port is available.  
The software retains the recently used payload load/save location and uses it on the next start.

## License, releases, and stuff
The software was made for this specific case study, though it is possible to exted it in the future (until it reads email, Zawinski's Law), depending on requests.  

It is provided as is (MIT license), without any warranty, and me, the author, can not be made liable for the usage of any kind.  

Source code is available on GitHub, enjoy.  

Official releases (executables) are not planned as for now (14 July 2026). Contact me if you need an executable. Same if you would like to request specific features.
