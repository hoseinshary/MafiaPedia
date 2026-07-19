/** @type {import('tailwindcss').Config} */
export default {
  darkMode: 'class',
  content: [
    "./index.html",
    "./src/**/*.{vue,js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {
      colors: {
        bg: 'var(--color-bg)',
        surface: 'var(--color-surface)',
        'surface-hover': 'var(--color-surface-hover)',
        border: 'var(--color-border)',
        input: 'var(--color-input-bg)',
        'input-border': 'var(--color-input-border)',
        fg: 'var(--color-fg)',
        muted: 'var(--color-muted)',
        gold: 'var(--color-gold)',
        'gold-text': 'var(--color-gold-text)',
        success: 'var(--color-success)',
        danger: 'var(--color-danger)',
        info: 'var(--color-info)',
      },
    },
  },
  plugins: [],
}
