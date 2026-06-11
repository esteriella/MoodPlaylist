const config = {
  content: [
  "./app/**/*.{js,ts,jsx,tsx}",
  "./components/**/*.{js,ts,jsx,tsx}",
  ],
  // theme:{
  // extend: {
  //   colors: {
  //     moodGradientStart: "#6366F1",
  //     moodGradientMid: "#8B5CF6",
  //     moodGradientEnd: "#EC4899",
  //     },
  //   },
  // },
  plugins: {
    "@tailwindcss/postcss": {},
  },
};

export default config;

// /** @type {import('tailwindcss').Config} */
// export const content = [
//   "./app/**/*.{js,ts,jsx,tsx}",
//   "./components/**/*.{js,ts,jsx,tsx}",
// ];
// export const theme = {
//   extend: {
//     colors: {
//       moodGradientStart: "#6366F1",
//       moodGradientMid: "#8B5CF6",
//       moodGradientEnd: "#EC4899",
//     },
//   },
// };
// export const plugins = [];

