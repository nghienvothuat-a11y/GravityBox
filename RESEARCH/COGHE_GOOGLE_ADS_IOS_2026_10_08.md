---
title: "COghe — Google Ads App campaigns for iOS (US), research 2026-10-08"
tags: [coghe, google-ads, ios, user-acquisition, research]
status: active
created: 2026-10-08
---

# COghe — Google Ads App campaigns for iOS (US)

Research date 2026-10-08. Scope: a ~1-month US-only paid UA test on iOS, budget ramp $20 → $30 → $50 → $100/day, measured with Firebase/GA4 + AdMob + App Store Connect.

How to read this note:
- Each fact is followed by its source URL. Google Help pages change without version numbers, so treat "as read on 2026-10-08".
- **[INFERENCE]** marks my own reasoning or arithmetic, not a sourced fact.
- **[UNVERIFIED]** marks something I could not confirm on a primary page (search snippet or third-party only).
- **[THIRD-PARTY]** marks guidance from agencies/blogs, not Google or Apple.

---

## 0. Repo check (local, not web)

Checked `REPOS/GravityBox` on branch `NewGraphic`, HEAD `289bb4e4` (2026-10-06):
- `Assets/_Game/Venom/Services/COgheGoogleServices.cs` boots Firebase + AdMob only under `#if UNITY_ANDROID`. No `GoogleService-Info.plist` exists in the repo. **Today the iOS build would send no Firebase events, so none of the Google Ads iOS measurement below would work.** This has to be fixed before launch.
- Firebase Unity SDK is **13.17.0**, per `Assets/Plugins/Android/mainTemplate.gradle`. Firebase's release notes list Unity 13.17.0 (2026-09-17) as wrapping **Firebase iOS SDK 12.19.0**. That is above both the on-device measurement minimum (11.14.0) and Google's recommended iOS minimum (12.12.1); see §1.3. https://firebase.google.com/support/release-notes/unity
- I found no StoreKit or Unity IAP integration (`COgheEntitlements.StoreAvailable` depends on a `Store` provider). If IAP is later built on StoreKit 2, `in_app_purchase` must be logged by hand (see §1.2).

---

## 1. Setup requirements for an iOS App campaign

### 1.1 Linking Firebase / GA4 to Google Ads
- You can link either a Google Analytics property or a Firebase project to Google Ads. Once linked, you pick which Firebase events to import as Google Ads conversion actions. No new app code is needed. https://support.google.com/google-ads/answer/6397604?hl=en
- To bid on app events through GA, Google lists these steps:
  1. Add the GA for Firebase SDK.
  2. Implement events.
  3. Mark them as key events.
  4. Link GA to Google Ads (Admin → Product links → Google Ads links).
  5. Create Google Ads conversions from the key events.
  6. Enable auto-tagging.
  7. Bid on the conversions.

  https://support.google.com/google-ads/answer/13823256?hl=en
- **iOS-specific:** for iOS key events you must add the **App Store ID** to the Firebase project, and enable the **AdSupport framework** in Xcode to access the IDFA. https://support.google.com/google-ads/answer/13823256?hl=en
- In Google Ads: Goals → Conversions → New → **App** → **Google Analytics (Firebase)** → tick the events → Import. https://developers.google.com/tag-platform/devguides/app-conversions
- Import events from the GA4F **primary (Standard) property**, not sub-properties. Do not use custom event names starting with `firebase_`, `google_` or `ga_`. https://support.google.com/google-ads/answer/10384955?hl=en

### 1.2 Conversions to import (first_open, in-app events)
- The GA SDK collects `first_open` and `in_app_purchase` automatically. https://support.google.com/google-ads/answer/13823256?hl=en
- Google's minimum is to "track installs and at least one in-app event. Ideally ... multiple events, including revenue generating events." https://support.google.com/google-ads/answer/9176652?hl=en
- Optimize each campaign for **one** event only. In an installs campaign, "check that only your first open or install event is marked as 'Yes' under 'Include in Conversions'." Google's App campaign for installs checklist (PDF): https://services.google.com/fh/files/misc/app_campaign_for_installs_checklist.pdf
- `in_app_purchase` on iOS is logged automatically with **StoreKit 1**. With **StoreKit 2** you must call `Analytics.logTransaction()` (Swift only). https://firebase.google.com/docs/analytics/ios/measure-in-app-purchases
- Ad-revenue events: `ad_impression` (AdMob → GA) is **reserved** for "tROAS for ad revenue (ARO)" campaigns and can't be picked in other campaign types. ARO works on Android **and iOS**. It requires the app to bid on GA events and to "derive a significant majority or entirety of in-app revenue from ads." https://support.google.com/google-ads/answer/13799577?hl=en
- Default conversion window is 30 days. If an MMP is used, its window should match Google Ads. https://support.google.com/google-ads/answer/9176652?hl=en ; https://services.google.com/fh/files/misc/app_campaign_for_installs_checklist.pdf

### 1.3 On-device conversion measurement (ODM) for iOS
- **What it is:** Google's privacy-preserving way to measure installs and in-app actions from iOS App campaigns. It "improves the amount of observable conversions to enhance campaign optimization and reporting." https://support.google.com/google-ads/answer/12119136?hl=en
- **Two methods:**
  - **First-party data method:** a consented, user-provided email or phone number from your sign-in flow is matched on the device. Google reports a "median 19% reduction in CPA on Google's owned inventory." https://support.google.com/google-ads/answer/12119136?hl=en
  - **Event data method:** uses de-identified app event data. Requires linking the GA property to the Google Ads account that runs the iOS App campaigns. https://support.google.com/google-ads/answer/12119136?hl=en
  - COghe has no sign-in, so **only the event data method applies** **[INFERENCE]**.
- **SDK requirements:**
  - Event data method needs GA for Firebase iOS SDK **≥ 11.14.0** (June 2025). With that version or later, the SDK "automatically gives your app access to on-device conversion measurement functionalities." https://firebase.google.com/docs/tutorials/ads-ios-on-device-measurement
  - Requires iOS 12+. https://support.google.com/google-ads/answer/12119136?hl=en
  - Google's iOS best-practices page recommends upgrading to Firebase iOS SDK **v12.12.1 or later** "to maximize performance benefits." https://support.google.com/google-ads/answer/10384955?hl=en
- **Pods:**
  - The default `FirebaseAnalytics` pod includes IDFA support and both ODM methods.
  - `FirebaseAnalytics/Core` has no IDFA and needs the `GoogleAdsOnDeviceConversion` pod added.
  - The legacy `WithoutAdIdSupport` and `FirebaseAnalyticsOnDeviceConversion` pods are deprecated (removed in v12.0.0).

  https://firebase.google.com/docs/tutorials/ads-ios-on-device-measurement
- Unity: event-data ODM went to open beta in Firebase Unity SDK 12.10.0 (2025-06-12). COghe's 13.17.0 is newer. https://firebase.google.com/support/release-notes/unity
- ODM is inactive for users in the EEA, UK and Switzerland. This does not matter for a US-only test. https://support.google.com/google-ads/answer/12119136?hl=en
- Google announced event-data ODM, tROAS on iOS and Maximize conversions for in-app actions on **2025-08-14**. https://blog.google/products/ads-commerce/3-new-ways-to-unlock-ios-app-campaign-performance/

### 1.4 SKAdNetwork / SKAN 4 / AdAttributionKit
- Google Ads uses SKAdNetwork for aggregated iOS measurement. "**Google does not currently support AAK [AdAttributionKit] for attribution at this time.**" https://support.google.com/google-ads/answer/10384955?hl=en
- **SKAN 4:** "Google's conversion modeling only uses the fine conversion values ... We currently do **not support SKAN version 4's coarse conversion values**." https://support.google.com/google-ads/answer/13286653
- **Who manages conversion values:** you can set the SKAN conversion-value schema in **Google Analytics**, in an App Attribution Partner, or through the Google Ads API. Google advises picking one place. https://support.google.com/google-ads/answer/13286653
  - If the schema is set in GA and `applyConversionValues` is enabled, "the GA SDK will set conversion values using this schema definition, and schema will be exported to any Google Ads accounts linked to this property." **So yes, Firebase/GA can manage conversion values for you.** https://developers.google.com/analytics/devguides/config/admin/v1/rest/v1alpha/properties.dataStreams.sKAdNetworkConversionValueSchema
  - Window 1 (0–2 days) holds a fine value 0–63 plus a coarse value. Windows 2 and 3 are coarse only. https://developers.google.com/analytics/devguides/config/admin/v1/rest/v1alpha/properties.dataStreams.sKAdNetworkConversionValueSchema
  - The checklist says "The latest version of the Google Analytics for Firebase SDK provides automatic integration with SKAdNetwork." https://services.google.com/fh/files/misc/app_campaign_for_installs_checklist.pdf
- Any event in the schema should have **>10 conversion events per day**. https://support.google.com/google-ads/answer/13286653
  - **[INFERENCE]** At $20–100/day (roughly 5–40 installs/day), few in-app events will clear that bar. Apple's privacy thresholds will also null out many fine values at this volume. Expect SKAN reporting to be thin; rely on GA4/ODM-modeled conversions in Google Ads.
- AdMob side (COghe as a publisher): add `SKAdNetworkItems` to Info.plist. Google's ID is `cstr6suwn9.skadnetwork`, plus the third-party buyer IDs. https://developers.google.com/admob/ios/privacy/strategies
- **Reporting delay:** iOS App campaign modeled conversions "may take up to 5 days to appear." https://support.google.com/google-ads/answer/10625151?hl=en
  - The checklist also warns that early CPI "may be inflated ... this may take up to 5 days (especially on iOS)." https://services.google.com/fh/files/misc/app_campaign_for_installs_checklist.pdf

### 1.5 ATT prompt implications
- Google recommends using the ATT prompt plus AdSupport. "Users who accept the prompt will increase your observed conversions, improving conversion modeling and campaign optimization." An explainer screen before the prompt is suggested. https://support.google.com/google-ads/answer/10384955?hl=en
- AdMob iOS setup:
  - Add `NSUserTrackingUsageDescription`.
  - Call `requestTrackingAuthorization...`.
  - Google recommends "waiting for the completion callback prior to loading ads."
  - The UMP SDK can show an IDFA explainer, but "affects all users of your app."

  https://developers.google.com/admob/ios/privacy/strategies
- Apple: tracking needs ATT permission. You may not gate features or give rewards in exchange for consent (already noted in `RESEARCH/COGHE_MONETIZATION_SOURCES_2026_10_02.md`). https://developer.apple.com/app-store/user-privacy-and-data-use/
- **[INFERENCE]** ATT is not required to run iOS App campaigns. SKAN and event-data ODM work without IDFA. Opt-ins add observed conversions and usually raise AdMob eCPMs on those users.

### 1.6 iOS limit on number of campaigns
- "If you don't have On-device measurement event data implemented, we recommend you consolidate to **8 or fewer** app install campaigns for each iOS app." More than that degrades performance and SKAdNetwork reporting. https://support.google.com/google-ads/answer/10625151?hl=en
- The best-practices guide says this limit "will eventually be removed" for apps with ODM. https://support.google.com/google-ads/answer/10384955?hl=en
- Account limit: App campaigns allow at most **100 ad groups per campaign**. https://support.google.com/google-ads/answer/6372658?hl=en
- **[INFERENCE]** For a $20–100/day test, run **1 campaign**. The checklist itself says "Avoid creating too many campaigns, as they may compete with each other." https://services.google.com/fh/files/misc/app_campaign_for_installs_checklist.pdf

### 1.7 Custom Product Pages (CPP) on iOS
- **I found no evidence that Google App campaigns can send iOS traffic to an App Store Custom Product Page.**
  - Google's setup guide says users without the app "will be directed to the default store page for that app." https://support.google.com/google-ads/answer/12575501?hl=en
  - Google's own alternative, "Custom details pages," is "only available for App campaigns for installs (ACi) that promote Android apps." https://support.google.com/google-ads/answer/9948381?hl=en
- **[UNVERIFIED]** A search summary attributed to a third-party guide stated that Google App campaigns support neither Apple CPPs nor Google custom store listings. I could not open that page to confirm.
- **[INFERENCE]** Optimize the **default** App Store page (screenshots and preview video). App campaigns also pull store assets automatically ("app videos, cover photos, screenshots, descriptions, title, icon"). https://support.google.com/google-ads/answer/9948381?hl=en
- Apple context: up to **70** CPPs per app, usable with Apple Ads and by URL on other channels. https://developer.apple.com/app-store/custom-product-pages
  - Apple says ad networks *can* use CPPs in StoreKit-rendered ads, through `customProductPageIdentifier` with `SKOverlay` or `SKStoreProductViewController`. https://developer.apple.com/app-store/custom-product-pages/
  - So the limit is on Google's side: no Google setting for an iOS CPP was found.

---

## 2. Bidding and budget

### 2.1 Bid strategies for App campaigns
Source for the list: https://support.google.com/google-ads/answer/12073727?hl=en ; https://support.google.com/google-ads/answer/16550675?hl=en

| Goal | Strategy | Notes |
|---|---|---|
| Installs | **Target CPI (tCPI)** | Google aims for installs at your target |
| Installs | **Maximize conversions (installs)** | No target needed; spends the daily budget |
| In-app actions | **Target CPA (tCPA)** | Install plus the chosen action |
| In-app actions | **Maximize conversions (in-app actions)** | The 12073727 page still says **beta / limited** (third-party summaries and the 2025-08-14 blog describe an expansion) |
| Value | **Target ROAS** | Value from Firebase or Google Play codeless conversions only. **Available on iOS since 2025-08-14** |
| Value | Maximize conversion value | **Beta / limited** |
| Ad revenue | **tROAS for ad revenue (ARO)** | Android and iOS; imports `ad_impression`; for apps whose revenue is mostly ads |

- **Prerequisites:**
  - **In-app actions:** pick an action "completed by at least 10 different users per day." https://support.google.com/google-ads/answer/6167162?hl=en ; https://support.google.com/adwords/answer/6167156?hl=en
    - The checklist is stricter: the action should occur "at least 30x/day (preferably more)." https://services.google.com/fh/files/misc/app_campaign_for_installs_checklist.pdf
  - **tROAS:** Google's Target ROAS page lists App eligibility as **at least 10 conversions per day (or 300 in 30 days)**, with conversion value > 0. https://support.google.com/google-ads/answer/6268637?hl=en
    - Google's newer page says to start with Maximize conversions or tCPA. Move to tROAS once you reach "typically 30+ conversions in 30 days." If moving from tCPA, have "3 to 4 weeks of historical conversion value data." https://support.google.com/google-ads/answer/15995101?hl=en
    - A conversion window **under 30 days** is advised for tROAS. https://support.google.com/google-ads/answer/14104492?hl=en
  - **Note:** the older iOS 14 FAQ still says tROAS users "should switch to Target CPA right away." That advice predates the 2025 iOS tROAS launch. https://support.google.com/google-ads/answer/10625151?hl=en
- **iOS bid level:** "For iOS, a bid **1.5 times higher than Android** is recommended." Targeting a narrower in-app action should carry a bid at least 20% above baseline. https://support.google.com/google-ads/answer/12073727?hl=en
- **iOS and Maximize conversions:** Google pitches Maximize conversions for iOS because "setting precise targets is challenging due to App Tracking Transparency (ATT) restrictions." https://support.google.com/google-ads/answer/16550675?hl=en

### 2.2 Recommended daily budget vs. target
- "Set your average daily budget at **50 times your target CPI, or 10 times your target CPA**." https://support.google.com/google-ads/answer/9176652?hl=en
  - Best-practices page: tCPI budget ≥ 50× bid, ACi tCPA ≥ 10× bid, engagement tCPA ≥ 15× bid. https://support.google.com/google-ads/answer/14104492?hl=en
  - Another page phrases tCPA as "10–15 times." https://support.google.com/adwords/answer/6167156?hl=en
- Google's worked example: raising tCPI by $0.40 means checking that the budget "is at least **$120, or 50 times this new target CPI**," i.e. tCPI $2.40. https://support.google.com/google-ads/answer/7678575?hl=en
- **[INFERENCE]** At $20/day, 50× tCPI means a tCPI of $0.40, far below US iOS game CPIs (§4). At $100/day, 50× allows a $2 tCPI, still at or below typical US iOS casual CPI. **The whole ramp sits below Google's recommended budget-to-bid ratio.** Expect slow learning, uneven delivery and noisy CPI.

### 2.3 Learning period and changing budgets/targets
- Changes before the first **100 conversions** "may disrupt learning." https://support.google.com/google-ads/answer/14104492?hl=en
- Wait "for at least 100 conversions before making another bid change." Raise tCPI by **no more than 20% per day**. https://support.google.com/google-ads/answer/7678575?hl=en
- Avoid "changing the budget by >20%, or changing your CPI by >20%." https://support.google.com/google-ads/answer/9176652?hl=en
- The checklist adds:
  - "Once the learning and assessment phases are over (after **7–14 days**), gradually adjust the budget ... Do not change your budget by more than **20%** up or down."
  - "Give your campaign some time (**10 days**) to settle in."
  - "Are you ready to let the machine do its work and not touch it for at least 10 days?"

  https://services.google.com/fh/files/misc/app_campaign_for_installs_checklist.pdf
- "Allow at least 7–14 days for the system to stabilize." "Evaluate performance over a 30-day window." https://support.google.com/adwords/answer/6167156?hl=en
- Maximize conversions has a learning period. Big budget changes shift it, and early budget "may be spent quickly as the campaign starts and learns." https://support.google.com/google-ads/answer/16550675?hl=en
- **[THIRD-PARTY]** Adapty's playbook (updated 2026-01-05): do not change tCPI/tCPA more than 20% in 24h, scale budgets no more than 20% every 48–72h, learning typically 1–2 weeks. https://adapty.io/blog/google-app-campaigns-playbook-2025/
- **[THIRD-PARTY]** RevenueCat: raise budget about 20% once or twice a week. Don't start unless you can hold for a couple of weeks and 100+ events. https://www.revenuecat.com/blog/growth/a-practical-guide-to-google-app-campaigns

### 2.4 What makes sense for a $20–30/day start (US iOS) — [INFERENCE]
- **Goal:** installs (`first_open` as the only primary conversion). Import level/tutorial/IAP/ad events as **secondary**, for observation only.
  - At about 5–10 installs/day, no in-app action reaches Google's 10 users/day threshold.
  - tROAS and ARO need about 10 conversions/day or 300 per 30 days, so they are out of reach in this test.
- **Strategy:** Maximize conversions (installs) is the simplest fit for a fixed small budget, and Google pitches it for iOS (§2.1).
  - Alternative: **tCPI set at a realistic level** (around the US iOS casual CPI in §4), not a hoped-for low value. A low tCPI on a tiny budget tends to under-deliver.
- **The planned ramp breaks Google's 20% rule:** $20→$30 is +50%, $30→$50 is +67%, $50→$100 is +100%. Two options:
  1. Step in ≤20% increments every 2–3 days: 20→24→29→35→42→50→60→72→86→100. That is 9 steps, about 25 days.
  2. Keep the four steps but treat each as a new learning phase. Hold each step ≥7 days and judge only the second half of each step.
- **Total spend and data:** the planned ramp (7d×$20 + 7d×$30 + 7d×$50 + 9d×$100) is about **$1.6k**. At $2–5 CPI that buys roughly 320–800 installs.
  - That is enough for directional D1/D7 retention, ad ARPDAU and store conversion rate.
  - It is **not** enough to measure IAP payer rate or D30 with confidence.
- **Timing:** don't judge CPI daily. iOS conversions can lag up to 5 days (§1.4).

---

## 3. Asset specs for App campaigns

| Asset | Spec | Source |
|---|---|---|
| Headlines | Up to **5**, ≤ **30 characters** each | https://support.google.com/google-ads/answer/9948381?hl=en |
| Descriptions | Up to **5**, ≤ **90 characters** each | https://support.google.com/google-ads/answer/9948381?hl=en |
| Images | .jpg/.png, ≤ **5 MB** | https://support.google.com/google-ads/answer/9948381?hl=en |
| Image 1:1 | min 200×200, rec. **1200×1200** | same |
| Image 1.91:1 | min 600×314, rec. **1200×628** | same |
| Image 4:5 | min 320×400, rec. **1200×1500** | same |
| Videos | Must be **hosted on YouTube**; landscape, portrait or square. Google may auto-generate videos if none are supplied | same |
| HTML5 / playable | .ZIP ≤ **5 MB**, ≤ **512 files**, ≤ **20 ZIPs per ad group**, responsive, UTF-8 | same |
| Per ad group | Up to **20** each of images, videos and HTML5 | https://support.google.com/google-ads/answer/12575501?hl=en ; https://support.google.com/google-ads/answer/6167158?hl=en |
| Store assets | Pulled automatically from the store listing (screenshots, icon, title, description, preview video) | https://support.google.com/google-ads/answer/9948381?hl=en |

- **Video guidance:**
  - Lengths "varying in length between **10 to 30 seconds**." https://support.google.com/google-ads/answer/6167162?hl=en
  - Recommended ratios **16:9, 1:1, 2:3**. "Portrait videos have a **60% higher conversion rate** than landscape." https://services.google.com/fh/files/misc/app_campaign_for_installs_checklist.pdf
  - Hook "within the first 2 to 3 seconds," often with "2+ cut changes within the first 5 seconds," and **captions** because some inventory is sound-off. https://support.google.com/google-ads/answer/6167158?hl=en
  - Google's "video enhancements" (2025-08-14) adapt videos to other screen sizes automatically. https://blog.google/products/ads-commerce/3-new-ways-to-unlock-ios-app-campaign-performance/
- **Image guidance:** blank space no more than 80% of the image. Overlaid elements under 20% of the image area. https://support.google.com/google-ads/answer/6167158?hl=en
- **Text:** Google pages conflict on wording. The creative page says "4 for text." The checklist says use all ten lines (5 headlines + 5 descriptions), each able to stand alone. https://support.google.com/google-ads/answer/6167158?hl=en ; https://services.google.com/fh/files/misc/app_campaign_for_installs_checklist.pdf
- **Playables:** on 2025-08-14 Google announced playable end cards on some AdMob inventory after video ads. https://blog.google/products/ads-commerce/3-new-ways-to-unlock-ios-app-campaign-performance/
- **Asset performance labels:**
  - Pending = still processing.
  - Learning = "the campaign needs more data before it can rank the asset."
  - Low / Good / Best = ranked against other assets **of the same type** in the campaign.
  - Ratings use "the last 14 days of data."
  - https://support.google.com/google-ads/answer/6310436?hl=en
  - **Google states no fixed time or threshold before ratings appear.** **[UNVERIFIED]** A search summary claimed "~5,000 impressions over several days." I could not confirm it on a Google page.
  - The checklist lists 5 ratings (Waiting, Learning, Low, Good, Best) and says: "Focus on adding more assets before removing any existing assets." https://services.google.com/fh/files/misc/app_campaign_for_installs_checklist.pdf

---

## 4. Benchmarks (US, iOS, casual/puzzle, 2025–2026)

Compiled by a benchmark sub-researcher on 2026-10-08. Tags:
- **[V]** checked on the source page or PDF;
- **[V-chart]** read off a chart (about ±0.3 IPM, ±$0.5 eCPM);
- **[2nd]** only in an aggregator's summary;
- **[snippet]** search snippet only, unverified.

No benchmark exists specifically for Google Ads App campaigns.

### 4.1 CPI (cost per install)
- Liftoff × Singular, "2025 Casual Gaming Apps Report" (Feb 2024–Feb 2025, global; casual includes hyper-casual and puzzle). **[V]** https://liftoff.ai/2025-casual-gaming-apps-report/
  - CPI: **$1.41 iOS** / $0.14 Android.
  - D30 return on ad spend: 47% iOS / 15% Android.
- Adjust, "Gaming App Insights Report 2026" (2025 values, iOS and Android blended, all ad networks). https://a.storyblok.com/f/47007/x/20551740ee/gaming-app-insights-report-2026.pdf
  - All-games CPI: **US $1.71** (2024: $1.31), global $0.56. **[V]**
  - Puzzle genre CPI, global: about $0.75. **[V-chart]**
- "Puzzle $3 iOS / $2 Android (2024)", attributed to Business of Apps. **[2nd]** https://mapendo.co/blog/mobile-games-cpi-2025 — the original page returned 403.
- **[INFERENCE]** Working range for US iOS casual/puzzle on Google App campaigns: **about $1.5–5**.

### 4.2 IPM (installs per 1,000 impressions) and CTR
- Adjust 2026 (PDF above), iOS and Android blended:
  - Gaming IPM global 8.62. **[V]**
  - Puzzle about 8.5; US all games about 9.5. **[V-chart]**
- Liftoff 2025 casual report (URL above): casual CTR 8.8% iOS. **[V]**
- Liftoff "2025 Mobile Ad Creative Index": playable ads convert impressions to installs at 16x the rate of non-playable formats. **[V]** https://www.liftoff.ai/blog/highlights-2025-mobile-ad-creative-index/

### 4.3 App Store product page conversion
- AppTweak (2025, US): average across categories **8.56%** App Store; the lowest category, Games–Trivia, is 5.2%. No puzzle figure. **[V]** https://www.apptweak.com/aso-blog/average-app-conversion-rate-per-category
- Apple's peer-group benchmarks are only visible inside App Store Connect. They measure unique impressions to downloads. https://developer.apple.com/help/app-store-connect-analytics/benchmarks/peer-group-benchmarks/
- The often-quoted 4.47% games median (SplitMetrics) is from 2015 data and is stale. **[V]**

### 4.4 Retention
- GameAnalytics, "2025 Mobile Gaming Benchmarks" (calendar 2024, global). **[V]** https://files.gameindustrylibrary.com/documents/mobile-gaming-benchmarks-2025.pdf (mirror of the GameAnalytics PDF)
  - **Puzzle median:** D1 19.7–20.7%, D7 4.3–4.8%, D28 1.1–1.3%.
  - **All genres, top 25%:** D1 26.5–27.7% (iOS 31–33%), D7 7–8%, D28 about 2.7–3%.
- GameAnalytics, "2026 Mobile & PC Benchmarks" (calendar 2025, no genre split). **[V]** https://www.gameanalytics.com/reports/2026-mobile-pc-gaming-benchmarks
  - Median: D1 about 22%, D7 about 4%.
  - Top 25%: D1 about 30%, D7 6–7%.
- Adjust 2026: D1 puzzle 20%; US all games 19%. **[V]**
- Appodeal casual report 2025 (US, **Android only**): puzzle D1/D7/D30 = 30% / 13% / 8%. Much higher than GameAnalytics, likely because of a different sample. **[V]** https://files.gameindustrylibrary.com/documents/mobile-casual-benchmarks-report-2025.pdf
- Don't use the widely quoted "31.85% / 12.18% / 5.35%": it is AppsFlyer data from Q3 2022.

### 4.5 eCPM, US iOS (newest verifiable data is from 2024)
- Appodeal, "Quarterly Mobile eCPM Report Q4 2024", US: rewarded about **$17.3**, interstitial about **$13.1**, banner about **$0.4**. **[V-chart]** https://appodeal.com/wp-content/uploads/2025/03/Quarterly_Mobile_eCPM_Report_-_Q4_2024.pdf
- Tenjin × Clever Ads Solutions, Q2 2024, games, US iOS: rewarded $24.39, interstitial $23.17, banner $0.99. **[2nd]** https://gamedevreports.substack.com/p/tenjin-ad-monetization-in-mobile-00b

### 4.6 ARPDAU and payer share
- **ARPDAU:** no verified 2025–26 genre benchmark. Closest data:
  - AppsFlyer "App Monetization in 2026", casual at day 90: IAP revenue per user $1.34, ad revenue per user $0.55. **[2nd]** https://gamedevreports.substack.com/p/appsflyer-app-monetization-in-2026
  - Appodeal casual report: puzzle lifetime ad revenue per user about $2.1 (US, Android only). **[V-chart]**
- **Payer share:** Mistplay × AppsFlyer (US, 2023–24) puts puzzle at 7.82% iOS by day 30. **[2nd]** https://gamedevreports.substack.com/p/mistplay-and-appsflyer-player-loyalty
  - This skews toward big IAP-heavy puzzle games.
  - **[INFERENCE]** For an ads-first game with two one-time purchases, plan for low single digits or below.

---

## 5. Small-budget creative testing on Google App campaigns

### 5.1 What Google says
- Fill every asset slot before removing any. "If you have not uploaded the maximum permitted number of assets, add assets similar to those ranked as 'Best'. If you have reached the maximum ... replace assets ranked as 'Low'. Do so gradually." https://support.google.com/google-ads/answer/6167158?hl=en
  - "Low"-rated assets may still work in some placements; build up to the maximum before removing underperformers. https://support.google.com/google-ads/answer/7678575?hl=en
- Ad groups: "Aim to have **one evergreen ad group** that is always on. When you're ready, add additional, more targeted ad groups." https://services.google.com/fh/files/misc/app_campaign_for_installs_checklist.pdf
  - Ad groups can group creatives around a feature, a message or a demographic. https://support.google.com/google-ads/answer/6167158?hl=en
- Each ad group needs at least one approved text, image **and** video asset. https://support.google.com/google-ads/answer/14104492?hl=en
- **Formal creative A/B tools are mostly unavailable for iOS:**
  - "Directional experiments" (up to 20 videos head-to-head, video-only ACi) are "currently available for **Android** campaigns only." https://support.google.com/google-ads/answer/16638855?hl=en
  - App uplift experiments: iOS "currently not supported." They need 100+ (ideally 150+) conversions/day and about 30 days. https://support.google.com/google-ads/answer/14074599?hl=en

### 5.2 Practitioner guidance [THIRD-PARTY]
- **RevenueCat:**
  - Start with 2–3 assets per format.
  - Videos: two portrait, two square and two landscape, at lengths of 6, 10, 15 and 30 seconds.
  - Keep an "evergreen" ad group of winners plus one or two rotating test ad groups.

  https://www.revenuecat.com/blog/growth/a-practical-guide-to-google-app-campaigns
- **Adapty:** refresh high-volume asset groups every 2–3 weeks. Use 9:16 under 60 s for Shorts. https://adapty.io/blog/google-app-campaigns-playbook-2025/
- **AdManage:** small budgets should test 2–3 creatives at a time and spend about 2–3× target CPA per creative before judging. https://admanage.ai/blog/how-many-ad-creatives-to-test

### 5.3 Suggested plan for COghe at $20–100/day — [INFERENCE]
- **Structure:** 1 campaign, 1 ad group to start, with all 10 text lines and 3–6 videos.
  - Videos: at least 1 each of portrait 9:16, square 1:1 and landscape 16:9. Lengths of 10–30 s, plus 1 short (about 6–15 s) cut.
  - Add 3–6 images (1:1, 4:5, 1.91:1).
  - Splitting $20–30/day across several ad groups starves each of them.
- **Testing method:** with no iOS experiments tool, test sequentially.
  - Add 1–2 new videos per refresh, leaving earlier assets in place.
  - Compare installs/conversion rate per asset in the asset report once labels leave "Learning."
  - Swap out "Low" videos one at a time.
  - Cadence: about every 7–14 days (one refresh per budget step), not daily.
- **Lean on the existing pipeline:** the 30 s how-to-play clip and `assemble.py` (see memory `coghe-tutorial-clip`) can produce cut-downs and orientation variants cheaply.
- **Store page matters double:** iOS traffic can't go to a CPP (§1.7), and store screenshots and preview are pulled into ads. Polish the default App Store page before spending.
- **Optional cheap pre-test:** run the creative set on Android, where Directional experiments exist, to pre-screen hooks, then carry winners to iOS. Note that this changes the platform mix of the test.
