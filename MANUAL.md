## AutoSkippy - Instructions for use
### Starting the software
Start the software by double clicking on the AutoSkippy icon or select AutoSkippy from the start menu.  
You should see the following window appear:
<img width="1085" height="752" alt="image" src="https://github.com/user-attachments/assets/8d95005f-8c61-4880-b0e6-36902c87f19b" />

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

### Monitoring the progress
Progress of the current payload can be monitored by observing the progress bar - each command advances the progress bar the same step.  
Furthermore, the recent command sent is displayed in the *Recent command* field. Also the pre-fetch delay duration is displayed only in the *Recent command* field.

### Transferring the results
Once the results appear in the _Results_ section, it is possible to copy the entire content to the clipboard by pressing "Copy" button.  
It is also possible to select (using mouse cursor) only a portion of the results and copy it to clipboads by pressing Ctrl+C keys or pressing right mouse button and pressing on *Copy selected* in the context menu.

Paste the copied content to the location of your choice.

It may happen that the decimal separator returned by the device does not match the one of your system.  
Since it is not possible to reconfigure the device, the software allows to replace all the dots in the *Results* field to commas and the other way round. Check if it's necessary by comparing the decimal separator in the results with your system's detected setting, which is displayed next to the *Dots to commas* and *Commas to dots* buttons.

### Running another measurement session
You can run another measurement session by simply running the payload again, or load another payload and run it.

The received results will be appended to the current results unless the _Results_ section is not cleared by pressing "Clear" button.

### Running single steps
It is possible to run a single step, e.g. setup, loop, or teardown, by clicking the right mouse button and selecting *Run once* from the context menu.

### Exiting the software
You can close the software by pressing the X sign in the corner of the window. Any unsaved changes in your payload will be lost.  
It's a good practice to disconnect the connected device.

The software retains the recently used COM port number and selects it upon the next start, if this port is available.  
The software retains the recently used payload load/save location and uses it on the next start.
