# Synchronize_folder_to_RavenDB
Synchronize folder to RavenDB using C#<br />
An application that synchronizes a folder of JSON documents to a RavenDB database.<br />
The application is designed to be invoked(executed) every few minutes using the following command:<br />
	sync_folder_to_db /path/to/folder http://live-test.ravendb.net db_name
Assuming that the command is always invoked with the same parameters (the same from the first run).<br />
The application accepts three arguments:
●	The path to the folder to sync
●	The URL of the RavenDB server
●	The database name to use
You need to check only the files in the provided folder, you can assume that there are no subfolders in the specified folder.<br />
On each invocation of the application, you need to store to RavenDB all new files that does not exists already in the database, delete documents that represent files that were removed on the folder and update any document that was changed since it the last update.
The amount of work required per run should be proportional to the number of changes that happened to the files on the folder and not to the total number of files. <br />
In other words, if the folder contains 10,000 files and only 5 of them changed since the last invocation, you cannot read 10,000 documents from RavenDB to compare them.<br />
The files in the folder are modified by a 3rd party process over which you have no control. You can assume that files aren’t being modified while you are running the application, 
but you can make no assumptions about the state of the files between invocation of the application.
<br />
<br />
* C# & .Net 6.0 Application <br />
* Used RavenDB.Client (nuget) dependency.<br />
  (PM> Install-Package RavenDB.Client -Version 5.3.0)<br />
  https://www.nuget.org/packages/RavenDB.Client/<br />
