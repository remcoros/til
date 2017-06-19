# Add-Type caches the -Source parameter

So that it doesn't (have to) re-create the declared types in the source. 

If you change the source, but don't restart the powershell session, it will give an error like this:

    Add-Type : Cannot add type. The type name 'TodayILearned.ReadmeGenerator' already exists.
    At C:\Projects\til\UpdateReadme.ps1:8 char:1
    + Add-Type -TypeDefinition $Source
    + ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        + CategoryInfo          : InvalidOperation: (TodayILearned.ReadmeGenerator:String) [Add-Type], Exception
        + FullyQualifiedErrorId : TYPE_ALREADY_EXISTS,Microsoft.PowerShell.Commands.AddTypeCommand

It's most likely because the type is directly added the one AppDomain in use by the powershell session.

**Restart the powershell session to fix**
