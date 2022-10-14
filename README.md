# EAS-Inspector-For-Fiddler
This is an inspector add-in for Fiddler and is used in Fiddler to convert the binary content of Exchange ActiveSync (EAS) payloads from binary data (EAS WBXML) into non-bianary XML.

This application does not collect or transfer any data.  It only does a data conversion content Fidler is rendering if it is of EAS content.

To install:
-----------

Build the appliction and copy the EASInspectorFiddler.dll to the Inspector's folder of Fiddler.

Example:

	C:\Users\contosouser\AppData\Local\Programs\Fiddler\Inspectors\


To use:
-------

After intalling the EASInspectorFiddler.dll just start Fiddler and either trace or load a Fiddler trace file.

Select a line/frame of ActiveSync (EAS) traffic.  On the right is a horisontally split window which shows requests at the top
and responses starting in the middle.  You should see an "EAS XML" tab listed in each of these sections. Click on 
the "EAS XML" tabs of these sections".  When ActiveSync content is being viewed you should see the decoded content in those tabs.

