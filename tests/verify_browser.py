import json
import re
from datetime import datetime, timezone
from pathlib import Path
from playwright.sync_api import sync_playwright, expect

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / "artifacts/verification"
BASE = "http://127.0.0.1:5208"
OUT.mkdir(parents=True, exist_ok=True)
errors = []
failed = []
layouts = []
with sync_playwright() as p:
    browser = p.chromium.launch(channel="chrome", headless=True)
    context = browser.new_context(viewport={"width": 1440, "height": 1000})
    page = context.new_page()
    page.on("pageerror", lambda error: errors.append(str(error)))
    page.on("response", lambda response: failed.append({"status": response.status, "url": response.url}) if response.status >= 400 else None)
    for name, path in [("home", "/"), ("movie", "/Movies/Details/1"), ("booking", "/Bookings/Create/1"), ("admin", "/Admin"), ("movie-form", "/Admin/Movies/Create")]:
        response = page.goto(BASE + path, wait_until="networkidle")
        assert response.status == 200
        assert page.locator("img").evaluate_all("images => images.every(i => i.complete && i.naturalWidth > 0)")
        overflow = page.evaluate("document.documentElement.scrollWidth > innerWidth")
        assert not overflow, name
        layouts.append({"page": name, "viewport": "desktop", "horizontal_overflow": overflow})
        page.screenshot(path=str(OUT / (name + "-desktop.png")), full_page=True)
    page.goto(BASE + "/", wait_until="networkidle")
    page.get_by_role("link", name="Explore movies", exact=True).click()
    page.get_by_label("Movie title").fill("Orbit")
    page.get_by_role("button", name="Search movies").click()
    expect(page.locator(".movie-card")).to_have_count(1)
    page.get_by_label("Genre", exact=True).select_option("Science Fiction")
    page.get_by_label("Cinema", exact=True).select_option("1")
    page.get_by_role("button", name="Search movies").click()
    expect(page.locator(".movie-card")).to_have_count(1)
    page.get_by_role("link", name="Book tickets", exact=True).click()
    expect(page.get_by_role("heading", name="Orbit Nine", exact=True, level=1)).to_be_visible()
    page.get_by_role("link", name="Book tickets", exact=True).first.click()
    page.get_by_role("button", name="Confirm booking").click()
    expect(page.locator('[data-valmsg-for="CustomerName"]')).to_contain_text("required")
    page.get_by_label("Customer name").fill("Browser Flow Check")
    page.get_by_label("Phone", exact=True).fill("01098765432")
    page.get_by_label("Number of tickets").fill("2")
    expect(page.locator("[data-booking-total]")).to_contain_text("270.00 EGP")
    page.get_by_role("button", name="Confirm booking").click()
    page.wait_for_url(re.compile(r".*/Bookings/Confirmation\?code=[A-F0-9]{32}"))
    expect(page.locator(".ticket")).to_contain_text("Browser Flow Check")
    expect(page.locator(".ticket")).to_contain_text("270.00 EGP")
    assert re.fullmatch("[A-F0-9]{32}", page.locator(".ticket-code strong").inner_text())
    page.screenshot(path=str(OUT / "confirmation-desktop.png"), full_page=True)
    page.evaluate("window.__printed=false; window.print=()=>{window.__printed=true}")
    page.get_by_role("button", name="Print ticket").click()
    assert page.evaluate("window.__printed")
    page.emulate_media(media="print")
    expect(page.locator(".public-header")).to_be_hidden()
    expect(page.locator(".ticket")).to_be_visible()
    page.pdf(path=str(OUT / "ticket.pdf"), format="A4", print_background=True)
    page.screenshot(path=str(OUT / "ticket-print.png"), full_page=True)
    page.emulate_media(media="screen")
    page.goto(BASE + "/Admin/Actors/Create", wait_until="networkidle")
    actor_name = "Browser Actor " + datetime.now().strftime("%H%M%S")
    page.get_by_label("Name", exact=True).fill(actor_name)
    page.get_by_label("Biography", exact=True).fill("A fictional actor created by a real browser form submission.")
    page.get_by_role("button", name="Save actor").click()
    row = page.get_by_role("row").filter(has_text=actor_name)
    expect(row).to_be_visible()
    row.get_by_role("link", name="Edit", exact=True).click()
    page.get_by_label("Biography", exact=True).fill("The biography was edited through the browser verification flow.")
    page.get_by_role("button", name="Save actor").click()
    page.get_by_role("row").filter(has_text=actor_name).get_by_role("link", name="Delete", exact=True).click()
    page.get_by_role("button", name="Delete record", exact=True).click()
    expect(page.get_by_role("row").filter(has_text=actor_name)).to_have_count(0)
    page.set_viewport_size({"width": 390, "height": 844})
    for name, path in [("home", "/"), ("movie", "/Movies/Details/1"), ("booking", "/Bookings/Create/1"), ("admin", "/Admin"), ("movie-form", "/Admin/Movies/Create"), ("showtimes", "/Admin/Showtimes")]:
        page.goto(BASE + path, wait_until="networkidle")
        overflow = page.evaluate("document.documentElement.scrollWidth > innerWidth")
        assert not overflow, name
        layouts.append({"page": name, "viewport": "mobile", "horizontal_overflow": overflow})
        page.screenshot(path=str(OUT / (name + "-mobile.png")), full_page=True)
    page.goto(BASE + "/")
    page.get_by_role("button", name="Toggle navigation").click()
    expect(page.get_by_role("link", name="Cinemas", exact=True)).to_be_visible()
    page.get_by_role("link", name="Cinemas", exact=True).click()
    expect(page.get_by_role("heading", name="Our cinemas", exact=True)).to_be_visible()
    browser.close()
report = {"run_utc": datetime.now(timezone.utc).isoformat(), "browser": "Installed Google Chrome via Playwright", "layouts": layouts, "javascript_errors": errors, "failed_requests": failed, "public_booking": "passed", "admin_actor_crud": "passed", "print_ticket_pdf": "passed", "mobile_navigation": "passed"}
(OUT / "browser-results.json").write_text(json.dumps(report, indent=2))
assert not errors, errors
assert not failed, failed
print(json.dumps(report, indent=2))
