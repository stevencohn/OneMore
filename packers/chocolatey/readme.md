# Steps to Publish OneMore to Chocolatey

1. Run prepare.ps1 (you've done this)

   `.\packers\prepare.ps1 -version 6.8.4`

   This updates the nuspec, install script, and VERIFICATION.txt with the new version and checksums.

2. Build the .nupkg package

   `choco pack .\packers\chocolatey\onemore.nuspec`

   This creates onemore.6.8.4.nupkg in the current directory.

3. (Optional) Validate locally

## Install Chocolatey package validator (if not already installed)

`choco install chocolatey-package-validator`

## Run validation

`choco-validator.exe onemore.6.8.4.nupkg`

  This catches common issues before submission.

4. Get an API key
  - Sign in at https://chocolatey.org/
  - Go to Account Settings → API Key
  - Copy your API key

5. Register the API key locally
  choco apikey -k <your-api-key> -s https://push.chocolatey.org/

6. Push the package
  choco push onemore.6.8.4.nupkg -s https://push.chocolatey.org/

7. Wait for moderation
  - Your package goes to the moderation queue (usually 1–2 days)
  - Chocolatey team reviews it and either approves or requests changes
  - Check https://chocolatey.org/packages/onemore for status

If rejected, the moderators will comment on what needs fixing. Run prepare.ps1 again to update the problematic files, rebuild, and resubmit