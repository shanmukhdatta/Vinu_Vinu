# Vinu Vinu website

Static HTML, CSS and JavaScript under the website folder. No build tool, framework, cookies, analytics or audio capture.

Files: index.html (page), styles.css (responsive theme), app.js (public GitHub Release lookup), favicon.svg, and check.mjs (dependency-free test).

The download button defaults to GitHub Releases. It switches to a permanent Windows ZIP asset only when the GitHub Releases API confirms an asset named VinuVinu-win-x64.zip.

## Publish through GitHub Pages

The automated workflow is .github/workflows/pages.yml.
If GitHub Pages has not been enabled for the repository, open Settings -> Pages -> Build and deployment and select GitHub Actions as the source. Then run the Website | GitHub Pages workflow, or update website files on main.

Expected URL after a successful deployment: https://shanmukhdatta.github.io/Vinu_Vinu/. Verify a successful deployment before calling it live.

The site only describes the native Windows app. Release binaries are stored on public GitHub Releases, not in the website folder.
