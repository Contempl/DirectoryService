import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  output: "standalone",
  async rewrites() {
    return process.env.NODE_ENV === "production"
      ? []
      : [
          {
            source: "/connect/:path*",
            destination: "http://localhost:5200/connect/:path*",
          },
        ];
  },
};

export default nextConfig;
