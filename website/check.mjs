// Dependency-free site verification for GitHub Pages and PRs.
import { readFileSync, existsSync } from "node:fs";
import { join, dirname } from "node:path";
import { fileURLToPath } from "node:url";
const dir = dirname(fileURLToPath(import.meta.url));
const html = readFileSync(join(dir, "index.html"), "utf8");
const css = readFileSync(join(dir, "styles.css"), "utf8");
const script = readFileSync(join(dir, "app.js"), "utf8");
const assertions = [
  ["HTML document and landmark", html.startsWith("<!doctype html>") && html.includes('<main id="main">')],
  ["Visible download action", html.includes('id="download-action"') && html.includes('id="bottom-download"')],
  ["Public source link", html.includes("https://github.com/shanmukhdatta/Vinu_Vinu")],
  ["Honest Bluetooth limitations", html.includes("not guaranteed") && html.includes("Windows drivers")],
  ["Explicit upstream author credit", html.includes("Sugumar S")],
  ["Mobile viewport and responsive layout", html.includes('name="viewport"') && css.includes("@media(max-width:580px)")],
  ["Keyboard accessibility", css.includes(":focus-visible") && html.includes("Skip to main content")],
  ["No external CSS frameworks", !html.includes("cdn.") && !html.includes("fonts.googleapis.com")],
  ["Release asset guarded by API response", script.includes('item.name === "VinuVinu-win-x64.zip"') && script.includes(".catch(")],
  ["No client audio capture", !script.includes("getUserMedia") && !script.includes("MediaRecorder")]
];
const ids = [...html.matchAll(/\bid="([^"]+)"/g)].map((match) => match[1]);
const duplicates = ids.filter((v, i) => ids.indexOf(v) !== i);
assertions.push(["No duplicate element IDs", duplicates.length === 0]);
for (const name of ["styles.css", "app.js", "favicon.svg", ".nojekyll"]) {
  assertions.push(["Local asset " + name, existsSync(join(dir, name))]);
}
for (const match of html.matchAll(/href="#([^"]+)"/g)) {
  assertions.push(["Navigation anchor #" + match[1], ids.includes(match[1])]);
}
for (const [label, passed] of assertions) console.log((passed ? "PASS" : "FAIL") + " " + label);
if (assertions.some(([, passed]) => !passed)) process.exit(1);
console.log("Static site checks completed: " + assertions.length);
