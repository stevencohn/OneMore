# Steps to Publish OneMore to Chocolatey

1. Run prepare.ps1 (you've done this)

   ```powershell
   $omVersion = '6.8.4'
   .\prepare.ps1 -version $omVersion
   ```

   This updates the nuspec, install script, and VERIFICATION.txt with the new version and checksums.

2. Build the .nupkg package

   ```powershell
   choco pack .\chocolatey\onemore.nuspec
   ```

   This creates `onemore.$omVersion.nupkg` in the current directory.

## Publish and Validate

4. [ONE TIME] Get an API key

   - Sign in at https://chocolatey.org/
   - Go to Account Settings → API Key
   - Copy your API key
   - Store it as a system environment variable "CHOCO_APIKEY"

5. Register the API key locally

   ```powershell
   choco apikey -k $env:CHOCO_APIKEY -s https://push.chocolatey.org/
   ```

6. Push the package

   ```powershell
   choco push onemore.$omVersion.nupkg -s https://push.chocolatey.org/
   ```

7. Wait for moderation

   - Your package goes to the moderation queue (usually 1–2 days)
   - Chocolatey team reviews it and either approves or requests changes
   - Check https://chocolatey.org/packages/onemore for status

   If rejected, the moderators will comment on what needs fixing. Run prepare.ps1 again to update the problematic files, rebuild, and resubmit