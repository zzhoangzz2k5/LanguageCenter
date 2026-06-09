const { chromium } = require("playwright");

(async () => {
  const browser = await chromium.launch({ headless: true });
  const page = await browser.newPage({
    viewport: { width: 1440, height: 900 },
    deviceScaleFactor: 1,
  });

  const sites = [
    {
      url: "https://www.britishcouncil.vn/en/english",
      path: "D:/LanguageCenter/british-council-home.png",
    },
    {
      url: "https://vus.edu.vn/",
      path: "D:/LanguageCenter/vus-home.png",
    },
  ];

  for (const site of sites) {
    await page.goto(site.url, { waitUntil: "domcontentloaded", timeout: 90000 });
    await page.screenshot({ path: site.path, fullPage: false });
    console.log(`${site.path}: ${await page.title()}`);
  }

  await browser.close();
})();
