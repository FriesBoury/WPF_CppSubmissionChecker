# WPF_CppSubmissionChecker
WPF application that speeds up the checking process of a large number of C++ Projects
This tool allows you to check student submissions one by one, without the need of unpacking everything first.

<h1>Usage instructions</h1>
Download the <b>submissions.zip</b> from the assignment you want to check and open it in the CppSubmissionChecker.
Every submission inside the submissions.zip should be a .zip file where the name starts with the studentname_ 
(.rar or .7z are also supported, but may result in unstable behavior).

<h2>First Use</h2>
The first time the program is started, the preferences window will open.
There you can select the Visual Studio Location and a Temp Folder Location.
These settings <b>must be configured</b> before you can use the application.

<h2>Temp Folder</h2>
The temp folder will be used to unpack the submissions to.
A folder /SubmissionChecker will be created inside that folder.
Every time a new submission gets opened, the /SubmissionChecker folder will be cleaned to prevent cluttering your disk.

<h2>Keep Open</h2>
In the Directory Tree view under the <b>Project Source</b> tab, there's a checkbox next to each file that can be previewed.
By checking the checkbox, you mark that file for "Keep Open".
Selecting another submission will then automatically open files with the same relative path of the newly opened submission.
You can specify the folder name this relative path has to start with in order to match with the checked files in the preferences window.

<h1>Feedback</h1>
Please feel free to provide input on how to improve this application by creating an issue.


