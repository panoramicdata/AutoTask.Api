$ErrorActionPreference = "Stop"

# This script will publish to nuget using the api key in nuget-api-key.txt in the same folder.
# The api key issued by nuget.org should ideally only have permissions to update a single package
# with new versions.

$apiKeyFilename = "nuget-api-key.txt";
if(-not (Test-Path($apiKeyFilename))){
	Write-Error "$apiKeyFilename does not exist"
	exit 1;
}
$apiKey = Get-Content $apiKeyFilename;

# Getting changes into master branch
Write-Information -InformationAction Continue "Fetching latest commits..."
&git fetch

$branch= &git rev-parse --abbrev-ref HEAD
if ($branch -ne "master") {
	$title = "Not on master branch - confirm that you want to merge the current branch into master and release."
	$message = "Do you want to merge and publish?"
	$yes = New-Object System.Management.Automation.Host.ChoiceDescription "&Yes", "Merges current branch to master and publishes."
	$no = New-Object System.Management.Automation.Host.ChoiceDescription "&No", "Aborts execution."
	$options = [System.Management.Automation.Host.ChoiceDescription[]]($yes, $no)
	$result = $host.ui.PromptForChoice($title, $message, $options, 0)
	switch ($result)
   {
		0 { Write-Information -InformationAction Continue "Proceeding..." }
		1 { Write-Information -InformationAction Continue "ABORTED."; exit 1; }
	}

	try {
		Write-Information -InformationAction Continue "Checking out master..."
		&git checkout master
		if (-not $?) {throw "Error with git checkout"}

		Write-Information -InformationAction Continue "Pulling..."
		&git pull
		if (-not $?) {throw "Error with git pull"}

		Write-Information -InformationAction Continue "Merging $branch into master..."
		&git merge $branch --no-edit
		if (-not $?) {throw "Error with git merge"}

		Write-Information -InformationAction Continue "Pushing master..."
		&git push
		if (-not $?) {throw "Error with git push"}
	}
	catch
	{
		# If there was a problem and we were not on master then switch back
		if ($branch -ne "master") {
			Write-Information -InformationAction Continue "Switching back to $branch branch"
			&git checkout $branch
		}
		exit 1;
	}
}


try {

	# Build and test
	dotnet build -c Release
	#dotnet build ..\AutoTask.Api -c Release
	#dotnet test ..\AutoTask.Api.Test -c Release
	#if ($lastexitcode -ne 0) {
		#Write-Error "One or more tests failed. Aborting..."
		#exit 1;
	#}

	dotnet pack -c Release

	$mostRecentPackage = Get-ChildItem bin\Release\*.nupkg | Sort-Object LastWriteTime | Select-Object -last 1
	Write-Information -InformationAction Continue "Publishing $mostRecentPackage..."
	# If you don't have nuget.exe - download from https://www.nuget.org/downloads and place in "C:\Users\xxx\AppData\Local\Microsoft\WindowsApps"
	nuget.exe push -Source https://api.nuget.org/v3/index.json -ApiKey $apiKey "$mostRecentPackage"
}
finally
{
	# If we were not on master then switch back
	if ($branch -ne "master") {
		Write-Information -InformationAction Continue "Switching back to $branch branch"
		&git checkout $branch
	}
}