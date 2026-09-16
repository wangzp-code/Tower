#!/bin/bash
# 本地服务器脚本 - 用于热更新测试
# 使用方法: ./StartLocalServer.sh [端口]

PORT=${1:-8080}
BUNDLE_DIR="$(pwd)/Bundles"

echo "=== 本地热更新服务器 ==="
echo "端口: $PORT"
echo "资源目录: $BUNDLE_DIR"
echo "访问地址: http://$(ipconfig getifaddr en0 2>/dev/null || ifconfig en0 2>/dev/null | grep 'inet ' | awk '{print $2}'):$PORT"
echo ""

if [ ! -d "$BUNDLE_DIR" ]; then
    echo "警告: $BUNDLE_DIR 目录不存在，请先使用 Unity 打包资源"
    mkdir -p "$BUNDLE_DIR"
fi

cd "$BUNDLE_DIR"

# 检查可用的 Python 版本
if command -v python3 &> /dev/null; then
    echo "使用 Python3 启动服务器..."
    python3 -m http.server "$PORT" --bind 0.0.0.0
elif command -v python &> /dev/null; then
    echo "使用 Python 启动服务器..."
    python -m SimpleHTTPServer "$PORT"
elif command -v node &> /dev/null; then
    echo "使用 Node.js 启动服务器..."
    npx http-server -p "$PORT" -a 0.0.0.0
else
    echo "错误: 未找到 Python3/Python/Node.js"
    echo "请安装 Python3: brew install python3"
    exit 1
fi
