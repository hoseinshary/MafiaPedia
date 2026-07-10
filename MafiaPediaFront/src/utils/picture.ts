export function getPictureUrl(picture: string | null | undefined): string {
  if (!picture) return ''
  if (picture.startsWith('http')) return picture
  const base = (import.meta.env.VITE_API_BASE_URL as string || 'http://localhost:5272/api').replace(/\/api$/, '')
  return `${base}${picture.startsWith('/') ? '' : '/'}${picture}`
}
