/* Only checks public GitHub Releases. No analytics, cookies, tracking, or audio. */
(() => {
  "use strict";
  const releases = "https://github.com/shanmukhdatta/Vinu_Vinu/releases";
  const endpoint = "https://api.github.com/repos/shanmukhdatta/Vinu_Vinu/releases/latest";
  const action = document.getElementById("download-action");
  const label = document.getElementById("download-label");
  const status = document.getElementById("release-status");
  const bottom = document.getElementById("bottom-download");
  if (!action || !label || !status || !bottom) return;
  function safeAssetUrl(value) {
    try {
      const url = new URL(value);
      return url.protocol === "https:" &&
        (url.hostname === "github.com" || url.hostname === "objects.githubusercontent.com")
        ? url.href : null;
    } catch (_) { return null; }
  }
  fetch(endpoint, { headers: { Accept: "application/vnd.github+json" } })
    .then((response) => {
      if (!response.ok) throw new Error("Release is unavailable");
      return response.json();
    })
    .then((release) => {
      const assets = Array.isArray(release.assets) ? release.assets : [];
      const zip = assets.find((item) => item && item.name === "VinuVinu-win-x64.zip");
      const url = zip && safeAssetUrl(zip.browser_download_url);
      if (!url) throw new Error("Windows asset unavailable");
      const version = typeof release.tag_name === "string" ? release.tag_name : "latest";
      action.href = url;
      label.textContent = "Download for Windows";
      bottom.href = url;
      bottom.textContent = "Download for Windows ↗";
      status.textContent = "Windows x64 · " + version + " · Public GitHub Release";
    })
    .catch(() => {
      action.href = releases;
      label.textContent = "View Windows releases";
      bottom.href = releases;
      status.textContent = "The permanent Windows download will appear here when the release is published.";
    });
})();
