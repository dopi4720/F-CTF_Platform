/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./src/**/*.{js,jsx,ts,tsx}",
  ],
  theme: {
    extend: {
      colors: {
        primary: '#ff6700', // thay màu này bằng màu bạn muốn cho primary
        secondary: '#00ba31', // thay màu này bằng màu bạn muốn cho secondary
        'theme-color-primary': '#ff6700',
        'theme-color-primary-dark':'#cc5200',
        "theme-color-secondary":"#00ba31",
        "theme-color-secondary-dark":"#009427"
      }
    },
  },
  plugins: [],
}
