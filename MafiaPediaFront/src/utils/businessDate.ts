/**
 * Mirrors BusinessDateHelper.Today() from the backend.
 * Cutoff: 12:00 PM (noon). Before noon → yesterday, noon or later → today.
 */
export function getBusinessDateStr(): string {
  const now = new Date()
  const d = new Date(now)
  if (d.getHours() < 12) {
    d.setDate(d.getDate() - 1)
  }
  const y = d.getFullYear()
  const m = String(d.getMonth() + 1).padStart(2, '0')
  const day = String(d.getDate()).padStart(2, '0')
  return `${y}-${m}-${day}`
}
