# Read the Versions.props file
[xml]$versionsProps = Get-Content -Path 'Versions.props'

# Get the value of MicrosoftBuildPackageVersion
$version = $versionsProps.Project.PropertyGroup.MicrosoftBuildPackageVersion

# Read the SourceBuildPrebuiltBaseline.xml file
[xml]$sourceBuild = Get-Content -Path 'SourceBuildPrebuiltBaseline.xml'

# Replace all '17.11.0-preview-24314-04' in IdentityGlob
foreach ($usagePattern in $sourceBuild.DocumentElement.UsagePattern) {
    if ($usagePattern.IdentityGlob -like '*17.11.0-preview-24314-04*') {
        $usagePattern.IdentityGlob = $usagePattern.IdentityGlob.Replace('17.11.0-preview-24314-04', $version)
    }
}

# Save the modified SourceBuildPrebuiltBaseline.xml file
$sourceBuild.Save('SourceBuildPrebuiltBaseline.xml')
