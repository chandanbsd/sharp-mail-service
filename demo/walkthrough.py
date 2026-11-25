"""Exercise the local ASP.NET Core API and inspect its captured SMTP message."""
import argparse
import json
from concurrent.futures import ThreadPoolExecutor
from datetime import datetime, timezone
from urllib.error import HTTPError
from urllib.request import Request, urlopen

PORTS = {"sql": 18081, "blobs": 18082, "cosmos": 18083}


def request(method, url, data=None, expected=200):
    payload = json.dumps(data).encode() if data is not None else None
    message = Request(url, data=payload, method=method,
                      headers={"Content-Type": "application/json"})
    try:
        response = urlopen(message, timeout=30)
    except HTTPError as error:
        response = error
    with response:
        body = response.read()
        if response.status != expected:
            raise RuntimeError(f"{method} {url}: {response.status} {body[:400]!r}")
        return json.loads(body)


def demonstrate(store):
    api = f"http://localhost:{PORTS[store]}"
    attempt_id = request("POST", f"{api}/attempt-ids")["attemptId"]
    attempt_url = f"{api}/attempts/{attempt_id}"
    content = request("POST", f"{api}/preview", {
        "templateKey": "orders/receipt",
        "model": {"orderNumber": "42", "customerName": "Pat & Jo"}
    })
    assert "Pat &amp; Jo" in content["htmlBody"] and "Pat & Jo" in content["textBody"]
    message = {
        "sender": {"address": "store@demo.invalid"},
        "to": [{"address": "customer@demo.invalid"}],
        "cc": [{"address": "accounts@demo.invalid"}],
        "bcc": [{"address": "archive@demo.invalid"}],
        "content": content
    }
    submission = {"correlationId": "order-42", "message": message}
    receipt = request("PUT", attempt_url, submission)
    assert receipt["attempt"]["status"] == "Accepted" and receipt["outcomePersisted"]
    assert request("PUT", attempt_url, submission)["reused"]
    conflict = {**submission, "message": {**message, "content": {**content, "subject": "Changed"}}}
    request("PUT", attempt_url, conflict, expected=409)

    for role, kind in [("To", "Delivered"), ("Cc", "Bounced")]:
        callback = {
            "transport": "local-mailpit", "providerMessageId": receipt["attempt"]["providerMessageId"],
            "notification": {"eventId": f"event-{kind}", "recipientRole": role, "recipientIndex": 0,
                             "kind": kind, "occurredAtUtc": datetime.now(timezone.utc).isoformat()}
        }
        assert request("POST", f"{api}/delivery-events", callback)["appended"]
        assert not request("POST", f"{api}/delivery-events", callback)["appended"]
    detail = request("GET", attempt_url)
    assert detail["attempt"]["message"]["content"] == content
    assert len(detail["deliveryEvents"]) == 2 and detail["attempt"]["acceptedBySdkAtUtc"]
    inbox = request("GET", f"http://localhost:18025/api/v1/search?query=message-id:{attempt_id}@sharpmail.invalid")
    assert inbox["messages_count"] == 1, inbox
    print(f"{store}: {attempt_url} — accepted once, original content retained, two delivery events")

    failed_id = request("POST", f"{api}/attempt-ids")["attemptId"]
    rejected = {**message, "to": [{"address": "rejected@example.test"}], "cc": [], "bcc": []}
    result = request("PUT", f"{api}/attempts/{failed_id}", {**submission, "message": rejected})
    failed = result["attempt"]
    assert failed["status"] == "Failed" and failed["acceptedBySdkAtUtc"] is None
    assert result["outcomePersisted"]
    print(f"{store}: {failed_id} — local SMTP rejection recorded")
    demonstrate_concurrent_submissions(api, submission)


def demonstrate_concurrent_submissions(api, submission):
    attempt_id = request("POST", f"{api}/attempt-ids")["attemptId"]
    url = f"{api}/attempts/{attempt_id}"

    def submit_variant(number):
        subject = f"Concurrent receipt {number}"
        variant = {**submission["message"], "content": {"subject": subject, "textBody": "Plain receipt"}}
        payload = json.dumps({**submission, "message": variant}).encode()
        call = Request(url, data=payload, method="PUT", headers={"Content-Type": "application/json"})
        try:
            with urlopen(call, timeout=30) as response:
                assert json.load(response)["attempt"]["status"] == "Accepted"
                return subject
        except HTTPError as error:
            if error.code != 409:
                raise
            return None

    with ThreadPoolExecutor(max_workers=6) as executor:
        winners = [subject for subject in executor.map(submit_variant, range(6)) if subject]
    assert len(winners) == 1
    assert request("GET", url)["attempt"]["message"]["content"]["subject"] == winners[0]
    inbox = request("GET", f"http://localhost:18025/api/v1/search?query=message-id:{attempt_id}@sharpmail.invalid")
    assert inbox["messages_count"] == 1
    print(f"{url} — six competing submissions, one accepted message")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("stores", nargs="*", default=list(PORTS))
    args = parser.parse_args()
    for provider in args.stores:
        demonstrate(provider)
