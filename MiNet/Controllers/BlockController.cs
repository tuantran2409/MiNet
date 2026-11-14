using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MiNet.Controllers.Base;
using MiNet.Data.Helpers.Constants;
using MiNet.Data.Services;
using System.Threading.Tasks;

namespace MiNet.Controllers
{
    [Authorize(Roles = AppRoles.User)]
    public class BlockController : BaseController
    {
        private readonly IBlockService _blockService;

        public BlockController(IBlockService blockService)
        {
            _blockService = blockService;
        }

        [HttpPost]
        public async Task<IActionResult> BlockUser(int targetUserId)
        {
            var loggedInUserId = GetUserId();
            if (loggedInUserId == null) return Unauthorized();

            if (loggedInUserId.Value == targetUserId)
                return BadRequest("Cannot block yourself.");

            bool success = await _blockService.BlockUserAsync(loggedInUserId.Value, targetUserId);
            return Json(new { success });
        }

        [HttpPost]
        public async Task<IActionResult> UnblockUser(int targetUserId)
        {
            var loggedInUserId = GetUserId();
            if (loggedInUserId == null) return Unauthorized();

            bool success = await _blockService.UnblockUserAsync(loggedInUserId.Value, targetUserId);
            return Json(new { success });
        }
    }
}

