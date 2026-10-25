# Public website and Windows release publishing

Vinu Vinu offers three distinct things:

1. **Source:** https://github.com/shanmukhdatta/Vinu_Vinu/tree/main/src/VinuVinu — MIT-licensed C#/.NET 10 WPF app source (original SoundSync MIT notice retained).
2. **Binary release:** https://github.com/shanmukhdatta/Vinu_Vinu/releases — durable, public, versioned Windows x64 ZIP assets. The release builder in .github/workflows/release-windows.yml rebuilds the app, runs tests, copies MIT notices and publishes checksum text. Actions build artifacts from CI are temporary and not equivalent to permanent Releases.
3. **Website source:** https://github.com/shanmukhdatta/Vinu_Vinu/tree/main/website — static public landing page, automatically deployed by GitHub Pages on commits when Pages is enabled.

## Enable Pages once

The GitHub Actions workflow cannot enable a repo's Pages site using the default GITHUB_TOKEN. The repository administrator must navigate to **Settings → Pages → Build and deployment → Source → GitHub Actions** to enable it if needed. This one-time setting is handled by GitHub and cannot be assumed enabled just because a workflow exists.

After selection, re-run the **Website | GitHub Pages** workflow. The expected URL is https://shanmukhdatta.github.io/Vinu_Vinu/ . Do not claim it is live until the deployment workflow succeeds and the URL can be opened.

## Maintaining download links

The website starts with a button to the Releases overview. JavaScript requests the public GitHub Releases API and, when it confirms a public asset named VinuVinu-win-x64.zip, uses the verified asset's download URL instead. If GitHub's API is rate-limited or no release exists, the button remains the Releases link.

For new releases, update the version in src/VinuVinu/VinuVinu.csproj; push a matching vX.Y.Z tag or run the release workflow from main. The first v0.2.0 release is built automatically when the release workflow is first merged. Never hardcode an unverified download path in the site.

**Security:** Releases contain an unsigned self-contained EXE. Users can verify the SHA256SUMS.txt or build from source. The website intentionally has no audio-capture or user-account functionality.
