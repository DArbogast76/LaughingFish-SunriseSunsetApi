namespace LaughingFish.SunriseSunsetApi.Documentation;

public static class ApiDocumentationHtml
{
    public static string Build(string baseUrl)
    {
        var host = string.IsNullOrWhiteSpace(baseUrl)
            ? "https://func-sunrisesunsetapi-prod01-fmbmcnaqbjfsdgej.centralus-01.azurewebsites.net"
            : baseUrl.TrimEnd('/');

        return $$"""
<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="utf-8">
  <meta name="viewport" content="width=device-width, initial-scale=1">
  <title>LaughingFish Sunrise Sunset API</title>
  <style>
    :root {
      --ink: #1c2430;
      --muted: #5b6573;
      --line: #d7dde5;
      --paper: #f6f8fa;
      --card: #ffffff;
      --accent: #1f4e79;
      --accent-soft: #e8f0f7;
      --code: #0f1720;
    }
    * { box-sizing: border-box; }
    body {
      margin: 0;
      font-family: "Segoe UI", Calibri, system-ui, sans-serif;
      color: var(--ink);
      background: var(--paper);
      line-height: 1.55;
    }
    header {
      background: var(--accent);
      color: #fff;
      padding: 2rem 1.5rem 1.6rem;
    }
    header p { margin: 0.4rem 0 0; opacity: 0.9; max-width: 52rem; }
    main { max-width: 52rem; margin: 0 auto; padding: 1.5rem; }
    h1 { margin: 0; font-size: 1.7rem; font-weight: 600; }
    h2 { margin: 2rem 0 0.6rem; font-size: 1.25rem; color: var(--accent); }
    h3 { margin: 1.2rem 0 0.4rem; font-size: 1.05rem; }
    p, li { color: var(--ink); }
    .lede { color: var(--muted); }
    .card {
      background: var(--card);
      border: 1px solid var(--line);
      border-radius: 8px;
      padding: 1rem 1.15rem;
      margin: 0.8rem 0 1.2rem;
    }
    table {
      width: 100%;
      border-collapse: collapse;
      font-size: 0.95rem;
    }
    th, td {
      text-align: left;
      padding: 0.45rem 0.5rem;
      border-bottom: 1px solid var(--line);
      vertical-align: top;
    }
    th { color: var(--muted); font-weight: 600; width: 28%; }
    code, pre {
      font-family: Consolas, "Cascadia Mono", monospace;
      font-size: 0.86rem;
    }
    code {
      background: var(--accent-soft);
      padding: 0.08rem 0.3rem;
      border-radius: 4px;
    }
    pre {
      background: var(--code);
      color: #e8edf3;
      padding: 0.9rem 1rem;
      border-radius: 8px;
      overflow-x: auto;
      white-space: pre-wrap;
      word-break: break-all;
    }
    ul, ol { padding-left: 1.2rem; }
    footer {
      max-width: 52rem;
      margin: 0 auto 2rem;
      padding: 0 1.5rem;
      color: var(--muted);
      font-size: 0.88rem;
    }
  </style>
</head>
<body>
  <header>
    <h1>LaughingFish Sunrise Sunset API</h1>
    <p>Sunrise, sunset, solar noon, day length, and twilight for a date and location.</p>
  </header>
  <main>
    <p class="lede">Give the API a latitude, a longitude, and a calendar date. It calculates the solar events for that point on that date and returns each event in UTC and in local time.</p>

    <h2>Using the endpoints together</h2>
    <div class="card">
      <ol>
        <li>Call <code>GET /api/health</code> to confirm the service is available.</li>
        <li>Call <code>GET</code> or <code>POST /api/v1/sunrise-sunset</code> with latitude, longitude, and date.</li>
        <li>Read <code>condition</code>, then use the event times that are present in <code>events</code>.</li>
      </ol>
    </div>

    <h2>1. Health</h2>
    <div class="card">
      <p><code>GET /api/health</code></p>
      <p>Returns a short status payload so a caller can confirm the service is available.</p>
      <pre>{{host}}/api/health</pre>
      <h3>Response</h3>
      <ul>
        <li><code>status</code> is <code>ok</code> when the service is available.</li>
        <li><code>utc</code> is the time of the response.</li>
        <li><code>invocationId</code> identifies the call.</li>
      </ul>
    </div>

    <h2>2. Sunrise and sunset</h2>
    <div class="card">
      <p><code>GET</code> or <code>POST /api/v1/sunrise-sunset</code></p>
      <p>Calculates official sunrise and sunset, solar noon, day length, and twilight for the requested date and coordinates. Official rise and set use the standard upper-limb horizon (zenith 90.833°), which includes average refraction.</p>
      <table>
        <tr><th>lat</th><td>Required. Decimal degrees, −90 to 90.</td></tr>
        <tr><th>lon</th><td>Required. Decimal degrees, −180 to 180.</td></tr>
        <tr><th>date</th><td>Required. Civil date as <code>yyyy-MM-dd</code>. Year must be 1900–2100.</td></tr>
        <tr><th>timeZone</th><td>Optional. IANA or Windows id, for example <code>America/New_York</code>. Sets <code>local</code> clock times and applies daylight saving for that date. When omitted, <code>local</code> uses an offset of <code>round(lon / 15)</code> hours.</td></tr>
      </table>
      <h3>GET</h3>
      <pre>{{host}}/api/v1/sunrise-sunset?lat=38.9784&amp;lon=-76.4922&amp;date=2026-09-22&amp;timeZone=America/New_York</pre>
      <h3>POST</h3>
      <pre>POST {{host}}/api/v1/sunrise-sunset
Content-Type: application/json

{
  "lat": 38.9784,
  "lon": -76.4922,
  "date": "2026-09-22",
  "timeZone": "America/New_York"
}</pre>
      <h3>Response</h3>
      <ul>
        <li>JSON object. <code>schema</code> is <code>laughingfish.sunriseSunset.v1</code>.</li>
        <li><code>location.lat</code> and <code>location.lon</code> are the coordinates you sent. <code>date</code> is the civil date you sent.</li>
        <li><code>timeZone.id</code> is the zone used for local clock times. <code>timeZone.offset</code> is the offset on that date, such as <code>-04:00</code>. <code>timeZone.source</code> is <code>request</code> when you supplied <code>timeZone</code>, or <code>longitude</code> when the offset comes from longitude.</li>
        <li><code>condition</code> describes the solar day:
          <ul>
            <li><code>riseAndSet</code> — official sunrise and sunset are in <code>events</code>.</li>
            <li><code>polarDay</code> — continuous daylight. <code>dayLength.seconds</code> is 86400. <code>events.solarNoon</code> is present. Sunrise and sunset are omitted.</li>
            <li><code>polarNight</code> — continuous night. <code>dayLength.seconds</code> is 0. <code>events.solarNoon</code> is present. Sunrise and sunset are omitted.</li>
          </ul>
        </li>
        <li><code>confidence</code> is <code>standard</code> for a normal calculation. It is <code>low</code> when the Sun skims the official horizon and refraction can change rise or set. <code>confidenceNote</code> is included when confidence is <code>low</code>.</li>
        <li>Each present event has <code>utc</code> (ISO-8601 UTC) and <code>local</code> (the same instant in the resolved offset). Absent events are omitted from <code>events</code>.</li>
        <li><code>events</code> fields, when present: <code>astronomicalDawn</code>, <code>nauticalDawn</code>, <code>civilDawn</code>, <code>sunrise</code>, <code>solarNoon</code>, <code>sunset</code>, <code>civilDusk</code>, <code>nauticalDusk</code>, <code>astronomicalDusk</code>.</li>
        <li><code>dayLength.seconds</code> is sunset minus sunrise when both times are present, 86400 on polar day, and 0 on polar night. <code>dayLength.hours</code> is the same duration in hours.</li>
        <li><code>sun.declinationDeg</code> and <code>sun.equationOfTimeMinutes</code> are the solar geometry used for that date.</li>
        <li>Use <code>utc</code> as the machine timestamp. Use <code>local</code> for display in the resolved zone. Near midnight, the UTC calendar date and the local calendar date can differ; both timestamps refer to the same instant.</li>
      </ul>
      <h3>Errors</h3>
      <table>
        <tr><th>400 invalid_request</th><td>lat, lon, or date was not supplied.</td></tr>
        <tr><th>400 invalid_coordinates</th><td>lat or lon is missing, not a number, or out of range.</td></tr>
        <tr><th>400 invalid_date</th><td>date is missing or not <code>yyyy-MM-dd</code> in 1900–2100.</td></tr>
        <tr><th>400 invalid_time_zone</th><td>timeZone is not a known IANA or Windows id.</td></tr>
        <tr><th>400 invalid_json</th><td>POST body is not JSON.</td></tr>
        <tr><th>500 sunrise_sunset_failed</th><td>The calculation could not be completed. Retry the same request.</td></tr>
      </table>
    </div>
  </main>
  <footer>
    LaughingFish Sunrise Sunset API
  </footer>
</body>
</html>
""";
    }
}
