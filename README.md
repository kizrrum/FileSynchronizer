# FileSynchronizer
 
 To start working, simply specify the paths to the source and target directories in FileSynchronizer.ini.

А program for synchronizing two directories - the source and the target. This program allows you to automatically copy files from the source to the target directory if they have been modified or added to the source. Also, if files or directories have been deleted in the source, they will be removed in the target directory as well.

The program is very easy to use and does not require any special knowledge. To get started, you just need to specify the path to the source and target directories. After that, the program will start monitoring changes in the source directory and automatically copy modified files to the target directory.

In program has a feature that allows it to work as a service. To enable this feature, you need to use the -install argument.
The delay time between checking for changes, you need to use the -delay argument.

Options will be added later:
Additionally, the program has the ability to adjust synchronization parameters, ignoring certain files or directories, and others.

