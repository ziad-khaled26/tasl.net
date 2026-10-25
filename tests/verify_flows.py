import concurrent.futures
import http.cookiejar
import io
import json
import re
import subprocess
import sys
import threading
import unittest
import urllib.error
import urllib.parse
import urllib.request
import uuid
from datetime import datetime, timedelta, timezone
from html.parser import HTMLParser
from pathlib import Path

BASE = "http://127.0.0.1:5208"
DATABASE = "CinemaBooking_Verification"
ROOT = Path(__file__).resolve().parents[1]

class NoRedirect(urllib.request.HTTPRedirectHandler):
    def redirect_request(self, request, fp, code, msg, headers, newurl):
        return None

class Inputs(HTMLParser):
    def __init__(self, html):
        super().__init__()
        self.values = {}
        self.select_name = None
        self.first_option = False
        self.feed(html)
    def handle_starttag(self, tag, attrs):
        attrs = dict(attrs)
        if tag == "input" and attrs.get("name") and attrs.get("type") != "file":
            self.values[attrs["name"]] = attrs.get("value", "")
        if tag == "select":
            self.select_name = attrs.get("name") if "multiple" not in attrs else None
            self.first_option = True
        if tag == "option" and self.select_name:
            if "selected" in attrs or self.first_option:
                self.values[self.select_name] = attrs.get("value", "")
            self.first_option = False
    def handle_endtag(self, tag):
        if tag == "select":
            self.select_name = None

class Client:
    def __init__(self):
        self.cookies = http.cookiejar.CookieJar()
        self.opener = urllib.request.build_opener(urllib.request.HTTPCookieProcessor(self.cookies), NoRedirect())
    def request(self, path, data=None, upload=None):
        headers = {}
        body = None
        if data is not None:
            pairs = list(data.items()) if isinstance(data, dict) else data
            if upload is None:
                body = urllib.parse.urlencode(pairs).encode()
                headers["Content-Type"] = "application/x-www-form-urlencoded"
            else:
                boundary = "CinemaTest" + uuid.uuid4().hex
                stream = io.BytesIO()
                for name, value in pairs:
                    stream.write((f"--{boundary}\r\nContent-Disposition: form-data; name=\"{name}\"\r\n\r\n{value}\r\n").encode())
                filename, content, content_type = upload
                stream.write((f"--{boundary}\r\nContent-Disposition: form-data; name=\"Image\"; filename=\"{filename}\"\r\nContent-Type: {content_type}\r\n\r\n").encode())
                stream.write(content)
                stream.write((f"\r\n--{boundary}--\r\n").encode())
                body = stream.getvalue()
                headers["Content-Type"] = "multipart/form-data; boundary=" + boundary
        request = urllib.request.Request(BASE + path, data=body, headers=headers)
        try:
            response = self.opener.open(request, timeout=40)
        except urllib.error.HTTPError as error:
            response = error
        return response.code, response.read().decode("utf-8", errors="replace"), response.headers
    def form(self, path):
        status, html, _ = self.request(path)
        assert status == 200, (path, status, html[:400])
        return Inputs(html).values, html

def sql(query):
    assert DATABASE == "CinemaBooking_Verification"
    result = subprocess.run(["sqlcmd", "-S", ".", "-E", "-C", "-d", DATABASE, "-h", "-1", "-W", "-b", "-Q", "SET NOCOUNT ON; " + query], capture_output=True, text=True, check=True)
    return result.stdout.strip()

class CinemaFlows(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.client = Client()
        cls.prefix = "Check " + uuid.uuid4().hex[:8]
        cls.future = (datetime.now(timezone.utc) + timedelta(days=4)).strftime("%Y-%m-%dT19:30")
        cls.original_cinema = int(sql("SELECT MIN(Id) FROM Cinemas"))
        cls.original_movie = int(sql("SELECT MIN(Id) FROM Movies"))
        cls.actor_ids = [int(x) for x in sql("SELECT TOP(2) Id FROM Actors ORDER BY Id").split()]
    def post_form(self, path, values, upload=None):
        data, _ = self.client.form(path)
        if "__RequestVerificationToken" not in data:
            token_form, _ = self.client.form("/Admin/Actors/Create")
            data["__RequestVerificationToken"] = token_form["__RequestVerificationToken"]
        data.update(values)
        return self.client.request(path, data, upload)
    def create_show(self, capacity=5, price="170.50"):
        response = self.post_form("/Admin/Showtimes/Create", {"MovieId": str(self.original_movie), "CinemaId": str(self.original_cinema), "StartAt": self.future, "TicketPrice": price, "Capacity": str(capacity)})
        self.assertEqual(response[0], 302, response[1])
        return int(sql("SELECT MAX(Id) FROM Showtimes"))
    def booking(self, show_id, client=None, quantity=1, customer=None):
        client = client or self.client
        data, _ = client.form(f"/Bookings/Create/{show_id}")
        data.update({"CustomerName": customer or self.prefix, "Phone": "01012345678", "Quantity": str(quantity)})
        return data
    def test_01_public_navigation_and_filters(self):
        for path in ["/", "/Movies", "/Cinemas", "/Admin", "/Admin/Movies", "/Admin/Cinemas", "/Admin/Actors", "/Admin/Showtimes", "/Admin/Bookings"]:
            self.assertEqual(self.client.request(path)[0], 200, path)
        html = self.client.request("/Movies?Search=Orbit")[1]
        self.assertIn("Orbit Nine", html)
        self.assertNotIn('alt="Poster for Beyond the Nile"', html)
        self.assertIn("Orbit Nine", self.client.request("/Movies?Genre=Science%20Fiction")[1])
        self.assertIn("No movies found", self.client.request("/Movies?Search=ThereIsNoSuchMovie")[1])
        self.assertIn("Upcoming screenings", self.client.request(f"/Cinemas/Details/{self.original_cinema}")[1])
        self.assertIn("The cast", self.client.request(f"/Movies/Details/{self.original_movie}")[1])
        self.assertEqual(self.client.request("/Admin/Bookings?To=9999-12-31")[0], 200)
        for path in ["/Movies/Details/2147483647", "/Cinemas/Details/2147483647", "/Bookings/Create/2147483647", "/Bookings/Index", "/Bookings/Details/1", "/Bookings/Confirmation/1", "/Bookings/Confirmation?code=1234"]:
            self.assertEqual(self.client.request(path)[0], 404, path)
    def test_02_cinema_crud_upload_and_validation(self):
        values = {"Name": self.prefix + " Cinema", "Address": "10 Test Avenue, Cairo", "Description": "A temporary cinema created by the integration verification suite."}
        bad = self.post_form("/Admin/Cinemas/Create", values | {"Name": ""})
        self.assertEqual(bad[0], 200)
        self.assertIn("field-validation-error", bad[1])
        self.assertEqual(self.client.request("/Admin/Cinemas/Create", values)[0], 400)
        image = (ROOT / "CinemaBooking/wwwroot/images/cinema-1.png").read_bytes()
        self.assertEqual(self.post_form("/Admin/Cinemas/Create", values, ("safe.png", image, "image/png"))[0], 302)
        cinema_id = int(sql(f"SELECT Id FROM Cinemas WHERE Name = '{values['Name']}'"))
        path = sql(f"SELECT ImagePath FROM Cinemas WHERE Id = {cinema_id}")
        self.assertRegex(path, r"^/uploads/[0-9a-f]{32}\.jpg$")
        edit_values = values | {"Name": values["Name"] + " Updated", "ExistingImage": "/evil.svg"}
        self.assertEqual(self.post_form(f"/Admin/Cinemas/Edit/{cinema_id}", edit_values)[0], 302)
        self.assertEqual(sql(f"SELECT ImagePath FROM Cinemas WHERE Id={cinema_id}"), path)
        self.assertEqual(self.client.request(f"/Admin/Cinemas/Delete/{cinema_id}")[0], 200)
        self.assertEqual(sql(f"SELECT COUNT(*) FROM Cinemas WHERE Id={cinema_id}"), "1")
        self.assertEqual(self.post_form(f"/Admin/Cinemas/Delete/{cinema_id}", {})[0], 302)
        self.assertEqual(sql(f"SELECT COUNT(*) FROM Cinemas WHERE Id={cinema_id}"), "0")
    def test_03_reject_unsafe_images(self):
        values = {"Name": self.prefix + " Invalid", "Biography": "This biography is long enough for validation."}
        png = (ROOT / "CinemaBooking/wwwroot/images/actor-1.png").read_bytes()
        cases = [("unsafe.svg", b'<svg xmlns="http://www.w3.org/2000/svg"/>', "image/svg+xml"), ("fake.png", b"MZ executable content", "image/png"), ("mismatch.jpg", png, "image/jpeg"), ("large.png", b"x" * (3 * 1024 * 1024 + 1), "image/png"), ("corrupt.jpg", b"\xff\xd8\xff\xe0not a jpeg", "image/jpeg")]
        for upload in cases:
            response = self.post_form("/Admin/Actors/Create", values, upload)
            self.assertEqual(response[0], 200, upload[0])
            self.assertIn('data-valmsg-for="Image"', response[1])
            self.assertIn("field-validation-error", response[1])
        self.assertEqual(sql(f"SELECT COUNT(*) FROM Actors WHERE Name='{values['Name']}'"), "0")
    def test_04_actor_movie_cast_crud(self):
        actor_name = self.prefix + " Actor"
        self.assertEqual(self.post_form("/Admin/Actors/Create", {"Name": actor_name, "Biography": "A fictional actor for the integration test."})[0], 302)
        actor_id = int(sql(f"SELECT Id FROM Actors WHERE Name='{actor_name}'"))
        self.assertEqual(self.post_form(f"/Admin/Actors/Edit/{actor_id}", {"Name": actor_name, "Biography": "An updated fictional biography for the integration test."})[0], 302)
        values = {"Title": self.prefix + " Movie", "Description": "A fictional movie used by this automated verification.", "Genre": "Drama", "DurationMinutes": "112"}
        data, _ = self.client.form("/Admin/Movies/Create")
        data.update(values)
        pairs = list(data.items()) + [("ActorIds", str(actor_id)), ("ActorIds", str(self.actor_ids[0]))]
        self.assertEqual(self.client.request("/Admin/Movies/Create", pairs)[0], 302)
        movie_id = int(sql(f"SELECT Id FROM Movies WHERE Title='{values['Title']}'"))
        self.assertEqual(sql(f"SELECT COUNT(*) FROM MovieActors WHERE MovieId={movie_id}"), "2")
        self.assertIn("Remove the cast assignment", self.client.request(f"/Admin/Actors/Delete/{actor_id}")[1])
        self.assertEqual(self.post_form(f"/Admin/Actors/Delete/{actor_id}", {})[0], 200)
        data, _ = self.client.form(f"/Admin/Movies/Edit/{movie_id}")
        data.update(values | {"DurationMinutes": "0"})
        response = self.client.request(f"/Admin/Movies/Edit/{movie_id}", list(data.items()) + [("ActorIds", str(actor_id)), ("ActorIds", str(self.actor_ids[0]))])
        self.assertEqual(response[0], 200)
        self.assertIn("field-validation-error", response[1])
        self.assertRegex(response[1], rf'<option selected="selected" value="{actor_id}">')
        data.update(values)
        self.assertEqual(self.client.request(f"/Admin/Movies/Edit/{movie_id}", list(data.items()) + [("ActorIds", str(actor_id))])[0], 302)
        self.assertEqual(sql(f"SELECT COUNT(*) FROM MovieActors WHERE MovieId={movie_id}"), "1")
        bad = self.post_form("/Admin/Movies/Create", values | {"ActorIds": "2147483647"})
        self.assertEqual(bad[0], 200)
        self.assertIn("Select existing actors", bad[1])
        self.assertEqual(self.post_form(f"/Admin/Movies/Delete/{movie_id}", {})[0], 302)
        self.assertEqual(self.post_form(f"/Admin/Actors/Delete/{actor_id}", {})[0], 302)
    def test_05_showtime_validation_crud_and_concurrency(self):
        values = {"MovieId": str(self.original_movie), "CinemaId": "2147483647", "StartAt": self.future, "TicketPrice": "100", "Capacity": "5"}
        response = self.post_form("/Admin/Showtimes/Create", values)
        self.assertEqual(response[0], 200)
        self.assertIn("existing movie and cinema", response[1])
        values["CinemaId"] = str(self.original_cinema)
        for invalid in [{"TicketPrice": "0"}, {"TicketPrice": "10.123"}, {"Capacity": "0"}]:
            rejected = self.post_form("/Admin/Showtimes/Create", values | invalid)
            self.assertEqual(rejected[0], 200)
            self.assertIn("field-validation-error", rejected[1])
        values["StartAt"] = "2020-01-01T19:30"
        self.assertIn("must be in the future", self.post_form("/Admin/Showtimes/Create", values)[1])
        show_id = self.create_show()
        path = f"/Admin/Showtimes/Edit/{show_id}"
        stale, _ = self.client.form(path)
        self.assertEqual(self.post_form(path, {"Capacity": "7"})[0], 302)
        stale["Capacity"] = "8"
        self.assertIn("edited by another request", self.client.request(path, stale)[1])
        self.assertEqual(sql(f"SELECT Capacity FROM Showtimes WHERE Id={show_id}"), "7")
        self.assertEqual(self.post_form(f"/Admin/Showtimes/Delete/{show_id}", {})[0], 302)
    def test_06_validation_and_related_id_tampering(self):
        show_id = self.create_show()
        path = f"/Bookings/Create/{show_id}"
        for changes in [{"Quantity": "0"}, {"Quantity": "11"}, {"Phone": "not-a-phone"}, {"Phone": "0      1"}, {"CustomerName": ""}]:
            data = self.booking(show_id) | changes
            response = self.client.request(path, data)
            self.assertEqual(response[0], 200)
            self.assertIn("field-validation-error", response[1])
            self.assertIn('value="01012345678"' if "Phone" not in changes else f'value="{changes["Phone"]}"', response[1])
        data = self.booking(show_id)
        data["SubmissionToken"] += "tampered"
        self.assertIn("expired or is invalid", self.client.request(path, data)[1])
        data = self.booking(show_id)
        self.assertEqual(self.client.request(f"/Bookings/Create/{show_id + 1}", data)[0], 400)
        other_id = self.create_show()
        data["ShowtimeId"] = str(other_id)
        self.assertIn("expired or is invalid", self.client.request(f"/Bookings/Create/{other_id}", data)[1])
        self.assertEqual(sql(f"SELECT COUNT(*) FROM Bookings WHERE ShowtimeId={show_id}"), "0")
    def test_07_booking_prg_authoritative_total_and_duplicates(self):
        show_id = self.create_show()
        path = f"/Bookings/Create/{show_id}"
        data = self.booking(show_id, quantity=2) | {"UnitPrice": "0.01", "TotalAmount": "0.02", "CreatedUtc": "2000-01-01", "Code": "ATTACK"}
        response = self.client.request(path, data)
        self.assertEqual(response[0], 302, response[1])
        location = response[2]["Location"]
        self.assertRegex(location, r"^/Bookings/Confirmation\?code=[A-F0-9]{32}$")
        self.assertEqual(sql(f"SELECT CONCAT(UnitPrice, '|', TotalAmount, '|', Quantity) FROM Bookings WHERE ShowtimeId={show_id}"), "170.50|341.00|2")
        self.assertEqual(sql(f"SELECT COUNT(*) FROM Bookings WHERE ShowtimeId={show_id} AND CreatedUtc > DATEADD(minute,-1,SYSUTCDATETIME())"), "1")
        self.assertEqual(self.client.request(location)[0], 200)
        html = self.client.request(location)[1]
        self.assertIn("341.00 EGP", html)
        self.assertIn("Print ticket", html)
        self.assertNotIn("01012345678", html)
        duplicate = self.client.request(path, data)
        self.assertEqual(duplicate[0], 302)
        self.assertEqual(duplicate[2]["Location"], location)
        self.assertEqual(sql(f"SELECT COUNT(*) FROM Bookings WHERE ShowtimeId={show_id}"), "1")
        changed = self.client.request(path, data | {"Quantity": "3"})
        self.assertEqual(changed[0], 200)
        self.assertIn("already been used", changed[1])
        self.__class__.booked_show = show_id
        self.__class__.confirmation = location
        (ROOT / "artifacts/verification/confirmation-path.txt").write_text(location)
    def test_08_booked_showtime_and_reference_guards(self):
        show_id = self.booked_show
        path = f"/Admin/Showtimes/Edit/{show_id}"
        for field, value in [("TicketPrice", "200"), ("MovieId", str(self.original_movie + 1)), ("CinemaId", str(self.original_cinema + 1)), ("StartAt", "2030-01-01T19:30")]:
            response = self.post_form(path, {field: value})
            self.assertEqual(response[0], 200)
            self.assertIn("cannot change", response[1])
        response = self.post_form(path, {"Capacity": "1"})
        self.assertIn("cannot be lower", response[1])
        self.assertEqual(self.post_form(path, {"Capacity": "8"})[0], 302)
        self.assertIn("history", self.post_form(f"/Admin/Showtimes/Delete/{show_id}", {})[1])
        self.assertEqual(self.post_form(f"/Admin/Movies/Delete/{self.original_movie}", {})[0], 200)
        self.assertEqual(self.post_form(f"/Admin/Cinemas/Delete/{self.original_cinema}", {})[0], 200)
        self.assertEqual(sql(f"SELECT COUNT(*) FROM Bookings WHERE ShowtimeId={show_id}"), "1")
        self.assertIn(self.prefix, self.client.request("/Admin/Bookings?Search=" + urllib.parse.quote(self.prefix))[1])
        booking_id = int(sql(f"SELECT Id FROM Bookings WHERE ShowtimeId={show_id}"))
        self.assertEqual(self.client.request(f"/Admin/Bookings/Details/{booking_id}")[0], 200)
    def test_09_expired_and_full_screenings(self):
        expired = self.create_show()
        data = self.booking(expired)
        sql(f"UPDATE Showtimes SET StartAt=DATEADD(day,-1,SYSDATETIME()) WHERE Id={expired}")
        response = self.client.request(f"/Bookings/Create/{expired}", data)
        self.assertEqual(response[0], 200)
        self.assertIn("already started", response[1])
        full = self.create_show(capacity=1)
        self.assertEqual(self.client.request(f"/Bookings/Create/{full}", self.booking(full))[0], 302)
        response = self.client.request(f"/Bookings/Create/{full}", self.booking(full))
        self.assertEqual(response[0], 200)
        self.assertIn("Only 0 tickets", response[1])
        self.assertEqual(sql(f"SELECT COUNT(*) FROM Bookings WHERE ShowtimeId={expired}"), "0")
    def test_10_concurrent_last_tickets(self):
        for capacity, quantity in [(1, 1), (2, 2)]:
            show_id = self.create_show(capacity=capacity)
            clients = [Client() for _ in range(8)]
            payloads = [self.booking(show_id, client, quantity, self.prefix + str(i)) for i, client in enumerate(clients)]
            barrier = threading.Barrier(len(clients))
            def submit(item):
                client, data = item
                barrier.wait(timeout=15)
                return client.request(f"/Bookings/Create/{show_id}", data)
            with concurrent.futures.ThreadPoolExecutor(max_workers=8) as pool:
                responses = list(pool.map(submit, zip(clients, payloads)))
            self.assertEqual([r[0] for r in responses].count(302), 1)
            self.assertTrue(all(r[0] in [200, 302] for r in responses))
            self.assertEqual(sql(f"SELECT SUM(Quantity) FROM Bookings WHERE ShowtimeId={show_id}"), str(capacity))
    def test_11_concurrent_duplicate_submission(self):
        show_id = self.create_show(capacity=1)
        data = self.booking(show_id)
        cookie_header = "; ".join(f"{c.name}={c.value}" for c in self.client.cookies)
        barrier = threading.Barrier(4)
        def submit(_):
            barrier.wait(timeout=15)
            request = urllib.request.Request(BASE + f"/Bookings/Create/{show_id}", data=urllib.parse.urlencode(data).encode(), headers={"Cookie": cookie_header})
            opener = urllib.request.build_opener(NoRedirect())
            try:
                response = opener.open(request, timeout=40)
            except urllib.error.HTTPError as error:
                response = error
            return response.code, response.headers.get("Location")
        with concurrent.futures.ThreadPoolExecutor(max_workers=4) as pool:
            responses = list(pool.map(submit, range(4)))
        self.assertEqual([r[0] for r in responses], [302] * 4)
        self.assertEqual(len(set(r[1] for r in responses)), 1)
        self.assertEqual(sql(f"SELECT COUNT(*) FROM Bookings WHERE ShowtimeId={show_id}"), "1")

if __name__ == "__main__":
    output = ROOT / "artifacts/verification"
    output.mkdir(parents=True, exist_ok=True)
    suite = unittest.defaultTestLoader.loadTestsFromTestCase(CinemaFlows)
    result = unittest.TextTestRunner(verbosity=2, stream=sys.stdout).run(suite)
    report = {"database": DATABASE, "base_url": BASE, "run_utc": datetime.now(timezone.utc).isoformat(), "tests_run": result.testsRun, "failures": [str(t) for t, _ in result.failures], "errors": [str(t) for t, _ in result.errors], "success": result.wasSuccessful()}
    (output / "integration-results.json").write_text(json.dumps(report, indent=2))
    raise SystemExit(0 if result.wasSuccessful() else 1)
