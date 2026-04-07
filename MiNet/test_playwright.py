import asyncio
from playwright.async_api import async_playwright

async def run():
    async with async_playwright() as p:
        browser = await p.chromium.launch()
        page = await browser.new_page()
        
        # Log all console messages
        page.on("console", lambda msg: print(f"CONSOLE: {msg.type}: {msg.text}"))
        page.on("pageerror", lambda err: print(f"PAGE ERROR: {err}"))
        
        print("Navigating to Login")
        await page.goto("http://localhost:5223/Authentication/Login")
        
        # Login
        await page.fill("input[name='Email']", "user@gmail.com")
        await page.fill("input[name='Password']", "Coding@1234?")
        await page.click("button[type='submit']")
        await page.wait_for_timeout(1000)
        
        print("Navigating to Chat")
        await page.goto("http://localhost:5223/Chat/Index")
        await page.wait_for_timeout(1000)
        
        # Click the first conversation
        first_chat = await page.query_selector("a[href^='/Chat/Message/']")
        if first_chat:
            href = await first_chat.get_attribute("href")
            print(f"Going to {href}")
            await page.goto(f"http://localhost:5223{href}")
            await page.wait_for_timeout(2000)
            
            # Click attach button
            print("Clicking attach button")
            await page.click("label#attachBtn")
            await page.wait_for_timeout(1000)
            
            # Click send button
            print("Clicking send button")
            await page.fill("textarea#messageInput", "Test")
            await page.click("button#sendBtn")
            await page.wait_for_timeout(1000)
        else:
            print("No conversation found!")
            
        await browser.close()

asyncio.run(run())
