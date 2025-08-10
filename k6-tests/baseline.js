import http from 'k6/http';
import { check, sleep } from 'k6';
import { htmlReport } from "https://raw.githubusercontent.com/benc-uk/k6-reporter/main/dist/bundle.js";

// --- Metadata ---
const TEST_NAME = "Baseline Load Test";
const TEST_DATE = new Date().toISOString();
const TEST_DESC = "Simple POST and GET /bookings load test to measure baseline API latency and error rate.";

export function handleSummary(data) {
    data.metadata = { testName: TEST_NAME, testDate: TEST_DATE, description: TEST_DESC };
    return { [`${TEST_NAME.replace(/\s+/g, "_")}.html`]: htmlReport(data) };
}

export let options = {
  vus: 10,              // 10 virtual users
  duration: '30s',      // run for 30 seconds
  thresholds: {
    http_req_duration: ['p(95)<500'], // 95% of requests must complete < 500ms
    http_req_failed: ['rate<0.01']    // <1% errors
  }
};

const BASE_URL = __ENV.TARGET_URL || 'http://localhost:5031';

function createBooking() {
    const res = http.post(`${BASE_URL}/bookings`, JSON.stringify({
        description: 'Temp create',
        type: 'Apartment',
        start: '2025-10-01',
        end: '2025-10-03',
        email: 'temp@example.com'
    }), { headers: { 'Content-Type': 'application/json' } });
    return res;
}

export default function () {
    createBooking();
    let res = http.get(`${BASE_URL}/bookings`);
    check(res, {
        'Status is 200': (r) => r.status === 200,
        'Body not empty': (r) => r.body && r.body.length > 0
  });

  sleep(1); // wait 1 second between iterations
}
