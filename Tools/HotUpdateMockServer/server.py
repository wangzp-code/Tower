#!/usr/bin/env python3
"""Mock hot update server with proper HTTP/1.1 support and CORS headers."""
import http.server
import socketserver
import json
import os
import hashlib

PORT = 8888
ROOT = os.path.dirname(os.path.abspath(__file__))

class MockHandler(http.server.SimpleHTTPRequestHandler):
    def do_GET(self):
        # Add CORS and cache headers
        self.send_header('Access-Control-Allow-Origin', '*')
        self.send_header('Access-Control-Allow-Methods', 'GET, POST, OPTIONS')
        self.send_header('Access-Control-Allow-Headers', 'Content-Type')
        self.send_header('Cache-Control', 'no-cache, no-store, must-revalidate')
        self.send_header('Pragma', 'no-cache')
        self.send_header('Expires', '0')
        
        # Let the parent handle the actual file serving
        return super().do_GET()
    
    def do_OPTIONS(self):
        self.send_response(200)
        self.send_header('Access-Control-Allow-Origin', '*')
        self.send_header('Access-Control-Allow-Methods', 'GET, OPTIONS')
        self.send_header('Access-Control-Allow-Headers', 'Content-Type')
        self.end_headers()
    
    def end_headers(self):
        self.send_header('Access-Control-Allow-Origin', '*')
        self.send_header('Cache-Control', 'no-cache')
        super().end_headers()
    
    def log_message(self, format, *args):
        print(f"[MockServer] {self.address_string()} - {format % args}")

if __name__ == '__main__':
    os.chdir(ROOT)
    
    # Compute hashes for test bundles
    manifest_path = os.path.join(ROOT, "1.1.0", "manifest.json")
    if os.path.exists(manifest_path):
        with open(manifest_path, 'r') as f:
            manifest = json.load(f)
        
        for bundle in manifest['bundles']:
            bundle_path = os.path.join(ROOT, "1.1.0", bundle['name'])
            if os.path.exists(bundle_path):
                with open(bundle_path, 'rb') as bf:
                    md5 = hashlib.md5(bf.read()).hexdigest()
                size = os.path.getsize(bundle_path)
                print(f"  Bundle: {bundle['name']} size={size} hash={md5}")
                bundle['size'] = size
                bundle['hash'] = md5
        
        with open(manifest_path, 'w') as f:
            json.dump(manifest, f, indent=4)
    
    with socketserver.TCPServer(("", PORT), MockHandler) as httpd:
        print(f"Mock Hot Update Server running at http://127.0.0.1:{PORT}/")
        print(f"Root directory: {ROOT}")
        print("Press Ctrl+C to stop")
        httpd.serve_forever()