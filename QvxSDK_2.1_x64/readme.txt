*******************************************************************************
*                             QVX SDK
*                        Version: 2.1.4
*
*
*                     QlikTech International AB
*                          February 20, 2014
*******************************************************************************

The QVX (QlikView data eXchange) SDK helps developers to create their own
QlikView custom connectors. There are two ways of getting data into QlikView
using a connector. You can either use a named pipe to stream data into
QlikView, or let your connector create a QVX file. The QVX SDK has support for
both approaches.

The sample code provided in the Examples folder is free to use and distribute
but is provided 'as-is' without warranties and is not supported. For issues
with the QvxLibrary.dll itself please contact support@qlikview.com.

This package contains:

* Demos\
    EventViewerStandalone.qvw          QlikView application that demonstrates
                                       how to import QVX files into QlikView.

    EventViewerStandNetworkPipe.qvw    QlikView application that demonstrates
                                       how to stream data into QlikView using
                                       the QvEventLogConnectorElaborate
                                       connector.

    QvFacebookConnector Example.qvw    QlikView application that demonstrates
                                       how to stream Facebook data into
                                       the QvEventLogConnectorElaborate
                                       QlikView using the QvFacebookConnector.
* Examples\
    EventLogSimple\                    Visual studio C# project that
                                       demonstrates how to use the QVX library
                                       to create a simple connector.

                                       EventLogSimple works with QlikView 12.

    EventLogElaborate\                 Visual studio C# project that
                                       demonstrates how to use the QVX library
                                       connector can stream data into QlikView
                                       and/or create a QVX file.

    QvFacebookConnector\               Visual studio C# project that
                                       demonstrates how to use the QVX library
                                       to create a connector that streams data
                                       from Facebook into QlikView.
                                       The project includes the Facebook.dll
                                       from csharpsdk.org which is under the
                                       Apache 2.0 license.

* QvxLibrary                           QVX library (DLL, PDB, XML and CHM
                                       files).
* verpatch.exe (Version 1.0.1.6)       Tool for setting file version
                                       information for executable files.


-------------------------------------------------------------------------------
Build name: V12-SDK-JOB1-4
