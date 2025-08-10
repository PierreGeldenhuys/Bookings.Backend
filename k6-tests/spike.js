import http from 'k6/http';
import { check, sleep } from 'k6';
import { htmlReport } from "https://raw.githubusercontent.com/benc-uk/k6-reporter/main/dist/bundle.js";

const TEST_NAME = "Spike Load Test";
const TEST_DATE = new Date().toISOString();
const TEST_DESC = "Simulates a sudden surge of traffic to test how the API handles rapid scaling and recovery.";

export function handleSummary(data) {
    data.metadata = { testName: TEST_NAME, testDate: TEST_DATE, description: TEST_DESC };
    return { [`${TEST_NAME.replace(/\s+/g, "_")}.html`]: htmlReport(data) };
}


export const options = {
  stages: [
    { duration: '5s', target: 0 },     // idle
    { duration: '10s', target: 200 },  // spike up
    { duration: '20s', target: 0 },    // drop to zero
  ],
  thresholds: {
    http_req_failed: ['rate<0.02'],
    http_req_duration: ['p(95)<800']
  }
};

const BASE = __ENV.TARGET_URL || 'http://localhost:5031';

export default function () {
  const payload = JSON.stringify({
    description: `Spike user ${__VU}`,
    type: 'Apartment',
    start: '2025-12-01',
    end: '2025-12-05',
    email: 'spike@example.com'
  });
  const res = http.post(`${BASE}/bookings`, payload, { headers: { 'Content-Type': 'application/json' } });

  check(res, {
    'created or ok': r => r.status === 201 || r.status === 200
  });

  sleep(0.2);
}
