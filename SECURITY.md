# Security policy

## Supported area

Security review currently covers the actively maintained desktop app in \`src/VinuVinu/\`, the local streaming server, input validation, configuration storage and the diagnostic/verifier tools. The code in \`legacy/\` is retained for historical purposes and is not recommended for production use.

## Report vulnerabilities privately

**Do not open public issues containing exploit details, tokens, private endpoint IDs or personal audio.** Use the repository's **Security** tab and private vulnerability-reporting / advisory function if GitHub makes it available to you, or privately contact the repository maintainer using an available GitHub contact method. Never paste secrets into public comments or PRs.

Please provide the affected commit, Windows version, affected feature, reproduction steps with dummy/sanitized data, realistic impact and a proposed fix if known. We cannot promise a fixed response deadline or that a newly reported bug can immediately be reproduced.

## Precautions

- A browser streaming URL may carry a bearer token. Treat it as a secret, do not share it in screenshots/logs and invalidate it if it leaks.
- Remote control must remain opt-in.
- A diagnostic JSON should not include credentials, raw audio or stable hardware identifiers; endpoint friendly names and OS details can still be identifying, so review before sharing.
- Windows uses native audio drivers and endpoint permissions; do not advise users to disable security controls to make audio work.

Please verify security-sensitive changes with focused automated tests and clearly stated manual validation.
