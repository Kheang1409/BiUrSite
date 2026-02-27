#!/bin/bash

# Colors for output
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${BLUE}BiUrSite Next.js Frontend Setup${NC}"
echo "================================"

# Check Node.js
if ! command -v node &> /dev/null; then
    echo -e "${YELLOW}Node.js is not installed. Please install Node.js 18+${NC}"
    exit 1
fi

echo -e "${GREEN}✓ Node.js version: $(node -v)${NC}"

# Check npm
if ! command -v npm &> /dev/null; then
    echo -e "${YELLOW}npm is not installed${NC}"
    exit 1
fi

echo -e "${GREEN}✓ npm version: $(npm -v)${NC}"

# Install dependencies
echo -e "${BLUE}Installing dependencies...${NC}"
npm install

if [ $? -eq 0 ]; then
    echo -e "${GREEN}✓ Dependencies installed${NC}"
else
    echo -e "${YELLOW}Failed to install dependencies${NC}"
    exit 1
fi

# Type check
echo -e "${BLUE}Running type check...${NC}"
npm run type-check

if [ $? -eq 0 ]; then
    echo -e "${GREEN}✓ Type check passed${NC}"
else
    echo -e "${YELLOW}Type check failed, but setup is complete${NC}"
fi

echo ""
echo -e "${GREEN}Setup complete!${NC}"
echo ""
echo -e "${BLUE}Next steps:${NC}"
echo "1. Configure environment variables in .env.local"
echo "2. Run: npm run dev"
echo "3. Open http://localhost:3000 in your browser"
echo ""
