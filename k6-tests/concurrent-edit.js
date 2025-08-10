import http from 'k6/http';
import { check, sleep } from 'k6';
import { htmlReport } from "https://raw.githubusercontent.com/benc-uk/k6-reporter/main/dist/bundle.js";

const TEST_NAME = "Concurrent Edit Test";
const TEST_DATE = new Date().toISOString();
const TEST_DESC = "Multiple users concurrently updating the same booking ID to check thread safety and data integrity.";

export function handleSummary(data) {
    data.metadata = { testName: TEST_NAME, testDate: TEST_DATE, description: TEST_DESC };
    return { [`${TEST_NAME.replace(/\s+/g, "_")}.html`]: htmlReport(data) };
}

export const options = {
  vus: 40,
  duration: '30s',
  thresholds: {
    http_req_failed: ['rate<0.02'],
    http_req_duration: ['p(95)<700']
  }
};

const BASE = __ENV.TARGET_URL || 'http://localhost:5031';

export function setup() {
  // Create one booking all VUs will hammer
  const seed = http.post(`${BASE}/bookings`, JSON.stringify({
    description: 'Contention seed',
    type: 'Vehicle',
    start: '2025-12-10',
    end: '2025-12-12',
    email: 'seed@example.com'
  }), { headers: { 'Content-Type': 'application/json' } });

  const id = (seed.json() || {}).id;
  if (!id) {
    throw new Error(`Failed to seed booking: status=${seed.status}, body=${seed.body}`);
  }
  return { id };
}

export default function (data) {
  const payload = JSON.stringify({
    description: `Updated by VU ${__VU}`,
    type: 'Vehicle',
    start: '2025-12-10',
    end: '2025-12-13',
    email: 'update@example.com'
  });

  const res = http.put(`${BASE}/bookings/${data.id}`, payload, { headers: { 'Content-Type': 'application/json' } });

  check(res, {
      'Status is 200': (r) => r.status === 200,
      'Body not empty': (r) => r.body && r.body.length > 0
  });

  sleep(0.2);
}
