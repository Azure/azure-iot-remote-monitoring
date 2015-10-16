@IF /I '%1' NEQ '' (
    Set Command=%1)

@IF /I '%2' NEQ '' (
    Set Configuration=%2)

@IF /I '%3' NEQ '' (
    Set EnvironmentName=%3)

@IF /I '%4' NEQ '' (
    Set ActionType=%4)

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
		Set EnvironmentName=%Command%
	)

@IF /I '%ActionType%' neq 'Clean' (
    Set ActionType=Update)

@IF /I '%Configuration%' == '' (
    Set Configuration=Debug)

@REM ----------------------------------------------
@REM Parse arguments
@REM ----------------------------------------------
@SET DeploymentScripts=%~dp0\Common\Deployment
@SET BuildPath=%~dp0Build_Output\%Configuration%
@SET PowerShellCmd=%windir%\system32\WindowsPowerShell\v1.0\powershell.exe -ExecutionPolicy Unrestricted -Command
@SET PublishCmd=%PowerShellCmd% %DeploymentScripts%\PrepareIoTSample.ps1 -environmentName %EnvironmentName% -configuration %Configuration%

@IF /I '%Command%' == 'Build' (
    @GOTO :Build)
@IF /I '%Command%' == 'Local' (
    @GOTO :Config)
@IF /I '%Command%' == 'Cloud' (
    @GOTO :Build)
@ECHO Invalid command '%Command%'
@GOTO :Error

:Build
@IF /I '%ActionType%' == 'Clean' (
    rmdir /s /q Build_Output)
msbuild RemoteMonitoring.sln /v:m /p:Configuration=%Configuration%

@IF /I '%ERRORLEVEL%' NEQ '0' (
    @echo Error msbuild IoTRefImplementation.sln /v:m /t:publish /p:Configuration=%Configuration%
    @goto :Error)

@IF /I '%Command%' == 'Build' (
    @GOTO :End)

:Package
@REM For Zip based deployments for private repos
msbuild DeviceAdministration\Web\Web.csproj /v:m /T:Package /P:VisualStudioVersion=12.0
@IF /I '%ERRORLEVEL%' NEQ '0' (
    @echo Error msbuild DeviceAdministration\Web\Web.csproj /v:m /T:Package /P:VisualStudioVersion=12.0
    @goto :Error)

msbuild WebJobHost\WebJobHost.csproj /v:m /T:Package /P:VisualStudioVersion=12.0
@IF /I '%ERRORLEVEL%' NEQ '0' (
    @echo Error msbuild WebJobHost\WebJobHost.csproj /v:m /T:Package /P:VisualStudioVersion=12.0
    @goto :Error)

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
@ECHO Arguments: build.cmd "Command" "Configuration" "EnvironmentName" "ActionType"
@ECHO   Command: build (just builds); local (config local); cloud (config cloud, build, and deploy)
@ECHO   Configuration: build configuration either Debug or Release; default is Debug
@ECHO   EnvironmentName: Name of cloud environment to deploy - default is local
@ECHO   ActionType: "Clean" flag indicating to clean before build/config - default is not to clean
@ECHO
@ECHO eg.
@ECHO   build - build.cmd build
@ECHO   local deployment: build.cmd local
@ECHO   cloud deployment: build.cmd cloud release mydeployment
:End
@Set Command=
@Set EnvironmentName=
@Set Configuration=
@Set ActionType=

