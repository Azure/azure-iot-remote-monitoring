@Set Command=
@Set EnvironmentName=
@Set Configuration=
@Set AzureEnvironmentName=

@IF /I '%1' NEQ '' (
    Set Command=%1)

@IF /I '%2' NEQ '' (
    Set Configuration=%2)

@IF /I '%3' NEQ '' (
    Set EnvironmentName=%3)

@IF /I '%4' NEQ '' (
    Set AzureEnvironmentName=%4)

@REM ----------------------------------------------
@REM Validate arguments
@REM ----------------------------------------------

@IF '%Command%' == '' (
    @ECHO Command was not provided
    @GOTO :Error)

@IF /I '%Command%' == 'Cloud' (
    @IF '%EnvironmentName%' == '' (
        @ECHO EnvironmentName was not provided
        @GOTO :Error)
) ELSE (
    Set AzureEnvironmentName=%EnvironmentName%
    Set EnvironmentName=%Command%
)

@IF /I '%Configuration%' == '' (
    Set Configuration=Debug)

@REM ----------------------------------------------
@REM Parse arguments
@REM ----------------------------------------------
@SET DeploymentScripts=%~dp0\Common\Deployment
@SET BuildPath=%~dp0Build_Output\%Configuration%
@SET PowerShellCmd="%windir%\system32\WindowsPowerShell\v1.0\powershell.exe" -ExecutionPolicy Unrestricted -Command
@SET PublishCmd=%PowerShellCmd% "& ""%DeploymentScripts%\PrepareIoTSample.ps1""" -environmentName %EnvironmentName% -configuration %Configuration% %SuiteArgs%
@SET PackageCmd=%PowerShellCmd% "& ""%DeploymentScripts%\PackageIoTSample.ps1""" -configuration %Configuration%

@IF /I '%AzureEnvironmentName%' NEQ '' (
    @Set PublishCmd=%PublishCmd% -azureEnvironmentName %AzureEnvironmentName%
)

@%PowerShellCmd% "if (!('%EnvironmentName%' -match '^(?![0-9]+$)(?!-)[a-zA-Z0-9-]{3,49}[a-zA-Z0-9]{1,1}$')) { throw 'Invalid EnvironmentName' }"
@IF /I '%ERRORLEVEL%' NEQ '0' (
    @echo Error EnvironmentName - '%EnvironmentName%' must start with a letter, end with a letter or number, between 3-50 characters in length, and only contain letters, numbers and dashes
    @echo
    @goto :Error)

@IF /I '%Command%' == 'Build' (
    @GOTO :Build)
@IF /I '%Command%' == 'Local' (
    @GOTO :Config)
@IF /I '%Command%' == 'Cloud' (
    @GOTO :Build)
@IF /I '%Command%' == 'Package' (
    @GOTO :Build)

@ECHO Invalid command '%Command%'
@GOTO :Error

:Build
msbuild RemoteMonitoring.sln /v:m /p:Configuration=%Configuration%

@IF /I '%ERRORLEVEL%' NEQ '0' (
    @echo Error msbuild IoTRefImplementation.sln /v:m /t:publish /p:Configuration=%Configuration%
    @goto :Error)

@IF /I '%Command%' == 'Build' (
    @GOTO :End)

:Package
@REM For Zip based deployments for private repos
msbuild DeviceAdministration\Web\Web.csproj /v:m /T:Package
@IF /I '%ERRORLEVEL%' NEQ '0' (
    @echo Error msbuild DeviceAdministration\Web\Web.csproj /v:m /T:Package
    @goto :Error)

msbuild WebJobHost\WebJobHost.csproj /v:m /T:Package
@IF /I '%ERRORLEVEL%' NEQ '0' (
    @echo Error msbuild WebJobHost\WebJobHost.csproj /v:m /T:Package
    @goto :Error)

@IF /I '%Command%' == 'Package' (
    %PackageCmd%
    @GOTO :END)

:Config
%PublishCmd%

@IF /I '%ERRORLEVEL%' NEQ '0' (
    @echo Error %PublishCmd%
    @goto :Error
)

@GOTO :End

:Error
@REM ----------------------------------------------
@REM Help on errors
@REM ----------------------------------------------
@ECHO Arguments: build.cmd "Command" "Configuration" "EnvironmentName" "AzureEnvironment"
@ECHO   Command: build (just builds); package (package build into zip files); local (config local); cloud (config cloud, build, and deploy)
@ECHO   Configuration: build configuration either Debug or Release; default is Debug
@ECHO   EnvironmentName: Name of cloud environment to deploy - default is local
@ECHO   AzureEnvironment: Name of the Azure Environment to deploy to - default is AzureCloud
@ECHO
@ECHO eg.
@ECHO   build - build.cmd build
@ECHO   package release build - build.cmd package release
@ECHO   local deployment: build.cmd local
@ECHO   cloud deployment: build.cmd cloud release mydeployment
@ECHO   national cloud deployment: same as above but include CloudName at end (eg. build.cmd local debug AzureGermanCloud or build.cmd cloud release mydeployment AzureGermanCloud)
:End
