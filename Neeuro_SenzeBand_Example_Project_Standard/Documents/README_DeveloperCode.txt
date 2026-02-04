IMPORTANT! The NEEURO SenzeBand SDK supports Android, IOS and Windows platforms through Unity3D game engine.


This document has 3 sections:
(A) About Developer Code
(B) How to use the Developer Code
(C) Support contact


——————————————————————————————————————————————————————————————————————————————

(A) About Developer Code

Your Developer Code is a unique ID that is given when you register as a Developer with Neeuro.
Please contact support@neeuro.com if you have not received your Developer Code.

In order to use the SENZEBAND-SDK-STANDARD functions, the app has to use the Developer Code to authenticate with Neeuro’s servers.
Internet connection is needed. Every successful authentication will give a period of time for the app to use the SDK functions. 



——————————————————————————————————————————————————————————————————————————————

(B) How to use the Developer Code

Demo 1 ("demo" scene - uses a Manager )
	Setup
	1. Select NSB_Manager game object in Hierarchy window, look for DEVELOPER CODE in Inspector. Set it to your Developer Code.

	Helper functions
	1. In NSB_Manager.cs, use GetAuthenticationStatus() to retrieve whether the app is successful with authenticating with Neeuro's Server. Required to have authentication in order to obtain the data from the SenzeBand (EEG data, mental states, frequency bands, and accelerometer values). At every connection to SenzeBand, the app will try to authenticate. Hence it is necessary to have Internet connection.
	2. In NSB_Manager.cs, we have a UnityEvent called authenticationUpdated that is triggered when authentication result is sent from SenzeBand to Unity or every second after authentication is successful. In the sample, it is used to show how many seconds left before authentication expires.



——————————————————————————————————————————————————————————————————————————————


(C) Support contact

For support, please email to support@neeuro.com


--------------------------------------------------------------------------------
Copyright © 2017 NEEURO PTE LTD. All rights reserved.
