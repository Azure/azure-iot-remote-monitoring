@IF /I '%1' NEQ '' (
    Set Command=%1)

@IF /I '%2' NEQ '' (
    Set Configuration=%2)

@IF /I '%3' NEQ '' (
    Set EnvironmentName=%3)

@IF /I '%4' NEQ '' (
    Set ActionType=%4)

@IF /I '%5' NEQ '' (
    Set Services=%5)

@IF /I '%6' NEQ '' (
    Set DeploymentLabel=%6)

@IF /I '%7' NEQ '' (
    Set Slot=%7)

@IF /I '%8' NEQ '' (
    Set VipSwap=%8)

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
@SET PublishCmd=%PowerShellCmd% %DeploymentScripts%\PrepareIoTSample.ps1 -environmentName %EnvironmentName% -buildPath %BuildPath%

@IF /I '%Command%' == 'Build' (
    @GOTO :Build)
@IF /I '%Command%' == 'Local' (
    @GOTO :Build)
@IF /I '%Command%' == 'Cloud' (
    @GOTO :Build)
@ECHO Invalid command '%Command%'
@GOTO :Error

:Build
@IF /I '%ActionType%' == 'Clean' (
    rmdir /s /q Build_Output)
msbuild RemoteMonitoring.sln /v:m /p:Configuration=%Configuration%
msbuild DeviceAdministration\Web\Web.csproj /v:m /T:Package /P:VisualStudioVersion=12.0 /p:OutputPath=%~dp0Build_Output\
@IF /I '%ERRORLEVEL%' NEQ '0' (
    @echo Error msbuild IoTRefImplementation.sln /v:m /t:publish /p:Configuration=%Configuration%
    @goto :Error
)
@IF /I '%Command%' == 'Build' (
    @GOTO :End)

:Config
@IF /I '%Services%' NEQ '' (
    @Set PublishCmd=%PublishCmd% -ServiceList %Services%
    )

@IF /I '%DeploymentLabel%' NEQ '' (
    @Set PublishCmd=%PublishCmd% -DeploymentLabel %DeploymentLabel%
    )

@IF /I '%Slot%' NEQ '' (
    @Set PublishCmd=%PublishCmd% -Slot %Slot%
    )

@IF /I '%VipSwap%' NEQ '' (
    @Set PublishCmd=%PublishCmd% -VipSwap %VipSwap%
    )

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
@ECHO Arguments: build.cmd "Command" "Configuration" "EnvironmentName" "ActionType" "Services" "DeploymentLabel" "Slot" "VipSwap"
@ECHO   Command: build (just builds); local (config local and build); cloud (config cloud, build, and deploy)
@ECHO   Configuration: build configuration either Debug or Release; default is Debug
@ECHO   EnvironmentName: Name of cloud environment to deploy - default is local
@ECHO   ActionType: "Clean" flag indicating to clean before build/config - default is not to clean
@ECHO   Services: Comma separated string of services to deploy, eg. "EventProcessor,VendingMachines" - default deploys all services
@ECHO   DeploymentLabel: A label used to describe the deployment - default is timestamped string
@ECHO   Slot: Either production or staging slot - default is staging
@ECHO   VipSwap: Indicates if VIP swap (swap staging and production) should occur after successful deployment - default true
@ECHO
@ECHO eg.
@ECHO   build - build.cmd build
@ECHO   local deployment: build.cmd local
@ECHO   local release clean deployment: build.cmd local release local clean
@ECHO   cloud deployment: build.cmd cloud release mydeployment
@ECHO   cloud deployment with args: build.cmd cloud release mydeployment update "EventProcessor"
:End
@Set Command=
@Set EnvironmentName=
@Set Configuration=
@Set ActionType=
@Set Services=
@Set DeploymentLabel=
@Set Slot=
@Set VipSwap=
