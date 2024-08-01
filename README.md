# RUNChecker
Ryan Hayden

## Requirements:

[.NET 8.0.2](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-aspnetcore-8.0.2-windows-x64-installer)
### Deployed To:
    N/A

#-

# Certificate Module

## Purpose
The purpose of this application is to retrieve the expiration dates of SSL certificates from designated websites. It then generates backlog items containing expiration details and sends an email notification to designated recipients with the relevant information.

## Database
<ul>
<li>
Hosts to be checked are defined<br>in [RUN].[RunChecker].[Certificates]

![image-97aafc4e-d20e-4d81-83f5-4a630d4772ef](https://github.com/user-attachments/assets/9e475f7c-1525-45e5-a404-138d74631357)


</li>

<li>
Each certificate in the database is labeled with an App Environment<br>in [RUN].[RunChecker].[AppEnvironments]


![image-615c0efe-b490-4a79-90f1-f34dff3394a0](https://github.com/user-attachments/assets/bbb032c1-8d02-4de0-9e3e-8142d48fefd0)

</li>
<li>
and an Application<br>in [RUN].[RunChecker].[Applications]

![image.png](/.attachments/image-cd70cd86-1795-4281-abc9-06a0300bf677.png)
</li>
<li>
Each Application is labeled with an Area for the backlog item to be created in.<br>in [RUN].[RunChecker].[Areas]

![image-cd70cd86-1795-4281-abc9-06a0300bf677](https://github.com/user-attachments/assets/50bfb509-385d-4d1e-8c00-b81cf92e0f81)

</li>
</ul>

## Example
Below is an example of an email sent by RUNChecker for Certificates.

![image-7b7bde2c-482d-4784-a5d9-37569f874367](https://github.com/user-attachments/assets/ced1c53f-9d94-4e67-9186-be77260219b5)

The emails also contain links to the related backlog item.

## Config
<b>In the appsettings you can specify:</b>
<ul>
<li>
How far away from expiration to create a backlog & email 

![image.png](/.attachments/image-c0adf640-1da6-40e9-8957-6be58e51e9c3.png)
</li>
<li>
Who to send the email to

![image-c0adf640-1da6-40e9-8957-6be58e51e9c3](https://github.com/user-attachments/assets/657b74d1-3153-4267-a21f-f5070665b142)

</li>

<li>
Access tokens to Azure DevOps & SQL Server
</li>

</ul>

## Miscellaneous
After running, the application also updates the database of certificates with their information. This is so queries may be ran in the future.

![image-db3ea4aa-4061-4407-a9ea-67e9c905d459](https://github.com/user-attachments/assets/341b61e1-eb03-4a8c-b049-d004263456bf)


#-

# Service Account Module

## Purpose
The purpose of this module is to check the expiration of Service Accounts in Windows Active Directory.
 Accounts to check are listed in [RUN].[RunChecker].[ServiceAccounts].

![image-6d1ad46e-c786-4f3d-b6f3-b3b9b8dc9d06](https://github.com/user-attachments/assets/4e271b2b-eb53-441f-bb27-1cd2c3994153)


## Information
It acts about the same as the Certificate Checker. Sending information to a backlog item and through an email.

![image-96468a08-a0aa-4ef3-b965-adfa8a92b486](https://github.com/user-attachments/assets/1e05e84e-796d-4531-96f6-23308c761993)


## Config

![image-d4d86d6d-5afd-4a17-ab6b-6fb810a8cabc](https://github.com/user-attachments/assets/4334b370-503e-4e0b-ae55-0621c17e8d0e)

#-

# Database:

## Structure:

####[RUN].[RunChecker].[AppEnvironments]

![image-7b18eb05-af7b-4b07-8255-7e734b653773](https://github.com/user-attachments/assets/e44b18af-8efe-4840-86e2-97fa49e76bd5)

####[RUN].[RunChecker].[Applications]

![image-b43efad5-aa0e-4d22-b130-f6c1ffca591f](https://github.com/user-attachments/assets/201159d6-32af-4f34-81f6-50c063a6e85a)

####[RUN].[RunChecker].[Areas]

![image-76388bf2-e914-44d4-b1f8-812c37cfb1ed](https://github.com/user-attachments/assets/16e90313-69ab-4b83-8456-23e64e5e1d4c)

####[RUN].[RunChecker].[Certificates]

![image-40d6aa33-2f6d-48ba-9bdc-f370bbaaee17](https://github.com/user-attachments/assets/2a68e7ff-3f8e-462e-bd92-af98118301e3)

####[RUN].[RunChecker].[ServiceAccounts]

![image-0615e5c1-fcc9-4a43-9558-b067125d7483](https://github.com/user-attachments/assets/d374e1b9-d83c-4a72-8b59-38de9954aaca)


## Servers:
### Dev
    N/A
### Test
    N/A
### Prod
    N/A
### Tables:
    [RUN].[RunChecker].[AppEnvironments]
    [RUN].[RunChecker].[Applications]
    [RUN].[RunChecker].[Areas]
    [RUN].[RunChecker].[Certificates]
    [RUN].[RunChecker].[ServiceAccounts]

### Visual Studio Information:
    Microsoft Visual Studio Professional 2022
    Version 17.8.5
    VisualStudio.17.Release/17.8.5+34511.84
    Microsoft .NET Framework (8.0)
    Version 4.8.04084
    
    Installed Version: Professional
    
    ADL Tools Service Provider   1.0
    This package contains services used by Data Lake tools
    
    ASA Service Provider   1.0
    
    ASP.NET and Web Tools   17.8.358.6298
    ASP.NET and Web Tools
    
    Azure App Service Tools v3.0.0   17.8.358.6298
    Azure App Service Tools v3.0.0
    
    Azure Data Lake Tools for Visual Studio   2.6.5000.0
    Microsoft Azure Data Lake Tools for Visual Studio
    
    Azure Stream Analytics Tools for Visual Studio   2.6.5000.0
    Microsoft Azure Stream Analytics Tools for Visual Studio
    
    C# Tools   4.8.0-7.23572.1+7b75981cf3bd520b86ec4ed00ec156c8bc48e4eb
    C# components used in the IDE. Depending on your project type and settings, a different version of the compiler may be used.
    
    Common Azure Tools   1.10
    Provides common services for use by Azure Mobile Services and Microsoft Azure Tools.
    
    Microsoft Azure Hive Query Language Service   2.6.5000.0
    Language service for Hive query
    
    Microsoft Azure Stream Analytics Language Service   2.6.5000.0
    Language service for Azure Stream Analytics
    
    Microsoft JVM Debugger   1.0
    Provides support for connecting the Visual Studio debugger to JDWP compatible Java Virtual Machines
    
    NuGet Package Manager   6.8.0
    NuGet Package Manager in Visual Studio. For more information about NuGet, visit https://docs.nuget.org/
    
    SQL Server Data Tools   17.8.120.1
    Microsoft SQL Server Data Tools
    
    ToolWindowHostedEditor   1.0
    Hosting json editor into a tool window
    
    TypeScript Tools   17.0.20920.2001
    TypeScript Tools for Microsoft Visual Studio
    
    Visual Basic Tools   4.8.0-7.23572.1+7b75981cf3bd520b86ec4ed00ec156c8bc48e4eb
    Visual Basic components used in the IDE. Depending on your project type and settings, a different version of the compiler may be used.
    
    Visual F# Tools   17.8.0-beta.23475.2+10f956e631a1efc0f7f5e49c626c494cd32b1f50
    Microsoft Visual F# Tools
    
    Visual Studio IntelliCode   2.2
    AI-assisted development for Visual Studio.
