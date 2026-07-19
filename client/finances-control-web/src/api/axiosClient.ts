import axios from 'axios'

const unsafeMethods = new Set(['post', 'put', 'patch', 'delete'])

export const axiosClient = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL,
  timeout: 10000,
  withCredentials: true,
  xsrfCookieName: 'XSRF-TOKEN',
  xsrfHeaderName: 'X-XSRF-TOKEN',
  withXSRFToken: (config) => unsafeMethods.has(config.method?.toLowerCase() ?? ''),
  headers: {
    'Content-Type': 'application/json',
  },
})
