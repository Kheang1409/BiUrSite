import type { Config } from "tailwindcss";

const config: Config = {
  content: [
    "./src/pages/**/*.{js,ts,jsx,tsx,mdx}",
    "./src/components/**/*.{js,ts,jsx,tsx,mdx}",
    "./src/app/**/*.{js,ts,jsx,tsx,mdx}",
  ],
  theme: {
    extend: {
      colors: {
        // Primary palette - Brand colors
        primary: {
          1: "#0682a5", // Vibrant cyan-blue (main)
          2: "#223056", // Slate blue (hover)
          3: "#0f172a", // Deep navy (background)
        },
        // Secondary palette - Supporting colors
        secondary: {
          1: "#6b7280", // Gray 500
          2: "#374151", // Gray 700
        },
        // Tertiary palette - Accent colors
        tertiary: {
          1: "#7c3aed", // Violet 600
          2: "#5b21b6", // Violet 800
        },
        // Semantic colors
        danger: {
          1: "#ef4444", // Red 500
          2: "#b91c1c", // Red 800
        },
        success: {
          1: "#22c55e", // Green 500
          2: "#16a34a", // Green 600
        },
        warning: {
          1: "#f59e0b", // Amber 500
          2: "#d97706", // Amber 600
        },
        // Neutral/Utility colors
        muted: "#94a3b8",
        card: "rgba(255, 255, 255, 0.06)",
      },
      typography: {
        xs: "12px",
        sm: "14px",
        base: "16px",
        md: "25.9px",
        lg: "41.9px",
        xl: "67.8px",
        xxl: "109.7px",
      },
      borderRadius: {
        DEFAULT: "10px",
      },
      maxWidth: {
        container: "1200px",
      },
      backgroundImage: {
        "gradient-dark":
          "linear-gradient(to bottom, #0f172a 0%, #1e293b 50%, #0f172a 100%)",
      },
      spacing: {
        "safe-top": "env(safe-area-inset-top)",
        "safe-bottom": "env(safe-area-inset-bottom)",
        "safe-left": "env(safe-area-inset-left)",
        "safe-right": "env(safe-area-inset-right)",
      },
      animation: {
        "pulse-gentle": "pulse-gentle 2s cubic-bezier(0.4, 0, 0.6, 1) infinite",
      },
    },
  },
  darkMode: "class",
  plugins: [],
};
export default config;
