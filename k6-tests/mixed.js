import http from 'k6/http';
import { check, sleep } from 'k6';
import { htmlReport } from "https://raw.githubusercontent.com/benc-uk/k6-reporter/main/dist/bundle.js";

const TEST_NAME = "Mixed CRUD Load Test";
const TEST_DATE = new Date().toISOString();
const TEST_DESC = "Weighted mix: ~60% GET, 20% POST, 15% PUT, 5% DELETE with think times and rotating IDs.";

export function handleSummary(data) {
    data.metadata = { testName: TEST_NAME, testDate: TEST_DATE, description: TEST_DESC };
    return { [`${TEST_NAME.replace(/\s+/g, "_")}.html`]: htmlReport(data) };
}

export const options = {
    vus: 30,
    duration: '2m',
    thresholds: {
        http_req_failed: ['rate<0.01'],
        http_req_duration: ['p(95)<600']
    }
};

const BASE = __ENV.TARGET_URL || 'http://localhost:5031';

// Create a bunch upfront, then rotate among them to avoid cache artifacts.
export function setup() {
    const headers = { headers: { 'Content-Type': 'application/json' } };
    const makeOne = (i) => JSON.stringify({
        description: `Seed Booking #${i}`,
        type: i % 2 === 0 ? 'Apartment' : 'Vehicle',
        start: '2025-11-01',
        end: '2025-11-03',
        email: `seed${i}@example.com`
    });

    const ids = [];
    for (let i = 0; i < 20; i++) {
        const res = http.post(`${BASE}/bookings`, makeOne(i), headers);
        if (res.status === 201 || res.status === 200) {
            const id = (res.json() || {}).id;
            if (id) ids.push(id);
        }
        sleep(0.05);
    }
    return { ids };
}

function randomItem(arr) {
    return arr[Math.floor(Math.random() * arr.length)];
}

function createPayload() {
    const n = Math.floor(Math.random() * 100000);
    const startDay = 1 + (n % 20);
    const endDay = startDay + 1 + (n % 4);
    const pad = (x) => String(x).padStart(2, '0');

    return JSON.stringify({
        description: `New Booking ${n}`,
        type: (n % 3 === 0) ? 'Show' : ((n % 2 === 0) ? 'Apartment' : 'Vehicle'),
        start: `2025-10-${pad(startDay)}`,
        end: `2025-10-${pad(endDay)}`,
        email: `user${n}@example.com`
    });
}

function updatePayload() {
    const n = Math.floor(Math.random() * 100000);
    const startDay = 5 + (n % 20);
    const endDay = startDay + 1 + (n % 3);
    const pad = (x) => String(x).padStart(2, '0');

    return JSON.stringify({
        description: `Updated Booking ${n}`,
        type: (n % 2 === 0) ? 'Apartment' : 'Vehicle',
        start: `2025-11-${pad(startDay)}`,
        end: `2025-11-${pad(endDay)}`,
        email: `upd${n}@example.com`
    });
}

export default function (data) {
    const headers = { headers: { 'Content-Type': 'application/json' } };
    const r = Math.random();

    if (r < 0.60) {
        // GET list (most common)
        const res = http.get(`${BASE}/bookings`);
        check(res, { 'GET list 200': x => x.status === 200 });
        sleep(0.5 + Math.random() * 0.7); // think time
        return;
    }

    if (r < 0.80) {
        // POST create
        const res = http.post(`${BASE}/bookings`, createPayload(), headers);
        check(res, { 'POST created': x => x.status === 201 || x.status === 200 });
        sleep(1 + Math.random() * 1.5);
        return;
    }

    if (r < 0.95) {
        // PUT update (choose a seeded id)
        const id = randomItem(data.ids);
        if (id) {
            const res = http.put(`${BASE}/bookings/${id}`, updatePayload(), headers);
            check(res, { 'PUT ok': x => x.status === 200 || x.status === 204 });
        }
        sleep(0.6 + Math.random() * 0.8);
        return;
    }

    // DELETE (least common) - delete and (optionally) recreate to keep pool size healthy
    const id = randomItem(data.ids);
    if (id) {
        const del = http.del(`${BASE}/bookings/${id}`);
        check(del, { 'DEL ok': x => x.status === 200 || x.status === 204 });

        // optional: recreate to maintain pool size for subsequent steps
        const re = http.post(`${BASE}/bookings`, createPayload(), headers);
        const newId = (re.json() || {}).id;
        if (newId) {
            // replace the deleted id with a fresh one in the array
            const idx = data.ids.indexOf(id);
            if (idx >= 0) data.ids[idx] = newId;
        }
    }
    sleep(0.4 + Math.random() * 0.6);
}
