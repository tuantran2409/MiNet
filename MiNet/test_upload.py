import requests
import json

session = requests.Session()
# Go to login page to get anti-forgery token (optional if not needed)
res = session.get("http://localhost:5223/Authentication/Login")

# Assume Login uses Email and Password. Let's find exactly what to POST
# We can just attempt to login:
login_data = {
    "Email": "user@gmail.com",
    "Password": "Coding@1234?"
}
res = session.post("http://localhost:5223/Authentication/Login", data=login_data)
print("Login status:", res.status_code)

# Try uploading file
files = {'file': ('test.txt', b'hello world!')}
upload_res = session.post("http://localhost:5223/Chat/UploadAttachment", files=files)
print("Upload status:", upload_res.status_code)
print("Upload response:", upload_res.text)

