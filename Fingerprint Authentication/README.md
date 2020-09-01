This application requires arguments to start.   

To run this application from Visual Studio, follow the folowing steps:   
1. Open the project in Visual Studio.
2. In your solution explorer, open "Properties".
3. On the left, you'll find some options, click on "Debug".
4. Under the "Start Options" group, put in the fillowing into the "Command line argument" box:
	* To test for enrollment, fill in: 'functionToExecute enroll userID <put in a user ID here>'. Example: functionToExecute enroll userID CSF232.
	* To test for verifiation, fill in: 'functionToExecute verify eventID <put in a event ID here>'. Example: functionToExecute verify eventID 2232.  
	  **Note: The eventID should always be an integer.**

To run this application from command Prompt, follow the folowing steps:   
1. Open Command Prompt.
2. Using "cd", go to the directory where the application is located.
3. Type in the following:
	* To test for enrollment, fill in: '"Fingerprint authentication.exe" functionToExecute enroll userID <put in a user ID here>'. Example: "Fingerprint authentication.exe" functionToExecute enroll userID CSF232
	* To test for verifiation, fill in: '"Fingerprint authentication.exe"functionToExecute verify eventID <put in a event ID here>'. Example: "Fingerprint authentication.exe" functionToExecute verify eventID 2232
	  **Note: The eventID should always be an integer.**