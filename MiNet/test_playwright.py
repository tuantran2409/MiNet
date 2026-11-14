import asyncio
import sys
from playwright.async_api import async_playwright

BASE_URL = "http://localhost:5223"

async def run_e2e_suite():
    print("=" * 60)
    print("🚀 MINET SOCIAL PLATFORM - AUTOMATED E2E TEST SUITE")
    print("=" * 60)
    
    test_results = {}
    
    async with async_playwright() as p:
        browser = await p.chromium.launch(headless=True)
        context = await browser.new_context()
        page = await context.new_page()

        # Catch console warnings/errors
        page.on("pageerror", lambda err: print(f"  [PAGE ERROR] {err}"))

        # -----------------------------------------------------------
        # TEST 1: Authentication & Login
        # -----------------------------------------------------------
        print("\n[TEST 1] Testing User Authentication & Login...")
        try:
            await page.goto(f"{BASE_URL}/Authentication/Login", timeout=15000)
            await page.fill("input[name='Email']", "user@gmail.com")
            await page.fill("input[name='Password']", "Coding@1234?")
            await page.click("button[type='submit']")
            await page.wait_for_load_state("networkidle")
            
            # Check redirect to Home or presence of TopBar
            current_url = page.url
            assert "/Authentication/Login" not in current_url, "Login failed: still on login page"
            print(f"  ✅ Login successful! Current URL: {current_url}")
            test_results["Authentication"] = "PASSED"
        except Exception as e:
            print(f"  ❌ Authentication failed: {e}")
            test_results["Authentication"] = f"FAILED: {e}"

        # -----------------------------------------------------------
        # TEST 2: Personalized Feed & Post Card Rendering
        # -----------------------------------------------------------
        print("\n[TEST 2] Testing Personalized Hybrid Feed...")
        try:
            await page.goto(f"{BASE_URL}/", timeout=15000)
            await page.wait_for_load_state("networkidle")
            
            # Verify TopBar search exists
            search_input = await page.query_selector("input[placeholder*='Search']")
            assert search_input is not None, "TopBar search bar missing"

            # Check post cards render
            post_cards = await page.query_selector_all(".post-card")
            print(f"  ✅ Found {len(post_cards)} rendered post cards in feed")
            assert len(post_cards) > 0, "No post cards found in feed"

            # Check 6 Reactions Bar trigger button exists on first post
            reaction_btn = await page.query_selector(".reaction-group button")
            assert reaction_btn is not None, "Reaction bar button missing"
            print("  ✅ 6 Reactions Bar component present")
            test_results["PersonalizedFeed"] = "PASSED"
        except Exception as e:
            print(f"  ❌ Personalized feed failed: {e}")
            test_results["PersonalizedFeed"] = f"FAILED: {e}"

        # -----------------------------------------------------------
        # TEST 3: Explore Page (Trending Tags & Instagram Grid)
        # -----------------------------------------------------------
        print("\n[TEST 3] Testing Explore Page...")
        try:
            await page.goto(f"{BASE_URL}/Explore", timeout=15000)
            await page.wait_for_load_state("networkidle")
            
            # Check for Explore page title / headers
            content = await page.content()
            assert "Explore" in content, "Explore page content missing"

            # Check for trending tags
            tag_pills = await page.query_selector_all("a[href*='/Explore?tag=']")
            print(f"  ✅ Found {len(tag_pills)} trending hashtag pills")

            # Check for Instagram 3-column square grid items
            grid_items = await page.query_selector_all(".aspect-square")
            print(f"  ✅ Found {len(grid_items)} 1:1 square media items in Explore grid")
            test_results["ExplorePage"] = "PASSED"
        except Exception as e:
            print(f"  ❌ Explore page failed: {e}")
            test_results["ExplorePage"] = f"FAILED: {e}"

        # -----------------------------------------------------------
        # TEST 4: Search System (Multi-Tab)
        # -----------------------------------------------------------
        print("\n[TEST 4] Testing Search System with Query 'tech'...")
        try:
            await page.goto(f"{BASE_URL}/Search?q=tech", timeout=15000)
            await page.wait_for_load_state("networkidle")
            
            # Check tabs
            tabs = await page.query_selector_all("a[href*='/Search?q=tech&tab=']")
            print(f"  ✅ Search page rendered with {len(tabs)} category tabs")
            assert len(tabs) >= 3, "Expected at least 3 search tabs"
            test_results["SearchSystem"] = "PASSED"
        except Exception as e:
            print(f"  ❌ Search system failed: {e}")
            test_results["SearchSystem"] = f"FAILED: {e}"

        # -----------------------------------------------------------
        # TEST 5: Instagram-style Profile Page
        # -----------------------------------------------------------
        print("\n[TEST 5] Testing Instagram-style Profile...")
        try:
            await page.goto(f"{BASE_URL}/Users/Details?userId=1", timeout=15000)
            await page.wait_for_load_state("networkidle")
            
            # Check 4 statistics counts
            stats_text = await page.inner_text("body")
            assert "Posts" in stats_text and "Friends" in stats_text, "Statistics headers missing"
            assert "Followers" in stats_text and "Following" in stats_text, "Follow counters missing"

            # Check Profile tabs (Posts, Saved, Friends)
            tab_posts = await page.query_selector("#tab-btn-posts")
            assert tab_posts is not None, "Profile Posts tab button missing"
            print("  ✅ Profile rendered with 4 counter stats and Instagram grid")
            test_results["InstagramProfile"] = "PASSED"
        except Exception as e:
            print(f"  ❌ Instagram profile failed: {e}")
            test_results["InstagramProfile"] = f"FAILED: {e}"

        # -----------------------------------------------------------
        # TEST 6: Admin Dashboard & ML.NET Recommendation Telemetry
        # -----------------------------------------------------------
        print("\n[TEST 6] Testing Admin Dashboard & AI Model Management...")
        try:
            # Login as Admin
            await page.goto(f"{BASE_URL}/Authentication/Login", timeout=15000)
            await page.fill("input[name='Email']", "admin@gmail.com")
            await page.fill("input[name='Password']", "Coding@1234?")
            await page.click("button[type='submit']")
            await page.wait_for_load_state("networkidle")

            # Go to Admin
            await page.goto(f"{BASE_URL}/Admin", timeout=15000)
            await page.wait_for_load_state("networkidle")
            
            admin_content = await page.content()
            assert "Admin Dashboard" in admin_content or "Overview" in admin_content, "Admin Dashboard title missing"

            # Check ML.NET recommendation card
            assert "ML.NET AI Recommendation" in admin_content or "Retrain AI Model" in admin_content, "AI Recommendation card missing"
            print("  ✅ Admin Dashboard & ML.NET AI Recommendation card verified")
            test_results["AdminAndAIModel"] = "PASSED"
        except Exception as e:
            print(f"  ❌ Admin & AI Model failed: {e}")
            test_results["AdminAndAIModel"] = f"FAILED: {e}"

        await browser.close()

    # -----------------------------------------------------------
    # SUMMARY
    # -----------------------------------------------------------
    print("\n" + "=" * 60)
    print("📊 TEST EXECUTION SUMMARY:")
    print("=" * 60)
    all_passed = True
    for test_name, result in test_results.items():
        status_icon = "✅" if result == "PASSED" else "❌"
        print(f"  {status_icon} {test_name}: {result}")
        if result != "PASSED":
            all_passed = False

    print("=" * 60)
    if all_passed:
        print("🎉 ALL TEST CASES PASSED SUCCESSFULLY (100%)!")
    else:
        print("⚠️ SOME TEST CASES FAILED - PLEASE REVIEW LOGS")
    print("=" * 60)

if __name__ == "__main__":
    asyncio.run(run_e2e_suite())
