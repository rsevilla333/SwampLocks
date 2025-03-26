import type { Config } from "tailwindcss";

export default {
  content: [
    "./src/pages/**/*.{js,ts,jsx,tsx,mdx}",
    "./src/components/**/*.{js,ts,jsx,tsx,mdx}",
    "./src/app/**/*.{js,ts,jsx,tsx,mdx}",
  ],
  // #000000, #0B3D0B, #166D16, #219D21, #2CCE2C
  theme: {
    extend: {
      colors: {
        background: '#F8FAFC',  
        primary:  '#D9EAFD',     
        secondary: '#BCCCDC',   
        accent: '#9AA6B2',
      },
    },
  },
  plugins: [],
} satisfies Config;
